using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace BrodieTheatre
{
    public class Logging
    {
        public static void writeLog(string Message)
        {
            DateTime now = DateTime.Now;
            int counter = 0;
            bool success = false;
            while (!success && counter < 3)
            {
                try
                {
                    using (StreamWriter file = File.AppendText("brodietheatre_log.txt"))
                    {
                        file.WriteLine(now.ToString("yyyy-MM-dd HH:mm:ss") + " " + Message);
                        success = true;
                    }
                }
                catch { }
            }
        }
    }

    public partial class FormMain : Form
    {
        static async Task doDelay(int ms)
        {
            await Task.Delay(ms);
        }
    }
}
