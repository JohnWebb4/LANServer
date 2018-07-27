using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections;

namespace LANServer.Client
{

    /// <summary>
    /// Define client
    /// </summary>
    public class AsynchClient
    {
        /// <summary>
        /// Used for writting and receiving from the GUI
        /// </summary>
        public static User GUI;

        /// <summary>
        /// Writting and receiving from the Server
        /// </summary>
        protected static User Server;

        /// <summary>
        /// Called if server grew
        /// </summary>
        public static event ChangedEventHandler GUISend;

        /// <summary>
        /// Called to clear GUI
        /// </summary>
        public static event ChangedEventHandler GUIClear;

        /// <summary>
        /// If joined valid server
        /// </summary>
        public static bool isJoined { get { return _isJoined; } }

        /// <summary>
        /// Is valid server IP address
        /// </summary>
        public static bool isServer { get { return _isServer; } }

        /// <summary>
        /// Should exit client
        /// </summary>
        public static bool isExit { get { return _isExit; } }

        /// <summary>
        /// Whether or not send should be query or write
        /// </summary>
        protected static bool isQuery = false;

        /// <summary>
        /// Stores Local IP Address
        /// </summary>
        protected static string sLocalIP = "";

        /// <summary>
        /// Username
        /// </summary>
        protected static string name = "";

        /// <summary>
        /// If debugging
        /// </summary>
        protected static bool isDebug = false;

        /// <summary>
        /// If joined server
        /// </summary>
        protected static bool _isJoined = false;

        /// <summary>
        /// If valid server IP Address
        /// </summary>
        protected static bool _isServer = false;

        /// <summary>
        /// Should exit application
        /// </summary>
        protected static bool _isExit = false;

        /// <summary>
        /// If in process of exiting
        /// </summary>
        protected static bool _isExiting = false;

        /// <summary>
        /// Client socket
        /// </summary>
        protected static Socket client;

        /// <summary>
        /// Endpoint of connection
        /// </summary>
        protected static IPEndPoint remoteEP;

        // ManualResetEvent instances signal completion
        private static ManualResetEvent connectdone;
        private static ManualResetEvent sendDone;
        private static ManualResetEvent receiveDone;

        // Done processing messages
        private static ManualResetEvent processServerDone;

        /// <summary>
        /// Initialize static variables
        /// </summary>
        static AsynchClient()
        {
            // If debugging
#if DEBUG
            // Set debugging
            isDebug = true;
#endif
        }

        // Start Client
        public static void StartClient()
        {
            // Initialize GUI
            GUI = new User("You", null);

            // Initialize server
            Server = new User("Server", null);

            // Initialize connectDone
            connectdone = new ManualResetEvent(false);
            // Initialize sendDone
            sendDone = new ManualResetEvent(false);
            // Initialize receiveDone
            receiveDone = new ManualResetEvent(false);
            // Initialize processServerDone
            processServerDone = new ManualResetEvent(false);

            // Hook send grew
            GUI.toSendGrew += new ChangedEventHandler(OnGUISend);

            // Hook receive grew
            GUI.toReceiveGrew += new ChangedEventHandler(OnGUIReceive);

            // Hook server receive grew
            Server.toReceiveGrew += new ChangedEventHandler(OnServerReceive);

            // Reset isServer and isJoint
            _isServer = false;
            _isJoined = false;

            // Join a server
            Join();
        }

        /// <summary>
        /// Join a server
        /// </summary>
        protected static void Join()
        {
            _isExit = false;

            while (!_isServer)
            {
                // Get Local IP Address
                sLocalIP = GetLocalIP();

                // Loop till name accepted
                while (!_isJoined)
                {
                    // Attempt to connect to remove device
                    try
                    {
                        // Get username
                        name = GetName();

                        // Establish connection to socket
                        IPHostEntry ipHostInfo = Dns.GetHostEntry(sLocalIP);

                        // Get Address
                        IPAddress ipAddress = LAN.GetLocal(ipHostInfo);

                        // Get remote endpoint (Ipaddress and port)
                        remoteEP = new IPEndPoint(ipAddress, Values.IntroPort);

                        // Create TCP/IP socket
                        client = new Socket(AddressFamily.InterNetwork,
                            SocketType.Stream, ProtocolType.Tcp);

                        // Reset connection
                        connectdone.Reset();

                        // Connect to remove endpoint
                        client.BeginConnect(remoteEP,
                            new AsyncCallback(ConnectCallback), client);

                        // Lock thread until connected
                        connectdone.WaitOne();

                        // If invalid server
                        if (!_isServer)
                        {
                            // Stop attempting connection
                            // Request new IP
                            break;
                        }

                        // Request to joint server
                        string joinRequest = Strings.command_Join + " " + name;

                        // Send to remote device
                        Server.ToSendIn(joinRequest);
                        Send(client);
                        // Lock thread until sent
                        sendDone.WaitOne();

                        // Recieve response from remote device

                        // Create state object
                        StateObject state = new StateObject();
                        // Assign socket
                        state.workSocket = client;

                        // Begin recieving data from remote device
                        client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                            new AsyncCallback(RecieveCallback), state);

                        // Lcck thread until data received
                        receiveDone.WaitOne();

                        // Wait for server queue to be processed
                        processServerDone.WaitOne(Values.serverPollDead);

                        // Handle response
                        if (_isJoined)
                        {
                            // Write Response
                            WriteGUI(name + " accepted to chat.");
                        }
                        else
                        {
                            // Write rejected
                            WriteGUI(name + " rejected.");

                            // retry join connection loop
                        }

                    }
                    catch (SocketException e)
                    {
                        // Alert user
                        WriteGUI("Cannot connect to server at: " + sLocalIP);
                        WriteGUI(e.ToString());

                        // Is not a valid server
                        _isServer = false;

                        // Break
                        break;
                    }
                    catch (Exception e)
                    {
                        // Write to console
                        WriteGUI(e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// Get Local IP Address
        /// </summary>
        /// <returns>String of IP address</returns>
        protected static string GetLocalIP()
        {
            // Loop till local ip
            while (true)
            {
                // Query user
                WriteGUI("Enter local IP address: ");

                // Get response
                string temp = ReadLine();

                // Attempt to get local IP
                try
                {
                    // Check temp is a string
                    if (String.IsNullOrEmpty(temp))
                        throw new ArgumentNullException("IP address", "Invalid IP Address");

                    // Get Host Info
                    IPHostEntry tempInfo = Dns.GetHostEntry(temp);

                    // Get Local IP
                    IPAddress tempLocalIP = LAN.GetLocal(tempInfo);

                    // Return ip address
                    return temp;
                }
                catch (ArgumentNullException e)
                {
                    // Write message
                    WriteGUI(e.Message);
                }
                catch (ArgumentException)
                {
                    // Write user
                    WriteGUI(String.Format("Invalid IP address: {0}", temp));
                }
                catch (Exception e)
                {
                    // Write exception
                    WriteGUI(e.ToString());
                }

            }
        }

        /// <summary>
        /// Get the username
        /// </summary>
        /// <returns>The username</returns>
        protected static string GetName()
        {
            // Loop for name
            while (true)
            {
                // Write to console
                WriteGUI("Enter a name: ");

                // Get name
                string temp = ReadLine();

                // If not empty
                if (temp != string.Empty)
                {
                    // Assign name
                    return temp;
                }
                else
                {
                    WriteGUI("Invalid name.");
                }
            }
        }

        /// <summary>
        /// Called to Write to GUI
        /// </summary>
        /// <param name="line">String ot send</param>
        public static void WriteGUI(string line)
        {
            // Write to GUI
            GUI.ToSendIn(line);
        }

        /// <summary>
        /// Called to Read from GUI
        /// </summary>
        /// <returns></returns>
        public static string ReadLine()
        {
            // Read from GUI
            return ClientForm.ReadLine();
        }

        /// <summary>
        /// Called to send message to GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected static void OnGUISend(object sender, EventArgs e)
        {
            // If not empty
            if (GUISend != null)
            {
                // Call event
                GUISend(sender, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Called to process messages from GUI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected static void OnGUIReceive(object sender, EventArgs e)
        {
            // Get messages
            string[] messages = GUI.ToReceiveGetMessages();

            // Loop through messages
            foreach (string message in messages)
            {
                // Process line
                if (message.StartsWith(Strings.command_Read))
                {
                    // Call update to server
                    Update();
                }
                else if (message.StartsWith(Strings.command_Write))
                {
                    // Write to server
                    Server.ToSendIn(message);
                }
                else if (message.StartsWith(Strings.command_Debug))
                {
                    // If debugging
                    if (isDebug)
                    {
                        // Write to gui
                        WriteGUI("Hiding debugging notes.");

                        // Hide debugging
                        isDebug = false;

                    }
                    else
                    {
                        // Write to gui
                        WriteGUI("Displaying debugging notes.");

                        // Show debugging
                        isDebug = true;
                    }
                }
                else if (message.StartsWith(Strings.command_Clear))
                {
                    // Clear console
                    Clear();
                }
                else if (message.StartsWith(Strings.command_Exit))
                {
                    // Send to server
                    Server.ToSendIn(message);

                    // Exit
                    Exit(true);
                }
                else
                {
                    // Default: write
                    Server.ToSendIn(message);
                }
            }
        }

        /// <summary>
        /// Receive a message from the server
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private static void OnServerReceive(object sender, EventArgs e)
        {
            // Get messages
            string[] messages = Server.ToReceiveGetMessages();

            // Loop through messages
            foreach (string message in messages)
            {
                // If sucessful join
                if (message == Strings.command_Join + " " + name)
                {
                    // Set joined server
                    _isJoined = true;

                    // Assuming no more messages
                    break;

                }
                else if (message.StartsWith(Strings.command_Encrypt))
                {
                    // Encrypt console
                    BeginEncrypt(message);
                }
                else if (message.StartsWith(Strings.command_Exit))
                {
                    // Server exited
                    Exit(true);
                }
                else
                {
                    // Write to GUI
                    GUI.ToSendIn(message);
                }
            }

            // Done processing server
            processServerDone.Set();
        }

        /// <summary>
        /// BeginEncryp messages
        /// </summary>
        /// <param name="message"></param>
        protected static void BeginEncrypt(string message)
        {
            // If requesting encryption
            if (message == Strings.command_Encrypt)
            {
                // Send encryption command
                // Request keys
                Server.ToSendIn(Strings.command_Encrypt);

                // Write to GUI
                WriteGUI("Requesting Encryption keys");
            }
            // Else has keys
            else
            {
                // Attempt to get keys
                try
                {
                    // Get keys as strings
                    string sCrypt = message.Substring(Strings.command_Encrypt.Length,
                        44);
                    string sAuth = message.Substring(Strings.command_Encrypt.Length +
                        sCrypt.Length);

                    // Convert to bytes
                    byte[] cryptKey = Convert.FromBase64String(sCrypt);
                    byte[] authKey = Convert.FromBase64String(sAuth);

                    // If debugging
                    if (isDebug)
                    {
                        // Write keys
                        WriteGUI(String.Format("Using crypt key: {0}",
                            BitConverter.ToString(cryptKey)));
                        WriteGUI(String.Format("Using auth key: {0}",
                            BitConverter.ToString(authKey)));
                    }

                    // Begin encryption
                    Server.BeginEncrypt(cryptKey, authKey);

                    // Write GUI
                    WriteGUI("Encrypted connection.");

                    // Remove keys
                    sCrypt = null;
                    sAuth = null;
                    cryptKey = null;
                    authKey = null;
                }
                // If improper string length
                catch (ArgumentOutOfRangeException e)
                {
                    WriteGUI(String.Format("Cannot read {0}", e.ParamName));
                }
                catch (FormatException e)
                {
                    WriteGUI(String.Format("Cannot convert {0} to x64 bas string", e.Source));
                }
                catch (Exception e)
                {
                    // Write to GUI
                    WriteGUI(e.ToString());
                }
            }

        }

        /// <summary>
        /// Clear console
        /// </summary>
        protected static void Clear()
        {
            // If not empty
            if (GUIClear != null)
            {
                // Call event
                GUIClear(null, null);
            }
        }

        /// <summary>
        /// Update client to match server
        /// </summary>
        public static IEnumerator Update()
        {
            while (!_isExit)
            {
                try
                {
                    // Create TCP/IP socket
                    client = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Reset connection
                    connectdone.Reset();

                    // Connect to remove endpoint
                    client.BeginConnect(remoteEP,
                        new AsyncCallback(ConnectCallback), client);

                    // Lock thread until connected
                    connectdone.WaitOne();

                    // If invalid server
                    if (!_isServer)
                    {
                        // Write to gui
                        WriteGUI("Cannot connect to server at: " + sLocalIP);

                        // End update. Return
                        yield break;
                    }

                    // Read server
                    Server.ToSendIn(Strings.command_Read);
                    Send(client);

                    // Lock thread until sent
                    sendDone.WaitOne();

                    // Recieve response from remote device

                    StateObject state = new StateObject();
                    // Assign socket
                    state.workSocket = client;

                    // Begin recieving data from remote device
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(RecieveCallback), state);

                    // Lcck thread until data received
                    receiveDone.WaitOne();

                    // Lock thread until data process
                    processServerDone.WaitOne(Values.serverPollDead);
                }
                catch (Exception e)
                {
                    WriteGUI(e.ToString());
                }

                // If exiting
                if (_isExiting)
                {
                    // Set flags
                    _isExit = true;
                    _isExiting = false;
                    _isServer = false;
                }
                else
                {
                    // Wait for next update
                    yield return null;
                }
            }

           // Release socket
           client.Shutdown(SocketShutdown.Both);
           client.Close();

            // Return
            yield break;

        }

        /// <summary>
        /// Connect to server
        /// </summary>
        /// <param name="ar">Client socket</param>
        private static void ConnectCallback(IAsyncResult ar)
        {
            // Attempt to connect to server
            try
            {
                // Retrieve socket (client)
                Socket client = (Socket)ar.AsyncState;

                // Complete connection
                client.EndConnect(ar);

                // Write to client
                //ClientForm.WriteLine("Socket connected to " +
                //    client.RemoteEndPoint.ToString());

                // Is valid server
                _isServer = true;

                // Signal connection made
                connectdone.Set();
            }
            // Catch exceptions
            catch (Exception)
            {
                // Display exception
                WriteGUI("Failed ot connect to: " + sLocalIP);

                // Not valid server
                _isServer = false;

                // Failed to connect
                // Done attempting connection
                connectdone.Set();
            }
        }


        private static void RecieveCallback(IAsyncResult ar)
        {
            // Attempt to read from socket
            try
            {
                // Get State object
                StateObject state = (StateObject)ar.AsyncState;

                // Retrieve underlying socket
                Socket client = state.workSocket;

                // Read bytes
                int bytesRead = client.EndReceive(ar);

                // If some data was read
                if (bytesRead > 0)
                {
                    // Store data recieved so far
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                    // Get rest of data
                    // Continue to call RecieveCallback until complete
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(RecieveCallback), state);
                }
                // No data read. All data received.
                else
                {

                    // If the buffer contains data
                    if (state.sb.Length > 1)
                    {
                        // Get content
                        string content = state.sb.ToString();

                        // If debugging
                        if (isDebug)
                        {
                            // End string. Write to console
                            WriteGUI("Read " + content.Length +
                                " bytes from socket. \n Data: " + content);
                        }

                        // Convert to messages
                        string[] messages = LAN.SockToMessage(state.sb.ToString());

                        // If not empty, receive messages
                        if (messages.Length != 0 && messages != null)
                            Server.ToReceiveIn(messages);

                    }
                    // Signal done
                    receiveDone.Set();
                }
            }
            // Exception receiving data
            catch (Exception e)
            {
                // Write to console
                WriteGUI(e.ToString());
            }
        }

        /// <summary>
        /// Send message to remote device. Appends EOF tag
        /// </summary>
        /// <param name="client">Remote device socket</param>
        private static void Send(Socket client)
        {
            // Convert messages to string
            string data = LAN.MessageToSock(Server.ToSendGetMessages());

            // Convert string to byte using ASCII
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending data to remote device
            client.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), client);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallback(IAsyncResult ar)
        {
            // Attempt to get bytes sent to server
            try
            {
                // Get socket
                Socket client = (Socket)ar.AsyncState;

                // Complete send data to remote device
                int bytesSent = client.EndSend(ar);

                // If debugging
                if (isDebug)
                {
                    // Write bytes sent
                    WriteGUI("Sent " + bytesSent +
                        " bytes to server.");
                }

                // Signal all bytes sent
                sendDone.Set();
            }
            // Catch exception
            catch (Exception e)
            {
                // Write to console
                WriteGUI(e.ToString());
            }
        }

        /// <summary>
        /// Should exit application
        /// </summary>
        /// <param name="exit"></param>
        /// <returns></returns>
        public static void Exit(bool exit)
        {
            // Set exiting
            _isExiting = exit;
        }
    }
}