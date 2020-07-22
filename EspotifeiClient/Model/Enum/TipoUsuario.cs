using System.ComponentModel;

namespace Model.Enum
{
    public enum TipoUsuario
    {
        [Description("Creador de contenido")] CreadorDeContenido = 1,
        [Description("Consumidor de musica")] ConsumidorDeMusica = 2
    }
}