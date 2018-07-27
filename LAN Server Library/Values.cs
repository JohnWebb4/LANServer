using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LANServer
{
    /// <summary>
    /// Contains constant values
    /// </summary>
    public class Values
    {
        /// <summary>
        /// The introductory port
        /// </summary>
        public const int IntroPort = 11000;

        /// <summary>
        /// Number of lines in the console
        /// </summary>
        public const int consoleSize = 30;

        /// <summary>
        /// Time to poll Dead connection (ms)
        /// </summary>
        public const int serverPollDead = 5000; // ms
    }
}
