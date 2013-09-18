using FreeLeaf.Model;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FreeLeaf.View
{
    public partial class MainWindow : Window
    {
        private MainViewModel model;

        public MainWindow()
        {
            InitializeComponent();
            model = (MainViewModel)this.DataContext;

            DeviceList.Loaded += (sender, e) =>
            {
                var element = DeviceList.ItemContainerGenerator.ContainerFromIndex(DeviceList.Items.Count - 1) as ListViewItem;
                var TextBoxConnect = element.Template.FindName("PART_Text", element) as TextBox;
                TextBoxConnect.KeyUp += TextBoxConnect_KeyUp;
            };
        }

        void TextBoxConnect_KeyUp(object sender, KeyEventArgs e)
        {
            var text = sender as TextBox;

            if (e.Key == Key.Enter)
            {
                ShowTransferWindow(new DeviceItem() { Address = text.Text });
            }
            else if (e.Key == Key.Escape)
            {
                text.Clear();
            }
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as DeviceItem;

            if (item != null && item.ID != null)
            {
                ShowTransferWindow(item);
            }
        }

        private void ShowTransferWindow(DeviceItem item)
        {
            this.Hide();
            var transfer = new TransferWindow(item);
            transfer.Closing += (sender1, e1) => { this.Show(); };
            transfer.Show();
        }
    }
}
