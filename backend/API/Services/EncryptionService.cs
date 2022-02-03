using System.Security.Cryptography;
using System.Text;

namespace Skocko.Api.Services
{
    /// <summary>
    /// https://gist.github.com/davidsheardown/6781a4c45eaf85917392678d7c3993d6
    /// </summary>
    public class EncryptionService
    {
        public string EncryptString(string text, string keyString)
        {
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var encryptor = aesAlg.CreateEncryptor(key, aesAlg.IV))
                {
                    using (var msEncrypt = new MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        using (var swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(text);
                        }

                        var iv = aesAlg.IV;

                        var decryptedContent = msEncrypt.ToArray();

                        var result = new byte[iv.Length + decryptedContent.Length];

                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(decryptedContent, 0, result, iv.Length, decryptedContent.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }

        public string DecryptString(string cipherText, string keyString)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            var iv = new byte[16];
            var cipher = new byte[16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, iv.Length);
            var key = Encoding.UTF8.GetBytes(keyString);

            using (var aesAlg = Aes.Create())
            {
                using (var decryptor = aesAlg.CreateDecryptor(key, iv))
                {
                    string result;
                    using (var msDecrypt = new MemoryStream(cipher))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                result = srDecrypt.ReadToEnd();
                            }
                        }
                    }

                    return result;
                }
            }
        }

        public string? HashString(string? text, string hashKey)
        {
            if (text == null)
            {
                return null;
            }

            var key = System.Text.Encoding.UTF8.GetBytes(hashKey);

            using var myHash = new System.Security.Cryptography.HMACSHA256(key);
            /*
             * 2. Invoke the ComputeHash method by passing 
                  a byte array. 
             *    Just remember, you can pass any raw data, 
                  and you need to convert that raw data 
                  into a byte array.
             */
            var byteArrayResultOfRawData =
                  Encoding.UTF8.GetBytes(text);

            /*
             * 3. The ComputeHash method, after a successful 
                  execution it will return a byte array, 
             *    and you should store that in a variable. 
             */

            var byteArrayResult =
                 myHash.ComputeHash(byteArrayResultOfRawData);

            /*
             * 4. After the successful execution of ComputeHash, 
                  you can then convert 
                  the byte array into a string. 
             */

            return string.Concat(Array.ConvertAll(byteArrayResult, h => h.ToString("X2")));
        }
    }
}
