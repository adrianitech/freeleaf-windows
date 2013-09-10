using FreeLeaf.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

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

        private void DeviceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                model.SelectedItem = (DeviceItem)e.AddedItems[0];
                model.SelectedItem.LastUpdated = 1;
                model.SelectedItem.LastUpdated = 0;
                //UpdateActionIcon((DeviceItem)e.AddedItems[0]);
                //ActionGrid.IsEnabled = true;
               // ColumnDeviceInfo.Width = new GridLength(400);
            }
            else
            {
               // ColumnDeviceInfo.Width = new GridLength(0);
                //UpdateActionIcon(null);
                //ActionGrid.IsEnabled = false;
            }
        }

        private void UpdateActionIcon(DeviceItem item)
        {
            if (item != null && item.IsPinned)
            {
                ActionImage.Source = new BitmapImage(new Uri("Assets/delete-26.png", UriKind.Relative));
            }
            else
            {
                ActionImage.Source = new BitmapImage(new Uri("Assets/star-26.png", UriKind.Relative));
            }
        }

        private void ActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                var item = (DeviceItem)DeviceList.SelectedItem;
                model.EditPinned(item, !item.IsPinned);

                if (DeviceList.SelectedItem != null)
                {
                    UpdateActionIcon((DeviceItem)DeviceList.SelectedItem);
                }
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (DeviceList.SelectedItem != null)
            {
                model.SelectedItem = (DeviceItem)DeviceList.SelectedItem;
                ShowManagerButton.IsChecked = false;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext as DeviceItem;
            model.EditPinned(item, !item.IsPinned);
        }
    }
}
