using System.Windows.Media.Imaging;

namespace Model
{
    public class ListaReproduccion
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public string descripcion { get; set; }

        public int duracion_total { get; set; }

        public BitmapImage PortadaImagen { get; set; }
    }
}
