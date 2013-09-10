using FreeLeaf.ViewModel;
using System.Windows;
using System.Windows.Controls;

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

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            model.SearchDevice((sender as TextBox).Text);
        }
    }
}
