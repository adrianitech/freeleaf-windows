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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace FreeLeaf.Model
{
    public class MainViewModel : ViewModelBase
    {
        public static string[] Colors = new string[] { "#41bdbd", "#d76d93", "#7c4a81", "#eacb5f" };

        private UdpClient udpClient;

        private DispatcherTimer timer1;

        private ObservableCollection<DeviceItem> items;
        public ObservableCollection<DeviceItem> Items
        {
            get { return items; }
        }

        public MainViewModel()
        {
            items = new ObservableCollection<DeviceItem>();

            items.Add(new DeviceItem()
            {
                Name = "Connect via IP address",
                Color = "#e4715f"
            });

            if (IsInDesignMode) return;

            udpClient = new UdpClient(8888);

            timer1 = new DispatcherTimer();
            timer1.Interval = TimeSpan.FromSeconds(1);
            timer1.Tick += timer1_Tick;
            timer1.Start();

            Task.Run(() =>
            {
                var endpoint = new IPEndPoint(IPAddress.Any, 8888);

                while (true)
                {
                    var bytes = udpClient.Receive(ref endpoint);
                    var data = Encoding.UTF8.GetString(bytes);
                    var ip = endpoint.Address.ToString();

                    var array = (JArray)JsonConvert.DeserializeObject(data);
                    var id = array[0].Value<string>();

                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        var item = items.SingleOrDefault((i) =>
                        {
                            if (i.ID == null) return false;
                            return i.ID.Equals(id);
                        });

                        if (item == null)
                        {
                            var newItem = new DeviceItem()
                            {
                                ID = id,
                                Name = array[1].Value<string>(),
                                Device = array[2].Value<string>(),
                                Battery = array[3].Value<string>(),
                                Storage = array[4].Value<string>(),
                                Address = ip
                            };

                            newItem.Color = Colors[Getss(newItem.ID)];
                            items.Insert(0, newItem);
                        }
                        else
                        {
                            item.Name = array[1].Value<string>();
                            item.Battery = array[3].Value<string>();
                            item.Storage = array[4].Value<string>();
                            item.LastUpdated = 0;
                        }
                    }), DispatcherPriority.Background);

                    Task.Delay(100);
                }
            });
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].ID == null) continue;
                if (items[i].LastUpdated >= 6)
                {
                    items.RemoveAt(i);
                    i--;
                }
                else
                {
                    items[i].LastUpdated++;
                }
            }
        }

        private int Getss(string id)
        {
            int x = 0;
            for (int i = 0; i < id.Length; i++)
            {
                x += (int)id[i];
            }
            return x % Colors.Length;
        }

    }

    public class DeviceItem : ObservableObject
    {
        public int LastUpdated = 0;

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

        private string device;
        public string Device
        {
            get { return device; }
            set
            {
                device = value;
                RaisePropertyChanged("Device");
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

        private string color;
        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                RaisePropertyChanged("Color");
            }
        }

        private string battery;
        public string Battery
        {
            get { return battery; }
            set
            {
                battery = value;
                RaisePropertyChanged("Battery");
            }
        }

        private string storage;
        public string Storage
        {
            get { return storage; }
            set
            {
                storage = value;
                RaisePropertyChanged("Storage");
            }
        }
    }
}
