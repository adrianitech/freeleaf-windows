using FreeLeaf.View;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace FreeLeaf.Model
{
    public class TransferViewModel : ViewModelBase, IDropTarget
    {
        private DeviceItem device;
        public bool forceStop;

        private bool isBusy;
        public bool IsBusy
        {
            get { return isBusy; }
            set
            {
                isBusy = value;
                RaisePropertyChanged("IsBusy");
            }
        }

        private string localPath;
        public string LocalPath
        {
            get { return localPath; }
            set
            {
                localPath = value;
                RaisePropertyChanged("LocalPath");
            }
        }

        private string remotePath;
        public string RemotePath
        {
            get { return remotePath; }
            set
            {
                remotePath = value;
                RaisePropertyChanged("RemotePath");
            }
        }

        private ObservableCollection<FileItem> queue;
        public ObservableCollection<FileItem> Queue
        {
            get { return queue; }
            set
            {
                queue = value;
                RaisePropertyChanged("Queue");
            }
        }

        private ObservableCollection<FileItem> remoteFiles;
        public ObservableCollection<FileItem> RemoteFiles
        {
            get { return remoteFiles; }
            set
            {
                remoteFiles = value;
                RaisePropertyChanged("RemoteL");
            }
        }

        private ObservableCollection<FileItem> localFiles;
        public ObservableCollection<FileItem> LocalDrive
        {
            get { return localFiles; }
            set
            {
                localFiles = value;
                RaisePropertyChanged("LocalDrive");
            }
        }

        private string status;
        public string Status
        {
            get { return status; }
            set
            {
                status = value;
                RaisePropertyChanged("Status");
            }
        }

        public TransferViewModel()
        {
            if (IsInDesignMode) return;

            queue = new ObservableCollection<FileItem>();
            localFiles = new ObservableCollection<FileItem>();
            remoteFiles = new ObservableCollection<FileItem>();

            NavigateLocalHome();
        }

        private void SendMessage(string message)
        {
            TcpClient client = new TcpClient();

            try { client.Connect(new IPEndPoint(IPAddress.Parse(device.Address), 8080)); }
            catch { return; }

            NetworkStream ns = client.GetStream();

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            ns.Write(buffer, 0, buffer.Length);

            client.Close();
        }

        private string SendMessageWithReceive(string message)
        {
            TcpClient client = new TcpClient();

            try { client.Connect(new IPEndPoint(IPAddress.Parse(device.Address), 8080)); }
            catch { return null; }

            NetworkStream ns = client.GetStream();

            byte[] buffer = Encoding.UTF8.GetBytes(message);
            ns.Write(buffer, 0, buffer.Length);

            buffer = new byte[4096];
            MemoryStream ms = new MemoryStream();

            int bytesRead = 0;

            while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);
            }

            return Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
        }

        public void NavigateLocalHome()
        {
            LocalPath = "/";
            LocalDrive.Clear();

            var drives = DriveInfo.GetDrives();
            foreach (var drive in drives)
            {
                if (!drive.IsReady) continue;

                LocalDrive.Add(new FileItem()
                {
                    Path = drive.Name,
                    Name = string.Format("{0} ({1})",
                        string.IsNullOrEmpty(drive.VolumeLabel) ?
                        drive.DriveType.ToString() :
                        drive.VolumeLabel,
                        drive.Name.Substring(0, drive.Name.Length - 1)),
                    Size = Helper.SizeToString(drive.TotalSize),
                    Extension = drive.DriveFormat,
                    IsFolder = true
                });
            }
        }

        public Task SendFile(FileItem item)
        {
            return Task.Run(() =>
            {
                var info = new FileInfo(item.Path);
                var size = info.Length;

                SendMessage(string.Format("send:{0}:{1}:{2}", item.Name, item.Destination, size));

                TcpClient client = new TcpClient();
                try { client.Connect(new IPEndPoint(IPAddress.Parse(device.Address), 8080)); }
                catch { return; }

                NetworkStream ns = client.GetStream();

                int bytesRead = 0;
                long bytesTotal = 0, lastRead = 0, lastLeft = 0;
                byte[] buffer = new byte[8192];

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var stream = info.OpenRead();
                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (forceStop) break;

                    ns.Write(buffer, 0, bytesRead);
                    bytesTotal += bytesRead;
                    lastRead += bytesRead;

                    long diff = stopwatch.ElapsedMilliseconds;
                    if (diff >= 1000)
                    {
                        item.ProgressSize = Helper.SizeToString(bytesTotal);
                        item.Progress = 100 * bytesTotal / (double)size;
                        item.Speed = Helper.SizeToString(lastRead) + "/s";

                        double t1 = (size - bytesTotal) / (double)lastRead;
                        double t2 = diff / (double)1000;
                        long timeLeft = (int)(t1 / t2) + 1;

                        if (lastLeft == 0) lastLeft = timeLeft;
                        if (timeLeft > lastLeft) timeLeft = lastLeft + 1;
                        lastLeft = timeLeft;

                        item.TimeLeft = Helper.getTimeToETA(timeLeft);

                        lastRead = 0;
                        stopwatch.Restart();
                    }
                }

                client.Close();
                stream.Close();
            });
        }

        public Task ReceiveFile(FileItem item)
        {
            return Task.Run(() =>
            {
                var value = SendMessageWithReceive("receive:" + item.Path);
                long size = long.Parse(value);

                TcpClient client = new TcpClient();
                try { client.Connect(new IPEndPoint(IPAddress.Parse(device.Address), 8080)); }
                catch { return; }

                NetworkStream ns = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes("receive");
                ns.Write(buffer, 0, buffer.Length);

                int bytesRead = 0;
                long bytesTotal = 0, lastRead = 0, lastLeft = 0;
                buffer = new byte[8192];

                var stopwatch = new Stopwatch();
                stopwatch.Start();

                var stream = File.OpenWrite(Path.Combine(item.Destination, item.Name));
                while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (forceStop) break;

                    stream.Write(buffer, 0, bytesRead);
                    bytesTotal += bytesRead;
                    lastRead += bytesRead;

                    long diff = stopwatch.ElapsedMilliseconds;
                    if (diff >= 1000)
                    {
                        item.ProgressSize = Helper.SizeToString(bytesTotal);
                        item.Progress = 100 * bytesTotal / (double)size;
                        item.Speed = Helper.SizeToString(lastRead) + "/s";

                        double t1 = (size - bytesTotal) / (double)lastRead;
                        double t2 = diff / (double)1000;
                        long timeLeft = (int)(t1 / t2) + 1;

                        if (lastLeft == 0) lastLeft = timeLeft;
                        if (timeLeft > lastLeft) timeLeft = lastLeft + 1;
                        lastLeft = timeLeft;

                        item.TimeLeft = Helper.getTimeToETA(timeLeft);

                        lastRead = 0;
                        stopwatch.Restart();
                    }
                }

                stream.Close();
            });
        }

        public async void NavigateLocal(string path)
        {
            Status = "Populating folder";
            LocalPath = path;
            LocalDrive.Clear();

            await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                var di = new DirectoryInfo(path);
                if (!di.Exists) return;

                var dirs = di.EnumerateDirectories();
                foreach (var dir in dirs)
                {
                    if (dir.Attributes.HasFlag(FileAttributes.Hidden |
                        FileAttributes.System)) continue;

                    LocalDrive.Add(new FileItem()
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

                    var item = Queue.FirstOrDefault((t) => t.Path.Equals(file.FullName));
                    LocalDrive.Add(item != null ?
                        item :
                        new FileItem()
                        {
                            Model = this,
                            Path = file.FullName,
                            Name = file.Name,
                            Size = Helper.SizeToString(file.Length),
                            Extension = file.Extension.Length >= 1 ? file.Extension.Substring(1).ToUpper() : "",
                            Date = file.LastWriteTime.ToString(),
                            IsFolder = false
                        });
                }
            }), DispatcherPriority.Background);

            Status = string.Empty;
        }

        public void NavigateLocalUp()
        {
            var info = Directory.GetParent(LocalPath);
            if (info == null) NavigateLocalHome();
            else NavigateLocal(info.FullName);
        }

        public void NavigateRemoteUp()
        {
            var parent = SendMessageWithReceive("up:" + RemotePath);
            PopulateRemoteFolder(parent);
        }

        public void PopulateRemoteFolder(string path)
        {
            RemotePath = path;
            remoteFiles.Clear();

            var json = SendMessageWithReceive("list:" + path);
            JArray array;

            try { array = JArray.Parse(json); }
            catch { return; }

            foreach (var obj in array)
            {
                var name = obj.Value<string>("name");
                var rpath = obj.Value<string>("path");
                var folder = obj.Value<bool>("folder");

                var size = "";
                if (!folder)
                {
                    size = Helper.SizeToString(obj.Value<long>("size"));
                }

                var ext = Path.GetExtension(name).ToUpper();
                if (ext.Length > 0) ext = ext.Substring(1);

                var date = new DateTime(1970, 1, 1, 0, 0, 0);
                date = date.AddMilliseconds(obj.Value<long>("date"));

                RemoteFiles.Add(new FileItem()
                {
                    Model = this,
                    Path = rpath,
                    Name = name,
                    Size = size,
                    Extension = folder ? "FOLDER" : ext,
                    Date = date.ToString(),
                    IsFolder = folder,
                    IsRemote = true
                });
            }
        }

        

        public void SetDevice(DeviceItem item)
        {
            remoteFiles.Clear();
            device = item;

            var root = SendMessageWithReceive("root");
            if (root == null) return;

            PopulateRemoteFolder(root);
        }

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                if (dropInfo.TargetItem is FileItem && (dropInfo.TargetItem as FileItem).IsFolder)
                    dropInfo.DropTargetAdorner = typeof(sss);
                dropInfo.Effects = DragDropEffects.Copy;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            string path = null;

            if (dropInfo.TargetItem is FileItem && (dropInfo.TargetItem as FileItem).IsFolder)
            {
                var dropItem = dropInfo.TargetItem as FileItem;
                path = dropItem.Path;
            }
            else if (dropInfo.VisualTarget is ListView)
            {
                var dropItem = dropInfo.VisualTarget as ListView;
                path = dropItem.Tag.ToString();
            }

            if (dropInfo.Data is List<FileItem>)
            {
                var dragItems = dropInfo.Data as List<FileItem>;
                foreach (var item in dragItems)
                {
                    if (item.IsFolder) continue;
                    if (!Queue.Contains(item))
                    {
                        item.Destination = path;
                        Queue.Add(item);
                    }
                }
            }
            else if (dropInfo.Data is FileItem)
            {
                var dragItem = dropInfo.Data as FileItem;
                if (!dragItem.IsFolder)
                {
                    if (!Queue.Contains(dragItem))
                    {
                        dragItem.Destination = path;
                        Queue.Add(dragItem);
                    }
                }
            }
        }
    }

    public class sss : DropTargetAdorner
    {
        public sss(UIElement adornedElement)
            : base(adornedElement)
        {
            SetValue(RenderOptions.EdgeModeProperty, EdgeMode.Aliased);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (DropInfo.VisualTargetItem != null)
            {
                Rect rect = new Rect(
                    DropInfo.VisualTargetItem.TranslatePoint(new Point(), AdornedElement),
                    VisualTreeHelper.GetDescendantBounds(DropInfo.VisualTargetItem).Size);
                rect.Inflate(-1, -1);
                drawingContext.DrawRectangle(null, new Pen(Brushes.Red, 1), rect);
            }
        }
    }


    public class FileItem : ObservableObject
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
                destination = value;
                RaisePropertyChanged("Destination");
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

        private bool isRemote;
        public bool IsRemote
        {
            get { return isRemote; }
            set
            {
                isRemote = value;
                RaisePropertyChanged("IsRemote");
            }
        }

        private double progress = 0;
        public double Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                RaisePropertyChanged("Progress");
            }
        }

        private string progressSize;
        public string ProgressSize
        {
            get { return progressSize; }
            set
            {
                progressSize = value;
                RaisePropertyChanged("ProgressSize");
            }
        }

        private string speed;
        public string Speed
        {
            get { return speed; }
            set
            {
                speed = value;
                RaisePropertyChanged("Speed");
            }
        }

        private string timeLeft;
        public string TimeLeft
        {
            get { return timeLeft; }
            set
            {
                timeLeft = value;
                RaisePropertyChanged("TimeLeft");
            }
        }


        public ICommand RemoveFromQueue
        {
            get
            {
                return new RelayCommand(new Action(() =>
                {
                    Model.Queue.Remove(this);
                }));
            }
        }
    }
}
