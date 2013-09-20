using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeLeaf.Model
{
    public class Helper
    {
        public static string SizeToString(long size)
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

        public static string getTimeToETA(double sec)
        {
            var ts = TimeSpan.FromSeconds(sec);

            if (ts.Hours == 1)
            {
                return "1 hour remaining";
            }
            else if (ts.Hours > 1)
            {
                return ts.Hours + " hours remaining";
            }
            else if (ts.Minutes == 1)
            {
                return "1 minute remaining";
            }
            else if (ts.Minutes > 1)
            {
                return ts.Minutes + " minutes remaining";
            }
            else if (ts.Seconds == 1)
            {
                return "1 second remaining";
            }
            else
            {
                return ts.Seconds + " seconds remaining";
            }
        }
    }
}
