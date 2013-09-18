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
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as DeviceItem;

            if (item != null)
            {
                if (item.ID == null)
                {
                    var li = sss.ContainerFromElement(element) as UIElement;

                    d.Width = sss.ActualWidth;
                    d.PlacementTarget = li;
                    d.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                    d.IsOpen = true;
                }
                else
                {
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

        private void TextBox_PreviewTextInput_1(object sender, TextCompositionEventArgs e)
        {
            int result;
            if (!(int.TryParse(e.Text, out result) || e.Text == "."))
            {
                e.Handled = true;
            }
        }
    }
}
