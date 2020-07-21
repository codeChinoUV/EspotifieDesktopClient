using Model.Enum;

namespace Model
{
    public class Usuario
    {
        public string nombre_usuario { get; set; }
        
        public string nombre { get; set; }
        
        public string contrasena { get; set; }
        
        public string correo_electronico { get; set; }
        
        public TipoUsuario tipo_usuario { get; set; }
    }
}