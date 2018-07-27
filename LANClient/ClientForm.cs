using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;

namespace LANServer.Client
{
    public partial class ClientForm : Form
    {
        /// <summary>
        /// Called on read
        /// </summary>
        protected static ManualResetEvent onRead =
            new ManualResetEvent(false);

        /// <summary>
        /// Called if reading
        /// </summary>
        protected static bool reading;

        /// <summary>
        /// The input to the client
        /// </summary>
        protected static string input;

        /// <summary>
        /// Used to update client
        /// </summary>
        protected IEnumerator clientUpdate = AsynchClient.Update();

        /// <summary>
        /// Initialzie client form
        /// </summary>
        public ClientForm()
        {
            // Initialize components
            InitializeComponent();

            // Add text changed event
            AsynchClient.GUISend += new ChangedEventHandler(onGUIChange);
            // Add clear console event
            AsynchClient.GUIClear += new ChangedEventHandler(onGUIClear);
        }

        /// <summary>
        /// Send text to console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bttnSend_Click(object sender, EventArgs e)
        {
            // Get input text
            input = tbInput.Text;

            // Clear text box input
            tbInput.Text = "";

            // If not reading
            // Readline called (onRead is reset)
            if (!reading)
            {
                // Is null or empty
                if (String.IsNullOrEmpty(input))
                {
                    // Return
                    return;
                }

                // Pass to server
                AsynchClient.GUI.ToReceiveIn(input);
            }
            else
            {
                // Set on read
                onRead.Set();
            }
        }

        /// <summary>
        /// Update text to match console
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rTBConsole_TextChanged(object sender, EventArgs e)
        {

            // Attempt to get messages
            try
            {
                // Get console
                List<string> lines = new List<string>(rTBConsole.Lines);

                // Get messages
                string[] messages = AsynchClient.GUI.ToSendGetMessages();

                // Loop through messages
                foreach (string message in messages)
                {
                    // Add message
                    lines.Add(message);

                    // If over capacity
                    while (lines.Count > Values.consoleSize)
                    {
                        // Remove last element
                        lines.RemoveAt(0);
                    }
                }

                // Queue empty
                // Assign lines
                rTBConsole.Lines = lines.ToArray();

                // Set position to end
                rTBConsole.SelectionStart = rTBConsole.Text.Length;

                // Scroll to position
                rTBConsole.ScrollToCaret();
            }
            catch(InvalidOperationException excep)
            {
                // Write to GUI
                rTBConsole.AppendText("\n" + excep.ToString());

            }
        }

        /// <summary>
        /// Called if console is changed
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        protected void onGUIChange(object sender, EventArgs e)
        {
            // Define rtbConsole_TextChanged arguments
            object[] args = { this, EventArgs.Empty };

            // Catch message
            try
            {
                // Begin invoke textchanged
                rTBConsole.BeginInvoke(new ChangedEventHandler(rTBConsole_TextChanged),
                    args);
            }
            // Can't display to console
            catch (InvalidOperationException excep)
            {
                // Show message box of exception
                MessageBox.Show(excep.ToString());
            }
        }

        /// <summary>
        /// Call to clear console
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void onGUIClear(object sender, EventArgs e)
        {
            // Define event handler arguments
            object[] args = { this, EventArgs.Empty };

            // Begin invoke clear
            rTBConsole.BeginInvoke(new ChangedEventHandler(GUIClear),
                args);

        }

        /// <summary>
        /// Clear console
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        protected void GUIClear(object sender, EventArgs e)
        {
            // Clear console
            rTBConsole.Text = "";
        }

        /// <summary>
        /// Read line
        /// </summary>
        /// <returns></returns>
        public static string ReadLine()
        {
            // Reset on read
            onRead.Reset();

            // Set reading true
            reading = true;

            // Lock thread till button send
            onRead.WaitOne();

            // Set reading false
            reading = false;

            // Return input
            return input;
        }

        /// <summary>
        /// Runs client server
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">arguments</param>
        private void bWorkStart_DoWork(object sender, DoWorkEventArgs e)
        {
            // Start client
            AsynchClient.StartClient();
        }

        /// <summary>
        /// Called by timer to update to server
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Arguments</param>
        private void timerUpdate_Tick(object sender, EventArgs e)
        {
            // If startup done and
            // no background read
            if (!bWorkStart.IsBusy && !bWorkRead.IsBusy)
            {
                // Start read
                bWorkRead.RunWorkerAsync();
            }
        }

        private void bWorkRead_DoWork(object sender, DoWorkEventArgs e)
        {
            // If joined server
            if (AsynchClient.isJoined)
            {
                // Update
                clientUpdate.MoveNext();
            }
            // If disconnected
            if (AsynchClient.isJoined && !AsynchClient.isServer)
            {
                // Write to gui
                AsynchClient.WriteGUI("Disconnected from server.");

                // Attempt new connection
                bWorkStart.RunWorkerAsync();

                // Reset update
                clientUpdate = AsynchClient.Update();
            }
        }

        private void ClientForm_SizeChanged(object sender, EventArgs e)
        {
            // Resize console
            rTBConsole.Height = this.Height - pnlSend.Height - 45;

            // Resize input
            tbInput.Width = this.Width - bttnSend.Width - 15;
        }

        /// <summary>
        /// Finish client commands
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Exit
            AsynchClient.GUI.ToReceiveIn(Strings.command_Exit);

            // Attempt to update
            try
            {
                // Call update
                bWorkRead.RunWorkerAsync();
            }
            catch(InvalidOperationException)
            {
                // Already Updating
                // Ignore and wait for close
                // Write GUI
                rTBConsole.AppendText("\nWaiting for close.");
            }
        }

        /// <summary>
        /// Called after form is initially shown
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClientForm_Shown(object sender, EventArgs e)
        {
            // Start background client
            bWorkStart.RunWorkerAsync();
        }
    }
}
