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

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var item = button.DataContext as FileItem;
            model.Queue.Remove(item);
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            model.Queue.Clear();
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            model.forceStop = true;
        }

        private async void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            model.IsBusy = true;
            model.forceStop = false;

            for (int i = 0; i < model.Queue.Count; i++)
            {
                ListQueue.ScrollIntoView(model.Queue[i]);

                if (!model.Queue[i].IsRemote) await model.SendFile(model.Queue[i]);
                else await model.ReceiveFile(model.Queue[i]);

                if (model.forceStop) break;

                model.Queue.Remove(model.Queue[i]);
                i--;
            }

            model.IsBusy = false;
        }
    }
}
