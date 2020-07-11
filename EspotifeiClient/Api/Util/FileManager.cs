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

            var sha256 = "";
            if (File.Exists(filePath))
            {
                SHA256 mySHA256 = SHA256Managed.Create();

                var fileStream = new FileStream(filePath, FileMode.Open);

                fileStream.Position = 0;

                byte[] hashValue = mySHA256.ComputeHash(fileStream);

                sha256 = BitConverter.ToString(hashValue).Replace("-", String.Empty);

                fileStream.Close();  
            }

            return sha256;
        }


    }
}