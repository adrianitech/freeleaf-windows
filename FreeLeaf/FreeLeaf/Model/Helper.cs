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

        public static string getTimeToETA(long timeLeft)
        {
            string msgLeft;

            if (timeLeft < 60)
            {
                if (timeLeft == 1)
                {
                    msgLeft = " second left";
                }
                else
                {
                    msgLeft = " seconds left";
                }
            }
            else if (timeLeft < 3600)
            {
                timeLeft /= 60;
                if (timeLeft == 1)
                {
                    msgLeft = " minute left";
                }
                else
                {
                    msgLeft = " minutes left";
                }
            }
            else
            {
                timeLeft /= 3600;
                if (timeLeft == 1)
                {
                    msgLeft = " hour left";
                }
                else
                {
                    msgLeft = " hours left";
                }
            }

            return timeLeft + msgLeft;
        }
    }
}
