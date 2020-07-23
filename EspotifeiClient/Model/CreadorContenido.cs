using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace Model
{
    public class CreadorContenido
    {
        public int id { get; set; }
        
        public string nombre { get; set; }

        public string biografia { get; set; }

        public List<Genero> generos { get; set; }

        public bool es_grupo { get; set; }

        public BitmapImage PortadaImagen { get; set; }
        
        public List<Album> Albums { get; set; }
    }
}
