using System;
using System.Windows.Forms;

namespace WINReplacer
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);
            //TODO: SEARCH DISABLE
            Application.Run(new WIN());
        }
    }
}
