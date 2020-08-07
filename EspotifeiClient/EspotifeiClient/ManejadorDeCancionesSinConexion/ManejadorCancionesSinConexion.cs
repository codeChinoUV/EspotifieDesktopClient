using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Api.GrpcClients.Clients;
using EspotifeiClient.ManejoUsuarios;
using ManejadorDeArchivos;
using Model;

namespace EspotifeiClient.ManejadorDeCancionesSinConexion
{
    /// <summary>
    ///     Logica para el manejo de las descargas de las canciones sin conexion
    /// </summary>
    public class ManejadorCancionesSinConexion
    {
        public delegate void ActualizacionElementosCola();

        private static readonly ManejadorCancionesSinConexion _manejadorCancionesSinConexion =
            new ManejadorCancionesSinConexion();

        private MemoryStream _bufferCancion;
        private readonly Queue<CancionSinConexion> _colaCancionesSinConexion = new Queue<CancionSinConexion>();
        private EstadoManjadorDeCanciones _estadoManjadorDeCanciones = EstadoManjadorDeCanciones.Detenido;
        private string _extensionCancion;
        private CancionSinConexion _seEncuentraDescargando;
        private readonly SongsClient _songsClient = new SongsClient();

        private ManejadorCancionesSinConexion()
        {
            var usuario = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            if (usuario != null)
            {
                var cancionesPendientes = usuario.canciones_pendientes_descarga;
                foreach (var cancionSinConexion in cancionesPendientes)
                    _colaCancionesSinConexion.Enqueue(cancionSinConexion);
                if (_colaCancionesSinConexion.Count > 0) IniciarDescarga();
            }
        }

        public event ActualizacionElementosCola OnActualizacionElementosCola;

        /// <summary>
        ///     Devuelve la instancia del singleton
        /// </summary>
        /// <returns>El singleton de ManejadorCancionesSinConexion</returns>
        public static ManejadorCancionesSinConexion GetManejadorDeCancionesSinConexion()
        {
            return _manejadorCancionesSinConexion;
        }

        /// <summary>
        ///     Devuelve la cancion actual que se encuentra descargando
        /// </summary>
        /// <returns>La cancion que se encuentra descargando</returns>
        public CancionSinConexion GetCancionDescargando()
        {
            return _seEncuentraDescargando;
        }

        /// <summary>
        ///     Agrega una cancion a la cola de descarga para las canciones sin conexion
        /// </summary>
        /// <param name="cancion">La cancion a agregar a canciones sin conexion</param>
        public void AgregarCancionSinConexion(Cancion cancion)
        {
            if (!SeEncuentraEnCancionesSinConexion(cancion.id))
            {
                var cancionSinConexion = CrearCancionSinConexionDeCancion(cancion);
                _colaCancionesSinConexion.Enqueue(cancionSinConexion);
                ActualizarCancionesPendientesDeUsuario();
                if (_seEncuentraDescargando == null)
                {
                    _seEncuentraDescargando = _colaCancionesSinConexion.Dequeue();
                    _seEncuentraDescargando.estado_descarga = CancionSinConexion.EstadoDescarga.Descargando;
                    EmpezarADescargarCancion(_seEncuentraDescargando.id);
                }
            }
        }

        /// <summary>
        ///     Crea una cancion sin conexion a partir de una cancion
        /// </summary>
        /// <param name="cancion">La cancion de la cual se creara la cancion sin conexion</param>
        /// <returns>La cancion sin conexion creada</returns>
        private CancionSinConexion CrearCancionSinConexionDeCancion(Cancion cancion)
        {
            return new CancionSinConexion
            {
                id = cancion.id,
                nombre = cancion.nombre,
                duracion = cancion.duracion,
                album = cancion.album,
                calificacion_promedio = cancion.calificacion_promedio,
                cantidad_de_reproducciones = cancion.cantidad_de_reproducciones,
                creadores_de_contenido = cancion.creadores_de_contenido,
                generos = cancion.generos,
                estado_descarga = CancionSinConexion.EstadoDescarga.Espera
            };
        }

        /// <summary>
        ///     Empieza a descargar la cancion con el id indicado
        /// </summary>
        /// <param name="idCancion">El id de la cancion a descargar</param>
        private void EmpezarADescargarCancion(int idCancion)
        {
            _songsClient.OnInitialRecivedSong += RecibirCancion;
            _songsClient.OnSongChunkRived += RecibirChunk;
            _songsClient.OnTerminatedRecivedSong += TerminarDeRecibirCancion;
            _songsClient.OnErrorRaised += ManejarError;
            _songsClient.GetSong(idCancion, Calidad.Alta, false);
            _estadoManjadorDeCanciones = EstadoManjadorDeCanciones.Descargando;
            ActualizarCancionesPendientesDeUsuario();
        }

        /// <summary>
        ///     Se encarga de manejar los errores que puedan ocurrir al descargar la cancion
        /// </summary>
        /// <param name="message">El mensaje del error</param>
        private void ManejarError(string message)
        {
            _seEncuentraDescargando.estado_descarga = CancionSinConexion.EstadoDescarga.Error;
            _colaCancionesSinConexion.Enqueue(_seEncuentraDescargando);
            _seEncuentraDescargando = null;
            ActualizarCancionesPendientesDeUsuario();
            Thread.Sleep(10000);
            IniciarDescarga();
        }

        /// <summary>
        ///     Se encarga de almacenar la cancion cuando ya se termino de descargar y de agregarla a la lista de canciones
        ///     descargadas
        /// </summary>
        private void TerminarDeRecibirCancion()
        {
            _songsClient.OnInitialRecivedSong -= RecibirCancion;
            _songsClient.OnSongChunkRived -= RecibirChunk;
            _songsClient.OnTerminatedRecivedSong -= TerminarDeRecibirCancion;
            _estadoManjadorDeCanciones = EstadoManjadorDeCanciones.Guardando;
            var usuarioActual = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            var ruta = CrearRutaParaGuardarCancion(usuarioActual.nombre_usuario, _seEncuentraDescargando.id,
                _extensionCancion);
            var seGuardo = GuardarCancion(_bufferCancion, ruta);
            _bufferCancion.Dispose();
            if (seGuardo)
            {
                _seEncuentraDescargando.ruta_cancion = ruta;
                _seEncuentraDescargando.estado_descarga = CancionSinConexion.EstadoDescarga.Descargado;
                usuarioActual.canciones_sin_conexion.Add(_seEncuentraDescargando);
            }
            else
            {
                _colaCancionesSinConexion.Enqueue(_seEncuentraDescargando);
                _seEncuentraDescargando.estado_descarga = CancionSinConexion.EstadoDescarga.Error;
            }

            _seEncuentraDescargando = null;
            ActualizarCancionesPendientesDeUsuario();
            IniciarDescarga();
        }

        /// <summary>
        ///     Recibe un pedazo de la cancion y lo guarda en el buffer temporal
        /// </summary>
        /// <param name="bytesSong">Los bytes de la cancion a guardar</param>
        private void RecibirChunk(byte[] bytesSong)
        {
            _bufferCancion.Write(bytesSong, 0, bytesSong.Length);
        }

        /// <summary>
        ///     Recibe el primer pedazo de la informacion de la cancion junto a la extension de la cancion
        /// </summary>
        /// <param name="bytesSong">Los bytes de la cancion</param>
        /// <param name="extension">La extension de la cancion. Ejemplo: mp3</param>
        private void RecibirCancion(byte[] bytesSong, string extension)
        {
            _extensionCancion = extension;
            _bufferCancion = new MemoryStream();
            _bufferCancion.Write(bytesSong, 0, bytesSong.Length);
        }

        /// <summary>
        ///     Calcula una ruta para guardar la cancion y crea las carpetas necesarias
        /// </summary>
        /// <param name="nombreUsuario">El nombre del usuario logeado</param>
        /// <param name="idCancion">El id de la cancion a guardar</param>
        /// <param name="extension">La extension de la cancion</param>
        /// <returns>La ruta calculada</returns>
        private string CrearRutaParaGuardarCancion(string nombreUsuario, int idCancion, string extension)
        {
            var rutaArchivo = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "Espotifei");
            rutaArchivo = Path.Combine(rutaArchivo, nombreUsuario);
            Directory.CreateDirectory(rutaArchivo);
            var rutaConCancion = Path.Combine(rutaArchivo, $"{idCancion.ToString()}.{extension}");
            return rutaConCancion;
        }

        /// <summary>
        ///     Garda el buffer de la cancion en la ruta especificada
        /// </summary>
        /// <param name="bufferDeCancion">El buffer en donde se encuentran los bytes de la cancion</param>
        /// <param name="ruta">La ruta en donde se guardara la cancion</param>
        /// <returns>True si la cancion se guardo o False si no</returns>
        private bool GuardarCancion(MemoryStream bufferDeCancion, string ruta)
        {
            bool seGuardo;
            try
            {
                using (var fs = new FileStream(ruta, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bufferDeCancion.ToArray(), 0, bufferDeCancion.ToArray().Length);
                    seGuardo = true;
                }
            }
            catch (Exception)
            {
                seGuardo = false;
            }

            return seGuardo;
        }

        /// <summary>
        ///     Valida si ya existe en las canciones sin conexion alguna cancion con el mismo id
        /// </summary>
        /// <param name="idCancion">El id de la cancion a validar si existe</param>
        /// <returns>True si ya se encuentra en la lista de las canciones sin conexion, False si no</returns>
        private bool SeEncuentraEnCancionesSinConexion(int idCancion)
        {
            var seEncuentra = false;
            var usuario = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            if (usuario.canciones_sin_conexion != null)
                foreach (var cancionSinConexion in usuario.canciones_sin_conexion)
                    if (cancionSinConexion.id == idCancion)
                    {
                        seEncuentra = true;
                        break;
                    }

            return seEncuentra;
        }

        /// <summary>
        ///     Coloca la siguiente cancion en la cola a descargar
        /// </summary>
        private void IniciarDescarga()
        {
            if (_colaCancionesSinConexion.Count > 0)
            {
                _seEncuentraDescargando = _colaCancionesSinConexion.Dequeue();
                _seEncuentraDescargando.estado_descarga = CancionSinConexion.EstadoDescarga.Descargando;
                EmpezarADescargarCancion(_seEncuentraDescargando.id);
            }
            else
            {
                _estadoManjadorDeCanciones = EstadoManjadorDeCanciones.Detenido;
            }

            ActualizarCancionesPendientesDeUsuario();
        }

        /// <summary>
        ///     Actualiza la lista de canciones pendientes por descargar del usuario
        /// </summary>
        private void ActualizarCancionesPendientesDeUsuario()
        {
            var usuario = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            usuario.canciones_pendientes_descarga = _colaCancionesSinConexion.ToList();
            OnActualizacionElementosCola?.Invoke();
        }

        /// <summary>
        ///     Coloca todas las canciones pendientes de descarga o descargando en la lista de canciones pendientes del usuario
        /// </summary>
        public void TerminarDeDescargarCanciones()
        {
            var usuario = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            if (usuario != null)
            {
                if (_seEncuentraDescargando != null) _colaCancionesSinConexion.Enqueue(_seEncuentraDescargando);
                usuario.canciones_pendientes_descarga = _colaCancionesSinConexion.ToList();
            }
        }

        /// <summary>
        ///     Le indica a la ventana si se puede cerrar
        /// </summary>
        /// <returns>True si se puede cerrar o false si no</returns>
        public bool SePuedeCerrarLaApp()
        {
            return _estadoManjadorDeCanciones != EstadoManjadorDeCanciones.Guardando;
        }

        private enum EstadoManjadorDeCanciones
        {
            Detenido,
            Descargando,
            Guardando
        }
    }
}