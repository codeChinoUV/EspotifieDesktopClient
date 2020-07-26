using System;
using System.Collections.Generic;

namespace Model
{
    public class Cancion
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public float duracion { get; set; }

        public int cantidad_de_reproducciones { get; set; }

        public float calificacion_promedio { get; set; }
        
        public List<Genero> generos { get; set; }
        
        public List<CreadorContenido> creadores_de_contenido { get; set; }

        public string duracionString
        {
            get
            {
                var time = TimeSpan.FromSeconds(duracion);
                return time.ToString("mm':'ss");
            }
        }

        public Album album { get; set; }
    }
}