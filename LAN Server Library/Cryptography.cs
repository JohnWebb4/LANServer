using System;
using System.Text;
using System.Security.Cryptography;
using System.IO;


namespace LANServer
{
    /// <summary>
    /// 
    /// </summary>
    public class Cryptography
    {
        /// <summary>
        /// Random number generator
        /// </summary>
        private static readonly RandomNumberGenerator Random = RandomNumberGenerator.Create();


        public static readonly int BlockBitSize = 128;

        /// <summary>
        /// Key size
        /// </summary>
        public static readonly int KeyBitSize = 256;

        /// <summary>
        /// Generate a new key
        /// </summary>
        /// <returns>Key</returns>
        public static byte[] NewKey()
        {
            // Declare key
            byte[] key = new byte[KeyBitSize / 8];

            // Initialize eky to random values
            Random.GetBytes(key);

            // return key
            return key;
        }

        /// <summary>
        /// AES Encryption using HMAC authenticaion on UTF8 message
        /// </summary>
        /// <param name="message">Message</param>
        /// <param name="cryptKey">Crypt key</param>
        /// <param name="authKey">Authentication key</param>
        /// <param name="nonSecretPayload">(Optional) non-secret payload</param>
        /// <returns>Encrypted message</returns>
        /// <exception cref="ArgumentException">Message empty</exception>"
        /// <exception cref="ArgumentNullException">Crypt key or Auth key are null</exception>
        public static string SimpleEncrypt(string message, byte[] cryptKey, byte[] authKey,
            byte[] nonSecretPayload = null)
        {
            // If null arguments
            if (String.IsNullOrEmpty(message))
                throw new ArgumentException("Secret message", "message");

            // Convert to UTF-8 byte array
            byte[] plain = Encoding.UTF8.GetBytes(message);

            // Encrypt to byte array
            byte[] encrypt = SimpleEncrypt(plain, cryptKey, authKey, nonSecretPayload);

            // Return encrypted string
            return Convert.ToBase64String(encrypt);

        }

        /// <summary>
        /// AES Encryption using HMAC authent. on UTF-8.
        /// </summary>
        /// <param name="plain">Plain message</param>
        /// <param name="cryptKey">Crypt key</param>
        /// <param name="authKey">Auth key</param>
        /// <param name="nonSecretPayload">Non-Secret payload</param>
        /// <returns>Encrypted Message</returns>
        /// <exception cref="ArgumentException">Argument error</exception>
        private static byte[] SimpleEncrypt(byte[] plain, byte[] cryptKey,
            byte[] authKey, byte[] nonSecretPayload = null)
        {
            // Check arguments null
            if (plain == null || plain.Length < 1)
                throw new ArgumentException("Need message", "message");
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(string.Format("Key must be {0} bits", KeyBitSize / 8),
                    "cryptKey");
            if (authKey == null || authKey.Length != KeyBitSize / 8)
                throw new ArgumentException(string.Format("Key must be {0} bits", KeyBitSize / 8),
                    "authkey");

            // Non-secret payload optional
            // If defined left, else right
            nonSecretPayload = nonSecretPayload ?? new byte[] { };

            // Holds encrypted text
            byte[] encrypted;
            // Holds iv
            byte[] iv;

            // Create AES managed
            using (AesManaged aes = new AesManaged
            {
                KeySize = KeyBitSize,
                BlockSize = BlockBitSize,
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
            {
                // Make IV
                aes.GenerateIV();
                iv = aes.IV;

                // Create crypto transform and memory stream
                using (ICryptoTransform encryptor = aes.CreateEncryptor(cryptKey, iv))
                using (MemoryStream cipherStream = new MemoryStream())
                {
                    // Create crypto stream and binary writer
                    using (CryptoStream csStream = new CryptoStream(cipherStream, encryptor, CryptoStreamMode.Write))
                    using (BinaryWriter bWriter = new BinaryWriter(csStream))
                    {
                        // Encrypt data
                        bWriter.Write(plain);
                    }

                    // Get encrypted
                    encrypted = cipherStream.ToArray();
                }
            }

            // Add authentication
            // Make MAC and memory stream
            using (HMACSHA256 hmac = new HMACSHA256(authKey))
            using (MemoryStream encryptedStream = new MemoryStream())
            {
                // Make binary writer
                using (BinaryWriter bWriter = new BinaryWriter(encryptedStream))
                {
                    // Write non-secret payload
                    bWriter.Write(nonSecretPayload);

                    // Write iv
                    bWriter.Write(iv);

                    // Write text
                    bWriter.Write(encrypted);

                    // Flusch
                    bWriter.Flush();

                    // Authenticate
                    byte[] tag = hmac.ComputeHash(encryptedStream.ToArray());

                    // Write tag
                    bWriter.Write(tag);
                }

                // Return memory stream to byte array
                return encryptedStream.ToArray();
            }
            
        }

        /// <summary>
        /// Decrypt message
        /// </summary>
        /// <param name="message">Message to decrypt</param>
        /// <param name="cryptKey">Crypt key</param>
        /// <param name="authkey">Auth key</param>
        /// <param name="nonSecretPayloadLength">Non-secret payload</param>
        /// <returns>Plain message</returns>
        /// <exception cref="ArgumentException">Needs encrypted message</exception>"
        public static string SimpleDecrypt(string message, byte[] cryptKey,
            byte[] authkey, int nonSecretPayloadLength = 0)
        {
            // If null arguments
            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Encrypted message required", "message");

            // Convert message to bytes
            byte[] encrypted = Convert.FromBase64String(message);

            // Decrypt to byte array
            byte[] plain = SimpleDecrypt(encrypted, cryptKey, authkey, nonSecretPayloadLength);

            // If null return null, else return UTF-8 string
            return plain == null ? null : Encoding.UTF8.GetString(plain);
        }

        /// <summary>
        /// Decrypt message
        /// </summary>
        /// <param name="encrypted">Message to decrypt</param>
        /// <param name="cryptKey">Crypt key</param>
        /// <param name="authkey">Auth key</param>
        /// <param name="nonSecretPayloadLength">Non-secret payload</param>
        /// <returns>Plain message</returns>
        /// <exception cref="ArgumentException">Argument error</exception>"
        private static byte[] SimpleDecrypt(byte[] encrypted, byte[] cryptKey,
            byte[] authkey, int nonSecretPayloadLength = 0)
        {
            // If arguments null
            if (encrypted == null || encrypted.Length == 0)
                throw new ArgumentException("Message required", "message");
            if (cryptKey == null || cryptKey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("CryptKey key must be {0} bits", KeyBitSize / 8), "cryptKey");
            if (authkey == null || authkey.Length != KeyBitSize / 8)
                throw new ArgumentException(String.Format("Auth key must be {0} bits.", KeyBitSize / 8), "authKey");

            // Create key
            using (var hmac = new HMACSHA256(authkey))
            {
                // Get tags
                // Get sent tag
                byte[] sentTag = new byte[hmac.HashSize / 8];
                // Calc tag
                byte[] calcTag = hmac.ComputeHash(encrypted, 0, encrypted.Length - sentTag.Length);
                // Get IV length
                int ivLength = (BlockBitSize / 8);

                // If too small
                if (encrypted.Length < sentTag.Length + nonSecretPayloadLength + ivLength)
                    return null;

                // Get sent tag
                Array.Copy(encrypted, encrypted.Length - sentTag.Length, sentTag, 0, sentTag.Length);

                // Compare tag
                int compare = 0;
                for (int i = 0; i < sentTag.Length; i++)
                    compare |= sentTag[i] ^ calcTag[i];

                // If authentication failed
                if (compare != 0)
                    return null;

                // Create AES managed
                using (AesManaged aes = new AesManaged
                {
                    KeySize = KeyBitSize,
                    BlockSize = BlockBitSize,
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7
                })
                {
                    // Get IV
                    byte[] iv = new byte[ivLength];
                    Array.Copy(encrypted, nonSecretPayloadLength, iv, 0, iv.Length);

                    using (ICryptoTransform decryptor = aes.CreateDecryptor(cryptKey, iv))
                    using (MemoryStream plainStream = new MemoryStream())
                    {
                        using (CryptoStream csStream = new CryptoStream(plainStream, decryptor, CryptoStreamMode.Write))
                        using (BinaryWriter bWriter = new BinaryWriter(csStream))
                        {
                            // Decrypt
                            bWriter.Write(encrypted, nonSecretPayloadLength + iv.Length,
                                encrypted.Length - nonSecretPayloadLength - iv.Length - sentTag.Length);
                        }

                        // Return plain text
                        return plainStream.ToArray();
                    }
                }
            }
        }


    }
}
