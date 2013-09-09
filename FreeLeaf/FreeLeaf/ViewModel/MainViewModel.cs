using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace FreeLeaf.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private UdpClient udpClient;
        private CollectionView view;

        private ObservableCollection<DeviceItem> items;
        public ObservableCollection<DeviceItem> Items
        {
            get { return items; }
        }

        private bool isRefreshing;
        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set
            {
                isRefreshing = value;
                RaisePropertyChanged("IsRefreshing");
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

        public MainViewModel()
        {
            items = new ObservableCollection<DeviceItem>();
            view = (CollectionView)CollectionViewSource.GetDefaultView(items);

            try
            {
                var json = File.ReadAllText("D:/pinned.txt");
                var objs = (JArray)JsonConvert.DeserializeObject(json);
                foreach (var obj in objs)
                {
                    items.Add(new DeviceItem()
                    {
                        Username = obj.Value<string>("Username"),
                        Device = obj.Value<string>("Device"),
                        Address = obj.Value<string>("Address"),
                        ID = obj.Value<string>("ID"),
                        IsPinned = true
                    });
                }
            }
            catch
            {
            }

            var groupDescription = new PropertyGroupDescription("IsPinned");
            view.GroupDescriptions.Add(groupDescription);

            if (IsInDesignMode) return;

            udpClient = new UdpClient(8888);
            udpClient.BeginReceive(new AsyncCallback(ReceiveDeviceInfo), null);

            //new Thread(ReceiveDeviceInfo).Start();
            new Thread(CheckDeviceAvailability).Start();
        }

        private void CheckDeviceAvailability()
        {
            while (true)
            {
                Thread.Sleep(1000);
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    for (int i = 0; i < items.Count; i++)
                    {
                        if (items[i].LastUpdated >= 2 * items[i].RefreshRate)
                        {
                            if (items[i].IsPinned)
                            {
                                items[i].IsAvailable = false;
                            }
                            else
                            {
                                if (SelectedItem == items[i]) SelectedItem = null;
                                items.RemoveAt(i);
                                i--;
                            }
                        }
                        else
                        {
                            items[i].LastUpdated++;
                        }
                    }
                }), DispatcherPriority.Background);
            }
        }

        private void ReceiveDeviceInfo(IAsyncResult ar)
        {
            var endpoint = new IPEndPoint(IPAddress.Any, 8888);
            var bytes = udpClient.EndReceive(ar, ref endpoint);
            var data = Encoding.UTF8.GetString(bytes);
            var ip = endpoint.Address.ToString();

            var array = (JArray)JsonConvert.DeserializeObject(data);
            var id = array[0].Value<string>();

            var item = items.SingleOrDefault((i) =>
            {
                if (i.ID == null) return false;
                return i.ID.Equals(id);
            });

            if (item == null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    var newItem = new DeviceItem()
                    {
                        ID = id,
                        IsAvailable = true,
                        Username = array[1].Value<string>(),
                        Device = array[2].Value<string>(),
                        Battery = array[3].Value<string>(),
                        Storage = array[4].Value<string>(),
                        Wifi = array[5].Value<string>(),
                        RefreshRate = array[6].Value<int>() / 1000,
                        Address = ip
                    };
                    items.Add(newItem);

                    if (SelectedItem == null) SelectedItem = newItem;
                }), DispatcherPriority.Background);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    item.LastUpdated = 0;
                    item.IsAvailable = true;
                    item.Username = array[1].Value<string>();
                    item.Device = array[2].Value<string>();
                    item.Battery = array[3].Value<string>();
                    item.Storage = array[4].Value<string>();
                    item.Wifi = array[5].Value<string>();
                    item.RefreshRate = array[6].Value<int>() / 1000;
                    item.Address = ip;

                    if (SelectedItem == null) SelectedItem = item;
                }), DispatcherPriority.Background);
            }

            udpClient.BeginReceive(new AsyncCallback(ReceiveDeviceInfo), null);
        }

        public void EditPinned(DeviceItem item, bool pinned)
        {
            item.IsPinned = pinned;
            if (!item.IsAvailable && !pinned)
            {
                if (SelectedItem == item) SelectedItem = null;
                items.Remove(item);
            }

            view.Refresh();

            var devices = items.Where((i) => i.IsPinned);
            var json = JsonConvert.SerializeObject(devices);
            File.WriteAllText("D:/pinned.txt", json);
        }
    }

    public class DeviceItem : ObservableObject
    {
        [JsonIgnore()]
        public string Name
        {
            get
            {
                return string.Concat(username, "'s ", device);
            }
        }

        private string username;
        public string Username
        {
            get { return username; }
            set
            {
                username = value;
                RaisePropertyChanged("Username");
                RaisePropertyChanged("Name");
            }
        }

        private string device;
        public string Device
        {
            get { return device; }
            set
            {
                device = value;
                RaisePropertyChanged("Device");
                RaisePropertyChanged("Name");
            }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                RaisePropertyChanged("Address");
            }
        }

        private string id;
        public string ID
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged("ID");
            }
        }

        private bool isPinned;
        [JsonIgnore()]
        public bool IsPinned
        {
            get { return isPinned; }
            set
            {
                isPinned = value;
                RaisePropertyChanged("IsPinned");
            }
        }

        private bool isAvailable;
        [JsonIgnore()]
        public bool IsAvailable
        {
            get { return isAvailable; }
            set
            {
                if (isAvailable != value && !value)
                {
                    Battery = "-";
                    Storage = "-";
                    Wifi = "-";
                }

                isAvailable = value;
                RaisePropertyChanged("IsAvailable");
            }
        }

        private string battery = "-";
        [JsonIgnore()]
        public string Battery
        {
            get { return battery; }
            set
            {
                battery = value;
                RaisePropertyChanged("Battery");
            }
        }

        private string storage = "-";
        [JsonIgnore()]
        public string Storage
        {
            get { return storage; }
            set
            {
                storage = value;
                RaisePropertyChanged("Storage");
            }
        }

        private string wifi = "-";
        [JsonIgnore()]
        public string Wifi
        {
            get { return wifi; }
            set
            {
                wifi = value;
                RaisePropertyChanged("Wifi");
            }
        }

        private int lastUpdated = 0;
        [JsonIgnore()]
        public int LastUpdated
        {
            get { return lastUpdated; }
            set
            {
                lastUpdated = value;
                RaisePropertyChanged("LastUpdated");
            }
        }

        public int RefreshRate = 3000;
    }
}
