using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeLeaf.Model
{
    public class PicturePreviewer
    {
        public static void PreviewItem(PictureFileItem item)
        {
            string path = null;

            if (!item.IsRemote)
            {
                path = item.Path;
            }
            else
            {
                if (item.TempPath != null)
                {
                    path = item.TempPath;
                }
                else
                {
                    Task.Run(() =>
                    {
                        item.IsLoading = true;

                        int bytesRead, bytesTotalRead = 0;

                        using (var client = new TcpClient())
                        {
                            client.Connect(TransferViewModel.device.Address, 8000);

                            using (var ns = client.GetStream())
                            {
                                byte[] buffer = Encoding.UTF8.GetBytes("receive:" + item.Path);

                                ns.Write(buffer, 0, buffer.Length);
                                ns.Flush();

                                buffer = new byte[8192];

                                item.TempPath = Path.ChangeExtension(Path.GetTempFileName(),
                                    Path.GetExtension(item.Path));

                                var sw = new Stopwatch();
                                sw.Start();

                                using (var stream = File.OpenWrite(item.TempPath))
                                {
                                    while ((bytesRead = ns.Read(buffer, 0, buffer.Length)) > 0)
                                    {
                                        stream.Write(buffer, 0, bytesRead);
                                        bytesTotalRead += bytesRead;

                                        if (sw.ElapsedMilliseconds >= 200)
                                        {
                                            item.Progress = 100 * bytesTotalRead / (double)item.Size;
                                            sw.Restart();
                                        }
                                    }
                                }

                                Process.Start(item.TempPath);
                            }
                        }

                        item.IsLoading = false;
                    });
                }
            }

            if (path != null) Process.Start(path);
        }
    }
}
