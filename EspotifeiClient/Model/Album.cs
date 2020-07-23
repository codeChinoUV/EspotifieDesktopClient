using System;
using System.Windows.Media.Imaging;

namespace Model
{
    public class Album
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public string anio_lanzamiento { get; set; }

        public BitmapImage PortadaImagen { get; set; }

        public float duracion_total { get; set; }

        public string duracion
        {
            get
            {
                var time = TimeSpan.FromSeconds(duracion_total);
                return time.ToString("hh':'mm':'ss");
            }
        }


    }
    
}