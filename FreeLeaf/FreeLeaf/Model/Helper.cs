using System;

namespace FreeLeaf.Model
{
    public class Helper
    {
        public static string SizeToString(long size)
        {
            double length = size;
            if (length == 0) return string.Empty;

            string[] sizes = { "B", "KB", "MB", "GB" };
            int order = 0;

            while (length >= 1024 && order + 1 < sizes.Length)
            {
                length = length / (double)1024;
                order++;
            }

            return string.Format("{0:0.##} {1}", length, sizes[order]);
        }

        public static string TimeToETA(double sec)
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
