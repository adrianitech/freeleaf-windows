using FreeLeaf.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FreeLeaf
{
    public partial class MainWindow : Window
    {
        private MainViewModel model;

        public MainWindow()
        {
            InitializeComponent();
            model = (MainViewModel)DataContext;
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DeviceItem;
            model.EditPinned(item, !item.IsPinned);
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            model.SearchDevice((sender as TextBox).Text);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            TransferWindow transfer = new TransferWindow(model.SelectedItem);
            transfer.Closing += (sender1, e1) => { this.Show(); };
            transfer.Show();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ListView_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var item = element.DataContext as DeviceItem;
            if (item != null)
            {
                if (item.ID == null)
                {

                }
                else
                {
                    this.Hide();
                    var transfer = new TransferWindow(model.SelectedItem);
                    transfer.Closing += (sender1, e1) => { this.Show(); };
                    transfer.Show();
                }
            }
        }
    }
}
