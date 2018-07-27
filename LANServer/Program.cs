using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Windows.Forms;

namespace LANServer.Server
{
    /// <summary>
    /// Server Program
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// Run a Server Form
        /// </summary>
        /// <param name="args">Input arguments</param>
        [STAThread]
        static void Main(String[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new ServerForm());
        }
    }
}


