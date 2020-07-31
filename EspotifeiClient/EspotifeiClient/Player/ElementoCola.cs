using Model;

namespace EspotifeiClient.Player
{
    public class ElementoCola
    {
        public Cancion Cancion { get; set; }

        public CancionPersonal CancionPersonal { get; set; }

        public CancionSinConexion CancionSinConexion { get; set; }

        public int Posicion { get; set; }

        public bool YaSeReproducio { get; set; }
    }
}