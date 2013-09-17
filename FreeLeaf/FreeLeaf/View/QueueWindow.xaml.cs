using FreeLeaf.Model;
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

namespace FreeLeaf.View
{
    public partial class QueueWindow : Window
    {
        TransferViewModel model;

        public QueueWindow()
        {
            InitializeComponent();
            model = (TransferViewModel)this.DataContext;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < model.Queue.Count;i++ )
            {
                if (!model.Queue[i].IsRemote) await model.SendFile(model.Queue[i]);
                else await model.ReceiveFile(model.Queue[i]);

                model.Queue.Remove(model.Queue[i]);
                i--;
            }
        }
    }
}
