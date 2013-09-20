using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GongSolutions.Wpf.DragDrop;
using Microsoft.Practices.ServiceLocation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Un4seen.Bass;

namespace FreeLeaf.Model
{
    public class TransferViewModel : ViewModelBase, IDropTarget
    {
        #region Internal

        private const int PORT = 8000;

        public static DeviceItem device;
        public bool forceStop;

        private int secElapsed, bytesSeqRead;
        private long bytesTotalRead, bytesTotal;
        private FileItem currentItem;

        private DispatcherTimer timerProgress;

        #endregion

        #region Properties

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
                RaisePropertyChanged("RemoteFiles");
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

        #endregion

        #region Constructor

        public MediaStreamer mss = new MediaStreamer();

        public TransferViewModel()
        {
            if (IsInDesignMode) return;

            Bass.BASS_Init(-1, 44100, Un4seen.Bass.BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);

            queue = new ObservableCollection<FileItem>();
            localFiles = new ObservableCollection<FileItem>();
            remoteFiles = new ObservableCollection<FileItem>();

            NavigateLocalHome();

            timerProgress = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
            timerProgress.Tick += TimerProgress_Tick;

            mss = new MediaStreamer();
        }

        private void TimerProgress_Tick(object sender, EventArgs e)
        {
            currentItem.ProgressSize = Helper.SizeToString(bytesTotalRead);
            currentItem.Progress = 100 * bytesTotalRead / (double)bytesTotal;
            currentItem.Speed = Helper.SizeToString(bytesSeqRead) + "/s";

            if (bytesTotalRead != 0)
            {
                long bytesRemaining = bytesTotal - bytesTotalRead;
                double secRemaining = bytesRemaining * secElapsed / (double)bytesTotalRead;
                currentItem.TimeLeft = Helper.getTimeToETA(secRemaining + 1);
            }

            bytesSeqRead = 0;
            secElapsed++;
        }

        public async void SetDevice(DeviceItem item)
        {
            remoteFiles.Clear();
            device = item;

            var root = await SendCommand("root");
            if (root == null) return;

            NavigateRemote(root);
        }

        #endregion

        #region Tcp

        public static Task<string> SendCommand(string message)
        {
            return Task.Run<string>(() =>
            {
                string msg = null;

                using (var client = new TcpClient())
                {
                    client.NoDelay = true;
                    client.ReceiveBufferSize = 8192;
                    client.SendBufferSize = 8192;
                    client.Connect(TransferViewModel.device.Address, PORT);

                    using (var ns = client.GetStream())
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes(message);

                        ns.Write(buffer, 0, buffer.Length);
                        ns.Flush();

                        int bytesRead = 0;
                        buffer = new byte[8192];

                        using (var ms = new MemoryStream())
                        {
                            while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                ms.Write(buffer, 0, bytesRead);
                            }
                            msg = Encoding.UTF8.GetString(ms.GetBuffer(), 0, (int)ms.Length);
                        }
                    }
                }

                return msg;
            });
        }

        public Task SendFile(FileItem item)
        {
            return Task.Run(async () =>
            {
                var info = new FileInfo(item.Path);
                var size = info.Length;

                await SendCommand(string.Format("send:{0}:{1}:{2}", item.Name, item.Destination, size));

                int bytesRead = 0;
                byte[] buffer = new byte[8192];

                secElapsed = 0;
                bytesTotalRead = 0;
                bytesSeqRead = 0;
                bytesTotal = size;
                currentItem = item;

                using (var stream = info.OpenRead())
                {
                    using (var client = new TcpClient())
                    {
                        client.NoDelay = true;
                        client.ReceiveBufferSize = 8192;
                        client.SendBufferSize = 8192;
                        client.Connect(device.Address, PORT);

                        using (var ns = client.GetStream())
                        {
                            timerProgress.Start();

                            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (forceStop) break;
                                ns.Write(buffer, 0, bytesRead);

                                bytesTotalRead += bytesRead;
                                bytesSeqRead += bytesRead;
                            }
                            ns.Flush();
                        }

                        timerProgress.Stop();
                        currentItem = null;
                    }
                }
            });
        }

        public Task ReceiveFile(FileItem item)
        {
            return Task.Run(async () =>
            {
                string value = await SendCommand("receive:" + item.Path);
                long size = long.Parse(value);

                int bytesRead = 0;

                secElapsed = 0;
                bytesTotalRead = 0;
                bytesSeqRead = 0;
                bytesTotal = size;
                currentItem = item;

                using (var client = new TcpClient())
                {
                    client.NoDelay = true;
                    client.ReceiveBufferSize = 8192;
                    client.SendBufferSize = 8192;
                    client.Connect(device.Address, PORT);

                    using (var ns = client.GetStream())
                    {
                        byte[] buffer = Encoding.UTF8.GetBytes("receive");

                        ns.Write(buffer, 0, buffer.Length);
                        ns.Flush();

                        buffer = new byte[8192];

                        using (var stream = File.OpenWrite(Path.Combine(item.Destination, item.Name)))
                        {
                            timerProgress.Start();

                            while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                if (forceStop) break;
                                //stream.Write(buffer, 0, bytesRead);

                                bytesTotalRead += bytesRead;
                                bytesSeqRead += bytesRead;
                            }
                        }

                        timerProgress.Stop();
                        currentItem = null;
                    }
                }
            });
        }

        #endregion

        #region Local

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

        public void NavigateLocal(string path)
        {
            if (path == "/")
            {
                NavigateLocalHome();
                return;
            }

            LocalPath = path;
            LocalDrive.Clear();

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

                FileItem item;

                if (MusicFileItem.IsMusicFile(file.FullName)) item = new MusicFileItem();
                else item = new FileItem();

                item.Model = this;
                item.Path = file.FullName;
                item.Name = file.Name;
                item.Size = Helper.SizeToString(file.Length);
                item.Extension = file.Extension.Length >= 1 ? file.Extension.Substring(1).ToUpper() : "";
                item.Date = file.LastWriteTime.ToString();
                item.IsFolder = false;

                LocalDrive.Add(item);
            }
        }

        public void NavigateLocalUp()
        {
            var info = Directory.GetParent(LocalPath);
            if (info == null) NavigateLocalHome();
            else NavigateLocal(info.FullName);
        }

        #endregion

        #region Remote

        public async void NavigateRemote(string path)
        {
            RemotePath = path;
            RemoteFiles.Clear();

            var json = await SendCommand("list:" + path);
            JArray array;

            try { array = JArray.Parse(json); }
            catch { return; }

            var temp = new List<FileItem>();

            foreach (var obj in array)
            {


                var rpath = obj.Value<string>("path");
                var folder = obj.Value<bool>("folder");


                if (!folder && mss.CurrentItem != null && mss.CurrentItem.Path.Equals(rpath))
                {
                    temp.Add(mss.CurrentItem);
                    continue;
                }

                var name = obj.Value<string>("name");
                

                var size = "";
                if (!folder)
                {
                    size = Helper.SizeToString(obj.Value<long>("size"));
                }

                var ext = Path.GetExtension(name).ToUpper();
                if (ext.Length > 0) ext = ext.Substring(1);

                var date = new DateTime(1970, 1, 1, 0, 0, 0);
                date = date.AddMilliseconds(obj.Value<long>("date"));

                FileItem item;

                if (MusicFileItem.IsMusicFile(rpath)) item = new MusicFileItem();
                else item = new FileItem();

                item.Model = this;
                item.Path = rpath;
                item.Name = name;
                item.Size = size;
                item.Extension = folder ? "FOLDER" : ext;
                item.Date = date.ToString();
                item.IsFolder = folder;
                item.IsRemote = true;

                temp.Add(item);
            }

            temp.Sort(new Comparison<FileItem>((t1, t2) =>
            {
                if (t1.IsFolder && !t2.IsFolder) return -1;
                else if (!t1.IsFolder && t2.IsFolder) return 1;
                return string.Compare(t1.Name, t2.Name, true);
            }));

            RemoteFiles = new ObservableCollection<FileItem>(temp);
        }

        public async void NavigateRemoteUp()
        {
            var parent = await SendCommand("up:" + RemotePath);
            NavigateRemote(parent);
        }

        #endregion

        #region Drag n' Drop

        public void DragOver(IDropInfo dropInfo)
        {
            if (dropInfo.DragInfo.VisualSource != dropInfo.VisualTarget)
            {
                if (dropInfo.TargetItem is FileItem && (dropInfo.TargetItem as FileItem).IsFolder)
                    dropInfo.DropTargetAdorner = typeof(CustomDropAdorner);
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
                var items = dropInfo.Data as List<FileItem>;
                foreach (var item in items)
                {
                    if (item.IsFolder) continue;
                    if (Queue.FirstOrDefault(t => t.Path.Equals(item.Path)) == null)
                    {
                        item.Progress = 0;
                        item.Destination = path;
                        Queue.Add(item);
                    }
                }
            }
            else if (dropInfo.Data is FileItem)
            {
                var item = dropInfo.Data as FileItem;
                if (!item.IsFolder)
                {
                    if (Queue.FirstOrDefault(t => t.Path.Equals(item.Path)) == null)
                    {
                        item.Progress = 0;
                        item.Destination = path;
                        Queue.Add(item);
                    }
                }
            }
        }

        #endregion
    }

    public class CustomDropAdorner : DropTargetAdorner
    {
        public CustomDropAdorner(UIElement adornedElement)
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

    public class MusicFileItem : FileItem
    {
        public static string[] Formats = new string[] { ".mp3", ".m4a", ".aac", ".wav" };

        public static bool IsMusicFile(string path)
        {
            var ext = System.IO.Path.GetExtension(path).ToLower();
            return Formats.Contains(ext);
        }

        private double vuValue;
        public double VuValue
        {
            get { return vuValue; }
            set
            {
                vuValue = value;
                RaisePropertyChanged("VuValue");
            }
        }

        private bool isPlaying;
        public bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }

        public ICommand Play
        {
            get
            {
                return new RelayCommand(new Action(async () =>
                {
                    Model.mss.Play(this);
                }));
            }
        }
    }
}
