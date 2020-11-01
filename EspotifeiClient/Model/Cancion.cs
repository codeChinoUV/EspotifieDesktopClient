using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject]
    [Serializable]
    public class Cancion
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public float duracion { get; set; }

        public int cantidad_de_reproducciones { get; set; }

        public float calificacion_promedio { get; set; }

        public List<Genero> generos { get; set; }

        public List<CreadorContenido> creadores_de_contenido { get; set; }

        public string artistas
        {
            get
            {
                var creadoresDeContenido = "";
                if (creadores_de_contenido != null)
                {
                    foreach (var creadorContenido in creadores_de_contenido)
                        creadoresDeContenido += $"{creadorContenido.nombre}, ";
                    if (creadoresDeContenido != "")
                        creadoresDeContenido = creadoresDeContenido.Substring(0, creadoresDeContenido.Length - 2);
                }

                return creadoresDeContenido;
            }
        }

        public string duracion_formateada
        {
            get
            {
                var time = TimeSpan.FromSeconds(duracion);
                return time.ToString("mm':'ss");
            }
        }

        public string calificacion_promedio_formateada => calificacion_promedio.ToString("0.0");

        public Album album { get; set; }
    }
}