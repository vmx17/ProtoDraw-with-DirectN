using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DirectNXAML.Views
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            var item = args.InvokedItemContainer as NavigationViewItem;
            if (args != null)
            {
                if ((string)(item.Tag) == "DirectNPage")
                {
                    ContentFrame.Navigate(typeof(DirectNPage), null);
                }
                else
                {
                    ContentFrame.Navigate(typeof(OtherPage), null);
                }
            }
        }
    }
}
