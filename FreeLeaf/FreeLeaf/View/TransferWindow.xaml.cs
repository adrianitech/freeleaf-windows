using FreeLeaf.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
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

        private void ButtonLocalFolderUp_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateLocalUp();
        }

        private void ButtonLocalNewFolder_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void ButtonLocalRename_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ListLocal.SelectedItems)
            {
                
            }
        }

        private void ButtonLocalDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Doy y dsfjdsklf",
                " das das dhasjk hd jkahjk dsa", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                FileItem[] items = new FileItem[ListLocal.SelectedItems.Count];
                ListLocal.SelectedItems.CopyTo(items, 0);

                foreach(var item in items)
                {
                    model.LocalDrive.Remove(item);
                }
            }
        }

        private void ButtonRemoteFolderUp_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateRemoteUp();
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

        private void RemoteList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
        }
    }
}
