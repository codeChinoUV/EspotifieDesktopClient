using System;
using System.Collections.Generic;
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

        public List<Cancion> canciones { get; set; }

        public string duracion
        {
            get
            {
                var duracionString = "00:00:00";
                float duracionSegundos = 0;
                if (canciones != null)
                {
                    foreach (var cancion in canciones)
                    {
                        duracionSegundos += cancion.duracion;
                    }
                    var time = TimeSpan.FromSeconds(duracionSegundos);
                    duracionString = time.ToString("hh':'mm':'ss");
                }

                return duracionString;
            }
        }
    }
}