using System;
using System.Collections.Generic;
using Model.Enum;
using Newtonsoft.Json;

namespace Model
{
    [JsonObject]
    [Serializable]
    public class Usuario
    {
        public string nombre_usuario { get; set; }

        public string nombre { get; set; }

        public string contrasena { get; set; }

        public string correo_electronico { get; set; }

        public TipoUsuario tipo_usuario { get; set; }
        
        public Login login { get; set; }
        
        public bool sesion_iniciada { get; set; }
        
        public List<CancionSinConexion> canciones_sin_conexion { get; set; } = new List<CancionSinConexion>();
        public List<CancionSinConexion> canciones_pendientes_descarga { get; set; } = new List<CancionSinConexion>();
    }
}