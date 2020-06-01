using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.StaticObject;
using Fantome.Libraries.League.IO.SimpleSkin;
using HelixToolkit.Wpf;
using System;
using System.Windows.Media.Imaging;
using Obsidian.Utilities;

namespace Obsidian.MVVM.ViewModels
{
    public class PreviewViewModel : PropertyNotifier
    {
        public PreviewType PreviewType
        {
            get => this._previewType;
            set
            {
                this._previewType = value;
                NotifyPropertyChanged();
            }
        }
        public string ContentType
        {
            get => this._contentType;
            set
            {
                this._contentType = value;
                NotifyPropertyChanged();
            }
        }
        public ViewportViewModel Viewport
        {
            get => this._viewport;
            set
            {
                this._viewport = value;
                NotifyPropertyChanged();
            }
        }
        public BitmapSource Image
        {
            get => this._image;
            set
            {
                this._image = value;
                NotifyPropertyChanged();
            }
        }

        private PreviewType _previewType;
        private string _contentType;
        private ViewportViewModel _viewport;
        private BitmapSource _image;

        public PreviewViewModel(HelixViewport3D viewport)
        {
            this._viewport = new ViewportViewModel(viewport);
        }

        public void Preview(SimpleSkin skn)
        {
            this.Viewport.LoadMesh(skn);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = Localization.Get("PreviewDescriptionSKN");
        }
        public void Preview(StaticObject staticObject)
        {
            this.Viewport.LoadMesh(staticObject);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = Localization.Get("PreviewDescriptionStaticObject");
        }
        public void Preview(ImageEngineImage image)
        {
            this.Image = image.GetWPFBitmap(512);

            this.PreviewType = PreviewType.Image;
            this.ContentType = Localization.Get("PreviewDescriptionDDS");
        }
        public void Preview(MapGeometry mgeo)
        {
            this.Viewport.LoadMap(mgeo);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = Localization.Get("PreviewDescriptionMapGeometry");
        }

        public void Clear()
        {
            this.Viewport.Clear();
            this._image = null;
            this.PreviewType = PreviewType.None;
            this.ContentType = string.Empty;
        }
    }

    public enum PreviewType
    {
        None,
        Viewport,
        Image
    }
}
