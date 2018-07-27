using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

namespace LANServer.Server
{
    /// <summary>
    /// An asynchronous server
    /// </summary>
    public class AsynchServer
    {
        /// <summary>
        /// When server is all done
        /// </summary>
        public static ManualResetEvent allDone;

        /// <summary>
        /// Called if server queue grew
        /// </summary>
        public static event ChangedEventHandler ServerGrew;

        /// <summary>
        /// Called to clear server
        /// </summary>
        public static event ChangedEventHandler ServerClear;

        /// <summary>
        /// Messages to send to server console
        /// </summary>
        public static User GUI;

        /// <summary>
        /// All users connected to server
        /// </summary>
        protected static List<User> clients;

        /// <summary>
        /// Accept new clients
        /// </summary>
        protected static bool isPublic = true;

        /// <summary>
        /// Exit server
        /// </summary>
        protected static bool isExit = false;

        /// <summary>
        /// If debugging
        /// </summary>
        protected static bool isDebug = false;

        /// <summary>
        /// Done processing client
        /// </summary>
        private static ManualResetEvent processClientDone;

        /// <summary>
        /// Empty constructor
        /// </summary>
        public AsynchServer() { }

        /// <summary>
        /// Initialize AsycnServer
        /// </summary>
        static AsynchServer()
        {
            // If debugging
#if DEBUG
            // Set debugging
            isDebug = true;
#endif

            // initialize allDone
            allDone = new ManualResetEvent(false);

            // Initialize clients
            clients = new List<User>();

            // Initialize GUI
            GUI = new User("Server", null);

            // Initialize process client
            processClientDone = new ManualResetEvent(false);

            // Hook server grew
            GUI.toSendGrew += new ChangedEventHandler(OnGUISend);

            // Hook to process grew
            GUI.toReceiveGrew += new ChangedEventHandler(OnGUIReceive);
        }

        /// <summary>
        /// Start listening
        /// </summary>
        public static void StartListening()
        {
            // Data buffer
            byte[] bytes = new Byte[1024];

            // Establish local endpoint for socket
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

            // Declare IP Address
            IPAddress ipAddress = LAN.GetLocal(ipHostInfo);

            // Write IP Address
            WriteGUI("Connecting to: " + ipAddress.ToString());

            // Create end-point
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, Values.IntroPort);

            // Create socket
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind socket to end-point and listen for incoming connection
            try
            {
                // Bind to end-point
                listener.Bind(localEndPoint);
                // Listen for 
                listener.Listen(100);

                // Open socket. Wait for connection
                WriteGUI("Waiting for a connection . . .");

                // While is alive
                while (!isExit)
                {
                    // Reeset connection
                    allDone.Reset();

                    // Accept connection
                    listener.BeginAccept(
                        new AsyncCallback(AcceptCallback),
                        listener);

                    // Lock thread till timeout.
                    // Poll dead connection
                    if (!allDone.WaitOne(Values.serverPollDead))
                    {
                        // Check dead connection to client for more than a second
                        bool deadConnection = listener.Poll(1000, SelectMode.SelectRead);
                        // Check no data read
                        bool noData = (listener.Available == 0);

                        // If both, ungracefull disconnect
                        if (deadConnection && noData)
                        {
                            // Write to server
                            WriteGUI("Dead connection at.");

                            // Retry
                            continue;
                        }
                    }
                }
            }
            // If exception
            catch (ArgumentNullException e)
            {
                // Write to console
                WriteGUI(e.ToString());
            }
            catch(SocketException)
            {
                WriteGUI(String.Format("Cannot connect to: {0}:{1}", ipAddress, Values.IntroPort));
            }

            // Write server closed
            WriteGUI("\nServer closed.");
            // Write press enter to close
            WriteGUI("\nPress ENTER to close application. . . .");
            ReadLine();
        }

        /// <summary>
        /// Write line to GUI
        /// </summary>
        /// <param name="line">String to write</param>
        public static void WriteGUI(string line)
        {
            // Send line to send to GUI
            GUI.ToSendIn(line);
        }

        /// <summary>
        /// Write line to all clients and GUI
        /// </summary>
        /// <param name="line">String to write</param>
        public static void WriteAll(string line)
        {
            // Write to GUI
            WriteGUI(line);

            // Loop through clients
            for (int i = 0; i < clients.Count; i++)
            {
                // Copy client
                User client = clients[i];

                // Write to client
                WriteUser(ref client, line);

                // Assign client
                clients[i] = client;
            }
        }

        /// <summary>
        /// Write a specific user
        /// </summary>
        /// <param name="client"
        /// <param name="line">String message</param>
        public static void WriteUser(ref User client, string line)
        {
            // Send to client
            client.ToSendIn(line);
        }

        /// <summary>
        /// Read line from GUI
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">
        /// Called if cannot readline from GUI</exception>
        protected static string ReadLine()
        {
            // Wait to readline
            return ServerForm.ReadLine();
        }

        /// <summary>
        /// Sort names based on lenght and then alphabetical
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int CompareClientsByName(User a, User b)
        {
            // Compare name
            if (a.name.Length > b.name.Length)
            {
                // a > b
                return -1;
            }
            else if (a.name.Length == b.name.Length)
            {
                // Compare alphabetical and return
                return String.Compare(a.name, b.name);
            }
            else
            {
                // a < b
                return 1;
            }
        }

        /// <summary>
        /// Handle client connection
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptCallback(IAsyncResult ar)
        {
            // If exiting, don't accept connection
            if (isExit)
            {
                // Return
                return;
            }

            // Signal done
            allDone.Set();

            // Get socket to handle request
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Get remote IP
            string remoteIP = (handler.RemoteEndPoint as IPEndPoint).Address.ToString();

            // If closed server
            if (!isPublic)
            {
                // Is client
                bool isClient = false;

                // Check remote IP against current clients
                // Loop through clients
                foreach(User client in clients)
                {
                    // If client
                    if (client.address == remoteIP)
                    {
                        // If client
                        isClient = true;

                        // Break search
                        break;
                    }
                }

                // If not a client on closed server, reject
                if (!isClient)
                {
                    // Reject
                    return;
                }
            }

            // Create state object
            StateObject state = new StateObject();

            // Populate state object
            state.workSocket = handler;

            // Read from client
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        /// <summary>
        /// Read from client
        /// </summary>
        /// <param name="ar">Socket</param>
        public static void ReadCallback(IAsyncResult ar)
        {
            // Initialize content to empty
            String content = String.Empty;

            // Get State Object
            StateObject state = (StateObject)ar.AsyncState;
            // Get socket
            Socket handler = state.workSocket;

            // Get endpoint ip
            string remoteIP = (handler.RemoteEndPoint as IPEndPoint).Address.ToString();

            // Get bytes received
            int bytesRead = handler.EndReceive(ar);

            // If bytes read
            if (bytesRead > 0)
            {
                // Store bytes received
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Get string so far
                content = state.sb.ToString();

                // If contains eof tag
                if (content.IndexOf(Strings.command_EOF) > -1)
                {
                    // If debugging
                    if (isDebug)
                    {
                        // End string. Write to console
                        WriteGUI("Read " + content.Length +
                            " bytes from socket. \n Data: " + content);
                    }

                    // Convert to messages
                    string[] messages = LAN.SockToMessage(content);

                    // If client
                    bool isClient = false;

                    // Cycle through clients
                    foreach (User client in clients)
                    {
                        // Check IPAddress
                        if (client.address == remoteIP)
                        {
                            // Is client
                            isClient = true;

                            // Reset process client
                            processClientDone.Reset();

                            // Receive messages
                            client.ToReceiveIn(messages);

                            // Wait to process client
                            processClientDone.WaitOne();

                            // Send pending messages
                            Send(handler,
                                LAN.MessageToSock(client.ToSendGetMessages()));

                            // Break
                            break;
                        }
                    }

                    // If not a client
                    if (!isClient)
                    {
                        // If join
                        if (messages[0].StartsWith(Strings.command_Join))
                        {
                            // Get name
                            string name = messages[0].Substring(Strings.command_Join.Length);

                            // Remove initial and end spaces
                            name = name.Trim();

                            // if name is unique
                            bool isUnique = true;

                            // Check name against other client names
                            // Cycle through clients
                            foreach(User client in clients)
                            {
                                // If similar name
                                if (name.ToLower() == client.name.ToLower())
                                {
                                    // Is not a unique name
                                    isUnique = false;
                                }
                            }

                            // If unique name
                            if (isUnique)
                            {
                                // Create user
                                User client = NewClient(name, remoteIP);

                                // Echo data with keys
                                Send(handler, LAN.MessageToSock(messages[0]));

                                // Alert user joined
                                WriteAll(name + " has joined the chat from: " + remoteIP);

                                // Write to server
                                WriteGUI("Requesting encryption.");

                                // Request encryption
                                WriteUser(ref client, Strings.command_Encrypt);

                                // Add to clients
                                clients.Add(client);
                            }
                            // Else similar name
                            else
                            {
                                // Write to server
                                WriteGUI(remoteIP +
                                    " attempted to join with similar name: " +
                                    name);

                                // Request new name
                                Send(handler, Strings.command_EOF);
                            }
                        }
                    }
                }
                else
                {
                    // Continue to read
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                        new AsyncCallback(ReadCallback), state);
                }
            }
        }

        /// <summary>
        /// Initialize a new client.
        /// </summary>
        /// <param name="name">Username</param>
        /// <param name="address">IP4 address</param>
        protected static User NewClient(string name, string address)
        {
            // initailize a new client
            User user = new User(name, address);

            // Perform event hooks
            user.toReceiveGrew += new ChangedEventHandler(OnClientReceive);


            // Return
            return user;
        }

        /// <summary>
        /// Called if server queue grew
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected static void OnGUISend(object sender, EventArgs e)
        {
            // If not empty
            if (ServerGrew != null)
            {
                // Call event
                ServerGrew(sender, e);
            }
        }

        /// <summary>
        /// Process the to process queue
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event Arguments</param>
        protected static void OnGUIReceive(object sender, EventArgs e)
        {

            // Prefix of a message
            string prefix = GUI.name + ": ";

            // Get all messages
            string[] messages = GUI.ToReceiveGetMessages();

            // Loop through messages
            foreach(string message in messages)
            {
                // Copy message
                string line = message;

                // Process line
                // If write command
                if (line.StartsWith(Strings.command_Write))
                {
                    // Format line
                    line = prefix + line.Substring(Strings.command_Write.Length).Trim();

                    // Write to all
                    WriteAll(line);
                }
                // If server only
                else if (line.StartsWith(Strings.command_Server))
                {
                    // Change prefix
                    prefix = GUI.name + " ONLY: ";
                    // Format line
                    line = prefix + line.Substring(Strings.command_Server.Length).Trim();

                    // Write to GUI
                    WriteGUI(line);
                }
                else if (line == Strings.command_public)
                {
                    // Set is public true
                    isPublic = true;

                    // Write to server
                    WriteGUI("Made public server");
                }
                else if (line == Strings.command_private)
                {
                    // Set is public false
                    isPublic = false;

                    // Write to GUI
                    WriteGUI("Made private server");
                }
                else if (line == Strings.command_Exit)
                {
                    // Set is exit true
                    Exit(true);

                    // Write to server
                    WriteGUI("Exiting...");

                    // Write to all clients
                    WriteAll("/exit");
                }
                else if (line.StartsWith(Strings.command_Debug))
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
                else if (line.StartsWith(Strings.command_Clear))
                {
                    // Clear console
                    Clear();
                }
                else if (line.StartsWith(Strings.command_Users))
                {
                    // Display users to GUI only
                    // Write to GUI
                    WriteGUI("There are " + clients.Count + " user(s).");

                    // Cycle through clients
                    foreach(User client in clients)
                    {
                        // Write user and address (Server only)
                        WriteGUI(client.name + " at " + client.address);
                    }
                }
                else if (line.StartsWith(Strings.command_Whisper))
                {
                    // Whispher
                    Whisper(ref GUI, line);
                }
                else if (line.StartsWith(Strings.command_Help))
                {
                    // Write help
                    Help(ref GUI);
                    HelpServer(ref GUI);
                }
                else if (line.StartsWith(Strings.command_Encrypt))
                {
                    // Request encryption from all users
                    // Write GUI
                    WriteAll("Requesting new encryption from all users.");

                    // Cycle through clients
                    for(int i = 0; i < clients.Count; i++)
                    {
                        // Get client
                        User client = clients[i];

                        // Call Begin Encrypt
                        WriteUser(ref client, Strings.command_Encrypt);

                        // Assign to clients
                        clients[i] = client;
                    }
                }
                // Default (No command)
                else
                {
                    // Format line
                    line = prefix + line;

                    // Default Write
                    WriteAll(line);
                }
            }
        }

        /// <summary>
        /// Called if received message from client
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected static void OnClientReceive(object sender, EventArgs e)
        {
            // For each client on server
            for (int i = 0; i < clients.Count; i++)
            {
                // Get client
                User client = clients[i];

                // Get prefix of message
                string prefix = client.name + ": ";

                // Get messages
                string[] messages = client.ToReceiveGetMessages();

                // Loop through messages
                foreach(string message in messages)
                {
                    // Copy message
                    string line = message;

                    // Process line
                    if (line.StartsWith(Strings.command_Write))
                    {
                        //Format line
                        line = prefix + line.Substring(Strings.command_Write.Length).Trim();

                        // Write to all
                        WriteAll(line);
                    }
                    else if (line.StartsWith(Strings.command_Read))
                    {
                        // If debugging
                        if (isDebug)
                        {
                            // Format line
                            line = prefix + line;

                            // Write to gui
                            WriteGUI(line);
                        }
                    }
                    else if (line.StartsWith(Strings.command_Users))
                    {
                        // Display users to GUI only
                        // Write to GUI
                        WriteUser(ref client, "There are " + clients.Count + " user(s).");

                        // Cycle through clients
                        foreach (User user in clients)
                        {
                            // Write user and address (Server only)
                            WriteUser(ref client, user.name);
                        }
                    }
                    else if (line.StartsWith(Strings.command_Whisper))
                    {
                        // Process whisper
                        Whisper(ref client, line);
                    }
                    else if (line.StartsWith(Strings.command_Help))
                    {
                        // Write help to client
                        Help(ref client);
                    }
                    else if (line.StartsWith(Strings.command_Exit))
                    {
                        // Write
                        WriteAll(client.name + " has left.");

                        // Remove client
                        clients.Remove(client);

                        // Return till next update
                        return;
                    }
                    else if (line.StartsWith(Strings.command_Encrypt))
                    {
                        EncryptUser(ref client);
                    }
                    else
                    {
                        // Format line
                        line = prefix + line;

                        // Default broadcast to all
                        WriteAll(line);
                    }
                }

                // Finally assign client
                clients[i] = client;
            }

            // Done processing
            processClientDone.Set();
        }

        protected static void Whisper(ref User client, string line)
        {
            // If no users
            if (clients.Count == 0)
            {
                WriteGUI("No user connected.");
                return;
            }

            // Next string is username
            string temp = line.Substring(Strings.command_Whisper.Length);

            // Trim white-space characters
            temp = temp.Trim();

            // Sort by lenght and then alpabetical
            clients.Sort(CompareClientsByName);

            // Declare whisper target name
            string name = "";

            bool whispered = false;

            // Cycle through clients
            for (int j = 0; j < clients.Count; j++)
            {
                // If name matches
                if (temp.ToLower().Contains(clients[j].name.ToLower()))
                {
                    int iText = temp.ToLower().IndexOf(clients[j].name.ToLower());

                    // Get name
                    name = temp.Substring(0, iText);
                    // Trim whitespaces
                    name = name.Trim();

                    // Get message
                    line = temp.Substring(iText +
                        clients[j].name.Length);
                    // Trim whitespaces
                    line = line.Trim();

                    // Copy user
                    User user = clients[j];

                    // Write to user
                    WriteUser(ref user, client.name + " whispered: " + line);

                    // Assign client
                    clients[j] = user;

                    // Completed whisper
                    whispered = true;

                    // Break search
                    break;
                }
            }

            // If no user with name
            if (!whispered)
            {
                WriteGUI("Failed to whisper " + temp);
            }
        }

        /// <summary>
        /// Creates a new encryption key for the user
        /// </summary>
        /// <param name="user">User to encrypt</param>
        protected static void EncryptUser(ref User user)
        {
            // Write to GUI
            WriteGUI(String.Format("{0}: Requested a new encryption key.", user.name));

            // Make keys
            byte[] cryptoKey = Cryptography.NewKey();
            byte[] authKey = Cryptography.NewKey();

            // Get string representation of keys
            string sCrypt = Convert.ToBase64String(cryptoKey);
            string sAuth = Convert.ToBase64String(authKey);

            // If debugging
            if (isDebug)
            {
                // Write key and iv
                WriteGUI(String.Format("{0}: cryptKey: {1}.",
                    user.name, BitConverter.ToString(cryptoKey)));
                WriteGUI(String.Format("{0}: authKey: {1}.",
                    user.name, BitConverter.ToString(authKey)));

            }

            // Write keys
            WriteUser(ref user, Strings.command_Encrypt + sCrypt + sAuth);

            // End encryption
            user.EndEncrypt();

            // Begin encryption
            user.BeginEncrypt(cryptoKey, authKey);

            // Write GUI
            WriteGUI(String.Format("{0}: Is Encrypted.", user.name));
        }

        /// <summary>
        /// Write help message to user
        /// </summary>
        /// <param name="user"></param>
        protected static void Help(ref User user)
        {
            // Write help
            WriteUser(ref user, "The following are a list of server commands.");
            // Read command
            WriteUser(ref user, "- /read: Forces client to read from server.");
            // Write command
            WriteUser(ref user, "- /write [msg]: Writes msg to all users.");
            // Join command
            WriteUser(ref user, "- /join [name]: Attempts to join server using the name.");
            // Exit command
            WriteUser(ref user, "- /exit: Close the client or server.");
            // Debug command
            WriteUser(ref user, "- /debug: Write debug information to console.");
            // Clear command
            WriteUser(ref user, "- /clear: Clear console.");
            // User command
            WriteUser(ref user, "- /users: Get users on server.");
            // Whisper command
            WriteUser(ref user, "- /whisper [name] [msg]: Write msg to name");
            // Encrypt command
            WriteUser(ref user, "- /encrypt: Establishes a new encryption key.");
            // Help command
            WriteUser(ref user, "- /help: Write help information");

        }

        /// <summary>
        /// Write server help
        /// </summary>
        /// <param name="user"></param>
        protected static void HelpServer(ref User user)
        {
            WriteUser(ref user, "- /server [msg]: Write a message to only the server.");
            WriteUser(ref user, "- /public: make the server public. Will accept new clinets");
            WriteUser(ref user, "- /private: make the server private. Will not accept new clients.");
        }

        /// <summary>
        /// Called to clear server
        /// </summary>
        protected static void Clear()
        {
            // if not empty
            if( ServerClear != null)
            {
                // Call clear
                ServerClear(null, null);
            }
        }

        /// <summary>
        /// Send string to client
        /// </summary>
        /// <param name="handler">Socket handler</param>
        /// <param name="data">String to send</param>
        private static void Send(Socket handler, String data)
        {
            // Convert to bytes
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // If debugging
            if (isDebug)
            {
                StringBuilder message = new StringBuilder();

                // Append bytes data
                message.Append("Sending " + byteData.Length);

                // Get IP
                string remoteIP = (handler.RemoteEndPoint as IPEndPoint).Address.ToString();

                bool isClient = false;

                // Get client name
                // Loop through clients
                foreach(User client in clients)
                {
                    // If client
                    if (client.address == remoteIP)
                    {
                        // Append name
                        message.Append(" bytes to " + client.name + " at " + remoteIP + ".");
                        isClient = true;
                        break;
                    }
                }

                // If unknown
                if(!isClient)
                {
                    // Appent unknown and address
                    message.Append(" bytes to unknown at " + remoteIP + ".");
                }

                // Append data
                message.Append("\nData: " + data);

                // Write to GUI
                WriteGUI(message.ToString());
            }

            // Begin send
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        /// <summary>
        /// Send Callback
        /// </summary>
        /// <param name="ar">Result</param>
        private static void SendCallback(IAsyncResult ar)
        {
            // Attempt to send
            try
            {
                // Get socket
                Socket handler = (Socket)ar.AsyncState;

                // Get bytes sent
                int bytesSent = handler.EndSend(ar);

                if (isDebug)
                {
                    // Write bytes sent
                    WriteGUI("Sent " + bytesSent +
                        " bytes to client.");
                }

                // Shutdown handler
                handler.Shutdown(SocketShutdown.Both);
                // Close handler
                handler.Close();
            }
            catch(Exception e)
            {
                // Write to console
                WriteGUI(e.ToString());
            }
        }

        /// <summary>
        /// Exit form
        /// </summary>
        /// <param name="exit"></param>
        public static void Exit(bool exit)
        {
            // Set is exit
            isExit = exit;
        }
    }
}
