using FreeLeaf.Model;
using System.Net;
using System.Windows;
using System.Windows.Controls;

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

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as DeviceItem;
                ShowTransferWindow(item);
                DeviceList.SelectedItem = null;
            }
        }

        private void ShowTransferWindow(DeviceItem item)
        {
            IPAddress temp;
            if (IPAddress.TryParse(item.Address, out temp))
            {
                this.Hide();
                var transfer = new TransferWindow(item);
                transfer.Closing += (sender1, e1) => { this.Show(); };
                transfer.Show();
            }
            else
            {
                MessageBox.Show("IP address is not valid!");
            }
        }
    }
}
