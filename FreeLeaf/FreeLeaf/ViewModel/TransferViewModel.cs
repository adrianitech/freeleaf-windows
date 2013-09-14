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
            NavigateLocalHome();
        }

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
            }

            LocalCurrentDir = "Your PC";
        }

        public void NavigateLocal(string path)
        {
            if (path.Equals("/"))
            {
                NavigateLocalHome();
                return;
            }

            LocalDrive.Clear();

            var ldinfo = new DirectoryInfo(path);
            LocalCurrentDir = ldinfo.Name;
            LocalDrive.Add(new DriveItem()
            {
                Path = ldinfo.Parent == null ? "/" : ldinfo.Parent.FullName,
                Name = "..",
                IsFolder = true,
                IsParent = true
            });

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var item = queue.FirstOrDefault((t) => t.Path.Equals(dir));
                if (item != null)
                {
                    LocalDrive.Add(item);
                }
                else
                {
                    var dinfo = new DirectoryInfo(dir);
                    if (!dinfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        LocalDrive.Add(new DriveItem()
                        {
                            Model = this,
                            Path = dir,
                            Name = dinfo.Name,
                            Size = "DIR",
                            Date = dinfo.CreationTime.ToString(),
                            IsFolder = true
                        });
                    }
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var item = queue.FirstOrDefault((t) => t.Path.Equals(file));
                if (item != null)
                {
                    LocalDrive.Add(item);
                }
                else
                {
                    var finfo = new FileInfo(file);
                    if (!finfo.Attributes.HasFlag(FileAttributes.Hidden))
                    {
                        LocalDrive.Add(new DriveItem()
                        {
                            Model = this,
                            Path = file,
                            Name = finfo.Name,
                            Size = SizeToString(finfo.Length),
                            Date = finfo.CreationTime.ToString(),
                            IsFolder = false,

                        });
                    }
                }
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

        private bool isParent;
        public bool IsParent
        {
            get { return isParent; }
            set
            {
                isParent = value;
                RaisePropertyChanged("IsParent");
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
