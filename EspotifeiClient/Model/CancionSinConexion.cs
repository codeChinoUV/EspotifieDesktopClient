using System;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject]
    [Serializable]
    public class CancionSinConexion : Cancion
    {
        public enum EstadoDescarga
        {
            Espera,
            Descargando,
            Descargado,
            Error
        }
        
        public string ruta_cancion { get; set; }
        
        public string ruta_imagen { get; set; }
        
        public EstadoDescarga estado_descarga { get; set; }

    }
}