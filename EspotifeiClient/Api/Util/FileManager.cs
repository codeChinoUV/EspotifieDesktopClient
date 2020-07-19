using System;
using System.IO;
using System.Text;
using SHA3.Net;

namespace Api.Util
{
    public class FileManager
    {
        public static byte[] SubArray(byte[] data, int index, int length)
        {
            var result = new byte[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static byte[] ByteArrayFromImageFile(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open);
            var image = new byte[Convert.ToInt32(fileStream.Length.ToString())];
            fileStream.Read(image, 0, image.Length);
            return image;
        }
    }
}