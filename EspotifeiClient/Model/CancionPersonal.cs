using System;

namespace Model
{
    public class CancionPersonal
    {
        public int id { get; set; }

        public string nombre { get; set; }

        public float duracion { get; set; }

        public string artistas { get; set; }

        public string album { get; set; }

        public string duracion_string
        {
            get
            {
                var time = TimeSpan.FromSeconds(duracion);
                return time.ToString("mm':'ss");
            }
        }
    }
}