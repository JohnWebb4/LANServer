namespace LANServer
{
    public class Strings
    {
        /// <summary>
        /// Denotes the end of file
        /// </summary>
        public const string command_EOF = "<EOF>";

        /// <summary>
        /// Denote end of message
        /// </summary>
        public const string command_EOM = "<EOM>";

        /// <summary>
        /// Call server to read update to client
        /// </summary>
        public const string command_Read = "/read";

        /// <summary>
        /// Call client to write to server
        /// </summary>
        public const string command_Write = "/write";

        /// <summary>
        /// Called to join server
        /// </summary>
        public const string command_Join = "/join";

        /// <summary>
        /// Denotes the text is server only
        /// </summary>
        public const string command_Server = "/server";

        /// <summary>
        /// Make public server
        /// </summary>
        public const string command_public = "/public";

        /// <summary>
        /// Make private server
        /// </summary>
        public const string command_private = "/private";

        /// <summary>
        /// Exit server
        /// </summary>
        public const string command_Exit = "/exit";

        /// <summary>
        /// Sets debugging notes
        /// </summary>
        public const string command_Debug = "/debug";

        /// <summary>
        /// Clear console
        /// </summary>
        public const string command_Clear = "/clear";

        /// <summary>
        /// Get all users
        /// </summary>
        public const string command_Users = "/users";

        /// <summary>
        /// Send message to a specific player
        /// </summary>
        public const string command_Whisper = "/whisper";

        /// <summary>
        /// Get help on commands
        /// </summary>
        public const string command_Help = "/help";

        /// <summary>
        /// Request new encryptions
        /// </summary>
        public const string command_Encrypt = "/encrypt";
    }
}
