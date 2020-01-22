using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SCB;
using Fantome.Libraries.League.IO.SCO;
using Fantome.Libraries.League.IO.SimpleSkin;
using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Imaging;

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

        public void Preview(SKNFile skn)
        {
            this.Viewport.LoadMesh(skn);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = "Simple Skin Model";
        }
        public void Preview(SCBFile scb)
        {
            this.Viewport.LoadMesh(scb);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = "Static Object";
        }
        public void Preview(SCOFile sco)
        {
            this.Viewport.LoadMesh(sco);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = "Static Object";
        }
        public void Preview(ImageEngineImage image)
        {
            try
            {

            }
            catch(Exception excp)
            {

            }
            this.Image = image.GetWPFBitmap(512);

            this.PreviewType = PreviewType.Image;
            this.ContentType = "Direct Draw Surface";
        }
        public void Preview(MGEOFile mgeo)
        {
            this.Viewport.LoadMap(mgeo);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = "Map Geometry";
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
