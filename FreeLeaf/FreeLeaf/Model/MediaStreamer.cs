using GalaSoft.MvvmLight;
using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using Un4seen.Bass;

namespace FreeLeaf.Model
{
    public class MediaStreamer
    {
        private int stream;
        private BASS_FILEPROCS fileproc;
        private DispatcherTimer timer;

        private MusicFileItem currentItem;
        public MusicFileItem CurrentItem
        {
            get { return currentItem; }
            set
            {
                currentItem = value;
                
            }
        }

        public MediaStreamer()
        {
            fileproc = new BASS_FILEPROCS(new FILECLOSEPROC(FileClose), new FILELENPROC(FileLength), new FILEREADPROC(FileRead), null);
            timer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
            timer.Tick += timer_Tick;
        }

        void timer_Tick(object sender, EventArgs e)
        {
            var level = Bass.BASS_ChannelGetLevel(stream);
            var left = Utils.LowWord32(level) / (double)Int16.MaxValue;
            var right = Utils.HighWord32(level)  / (double)Int16.MaxValue;

            var val = 200 * (left + right);

            currentItem.VuValue += (val - currentItem.VuValue) / 10f;

            currentItem.Time = TimeSpan.FromSeconds(
                Bass.BASS_ChannelBytes2Seconds(stream,
                    Bass.BASS_ChannelGetPosition(stream)));
        }

        private void FileClose(IntPtr data)
        {
            var handle = (GCHandle)data;
            var ns = (NetworkStream)handle.Target;
            if (ns != null) ns.Close();
        }

        private long FileLength(IntPtr data)
        {
            return 0; // Don't know the file size
        }

        private int FileRead(IntPtr i, int len, IntPtr data)
        {
            var handle = (GCHandle)data;
            var ns = (NetworkStream)handle.Target;

            if (ns == null) return 0;

            byte[] buffer = new byte[len];
            int bytesRead = 0;
            try { bytesRead = ns.Read(buffer, 0, len); }
            catch { }

            if (bytesRead > 0)
            {
                Marshal.Copy(buffer, 0, i, bytesRead);
            }

            return bytesRead;
        }

        public void Play(MusicFileItem item)
        {
            if (item == currentItem)
            {
                if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
                {
                    Bass.BASS_ChannelPause(stream);
                    timer.Stop();
                    currentItem.IsPlaying = false;
                }
                else
                {
                    Bass.BASS_ChannelPlay(stream, false);
                    timer.Start();
                    currentItem.IsPlaying = true;
                }
                return;
            }

            if (currentItem != null)
            {
                currentItem.VuValue = 0;
                currentItem.IsPlaying = false;
            }

            if (Bass.BASS_ChannelIsActive(stream) == BASSActive.BASS_ACTIVE_PLAYING)
            {
                Bass.BASS_StreamFree(stream);
            }

            currentItem = item;

            timer.Start();


            if (item.IsRemote)
            {

                var client = new TcpClient();

                client.NoDelay = true;
                client.Connect(TransferViewModel.device.Address, 8000);

                var ns = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes("receive:" + item.Path);

                ns.Write(buffer, 0, buffer.Length);
                ns.Flush();

                GCHandle handle = GCHandle.Alloc(ns);

                stream = Bass.BASS_StreamCreateFileUser(BASSStreamSystem.STREAMFILE_BUFFER, BASSFlag.BASS_DEFAULT, fileproc, (IntPtr)handle);

            }
            else
            {
                stream = Bass.BASS_StreamCreateFile(item.Path, 0, 0, BASSFlag.BASS_DEFAULT);
            }

            currentItem.IsPlaying = Bass.BASS_ChannelPlay(stream, false);
        }
    }
}
