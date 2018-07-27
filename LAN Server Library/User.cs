using System;
using System.Net;
using System.Collections.Generic;

namespace LANServer
{
    /// <summary>
    /// Called on value changed
    /// </summary>
    /// <param name="sender">Sender</param>
    /// <param name="e">Event arguments</param>
    public delegate void ChangedEventHandler(object sender, EventArgs e);

    /// <summary>
    /// User class
    /// </summary>
    public class User
    {
        /// <summary>
        /// Called if messages to send to user grew
        /// </summary>
        public event ChangedEventHandler toSendGrew;

        /// <summary>
        /// Called if messages received from user grew
        /// </summary>
        public event ChangedEventHandler toReceiveGrew;

        /// <summary>
        /// User IP addresss
        /// </summary>
        public string address { get { return _address; } }

        /// <summary>
        /// User name
        /// </summary>
        public string name { get { return _name; } }

        /// <summary>
        /// Number of messages to send to user
        /// </summary>
        public int toSendCount { get { return toSend.Count; } }

        /// <summary>
        /// Number of to receive messages from user
        /// </summary>
        public int toReceiveCount { get { return toReceive.Count; } }



        /// <summary>
        /// User IP address
        /// </summary>
        protected string _address;

        /// <summary>
        /// User name
        /// </summary>
        protected string _name;

        /// <summary>
        /// Cryptography key
        /// </summary>
        protected byte[] cryptKey;

        /// <summary>
        /// Authentication key
        /// </summary>
        protected byte[] authKey;

        /// <summary>
        /// All messages to send to user
        /// </summary>
        protected Queue<string> toSend;

        /// <summary>
        /// All messages received from user
        /// </summary>
        protected Queue<string> toReceive;

        /// <summary>
        /// Initialzie a new user class
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="address">User IP Address</param>
        public User(string name, string address)
        {
            // Initialize values
            init(name, address);
        }

        /// <summary>
        /// Initialize a new user class
        /// </summary>
        /// <param name="user">User to copy</param>
        public User(User user)
        {
            // Initialize user
            init(user._name, user._address);
        }

        /// <summary>
        /// Initialize a user class
        /// </summary>
        /// <param name="name">User name</param>
        /// <param name="address">User IP address</param>
        protected void init(string name, string address)
        {
            // Initialize values
            this._name = name;
            this._address = address;
            this.toSend = new Queue<string>();
            this.toReceive = new Queue<string>();
        }

        /// <summary>
        /// Add message to send to user
        /// </summary>
        /// <param name="line">String line</param>
        /// <exception cref="ArgumentException">Line is null or empty</exception>"
        public void ToSendIn(string line)
        {
            // Check argument
            if (String.IsNullOrEmpty(line))
                throw new ArgumentException("Must specify line", "line");

            // If encryption keys specified
            if (cryptKey != null && authKey != null)
            {
                // Encrypt message
                line = Cryptography.SimpleEncrypt(line, cryptKey, authKey);
            }

            // Add to to send
            toSend.Enqueue(line);

            // Call to send grew event
            GrewToSend(this, EventArgs.Empty);
        }

        /// <summary>
        /// Write multiple lines to user
        /// </summary>
        /// <param name="lines">Lines to write</param>
        /// <exception cref="ArgumentException">Lines null or empty</exception>"
        public void ToSendIn(string[] lines)
        {
            // Check args
            if (lines.Length == 0 || lines == null)
                throw new ArgumentException("Must specify lines.", "lines");

            // If encryption keys specified
            if(cryptKey != null && authKey != null)
            {
                // Cycle through lines
                for(int i = 0; i < lines.Length; i++)
                {
                    // Encrypt line
                    lines[i] = Cryptography.SimpleEncrypt(lines[i], cryptKey, authKey);

                    // Write line
                    toSend.Enqueue(lines[i]);
                }
            }
            else
            {
                // Cycle through lines
                foreach(string line in lines)
                {
                    // Write line
                    toSend.Enqueue(line);
                }
            }

            // Call to send grew event
            GrewToSend(this, EventArgs.Empty);
        }

        /// <summary>
        /// Gets messages to send to user and clears queue.
        /// </summary>
        /// <returns>Messages</returns>
        public string[] ToSendGetMessages()
        {

            // Declare array of to send size
            string[] results = toSend.ToArray();

            // Clear toSend
            toSend.Clear();

            // return messages
            return results;
        }

        /// <summary>
        /// Add message received from user
        /// </summary>
        /// <param name="line">Line to add</param>
        /// <exception cref="ArgumentException">Line is null or empty</exception>"
        public void ToReceiveIn(string line)
        {
            // Check argument
            if (String.IsNullOrEmpty(line))
                throw new ArgumentException("Must speficy line", "line");

            // If encryption keys specified
            if (cryptKey != null && authKey != null)
            {
                try
                {
                    // Decrypt message
                    line = Cryptography.SimpleDecrypt(line, cryptKey, authKey);
                }
                catch(FormatException)
                {
                    // Line is not encrypted
                    line = 
                        String.Format("{0} attempted to send unencrypted: {1}",
                        name, line);
                }
            }

            // Add to receive
            toReceive.Enqueue(line);

            // Call to receive grew event
            GrewToReceive(this, EventArgs.Empty);
        }

        /// <summary>
        /// Receive multiple lines from user
        /// </summary>
        /// <param name="lines">Line to recieve</param>
        /// <exception cref="ArgumentException">Lines is null or empty</exception>"
        public void ToReceiveIn(string[] lines)
        {
            // Check argument
            if (lines.Length == 0 || lines == null)
                throw new ArgumentException("Must specify lines", "lines");

            // If encryption keys specified
            if (cryptKey != null && authKey != null)
            {
                // Cycle through lines
                for(int i = 0; i < lines.Length; i++)
                {
                    // Attempt decrypt
                    try
                    {
                        // Decrypt line
                        lines[i] = Cryptography.SimpleDecrypt(lines[i], cryptKey, authKey);
                    }
                    catch(FormatException)
                    {
                        // Write error
                        lines[i] = 
                            String.Format("{0} attempted to send unencrypted: {1}",
                            name, lines[i]);
                    }

                    // Write line
                    toReceive.Enqueue(lines[i]);
                }
            }
            // If not encrypted
            else
            {
                // Write lines
                // Cycle through liens
                foreach (string line in lines)
                {
                    // Write line
                    toReceive.Enqueue(line);
                }
            }

            // Call to receive grew
            GrewToReceive(this, EventArgs.Empty);
        }

        /// <summary>
        /// Get messages from user and clears queue.
        /// </summary>
        /// <returns>messages</returns>
        public string[] ToReceiveGetMessages()
        {
            // Convert to receive to string array
            string[] results = toReceive.ToArray();

            // Clear toReceive
            toReceive.Clear();

            // Return messages
            return results;
        }

        /// <summary>
        /// Begin encrypting messages
        /// </summary>
        /// <param name="cryptKey">Crypt key</param>
        /// <param name="authKey">Auth key</param>
        /// <exception cref="ArgumentException">
        /// Crypt key or auth key is/are null or wrong size
        /// </exception>"
        public void BeginEncrypt(byte[] cryptKey, byte[] authKey)
        {
            // Check arguments
            if (cryptKey == null || cryptKey.Length != Cryptography.KeyBitSize / 8)
                throw new ArgumentException(
                    String.Format("Cryptkey must be {0} bits", Cryptography.KeyBitSize / 8),
                    "cryptkey");
            if (authKey == null || authKey.Length != Cryptography.KeyBitSize / 8)
                throw new ArgumentException(
                    String.Format("Authkey must be {0} bits", Cryptography.KeyBitSize / 8),
                    "authkey");

            // Assign keys
            this.cryptKey = cryptKey;
            this.authKey = authKey;
        }

        /// <summary>
        /// Ends encryption. Flushes keys.
        /// </summary>
        public void EndEncrypt()
        {
            // Remove encryption and authentication
            cryptKey = null;
            authKey = null;
        }

        /// <summary>
        /// Call toSend grew event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected void GrewToSend(object sender, EventArgs e)
        {
            // If event not empty
            if (toSendGrew != null)
            {
                // Call event
                toSendGrew(sender, e);
            }
        }

        /// <summary>
        /// Call toReceive grew event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected void GrewToReceive(object sender, EventArgs e)
        {
            // If event not empty
            if (toReceiveGrew != null)
            {
                // Call event
                toReceiveGrew(this, EventArgs.Empty);
            }
        }
    }
}
