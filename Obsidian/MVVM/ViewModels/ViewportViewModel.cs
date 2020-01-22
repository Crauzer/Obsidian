using Fantome.Libraries.League.Helpers.Structures;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SCB;
using Fantome.Libraries.League.IO.SCO;
using Fantome.Libraries.League.IO.SimpleSkin;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Obsidian.MVVM.ViewModels
{
    public class ViewportViewModel : PropertyNotifier
    {
        private static readonly List<Brush> BRUSHES = new List<Brush>()
        {
            Brushes.Gray,
            Brushes.DeepSkyBlue,
            Brushes.Red,
            Brushes.Yellow,
            Brushes.Teal,
            Brushes.Aqua,
            Brushes.Beige,
            Brushes.Brown,
            Brushes.Purple,
            Brushes.Pink,
            Brushes.Green,
            Brushes.Green,
            Brushes.Gold,
            Brushes.LightPink,
            Brushes.Coral,
            Brushes.Fuchsia,
            Brushes.Lime
        };
        private static readonly List<DirectionalLight> LIGHTS = new List<DirectionalLight>()
        {
            new DirectionalLight()
            {
                Color = Colors.White,
                Direction = new Vector3D(-1, -1, -1)
            },
            new DirectionalLight()
            {
                Color = Colors.White,
                Direction = new Vector3D(1, 1, 1)
            }
        };

        public PerspectiveCamera Camera { get; }
        public ObservableCollection<ModelVisual3D> Content
        {
            get => this._content;
            private set
            {
                this._content = value;
                NotifyPropertyChanged();
            }
        }

        private ObservableCollection<ModelVisual3D> _content = new ObservableCollection<ModelVisual3D>();
        private HelixViewport3D _viewport;

        public ViewportViewModel(HelixViewport3D viewport)
        {
            this._viewport = viewport;
        }

        public void LoadMesh(SKNFile skn)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            int submeshIndex = 0;
            foreach (SKNSubmesh submesh in skn.Submeshes)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(submeshIndex < BRUSHES.Count ? BRUSHES[submeshIndex] : Brushes.Gray);

                Int32Collection indices = new Int32Collection(submesh.Indices.Count);
                Point3DCollection vertices = new Point3DCollection(submesh.Vertices.Count);
                Vector3DCollection normals = new Vector3DCollection(submesh.Vertices.Count);
                foreach (ushort index in submesh.GetNormalizedIndices())
                {
                    indices.Add(index);
                }
                foreach (SKNVertex vertex in submesh.Vertices)
                {
                    vertices.Add(new Point3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                    normals.Add(new Vector3D(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
                }

                geometry.Positions = vertices;
                geometry.TriangleIndices = indices;
                geometry.Normals = normals;

                geometryModels.Add(new GeometryModel3D(geometry, material));

                submeshIndex++;
            }

            SetGeometryModels(geometryModels);
            SetCamera(skn.CalculateCentralPoint());
        }
        public void LoadMesh(SCBFile scb)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            Point3DCollection vertices = new Point3DCollection(scb.Vertices.Count);
            foreach (Vector3 vertex in scb.Vertices)
            {
                vertices.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
            }


            int submeshIndex = 0;
            foreach (KeyValuePair<string, List<SCBFace>> submesh in scb.Materials)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(submeshIndex < BRUSHES.Count ? BRUSHES[submeshIndex] : Brushes.Gray);

                Int32Collection indices = new Int32Collection(submesh.Value.Count * 3);
                foreach (SCBFace face in submesh.Value)
                {
                    indices.Add((int)face.Indices[0]);
                    indices.Add((int)face.Indices[1]);
                    indices.Add((int)face.Indices[2]);
                }

                geometry.Positions = vertices;
                geometry.TriangleIndices = indices;

                geometryModels.Add(new GeometryModel3D(geometry, material));

                submeshIndex++;
            }

            SetGeometryModels(geometryModels);
            SetCamera(scb.CalculateCentralPoint());
        }
        public void LoadMesh(SCOFile sco)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            Point3DCollection vertices = new Point3DCollection(sco.Vertices.Count);
            foreach (Vector3 vertex in sco.Vertices)
            {
                vertices.Add(new Point3D(vertex.X, vertex.Y, vertex.Z));
            }


            int submeshIndex = 0;
            foreach (KeyValuePair<string, List<SCOFace>> submesh in sco.Materials)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(submeshIndex < BRUSHES.Count ? BRUSHES[submeshIndex] : Brushes.Gray);

                Int32Collection indices = new Int32Collection(submesh.Value.Count * 3);
                foreach (SCOFace face in submesh.Value)
                {
                    indices.Add((int)face.Indices[0]);
                    indices.Add((int)face.Indices[1]);
                    indices.Add((int)face.Indices[2]);
                }

                geometry.Positions = vertices;
                geometry.TriangleIndices = indices;

                geometryModels.Add(new GeometryModel3D(geometry, material));

                submeshIndex++;
            }

            SetGeometryModels(geometryModels);
            SetCamera(sco.CalculateCentralPoint());
        }
        public void LoadMap(MGEOFile mgeo)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            foreach (MGEOObject mgeoObject in mgeo.Objects)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(Brushes.Gray);
                Point3DCollection vertices = new Point3DCollection();
                Int32Collection indices = new Int32Collection();

                foreach(ushort index in mgeoObject.Indices)
                {
                    indices.Add(index);
                }
                foreach(MGEOVertex vertex in mgeoObject.Vertices)
                {
                    vertices.Add(new Point3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                }

                geometry.TriangleIndices = indices;
                geometry.Positions = vertices;

                geometryModels.Add(new GeometryModel3D(geometry, material));
            }

            SetGeometryModels(geometryModels);
            SetCamera(new Vector3(0, 0, 0));
        }

        private void SetGeometryModels(List<GeometryModel3D> geometryModels)
        {
            foreach (DirectionalLight light in LIGHTS)
            {
                this.Content.Add(new ModelVisual3D() { Content = light });
            }
            foreach (GeometryModel3D geometryModel in geometryModels)
            {
                this.Content.Add(new ModelVisual3D()
                {
                    Content = geometryModel
                });
            }
        }
        private void SetCamera(Vector3 point)
        {
            this._viewport.Camera.Position = new Point3D(point.X, point.Y, point.Z);
            this._viewport.Camera.FitView(this._viewport.Viewport, new Vector3D(0, 0, -1), new Vector3D(0, 1, 0), 500);
        }

        public void Clear()
        {
            this.Content.Clear();
        }
    }
}
