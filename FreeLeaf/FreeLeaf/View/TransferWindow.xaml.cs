using FreeLeaf.Model;
using System.Windows;
using System.Windows.Input;

namespace FreeLeaf.View
{
    public partial class TransferWindow : Window
    {
        private TransferViewModel model;

        public TransferWindow(DeviceItem item)
        {
            InitializeComponent();
            model = (TransferViewModel)this.DataContext;
            model.SetDevice(item);
        }

        private void ButtonFolderUp_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateLocalUp();
        }

        private void ButtonFolderUp_Click1(object sender, RoutedEventArgs e)
        {
            model.NavigateRemoteUp();
        }

        private void ButtonSetDestination_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dragData = new DataObject("LinkItems", ListLocal.SelectedItems);
            DragDrop.DoDragDrop(ListLocal, dragData, DragDropEffects.Link);
        }

        private void ButtonClearDestination_Click(object sender, RoutedEventArgs e)
        {
            foreach (FileItem item in ListLocal.SelectedItems)
            {
                item.Destination = null;
            }
        }

        private void LocalList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as FileItem;
            if (item != null)
            {
                if (item.IsFolder)
                {
                    model.NavigateLocal(item.Path);
                }
            }
        }

        private void LocalList_MouseDoubleClick1(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as FileItem;
            if (item != null)
            {
                if (item.IsFolder)
                {
                    model.PopulateRemoteFolder(item.Path);
                }
            }
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            var queue = new QueueWindow() { Owner = this };
            queue.ShowDialog();
            /*foreach (var item in model.Queue)
            {
                if (!item.IsRemote) model.SendFile(item);
                else model.ReceiveFile(item);
            }*/
        }
    }
}
