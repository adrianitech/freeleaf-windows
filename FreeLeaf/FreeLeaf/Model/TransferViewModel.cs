using GalaSoft.MvvmLight;
using System.Collections.ObjectModel;
using System.IO;

namespace FreeLeaf.Model
{
    public class TransferViewModel : ViewModelBase
    {
        private string localPath, remotePath;

        private ObservableCollection<LocalFileItem> queue;
        public ObservableCollection<LocalFileItem> Queue
        {
            get { return queue; }
            set
            {
                queue = value;
                RaisePropertyChanged("Queue");
            }
        }

        private ObservableCollection<RemoteFileItem> removeFiles;
        public ObservableCollection<RemoteFileItem> RemoteL
        {
            get { return removeFiles; }
            set
            {
                removeFiles = value;
                RaisePropertyChanged("RemoteL");
            }
        }

        private ObservableCollection<LocalFileItem> localFiles;
        public ObservableCollection<LocalFileItem> LocalDrive
        {
            get { return localFiles; }
            set
            {
                localFiles = value;
                RaisePropertyChanged("LocalDrive");
            }
        }

        public TransferViewModel()
        {
            if (IsInDesignMode) return;

            queue = new ObservableCollection<LocalFileItem>();
            localFiles = new ObservableCollection<LocalFileItem>();
            removeFiles = new ObservableCollection<RemoteFileItem>();

            NavigateLocalHome();
        }

        public void NavigateLocalHome()
        {
            localPath = "/";
            remotePath = "/";
            LocalDrive.Clear();

            var drives = Directory.GetLogicalDrives();
            foreach (var drive in drives)
            {
                LocalDrive.Add(new LocalFileItem()
                {
                    Path = drive,
                    Name = drive,
                    Extension = "DRIVE",
                    IsFolder = true
                });

                RemoteL.Add(new RemoteFileItem()
                {
                    Path = drive,
                    Name = drive,
                    Items = new ObservableCollection<RemoteFileItem>() { new RemoteFileItem() }
                });
            }
        }

        public void NavigateLocal(string path)
        {
            localPath = path;
            LocalDrive.Clear();

            var di = new DirectoryInfo(path);

            var dirs = di.EnumerateDirectories();
            foreach (var dir in dirs)
            {
                if(dir.Attributes.HasFlag(FileAttributes.Hidden |
                    FileAttributes.System)) continue;

                LocalDrive.Add(new LocalFileItem()
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
                if (file.Attributes.HasFlag(FileAttributes.Hidden |
                       FileAttributes.System)) continue;

                LocalDrive.Add(new LocalFileItem()
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

        public void NavigateLocalUp()
        {
            var info = Directory.GetParent(localPath);
            if (info == null) NavigateLocalHome();
            else NavigateLocal(info.FullName);
        }

        public void PopulateRemoteFolder(RemoteFileItem item)
        {
            remotePath = item.Path;
            item.Items.Clear();

            var dirs = Directory.GetDirectories(item.Path);
            foreach (var dir in dirs)
            {
                var info = new DirectoryInfo(dir);
                item.Items.Add(new RemoteFileItem()
                {
                    Path = dir,
                    Name = info.Name,
                    Items = new ObservableCollection<RemoteFileItem>() { new RemoteFileItem() }
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
    }

    public class RemoteFileItem : ObservableObject
    {
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

        private ObservableCollection<RemoteFileItem> items;
        public ObservableCollection<RemoteFileItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged("Items");
            }
        }
    }

    public class LocalFileItem : ObservableObject
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
                    IsInQueue = !string.IsNullOrEmpty(destination);
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

        public bool IsInQueue
        {
            get { return Model.Queue.Contains(this); }
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
