using Model;
using System;

namespace EspotifeiClient.Player
{
    public class ElementoCola
    {
        public Cancion Cancion { get; set; }

        public CancionPersonal CancionPersonal { get; set; }

        public CancionSinConexion CancionSinConexion { get; set; }

        public int Posicion { get; set; }

        public bool YaSeReproducio { get; set; }

        public float Duracion { 
            get
            {
                var duracion = 0.0f;
                if (Cancion != null)
                    duracion = Cancion.duracion;
                else if (CancionPersonal != null)
                    duracion = CancionPersonal.duracion;
                else if (CancionSinConexion != null)
                    duracion = CancionSinConexion.duracion;
                return duracion;
            } 
        }

        public string DuracionString { 
            get
            {
                var time = TimeSpan.FromSeconds(Duracion);
                return time.ToString("mm':'ss");
            }
        }

        public string Nombre { 
            get
            {
                var nombre = "";
                if (Cancion != null)
                    nombre = Cancion.nombre;
                else if (CancionPersonal != null)
                    nombre = CancionPersonal.nombre;
                else if (CancionSinConexion != null)
                    nombre = CancionSinConexion.nombre;
                return nombre;
            }
        }

        public string Artistas
        {
            get
            {
                var artistas = "";
                if (Cancion != null)
                {
                    if (Cancion.creadores_de_contenido != null)
                    {
                        foreach (var creadorContenido in Cancion.creadores_de_contenido)
                            artistas += $"{creadorContenido.nombre}, ";
                        if (artistas != "")
                            artistas = artistas.Substring(0, artistas.Length - 2);
                    }
                }
                else if (CancionPersonal != null)
                    artistas = CancionPersonal.artistas;
                else if (CancionSinConexion != null)
                {
                    if (CancionSinConexion.creadores_de_contenido != null)
                    {
                        foreach (var creadorContenido in CancionSinConexion.creadores_de_contenido)
                            artistas += $"{creadorContenido.nombre}, ";
                        if (artistas != "")
                            artistas = artistas.Substring(0, artistas.Length - 2);
                    }
                }
                return artistas;
            }
        }

        public string TipoCancion 
        { 
            get
            {
                var tipoCancion = "";
                if (Cancion != null)
                    tipoCancion = "Cancion";
                else if (CancionPersonal != null)
                    tipoCancion = "Cancion De Biblioteca Personal";
                else if (CancionSinConexion != null)
                    tipoCancion = "Cancion Sin Conexión";
                return tipoCancion;
            }
        }
    }
}