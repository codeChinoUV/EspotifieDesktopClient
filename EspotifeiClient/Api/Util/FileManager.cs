using System;
using System.IO;
using System.Security.Cryptography;

namespace Api.Util
{
    public class FileManager
    {
        public static byte[] SubArray(byte[] data, int index, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
        
        public static string Sha256CheckSum(string filePath)
        {
            if (File.Exists(filePath))
            {
                using (SHA256 sha256 = SHA256Managed.Create())
                {
                    using (FileStream fileStream = File.OpenRead(filePath))
                        return Convert.ToBase64String(sha256.ComputeHash(fileStream));
                }    
            }

            return "";
        }


    }
}