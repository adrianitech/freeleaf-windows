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
                Name = "Parent directory",
                IsFolder = true,
                IsParent = true
            });

            var dirs = Directory.GetDirectories(path);
            foreach (var dir in dirs)
            {
                var dinfo = new DirectoryInfo(dir);
                if (!dinfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    LocalDrive.Add(new DriveItem()
                    {
                        Path = dir,
                        Name = dinfo.Name,
                        IsFolder = true
                    });
                }
            }

            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                var finfo = new FileInfo(file);
                if (!finfo.Attributes.HasFlag(FileAttributes.Hidden))
                {
                    LocalDrive.Add(new DriveItem()
                    {
                        Path = file,
                        Name = finfo.Name,
                        Size = SizeToString(finfo.Length),
                        IsFolder = false
                    });
                }
            }
        }

        private string SizeToString(long size)
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
    }
}
