using System;
using System.Net.Sockets;
using System.Text;

namespace LANServer
{
    /// <summary>
    /// State object for recieving data
    /// </summary>
    public class StateObject
    {
        /// <summary>
        /// Client socket
        /// </summary>
        public Socket workSocket = null;

        /// <summary>
        /// Size of recive buffer
        /// </summary>
        public const int BufferSize = 256;

        /// <summary>
        /// Recieve buffer
        /// </summary>
        public byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// Recieved data string
        /// </summary>
        public StringBuilder sb = new StringBuilder();
    }
}
