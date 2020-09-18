using CSharpImageLibrary;
using Fantome.Libraries.League.IO.MapGeometry;
using Fantome.Libraries.League.IO.SimpleSkinFile;
using Fantome.Libraries.League.IO.StaticObjectFile;
using HelixToolkit.Wpf;
using ICSharpCode.AvalonEdit.Document;
using Newtonsoft.Json.Linq;
using Obsidian.Utilities;
using System.IO;
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
        public TextDocument Document
        {
            get => this._document;
            set
            {
                this._document = value;
                NotifyPropertyChanged();
            }
        }

        private PreviewType _previewType;
        private string _contentType;
        private ViewportViewModel _viewport;
        private BitmapSource _image;
        private TextDocument _document;

        public PreviewViewModel()
        {
            this._viewport = new ViewportViewModel();
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
        public void Preview(ImageEngineImage ddsImage)
        {
            this.Image = ddsImage.GetWPFBitmap(512);

            this.PreviewType = PreviewType.Image;
            this.ContentType = Localization.Get("PreviewDescriptionDDS");
        }
        public void Preview(MapGeometry mgeo)
        {
            this.Viewport.LoadMap(mgeo);

            this.PreviewType = PreviewType.Viewport;
            this.ContentType = Localization.Get("PreviewDescriptionMapGeometry");
        }
        public void Preview(BitmapImage bitmap)
        {
            this.Image = bitmap;

            this.PreviewType = PreviewType.Image;
            this.ContentType = "";
        }
        public void PreviewText(Stream stream, string extension)
        {
            using (StreamReader reader = new StreamReader(stream))
            {
                string text = reader.ReadToEnd();

                if (extension == ".json")
                {
                    text = JToken.Parse(text).ToString(Newtonsoft.Json.Formatting.Indented);
                }

                this.Document = new TextDocument(text);
            }

            this.PreviewType = PreviewType.Text;
            this.ContentType = "";
        }

        public void Clear()
        {
            this.Viewport.Clear();
            this._image = null;
            this.PreviewType = PreviewType.None;
            this.ContentType = string.Empty;
        }

        public void SetViewport(HelixViewport3D viewport)
        {
            this.Viewport.SetViewport(viewport);
        }
    }

    public enum PreviewType
    {
        None,
        Viewport,
        Image,
        Text
    }
}
