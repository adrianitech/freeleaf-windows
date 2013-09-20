using FreeLeaf.Model;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;

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

            model.Queue.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                {
                    var sb = this.FindResource("PulseAnimationStory") as Storyboard;
                    sb.Begin();
                }
            };
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
            foreach (FileItem item in ListLocal.SelectedItems)
            {
                try
                {
                    if (item.IsFolder)
                        FileSystem.DeleteDirectory(item.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                    else
                        FileSystem.DeleteFile(item.Path, UIOption.AllDialogs, RecycleOption.SendToRecycleBin);
                }
                catch
                {
                }
            }

            model.NavigateLocal(model.LocalPath);
        }
        
        private async void ButtonRemoteDelete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Do you really want to delete the selected items?",
                "Delete selected items", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                foreach (FileItem item in ListRemote.SelectedItems)
                {
                    await model.SendCommand("delete:" + item.Path);
                }

                model.NavigateRemote(model.RemotePath);
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
                    model.NavigateRemote(item.Path);
                }
            }
        }

        private void ButtonQueue_Click(object sender, RoutedEventArgs e)
        {
            var queue = new QueueWindow() { Owner = this };
            queue.ShowDialog();
        }

        private void ButtonLocalRefresh_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateLocal(model.LocalPath);
        }

        private void ButtonRemoteRefresh_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateRemote(model.RemotePath);
        }
    }
}
