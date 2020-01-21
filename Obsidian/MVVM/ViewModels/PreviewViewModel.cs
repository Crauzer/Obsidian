using CSharpImageLibrary;
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
        }
        public void Preview(SCBFile scb)
        {
            this.Viewport.LoadMesh(scb);

            this.PreviewType = PreviewType.Viewport;
        }
        public void Preview(SCOFile sco)
        {
            this.Viewport.LoadMesh(sco);

            this.PreviewType = PreviewType.Viewport;
        }
        public void Preview(ImageEngineImage image)
        {
            this.Image = image.GetWPFBitmap(512);

            this.PreviewType = PreviewType.Image;
        }

        public void Clear()
        {
            this.Viewport.Clear();
        }
    }
}
