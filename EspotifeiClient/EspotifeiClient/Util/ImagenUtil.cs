using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace EspotifeiClient.Util
{
    public class ImagenUtil
    {
        /// <summary>
        ///     Crea un BitmapImage a partir de un MemoryStream
        /// </summary>
        /// <param name="imagen">El MemroyStream que contiene la imagen</param>
        /// <returns>Un BitmapImage</returns>
        public static BitmapImage CrearBitmapDeMemory(MemoryStream imagen)
        {
            var bitmapImage = new BitmapImage();
            try
            {
                using (imagen)
                {
                    imagen.Position = 0;
                    bitmapImage.BeginInit();
                    bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.UriSource = null;
                    bitmapImage.StreamSource = imagen;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                }
            }
            catch (Exception)
            {
                bitmapImage = null;
            }
            return bitmapImage;
        }
    }
}