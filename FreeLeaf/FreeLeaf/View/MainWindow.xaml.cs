using FreeLeaf.Model;
using System.Windows;
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
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as DeviceItem;

            if (item != null)
            {
                if (item.ID == null)
                {
                    var connect = new ConnectWindow() { Owner = this };
                    if (connect.ShowDialog() == true) item = connect.Item;
                    else item = null;
                }

                if (item != null)
                {
                    this.Hide();
                    var transfer = new TransferWindow(item);
                    transfer.Closing += (sender1, e1) => { this.Show(); };
                    transfer.Show();
                }
            }
        }
    }
}
