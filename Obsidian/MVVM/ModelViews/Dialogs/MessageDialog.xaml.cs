using System.Windows.Controls;

namespace Obsidian.MVVM.ModelViews.Dialogs
{
    /// <summary>
    /// Interaction logic for MessageDialog.xaml
    /// </summary>
    public partial class MessageDialog : UserControl
    {
        public string Message { get; }

        public MessageDialog(string message)
        {
            this.Message = message;

            InitializeComponent();

            this.DataContext = this;
        }
    }
}
