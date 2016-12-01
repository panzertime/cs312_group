using System;
using System.Windows.Forms;

namespace TSP
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
       {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(MainForm = new Mainform());
        }
        public static Mainform MainForm;

    }
}