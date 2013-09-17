using FreeLeaf.Model;
using System.Windows;
using System.Windows.Controls;

namespace FreeLeaf.View
{
    public partial class QueueWindow : Window
    {
        private TransferViewModel model;

        public QueueWindow()
        {
            InitializeComponent();
            model = (TransferViewModel)this.DataContext;
        }

        private async void ButtonStartStop_Click(object sender, RoutedEventArgs e)
        {
            if (model.IsBusy)
            {
                model.forceStop = true;
            }
            else
            {
                ButtonStartStop.Content = "STOP";
                model.IsBusy = true;
                model.forceStop = false;

                for (int i = 0; i < model.Queue.Count; i++)
                {
                    if (model.forceStop) continue;
                    ListQueue.ScrollIntoView(model.Queue[i]);

                    if (!model.Queue[i].IsRemote) await model.SendFile(model.Queue[i]);
                    else await model.ReceiveFile(model.Queue[i]);

                    model.Queue.Remove(model.Queue[i]);
                    i--;
                }

                ButtonStartStop.Content = "START";
                model.IsBusy = false;
            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as FileItem;
            model.Queue.Remove(item);
        }
    }
}
