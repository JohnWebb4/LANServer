using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using LANServer;
using System.Windows.Forms;

namespace LANServer.Client
{
    /// <summary>
    /// Entry-point of client application
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ClientForm());
        }
    }
}
