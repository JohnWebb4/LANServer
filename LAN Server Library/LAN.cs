using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace LANServer
{

    /// <summary>
    /// Basic LAN communications
    /// </summary>
    public class LAN
    {
        /// <summary>
        /// Get Local IP Address
        /// </summary>
        /// <param name="ipHostInfo">Host name</param>
        /// <returns>Local Ip Address</returns>
        /// <exception cref="ArgumentException">Local IP Address not found</exception>"
        public static IPAddress GetLocal(IPHostEntry ipHostInfo)
        {
            // Cycle through address list
            foreach (IPAddress ip in ipHostInfo.AddressList)
            {
                // If IP address
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    // Check local
                    if (CheckLocal(ip))
                    {
                        // Assign ip address
                        return ip;
                    }
                }
            }
            throw new ArgumentException("Local IP Address not found.");
        }

        /// <summary>
        /// Check if IP address is local
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>True if local</returns>
        public static bool CheckLocal(IPAddress ip)
        {
            // Check if local and return
            return CheckLocal(ip.ToString());
        }

        /// <summary>
        /// Check if IP address is local
        /// </summary>
        /// <param name="ip">IP address</param>
        /// <returns>True if local</returns>
        /// <remarks>Does not catch OpenVPN and Hamachi</remarks>
        public static bool CheckLocal(string ip)
        {
            // Separate to parts
            string[] sIpParts = ip.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);

            int[] ipParts = new int[sIpParts.Length];

            // Convert to strings
            // Loop through strings
            for (int i = 0; i < sIpParts.Length; i++)
            {
                // Convert string
                ipParts[i] = int.Parse(sIpParts[i]);
            }

            // Check IP
            if (ipParts[0] == 10 ||
                (ipParts[0] == 192 && ipParts[1] == 168) ||
                (ipParts[0] == 172 && (ipParts[1] >= 16 && ipParts[1] <= 31)))
            {
                return true;
            }

            // Else return false
            return false;
        }

        /// <summary>
        /// Converts messages to string. 
        /// Does not append empty strings
        /// </summary>
        /// <param name="messages">All messages to send through socket</param>
        /// <returns>String to send through socket</returns>
        public static string MessageToSock(string[] messages)
        {
            // Declare result to empty string
            string result = String.Empty;

            // Loop through messages
            foreach(string message in messages)
            {
                // If empty string
                if (message == String.Empty)
                {
                    // Do not append
                    // Skip to next message
                    continue;
                }

                // Append message with eom tag
                result = result + message + Strings.command_EOM;
            }

            // Finally append EOF tag
            result = result + Strings.command_EOF;

            // Return result
            return result;


        }

        /// <summary>
        /// Converts a message to a string. 
        /// Returns EOF if empty string
        /// </summary>
        /// <param name="message">Message to send</param>
        /// <returns>String to send to socket</returns>
        public static string MessageToSock(string message)
        {
            // If empty
            if (message == String.Empty)
            {
                // Return EOF
                return Strings.command_EOF;
            }

            // Return message with EOM and EOF tags
            return message + Strings.command_EOM + Strings.command_EOF;
        }

        /// <summary>
        /// Converts socket string to messages
        /// </summary>
        /// <param name="messages">Socket string</param>
        /// <returns>End messages</returns>
        public static string[] SockToMessage(string messages)
        {
            // copy message to new string
            string input = messages;

            // Declare empty string list
            List<string> result = new List<string>();

            // Get end of first message
            int index = input.IndexOf(Strings.command_EOM);

            // Loop through messages
            while(index > -1)
            {
                // Get message
                string message = input.Substring(0, index);

                // Remove te EOM
                input = input.Substring(index + Strings.command_EOM.Length);

                // Add message
                result.Add(message);
                
                // Finally get next message
                index = input.IndexOf(Strings.command_EOM);
            }

            // End of messages
            // Return array of result
            return result.ToArray();
        }
    }
}
