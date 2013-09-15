using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeLeaf.ViewModel
{
    public class TransferViewModel : ViewModelBase
    {
        private ObservableCollection<DriveItem> queue;
        public ObservableCollection<DriveItem> Queue
        {
            get { return queue; }
            set
            {
                queue = value;
                RaisePropertyChanged("Queue");
            }
        }

        private ObservableCollection<DriveItem1> localDrive1;
        public ObservableCollection<DriveItem1> RemoteL
        {
            get { return localDrive1; }
            set
            {
                localDrive1 = value;
                RaisePropertyChanged("RemoteL");
            }
        }

        private ObservableCollection<DriveItem> localDrive;
        public ObservableCollection<DriveItem> LocalDrive
        {
            get { return localDrive; }
            set
            {
                localDrive = value;
                RaisePropertyChanged("LocalDrive");
            }
        }

        private DeviceItem selectedItem;
        public DeviceItem SelectedItem
        {
            get { return selectedItem; }
            set
            {
                selectedItem = value;
                RaisePropertyChanged("SelectedItem");
            }
        }

        private string localCurrentDir;
        public string LocalCurrentDir
        {
            get { return localCurrentDir; }
            set
            {
                localCurrentDir = value;
                RaisePropertyChanged("LocalCurrentDir");
            }
        }

        public TransferViewModel()
        {
            queue = new ObservableCollection<DriveItem>();
            localDrive = new ObservableCollection<DriveItem>();
            localDrive1 = new ObservableCollection<DriveItem1>();
            NavigateLocalHome();
        }

        private string localPath, remotePath;

        public void NavigateLocalHome()
        {
            LocalDrive.Clear();
            var drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                LocalDrive.Add(new DriveItem()
                {
                    Path = drive,
                    Name = drive,
                    IsFolder = true
                });


                RemoteL.Add(new DriveItem1()
                {
                    Path = drive,
                    Name = drive,
                    Items = new ObservableCollection<DriveItem1>() { new DriveItem1() }
                });
            }

            localPath = "/";
        }

        public void Looo(DriveItem1 i)
        {
            i.Items.Clear();
            var dirs = Directory.GetDirectories(i.Path);
            foreach (var dir in dirs)
            {
                var dinfo = new DirectoryInfo(dir);
                if (!dinfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    i.Items.Add(new DriveItem1()
                    {
                        Path = dir,
                        Name = dinfo.Name,
                        Items = new ObservableCollection<DriveItem1>() { new DriveItem1() }
                    });
                }
            }
        }

        public void NavigateLocal(string path)
        {
            localPath = path;
            LocalDrive.Clear();

            var di = new DirectoryInfo(path);
            //if (di.Attributes.HasFlag(FileAttributes.Hidden)) return;

            var dirs = di.EnumerateDirectories();
            foreach (var dir in dirs)
            {
                LocalDrive.Add(new DriveItem()
                {
                    Model = this,
                    Path = dir.FullName,
                    Name = dir.Name,
                    Extension = "FOLDER",
                    Date = dir.LastWriteTime.ToString(),
                    IsFolder = true
                });
            }

            var files = di.EnumerateFiles();
            foreach (var file in files)
            {
                LocalDrive.Add(new DriveItem()
                {
                    Model = this,
                    Path = file.FullName,
                    Name = file.Name,
                    Size = SizeToString(file.Length),
                    Extension = file.Extension.Length >= 1 ? file.Extension.Substring(1).ToUpper() : "",
                    Date = file.LastWriteTime.ToString(),
                    IsFolder = false
                });
            }
        }

        public string SizeToString(long size)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = size;
            int order = 0;
            while (len >= 1024 && order + 1 < sizes.Length)
            {
                order++;
                len = len / (double)1024;
            }

            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        public void NavigateLocalUp()
        {
            var info = Directory.GetParent(localPath);
            if (info == null)
            {
                NavigateLocalHome();
            }
            else
            {
                NavigateLocal(info.FullName);
            }
        }
    }

    public class DriveItem1 : ObservableObject
    {
        public DriveItem1()
        {
            items = new ObservableCollection<DriveItem1>();
        }

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private ObservableCollection<DriveItem1> items;
        public ObservableCollection<DriveItem1> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }
    }

    public class DriveItem : ObservableObject
    {
        public TransferViewModel Model;

        private string path;
        public string Path
        {
            get { return path; }
            set
            {
                path = value;
                RaisePropertyChanged("Path");
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged("Name");
            }
        }

        private string date;
        public string Date
        {
            get { return date; }
            set
            {
                date = value;
                RaisePropertyChanged("Date");
            }
        }

        private string size;
        public string Size
        {
            get { return size; }
            set
            {
                size = value;
                RaisePropertyChanged("Size");
            }
        }

        private string extension;
        public string Extension
        {
            get { return extension; }
            set
            {
                extension = value;
                RaisePropertyChanged("Extension");
            }
        }

        private string destination;
        public string Destination
        {
            get { return destination; }
            set
            {
                if (!this.IsFolder)
                {
                    destination = value;
                    IsChecked = !string.IsNullOrEmpty(destination);
                    RaisePropertyChanged("Destination");
                }
            }
        }

        private bool isFolder;
        public bool IsFolder
        {
            get { return isFolder; }
            set
            {
                isFolder = value;
                RaisePropertyChanged("IsFolder");
            }
        }

        public bool IsChecked
        {
            set
            {
                if (value)
                {
                    if (!Model.Queue.Contains(this))
                    {
                        Model.Queue.Add(this);
                    }
                }
                else
                {
                    Model.Queue.Remove(this);
                }
            }
        }
    }
}
