using System;
using System.ComponentModel;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using LANServer.Server;

namespace LANServer
{
    /// <summary>
    /// Server form
    /// </summary>
    public partial class ServerForm : Form
    {
        /// <summary>
        /// Called to signify done reading
        /// </summary>
        protected static ManualResetEvent onRead =
            new ManualResetEvent(false);

        /// <summary>
        /// Called if reading line
        /// </summary>
        protected static bool reading = false;

        /// <summary>
        /// User input to the server
        /// </summary>
        protected static string input;

        /// <summary>
        /// Initialize the form
        /// </summary>
        public ServerForm()
        {
            InitializeComponent();

            // Hook server grew
            AsynchServer.ServerGrew +=
                new ChangedEventHandler(onServerGrew);

            AsynchServer.ServerClear +=
                new ChangedEventHandler(onServerClear);
        }

        /// <summary>
        /// Send tbInput text to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bttnSend_Click(object sender, EventArgs e)
        {
            // Get text
            input = tbSend.Text;

            // Clear text
            tbSend.Text = "";

            // If not reading
            if (!reading)
            {
                // Check if null or empty
                if (String.IsNullOrEmpty(input))
                {
                    // Return
                    return;
                }

                // Send to console
                AsynchServer.GUI.ToReceiveIn(input);
            }
            else
            {
                // Else set onRead event
                onRead.Set();
            }
        }

        /// <summary>
        /// Read a line from the console
        /// </summary>
        /// <returns></returns>
        public static string ReadLine()
        {
            // Reset on read
            onRead.Reset();

            // Set reading
            reading = true;

            // Wait till button send
            onRead.WaitOne();

            // Set readin
            reading = false;

            // Return input
            return input;
        }

        /// <summary>
        /// Called is console is changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected void onServerGrew(object sender, EventArgs e)
        {
            // Declare event handler arguemnts
            object[] args = { this, EventArgs.Empty };

            // Signal needs update
            rtbConsole.BeginInvoke(new ChangedEventHandler(rtbConsole_TextChanged),
                args);
        }

        /// <summary>
        /// Called on call to server clear
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguemnts</param>
        protected void onServerClear(object sender, EventArgs e)
        {
            // Declare event handler arguments
            object[] args = { this, EventArgs.Empty };

            // Call event handler
            rtbConsole.BeginInvoke(new ChangedEventHandler(ServerClear),
                args);
        }

        /// <summary>
        /// Clears console text
        /// </summary>
        /// <param name="sender">Object sender</param>
        /// <param name="e">Event arguements</param>
        private void ServerClear(object sender, EventArgs e)
        {
            // Clear text
            rtbConsole.Text = "";
        }

        /// <summary>
        /// Start server
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bWorkStart_DoWork(object sender, DoWorkEventArgs e)
        {
            // Start server
            AsynchServer.StartListening();

            return;
        }

        /// <summary>
        /// Called if rtb console needs to be updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbConsole_TextChanged(object sender, EventArgs e)
        {
            // Get console lines
            List<string> lines = new List<string>(rtbConsole.Lines);

            // Get messages
            string[] messages = AsynchServer.GUI.ToSendGetMessages();

            // Loop through messages
            foreach(string message in messages)
            {
                // Add message
                lines.Add(message);

                // while over capacity
                while (lines.Count > Values.consoleSize)
                {
                    // Remove last line
                    lines.RemoveAt(0);
                }
            }

            // Assign line
            rtbConsole.Lines = lines.ToArray();

            // Assign to bottom
            rtbConsole.SelectionStart = rtbConsole.Text.Length;

            // Scroll to end of console
            rtbConsole.ScrollToCaret();
        }

        /// <summary>
        /// Server closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bWorkStart_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Exit form
            this.Close();
        }

        /// <summary>
        /// User closes form
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Close server
            AsynchServer.Exit(true);
        }

        private void ServerForm_SizeChanged(object sender, EventArgs e)
        {
            // Resize rtbconsole
            rtbConsole.Height = this.Height - pnlSend.Height - 45;

            //Resize input
            tbSend.Width = this.Width - bttnSend.Width - 15;
        }

        /// <summary>
        /// Called when form initially shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerForm_Shown(object sender, EventArgs e)
        {
            // Start server
            bWorkStart.RunWorkerAsync();
        }
    }
}
