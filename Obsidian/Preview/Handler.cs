using Fantome.Libraries.League.IO.SimpleSkin;
using Fantome.Libraries.League.IO.WAD;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Fantome.Libraries.League.Helpers.Structures;
using Fantome.Libraries.League.IO.SCB;
using Fantome.Libraries.League.IO.SCO;
using Imaging.DDSReader;
using CheckBox = System.Windows.Controls.CheckBox;
using ComboBox = System.Windows.Controls.ComboBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;

namespace Obsidian.Preview
{
    class InfoHolder
    {
        public WADEntry sknPath { get; }
        public WADEntry sklPath { get; }
        public Dictionary<string, WADEntry> textures { get; }


        public InfoHolder(WADEntry sknPath, WADEntry sklPath, Dictionary<string, WADEntry> textures)
        {
            this.sknPath = sknPath;
            this.sklPath = sklPath;
            this.textures = textures;
        }
    }

    class MeshesComboBoxHolder
    {
        public MeshesComboBoxHolder(CheckBox checkBox, TextBlock block, ComboBoxItem item, GeometryModel3D model,
            Model3DGroup group)
        {
            this.checkBox = checkBox;
            this.block = block;
            this.item = item;
            this.model = model;
            this.group = group;
            this.visisble = true;
            this.checkBox.Click += clicked;
            this.checkBox.IsChecked = true;
        }

        void clicked(object sender, RoutedEventArgs e)
        {
            if (visisble)
                group.Children.Remove(model);
            else
                group.Children.Add(model);

            visisble = !visisble;
        }

        public CheckBox checkBox { get; }
        public TextBlock block { get; }
        public ComboBoxItem item { get; }
        public GeometryModel3D model { get; }
        public Model3DGroup group { get; }
        public bool visisble { get; private set; }
    }

    public class Handler
    {
        private int mode;

        private readonly MainWindow window;
        private readonly Viewport3D _viewPort;
        private readonly Model3DGroup _modelGroup;
        private readonly Label _previewNameLabel;
        private readonly Label _previewTypeLabel;
        private readonly ComboBox _previewTextureComboBox;
        private readonly ComboBox _previewMeshesComboBox;
        private readonly PerspectiveCamera _previewCamera;
        private readonly StackPanel _previewStackPanel;
        private readonly Image _previewImage;
        private readonly Expander _previewExpander;
        private readonly MouseHandler handler;
        public Tuple<string, ulong> PreSelect = null;


        private Dictionary<GeometryModel3D, string> materialMap = new Dictionary<GeometryModel3D, string>();
        private Dictionary<MeshesComboBoxHolder, bool> modelMap = new Dictionary<MeshesComboBoxHolder, bool>();

        public Handler(MainWindow window, Viewport3D viewPort, Model3DGroup modelGroup, Label previewNameLabel,
            Label previewTypeLabel, ComboBox previewTextureComboBox, ComboBox previewMeshesComboBox,
            PerspectiveCamera previewCamera, StackPanel previewStackPanel, Image prevcImage, Expander previewExpander)
        {
            this.window = window;
            _viewPort = viewPort;
            _modelGroup = modelGroup;
            _previewNameLabel = previewNameLabel;
            _previewTypeLabel = previewTypeLabel;
            _previewTextureComboBox = previewTextureComboBox;
            _previewMeshesComboBox = previewMeshesComboBox;
            _previewCamera = previewCamera;
            _previewStackPanel = previewStackPanel;
            _previewImage = prevcImage;
            _previewExpander = previewExpander;

            _previewMeshesComboBox.SelectionChanged += meshesComboBoxSelecChange;
            previewTextureComboBox.SelectionChanged += textureComboBoxChanged;

            _previewTextureComboBox.IsEnabled = false;
            _previewMeshesComboBox.IsEnabled = false;

            handler = new MouseHandler();
            handler.Attach(window);
            handler.Slaves.Add(_viewPort);
            handler.Enabled = true;
            clearUp();
        }

        public void clearUp()
        {
            _previewNameLabel.Content = "Name: ";
            _previewTypeLabel.Content = "Type: ";
            foreach (var pair in materialMap)
                _modelGroup.Children.Remove(pair.Key);

            materialMap.Clear();
            modelMap.Clear();
            _previewMeshesComboBox.Items.Clear();

            List<string> data = new List<string>();
            data.Add("default");
            foreach (var str in MainWindow.StringDictionary)
                if (str.Value.ToLower().EndsWith(".dds"))
                    data.Add(str.Value);

            _previewTextureComboBox.ItemsSource = data;
            _previewTextureComboBox.SelectedIndex = 0;
            handler.Reset();
        }


        private void handle(WADEntry entry, string namePath)
        {

            if (namePath.ToLower().EndsWith(".dds"))
            {
                if (mode == 0)
                {
                    mode = 1;
                    _viewPort.Visibility = Visibility.Collapsed;
                    _previewImage.Visibility = Visibility.Visible;
                    _previewTextureComboBox.IsEnabled = false;
                    _previewMeshesComboBox.IsEnabled = false;
                }

                _previewTypeLabel.Content = "Type: DirectDraw Surface";
                _previewNameLabel.Content = "Name: " + namePath.Split('/').Last();

                var stream = new MemoryStream();
                var image = DDS.LoadImage(entry.GetContent(true));
                image.Save(stream, ImageFormat.Png);


                var imageSource = new BitmapImage();
                imageSource.CacheOption = BitmapCacheOption.OnLoad;
                imageSource.BeginInit();
                imageSource.StreamSource = stream;
                imageSource.EndInit();
                imageSource.Freeze();
                _previewImage.Dispatcher.BeginInvoke(new Action(() =>
                {
                    _previewImage.Source = imageSource;
                    _previewImage.Width = image.Width;
                    _previewImage.Height = image.Height;
                }));
                return;
            }

            if (mode == 1)
            {
                mode = 0;
                _previewImage.Visibility = Visibility.Collapsed;
                _viewPort.Visibility = Visibility.Visible;
            }

            clearUp();
            _previewTextureComboBox.IsEnabled = true;
            _previewMeshesComboBox.IsEnabled = true;

            if (!_previewExpander.IsExpanded)
                _previewExpander.IsExpanded = true;


            if (namePath.ToLower().EndsWith(".scb"))
            {
                _previewMeshesComboBox.IsEnabled = false;
                var model = applyMesh(new SCBFile(new MemoryStream(entry.GetContent(true))));
                _previewTypeLabel.Content = "Type: Static Object Binary";
                _previewNameLabel.Content = "Name: " + namePath.Split('/').Last();

                if (PreSelect != null)
                {
                    foreach (var wadEntry in window.Wad.Entries)
                    {
                        if (wadEntry.XXHash == PreSelect.Item2)
                        {
                            applyMaterial(wadEntry, PreSelect.Item1, model);
                            break;
                        }
                    }

                    PreSelect = null;
                }
                else
                {
                    applyMaterial(model);
                }

                return;
            }

            if (namePath.ToLower().EndsWith(".sco"))
            {
                _previewMeshesComboBox.IsEnabled = false;
                var model = applyMesh(new SCOFile(new MemoryStream(entry.GetContent(true))));
                _previewTypeLabel.Content = "Type: Static Object Mesh";
                _previewNameLabel.Content = "Name: " + namePath.Split('/').Last();

                if (PreSelect != null)
                {
                    foreach (var wadEntry in window.Wad.Entries)
                    {
                        if (wadEntry.XXHash == PreSelect.Item2)
                        {
                            applyMaterial(wadEntry, PreSelect.Item1, model);
                            break;
                        }
                    }

                    PreSelect = null;
                }
                else
                {
                    applyMaterial(model);
                }

                return;
            }

            var result = generateInfoHolder(namePath);
            if (result == null) return;

            var skn = new SKNFile(new MemoryStream(result.sknPath.GetContent(true)));
            foreach (var subMesh in skn.Submeshes)
            {
                var model = applyMesh(subMesh);
                _previewNameLabel.Content = "Name: " + namePath.Split('/').Last();

                _previewTypeLabel.Content = "Type: Simple Skin Mesh";
                if (PreSelect != null)
                {
                    foreach (var wadEntry in window.Wad.Entries)
                    {
                        if (wadEntry.XXHash == PreSelect.Item2)
                        {
                            applyMaterial(wadEntry, PreSelect.Item1, model);
                            break;
                        }
                    }
                }
                else
                {
                    if (result.textures.Count > 0)
                        applyMaterial(result.textures.First().Value, result.textures.First().Key, model);
                    else
                        applyMaterial(model);
                }
            }

           PreSelect = null;
        }

        public GeometryModel3D applyMesh(SKNSubmesh subMesh)
        {
            var mesh = new MeshGeometry3D();
            foreach (var x in subMesh.Vertices)
            {
                mesh.Positions.Add(new Point3D(x.Position.X, x.Position.Y, x.Position.Z));
                mesh.Normals.Add(new Vector3D(x.Normal.X, x.Normal.Y, x.Normal.Z));
                mesh.TextureCoordinates.Add(new Point(x.UV.X, x.UV.Y));
            }

            foreach (var x in subMesh.GetNormalizedIndices())
                mesh.TriangleIndices.Add(x);

            var model = new GeometryModel3D();
            model.Geometry = mesh;
            _modelGroup.Children.Add(model);
            var boxHolder = getItem(model);
            modelMap.Add(boxHolder, true);
            _previewMeshesComboBox.Items.Add(boxHolder.item);
            boxHolder.block.Text = subMesh.Name;
            return model;
        }

        public GeometryModel3D applyMesh(SCBFile scb)
        {
            List<uint> indices = new List<uint>();
            List<Vector2> uv = new List<Vector2>();

            foreach (KeyValuePair<string, List<SCBFace>> material in scb.Materials)
            {
                foreach (SCBFace face in material.Value)
                {
                    indices.AddRange(face.Indices);
                    uv.AddRange(face.UVs);
                }
            }

            var mesh = new MeshGeometry3D();
            foreach (var x in scb.Vertices)
                mesh.Positions.Add(new Point3D(x.X, x.Y, x.Z));

            foreach (var x in indices)
                mesh.TriangleIndices.Add((int) x);

            foreach (var x in uv)
                mesh.TextureCoordinates.Add(new Point(x.X, x.Y));

            var model = new GeometryModel3D();
            model.Geometry = mesh;

            _modelGroup.Children.Add(model);

            return model;
        }

        public GeometryModel3D applyMesh(SCOFile scb)
        {
            List<uint> indices = new List<uint>();
            List<Vector2> uv = new List<Vector2>();

            foreach (KeyValuePair<string, List<SCOFace>> material in scb.Materials)
            {
                foreach (SCOFace face in material.Value)
                {
                    indices.AddRange(face.Indices);
                    uv.AddRange(face.UVs);
                }
            }

            var mesh = new MeshGeometry3D();
            foreach (var x in scb.Vertices)
                mesh.Positions.Add(new Point3D(x.X, x.Y, x.Z));

            foreach (var x in indices)
                mesh.TriangleIndices.Add((int) x);

            foreach (var x in uv)
                mesh.TextureCoordinates.Add(new Point(x.X, x.Y));

            var model = new GeometryModel3D();
            model.Geometry = mesh;
            _modelGroup.Children.Add(model);
            return model;
        }

        public void applyMaterial(WADEntry entry, string path, GeometryModel3D model)
        {
            var stream = new MemoryStream(entry.GetContent(true));
            ImageBrush colors_brush = new ImageBrush();
            var imageSource = new BitmapImage();
            imageSource.BeginInit();
            imageSource.StreamSource = stream;
            imageSource.EndInit();
            colors_brush.ViewboxUnits = BrushMappingMode.RelativeToBoundingBox;
            colors_brush.ViewportUnits = BrushMappingMode.Absolute;
            colors_brush.AlignmentY = AlignmentY.Top;
            colors_brush.AlignmentX = AlignmentX.Left;
            colors_brush.TileMode = TileMode.Tile;
            colors_brush.Stretch = Stretch.Fill;
            colors_brush.ImageSource = imageSource;


            DiffuseMaterial colors_material =
                new DiffuseMaterial(colors_brush);

            model.Material = colors_material;
            if (!materialMap.ContainsKey(model))
                materialMap.Add(model, path);
            else
                materialMap[model] = path;

            _previewTextureComboBox.SelectedItem = path;
        }

        public void applyMaterial(GeometryModel3D model)
        {
            DiffuseMaterial colors_material =
                new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(69, 62, 61)));

            model.Material = colors_material;

            if (!materialMap.ContainsKey(model))
                materialMap.Add(model, "default");
            else
                materialMap[model] = "default";

            _previewTextureComboBox.SelectedItem = "default";
        }

        private MeshesComboBoxHolder getItem(GeometryModel3D model)
        {
            var item = new ComboBoxItem();
            var stackPane = new StackPanel();
            stackPane.Orientation = Orientation.Horizontal;
            var checkBox = new CheckBox();
            checkBox.Width = 20;
            var block = new TextBlock();
            block.Width = 250;
            stackPane.Children.Add(checkBox);
            stackPane.Children.Add(block);
            item.Content = stackPane;
            return new MeshesComboBoxHolder(checkBox, block, item, model, _modelGroup);
        }

        private WADEntry getWadEntryByPath(string name)
        {
            foreach (var entry in MainWindow.StringDictionary)
            {
                if (entry.Value == name)
                {
                    foreach (var wadEntry in this.window.Wad.Entries)

                        if (wadEntry.XXHash == entry.Key)
                            return wadEntry;
                }
            }

            return null;
        }

        private string getFileNameFromPath(string inName)
        {
            var lastIndex = inName.Split('/').Last();
            var dotSplit = lastIndex.Split('.');

            var name = "";
            for (int x = 0; x < dotSplit.Count() - 1; x++)
            {
                name += ".";
                name += dotSplit[x];
            }

            return name.Substring(1).Replace("\r", "");
        }

        public void emit(WADEntry entry)
        {
            foreach (var nameEntry in MainWindow.StringDictionary)
            {
                if (nameEntry.Key == entry.XXHash)
                {
                    if (extensionMatch(nameEntry.Value))
                        handle(entry, nameEntry.Value);
                    else if (_previewExpander.IsExpanded) _previewExpander.IsExpanded = false;
                    break;
                }
            }
        }

        private bool extensionMatch(string r)
        {
            string path = r.ToLower();
            if (path.EndsWith(".skn") || path.EndsWith(".skl") || path.EndsWith(".dds") ||
                path.EndsWith(".scb") || path.EndsWith(".sco"))
                return true;
            return false;
        }

        private InfoHolder generateInfoHolder(string namePath)
        {
            var entryName = getFileNameFromPath(namePath);
            if (namePath.EndsWith(".skn"))
            {
                var sklPath = namePath.Replace(".skn", ".skl");
                var texturePath = "";
                if (!MainWindow.StringDictionary.ContainsValue(sklPath))
                {
                    return null;
                }

                var foundTexture = false;
                foreach (var e in MainWindow.StringDictionary)
                {
                    var rawVal = e.Value;
                    var value = e.Value.ToLower();
                    if (value.Contains("tx_cm"))
                    {
                        if (value.Contains((entryName.ToLower())) || value.Contains(entryName.ToLower() + "_body"))

                        {
                            if (value.EndsWith(".dds"))
                            {
                                if (namePath.ToLower().Contains("/base/"))
                                {
                                    if (value.Contains("_base") || value.Contains("body"))
                                    {
                                        foundTexture = true;
                                        texturePath = rawVal;
                                        break;
                                    }
                                }
                                else
                                {
                                    foundTexture = true;
                                    texturePath = rawVal;
                                    break;
                                }
                            }
                        }
                    }
                }

                var texturesMap = new Dictionary<string, WADEntry>();
                if (foundTexture) texturesMap.Add(texturePath, getWadEntryByPath(texturePath));

                return new InfoHolder(getWadEntryByPath(namePath), getWadEntryByPath(sklPath), texturesMap);
            }

            if (namePath.EndsWith(".skl"))
            {
                var sknPath = namePath.Replace(".skl", ".skn");
                var texturePath = "";
                if (!MainWindow.StringDictionary.ContainsValue(sknPath))
                {
                    return null;
                }

                var foundTexture = false;
                foreach (var e in MainWindow.StringDictionary)
                {
                    var rawVal = e.Value;
                    var value = e.Value.ToLower();
                    if (value.Contains("tx_cm"))
                    {
                        if (value.Contains((entryName.ToLower() + "_tx_cm")) ||
                            value.Contains(entryName.ToLower() + "_body_tx_cm"))

                        {
                            if (value.EndsWith(".dds"))
                            {
                                if (namePath.ToLower().Contains("/base/"))
                                {
                                    if (value.Contains("_base") || value.Contains("body"))
                                    {
                                        foundTexture = true;
                                        texturePath = rawVal;
                                        break;
                                    }
                                }
                                else
                                {
                                    foundTexture = true;
                                    texturePath = rawVal;
                                    break;
                                }
                            }
                        }
                    }
                }

                var texturesMap = new Dictionary<string, WADEntry>();
                if (foundTexture) texturesMap.Add(texturePath, getWadEntryByPath(texturePath));

                return new InfoHolder(getWadEntryByPath(sknPath), getWadEntryByPath(namePath), texturesMap);
            }


            return null;
        }


        private void meshesComboBoxSelecChange(object sender, SelectionChangedEventArgs e)
        {
            ((ComboBox) sender).SelectedItem = null;
        }

        private void textureComboBoxChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            string value = comboBox.SelectedItem as string;

            if (value == "default")
            {
                var list = new List<GeometryModel3D>();
                foreach (var models in materialMap)
                    list.Add(models.Key);

                foreach (var geometryModel3D in list)
                    applyMaterial(geometryModel3D);
            }

            foreach (var name in MainWindow.StringDictionary)
            {
                if (name.Value == value)
                {
                    foreach (var wadEntry in window.Wad.Entries)
                    {
                        if (wadEntry.XXHash == name.Key)
                        {
                            var list = new List<GeometryModel3D>();
                            foreach (var models in materialMap)
                                list.Add(models.Key);


                            foreach (var geometryModel3D in list)
                                applyMaterial(wadEntry, name.Value, geometryModel3D);
                        }
                    }

                    break;
                }
            }
        }
    }
}