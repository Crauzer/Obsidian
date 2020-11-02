using LeagueToolkit.Helpers.Structures;
using LeagueToolkit.IO.MapGeometry;
using HelixToolkit.Wpf;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System;
using LeagueToolkit.IO.StaticObjectFile;
using LeagueToolkit.IO.SimpleSkinFile;
using System.Numerics;

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

        public ViewportViewModel()
        {

        }

        public void LoadMesh(SimpleSkin skn)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            int submeshIndex = 0;
            foreach (SimpleSkinSubmesh submesh in skn.Submeshes)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(submeshIndex < BRUSHES.Count ? BRUSHES[submeshIndex] : Brushes.Gray);

                Int32Collection indices = new Int32Collection(submesh.Indices.Count);
                Point3DCollection vertices = new Point3DCollection(submesh.Vertices.Count);
                Vector3DCollection normals = new Vector3DCollection(submesh.Vertices.Count);
                foreach (ushort index in submesh.Indices)
                {
                    indices.Add(index);
                }
                foreach (SimpleSkinVertex vertex in submesh.Vertices)
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
            SetCamera(skn.GetBoundingBox().GetCentralPoint());
        }
        public void LoadMesh(StaticObject staticObject)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            int submeshIndex = 0;
            foreach (StaticObjectSubmesh submesh in staticObject.Submeshes)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(submeshIndex < BRUSHES.Count ? BRUSHES[submeshIndex] : Brushes.Gray);

                Int32Collection indices = new Int32Collection(submesh.Indices.Count);
                Point3DCollection vertices = new Point3DCollection(submesh.Vertices.Count);
                Vector3DCollection normals = new Vector3DCollection(submesh.Vertices.Count);
                foreach (ushort index in submesh.Indices)
                {
                    indices.Add(index);
                }
                foreach (StaticObjectVertex vertex in submesh.Vertices)
                {
                    vertices.Add(new Point3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));
                }

                geometry.Positions = vertices;
                geometry.TriangleIndices = indices;
                geometry.Normals = normals;

                geometryModels.Add(new GeometryModel3D(geometry, material));

                submeshIndex++;
            }

            SetGeometryModels(geometryModels);
            SetCamera(staticObject.GetBoundingBox().GetCentralPoint());
        }
        public void LoadMap(MapGeometry mgeo)
        {
            this.Content.Clear();

            List<GeometryModel3D> geometryModels = new List<GeometryModel3D>();

            foreach (MapGeometryModel mgeoObject in mgeo.Models)
            {
                MeshGeometry3D geometry = new MeshGeometry3D();
                DiffuseMaterial material = new DiffuseMaterial(Brushes.Gray);
                Point3DCollection vertices = new Point3DCollection();
                Int32Collection indices = new Int32Collection();

                foreach(ushort index in mgeoObject.Indices)
                {
                    indices.Add(index);
                }
                foreach(MapGeometryVertex vertex in mgeoObject.Vertices)
                {
                    vertices.Add(new Point3D(vertex.Position.Value.X, vertex.Position.Value.Y, vertex.Position.Value.Z));
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

        public void SetViewport(HelixViewport3D viewport)
        {
            this._viewport = viewport;
        }

        public void Clear()
        {
            this.Content.Clear();
        }
    }
}
