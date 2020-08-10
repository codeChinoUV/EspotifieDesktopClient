using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Model
{
    public class ListaReproduccion
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public string descripcion { get; set; }

        public int duracion_total { get; set; }
        
        public string duracion
        {
            get
            {
                var duracionString = "00:00:00";
                var time = TimeSpan.FromSeconds(duracion_total);
                duracionString = time.ToString("hh':'mm':'ss");
                return duracionString;
            }
        }

        public BitmapImage PortadaImagen { get; set; }

        public List<Cancion> canciones { get; set; }
    }
}