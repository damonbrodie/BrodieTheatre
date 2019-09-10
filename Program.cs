using System;
using System.Windows.Forms;
using HarmonyHub;

namespace BrodieTheatre
{
    static class Program
    {
        public static HarmonyClient Client { get; set; }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
