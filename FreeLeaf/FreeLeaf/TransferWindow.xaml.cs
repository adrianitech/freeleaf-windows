﻿using FreeLeaf.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FreeLeaf
{
    public partial class TransferWindow : Window
    {
        private TransferViewModel model;

        public TransferWindow(DeviceItem item)
        {
            InitializeComponent();
            model = (TransferViewModel)DataContext;
            model.SelectedItem = item;
        }

        private void LocalDriveList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = (e.OriginalSource as FrameworkElement).DataContext as DriveItem;
            if (item != null)
            {
                if (item.IsFolder)
                {
                    model.NavigateLocal(item.Path);
                }
            }
        }

        private void HyperlinkClearDestination_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ListLocal.SelectedItems)
            {
                (item as DriveItem).Destination = null;
            }
        }

        private void TreeView_Drop_1(object sender, DragEventArgs e)
        {
            var data = e.Data.GetData("myFormat") as System.Collections.IList;
            var item = (e.OriginalSource as FrameworkElement).DataContext as DriveItem1;
            foreach (var i in data)
            {
                (i as DriveItem).Destination = item.Path;
            }
        }

        private void Button_MouseLeftButtonDown_1(object sender, MouseButtonEventArgs e)
        {
            DataObject dragData = new DataObject("myFormat", ListLocal.SelectedItems);
            DragDrop.DoDragDrop(ListLocal, dragData, DragDropEffects.Link);
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            var item = e.OriginalSource as TreeViewItem;
            var i = item.Header as DriveItem1;
            model.Looo(i);
        }

        private void ButtonFolderUp_Click(object sender, RoutedEventArgs e)
        {
            model.NavigateLocalUp();
        }
    }
}
