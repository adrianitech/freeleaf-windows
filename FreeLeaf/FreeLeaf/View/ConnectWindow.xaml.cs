using FreeLeaf.Model;
using System.Windows;

namespace FreeLeaf.View
{
    public partial class ConnectWindow : Window
    {
        public ConnectWindow()
        {
            InitializeComponent();
        }

        public DeviceItem Item
        {
            get
            {
                var ip = TextIPAddress.Text;
                if (string.IsNullOrWhiteSpace(ip)) return null;
                return new DeviceItem() { Address = ip };
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
