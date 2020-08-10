using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Threading;
using Api.GrpcClients.Clients;
using ManejadorDeArchivos;
using Model;
using NAudio.Wave;

namespace EspotifeiClient.Player
{
    public class Player
    {
        public delegate void ActualizacionColaReproduccion(List<ElementoCola> elementosCola);

        public delegate void AvanzaCancion(double tiempoActual);

        public delegate void CambiaEstadoReproudccion(bool estaReproducciendo);

        public delegate void IniciaReproduccionCancion(Cancion cancion);

        public delegate void IniciaReproduccionCancionPersonal(CancionPersonal cancionPersonal);

        private static readonly Player Reproductor = new Player();
        private readonly Cola _colaDeReproduccion = new Cola();

        private readonly DispatcherTimer _seguidorDeEventosDelReproductor;
        private WaveStream _blockAlignedStream;
        private MemoryStream _bufferCancion;
        private long _cantidadPaquetesRecividos;
        private SongsClient _clienteCanciones;
        private float _duracionTotalDeCancionEnReproduccion;
        private EstadoReproductor _estadoReproductor = EstadoReproductor.Detenido;
        private DateTime _horaUltimoPaqueteRecibido;
        private Mp3FileReader _mp3Reader;
        private byte[] _mp3StreamBuffer;
        private long _tiempoTotalPaquetesRecibidos;
        private long _timepoPromedioConexion;
        private WaveOut _waveOutEvent;

        private Player()
        {
            _waveOutEvent = new WaveOut();
            _seguidorDeEventosDelReproductor = new DispatcherTimer();
            _seguidorDeEventosDelReproductor.Tick += SeguidorDeTiempoReproduccion;
            _seguidorDeEventosDelReproductor.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        public event ActualizacionColaReproduccion OnActualizacionCola;

        public event IniciaReproduccionCancion OnIniciaReproduccionCancion;

        public event IniciaReproduccionCancionPersonal OnIniciaReproduccionCancionPersonal;

        public event CambiaEstadoReproudccion OnCambioEstadoReproduccion;

        public event AvanzaCancion OnAvanceCancion;

        public static Player GetPlayer()
        {
            return Reproductor;
        }

        /// <summary>
        ///     Actualiza el volumen del reproductor
        /// </summary>
        /// <param name="volumenActualizado">El nuevo volumen del reproductor</param>
        public void ActualizarVolumen(int volumenActualizado)
        {
            var volumen = Convert.ToSingle(volumenActualizado / Convert.ToSingle(100));
            _waveOutEvent.Volume = volumen;
        }

        /// <summary>
        ///     Sigue el tiempo de reproduccion de la cancion en reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void SeguidorDeTiempoReproduccion(object sender, EventArgs e)
        {
            OnAvanceCancion?.Invoke(_blockAlignedStream.CurrentTime.TotalSeconds);
            if ((int)Math.Ceiling(_blockAlignedStream.CurrentTime.TotalSeconds)  >=
                (int)Math.Ceiling(_duracionTotalDeCancionEnReproduccion))
            {
                _seguidorDeEventosDelReproductor.Stop();
                _waveOutEvent.Stop();
                _estadoReproductor = EstadoReproductor.Detenido;
                OnCambioEstadoReproduccion?.Invoke(false);
                if (_bufferCancion != null && _blockAlignedStream != null)
                {
                    _bufferCancion.Dispose();
                    _blockAlignedStream.Dispose();
                }

                ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        ///     Pausa, Reproduce o Inicia la siguiente cancion dependiento de _estadoReproductor
        /// </summary>
        public void Play()
        {
            if (_estadoReproductor == EstadoReproductor.Reproduciendo)
            {
                _waveOutEvent.Pause();
                _seguidorDeEventosDelReproductor.Stop();
                OnCambioEstadoReproduccion?.Invoke(false);
                _estadoReproductor = EstadoReproductor.Pausado;
            }
            else if (_estadoReproductor == EstadoReproductor.Pausado)
            {
                _waveOutEvent.Play();
                _seguidorDeEventosDelReproductor.Start();
                OnCambioEstadoReproduccion?.Invoke(true);
                _estadoReproductor = EstadoReproductor.Reproduciendo;
            }
            else if (_estadoReproductor == EstadoReproductor.Detenido)
            {
                ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        ///     Empieza la reproduccion de la siguiente cancion en la cola
        /// </summary>
        public void ReproducirSiguienteCancion()
        {
            if (_colaDeReproduccion.ObtenerTipoDeCancionSiguiente() != Cola.TipoCancionAReproducir.Ninguno)
                switch (_colaDeReproduccion.ObtenerTipoDeCancionSiguiente())
                {
                    case Cola.TipoCancionAReproducir.Cancion:
                        var proximaCancion = _colaDeReproduccion.ObtenerCancion(false);
                        if (proximaCancion != null) EmpezarAReproducirCancion(proximaCancion);
                        break;
                    case Cola.TipoCancionAReproducir.CancionPersonal:
                        var proximaCancionPersonal = _colaDeReproduccion.ObtenerCancionPersonal(false);
                        if (proximaCancionPersonal != null) EmpezarAReproducirCancionPersonal(proximaCancionPersonal);
                        break;
                    case Cola.TipoCancionAReproducir.CancionSinConexion:
                        var proximaCancionSinConexion = _colaDeReproduccion.ObtenerCancionSinConexion(false);
                        if (proximaCancionSinConexion != null)
                            EmpezarAReproducirCancionSinConexion(proximaCancionSinConexion);
                        break;
                }
        }

        /// <summary>
        ///     Reproduce la cancion anterior de la cola o reinicia la reproduccion de la actual si el tiempo de reproduccion
        ///     es menor o igual a 10 segundos
        /// </summary>
        public void ReproducirCancionAnterior()
        {
            try
            {
                if (_blockAlignedStream.CurrentTime.TotalSeconds <= 10)
                {
                    switch (_colaDeReproduccion.ObtenerTipoDeCancionAnterior())
                    {
                        case Cola.TipoCancionAReproducir.Cancion:
                            var cancionAnterior = _colaDeReproduccion.ObtenerCancion(true);
                            if (cancionAnterior != null) EmpezarAReproducirCancion(cancionAnterior);
                            break;
                        case Cola.TipoCancionAReproducir.CancionPersonal:
                            var cancionPersonalAnterior = _colaDeReproduccion.ObtenerCancionPersonal(true);
                            if (cancionPersonalAnterior != null)
                                EmpezarAReproducirCancionPersonal(cancionPersonalAnterior);
                            break;
                        case Cola.TipoCancionAReproducir.CancionSinConexion:
                            var cancionSinConexionAnteriror = _colaDeReproduccion.ObtenerCancionSinConexion(true);
                            if (cancionSinConexionAnteriror != null)
                                EmpezarAReproducirCancionSinConexion(cancionSinConexionAnteriror);
                            break;
                    }
                }
                else
                {
                    if (_blockAlignedStream != null)
                    {
                        _blockAlignedStream.Seek(0, SeekOrigin.Begin);
                        OnAvanceCancion?.Invoke(0);
                        if (_estadoReproductor == EstadoReproductor.Detenido)
                        {
                            _waveOutEvent.Play();
                            if (_estadoReproductor == EstadoReproductor.Detenido ||
                                _estadoReproductor == EstadoReproductor.Pausado)
                            {
                                _seguidorDeEventosDelReproductor.Start();
                                _estadoReproductor = EstadoReproductor.Reproduciendo;
                            }

                            OnCambioEstadoReproduccion?.Invoke(true);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
            }
        }


        /// <summary>
        ///     Añade un Album a la cola de reproduccion
        /// </summary>
        /// <param name="album">El album a colocar en la cola de reproduccion sus canciones</param>
        public void AñadirCancionesDeAlbumACola(Album album)
        {
            _colaDeReproduccion.AgregarCancionesDeAlbumACola(album);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
        }

        /// <summary>
        ///     Añade las canciones del creador de contenido a la cola de reproduccion
        /// </summary>
        /// <param name="creadorContenido">El creador de contenido a agregar a la cola de reproduccion sus canciones</param>
        public void AñadirCancionesDeCreadorDeContenidoACola(CreadorContenido creadorContenido)
        {
            _colaDeReproduccion.AgregarCancionesDeCreadorDeContenidoACola(creadorContenido);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
        }

        /// <summary>
        ///     Añade las canciones de la lista de reproduccion a la cola
        /// </summary>
        /// <param name="listaReproduccion">La lista de canciones a agregar a la cola de reproduccion</param>
        public void AñadirCancionesDeListaDeReproduccionACola(ListaReproduccion listaReproduccion)
        {
            _colaDeReproduccion.AgregarCancionesDeListaDeReproduccionACola(listaReproduccion);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
        }

        /// <summary>
        ///     Añade todas las canciones de la biblioteca personal a la cola de reproduccion
        /// </summary>
        /// <param name="cancionesPersonales">La lista de canciones personales a agregar a la cola</param>
        public void AñadirBibliotecaPersonalACola(List<CancionPersonal> cancionesPersonales)
        {
            if (cancionesPersonales != null)
            {
                _colaDeReproduccion.LimpiarCola();
                foreach (var cancionPersonal in cancionesPersonales)
                    _colaDeReproduccion.AgregarCancionPersonalACola(cancionPersonal);
                var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
                if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        ///     Agrega las canciones de una radio a la cola de reproducción
        /// </summary>
        /// <param name="radio">La radio que contiene las canciones de la lista de reproducción</param>
        public void AñadirRadioAListaDeReproduccion(List<Cancion> radio)
        {
            if (radio != null)
            {
                _colaDeReproduccion.LimpiarCola();
                foreach (var cancion in radio) _colaDeReproduccion.AgregarCancionACola(cancion);
                var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
                if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        ///     Agrega todas las canciones sin conexion a la cola de reproduccion
        /// </summary>
        /// <param name="cancionesSinConexion">Las canciones sin conexion a agregar a la cola</param>
        public void AñadirTodasLasCancionesSinConexionACola(List<CancionSinConexion> cancionesSinConexion)
        {
            if (cancionesSinConexion != null)
            {
                _colaDeReproduccion.LimpiarCola();
                foreach (var cancion in cancionesSinConexion)
                    _colaDeReproduccion.AgregarCancionSinConexionACola(cancion);
                var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
                if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno) ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        ///     Añade una cancion a la cola de reproduccion
        /// </summary>
        /// <param name="cancion">La cancion a agregar</param>
        public void AñadirCancionAColaDeReproduccion(Cancion cancion)
        {
            _colaDeReproduccion.AgregarCancionACola(cancion);
        }

        /// <summary>
        ///     Añade una cancion personal a la cola de reproduccion
        /// </summary>
        /// <param name="cancionPersonal">La cancion perosnal a agregar</param>
        public void AñadirCancionPersonalAColaDeReproduccion(CancionPersonal cancionPersonal)
        {
            _colaDeReproduccion.AgregarCancionPersonalACola(cancionPersonal);
        }

        /// <summary>
        ///     Añade una cancion sin conexion a la cola de reproduccion
        /// </summary>
        /// <param name="cancionSinConexion"></param>
        public void AñadirCancionSinConexionAColaDeReproduccion(CancionSinConexion cancionSinConexion)
        {
            _colaDeReproduccion.AgregarCancionSinConexionACola(cancionSinConexion);
        }

        /// <summary>
        ///     Recupera los proximos elementos en la cola
        /// </summary>
        /// <returns>Una lista de ElementoCola</returns>
        public List<ElementoCola> RecuperarElementosRestantesEnCola()
        {
            return _colaDeReproduccion.ObtenerProximosElementosEnCola();
        }

        /// <summary>
        ///     Elimina un elemento de la cola de reproduccion
        /// </summary>
        /// <param name="posicion">La posicion del elemento a eliminar</param>
        public void EliminarElementoDeColaReproduccion(int posicion)
        {
            _colaDeReproduccion.EliminarElementoDeCola(posicion);
            OnActualizacionCola?.Invoke(_colaDeReproduccion.ObtenerProximosElementosEnCola());
        }

        /// <summary>
        ///     Empieza la reproduccion de una cancion
        /// </summary>
        /// <param name="cancion">La cancion a reproducir</param>
        public void EmpezarAReproducirCancion(Cancion cancion)
        {
            OnActualizacionCola?.Invoke(_colaDeReproduccion.ObtenerProximosElementosEnCola());
            OnIniciaReproduccionCancion?.Invoke(cancion);
            OnCambioEstadoReproduccion?.Invoke(true);
            _estadoReproductor = EstadoReproductor.Reproduciendo;
            _duracionTotalDeCancionEnReproduccion = cancion.duracion;
            ReproducirCancion(cancion.id, false);
        }

        /// <summary>
        ///     Empieza la reproduccion de una cancion personal sin afectar la cola de reproduccion
        /// </summary>
        /// <param name="cancionPersonal">La cancion personal a reproducir</param>
        public void EmpezarAReproducirCancionPersonal(CancionPersonal cancionPersonal)
        {
            OnActualizacionCola?.Invoke(_colaDeReproduccion.ObtenerProximosElementosEnCola());
            OnIniciaReproduccionCancionPersonal?.Invoke(cancionPersonal);
            OnCambioEstadoReproduccion?.Invoke(true);
            _estadoReproductor = EstadoReproductor.Reproduciendo;
            _duracionTotalDeCancionEnReproduccion = cancionPersonal.duracion;
            ReproducirCancion(cancionPersonal.id, true);
        }

        /// <summary>
        ///     Empieza la reproduccion de una cancion sin conexion sin afectar a la cola de reproduccion
        /// </summary>
        /// <param name="cancion">La cancion sin conexion a reproducir</param>
        public void EmpezarAReproducirCancionSinConexion(CancionSinConexion cancion)
        {
            OnActualizacionCola?.Invoke(_colaDeReproduccion.ObtenerProximosElementosEnCola());
            OnIniciaReproduccionCancion?.Invoke(cancion);
            OnCambioEstadoReproduccion?.Invoke(true);
            _estadoReproductor = EstadoReproductor.Reproduciendo;
            _duracionTotalDeCancionEnReproduccion = cancion.duracion;
            ReproducirCancionSinConexion(cancion.ruta_cancion);
        }

        /// <summary>
        ///     Se encarga de parar la recepción de los datos de la cancion anterior
        /// </summary>
        private void DetenerRecepcionDeCancion()
        {
            if (_clienteCanciones != null)
            {
                _clienteCanciones.OnSongChunkRived -= RecivedSongChunk;
                _clienteCanciones.OnInitialRecivedSong -= RecibirPrimerChunkDeCancion;
                _clienteCanciones.OnErrorRaised -= ManejarErrores;
                _clienteCanciones.StopGetSong();
                _clienteCanciones = null;
                _waveOutEvent.Stop();
                _waveOutEvent = new WaveOut(WaveCallbackInfo.FunctionCallback());
                _seguidorDeEventosDelReproductor.Stop();
                if (_bufferCancion != null && _blockAlignedStream != null)
                {
                    _bufferCancion.Dispose();
                    _blockAlignedStream.Dispose();
                }
            }
        }

        /// <summary>
        ///     Inicializa el cliente de canciones para solicitar la cancion con el idSong
        /// </summary>
        /// <param name="idSong">El id de la cancion a recuperar</param>
        /// <param name="isPersonal">Indica si la cancion es personal</param>
        private void ReproducirCancion(int idSong, bool isPersonal)
        {
            DetenerRecepcionDeCancion();
            _clienteCanciones = new SongsClient();
            _clienteCanciones.OnInitialRecivedSong += RecibirPrimerChunkDeCancion;
            _clienteCanciones.OnSongChunkRived += RecivedSongChunk;
            _clienteCanciones.OnErrorRaised += ManejarErrores;
            var calidadARecuperar = CalcularCalidadAObtenerCancion(_timepoPromedioConexion);
            _clienteCanciones.GetSong(idSong, calidadARecuperar, isPersonal);
        }

        /// <summary>
        ///     Se encarga de comenzar a reproducir una cancion desde un archivo local
        /// </summary>
        /// <param name="ruta">La ruta de la cancion a reproducir</param>
        private void ReproducirCancionSinConexion(string ruta)
        {
            try
            {
                DetenerRecepcionDeCancion();
                _waveOutEvent.Stop();
                _bufferCancion = new MemoryStream(File.ReadAllBytes(ruta));
                _mp3Reader = new Mp3FileReader(_bufferCancion);
                _blockAlignedStream = new WaveChannel32(_mp3Reader);
                _waveOutEvent = new WaveOut(WaveCallbackInfo.FunctionCallback());
                _waveOutEvent.Init(_blockAlignedStream);
                _estadoReproductor = EstadoReproductor.Reproduciendo;
                _seguidorDeEventosDelReproductor.Start();
                _waveOutEvent.Play();
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
                _waveOutEvent.Stop();
                OnCambioEstadoReproduccion?.Invoke(false);
                _seguidorDeEventosDelReproductor.Stop();
                _estadoReproductor = EstadoReproductor.Detenido;
                if (_bufferCancion != null && _blockAlignedStream != null)
                {
                    _bufferCancion.Dispose();
                    _blockAlignedStream.Dispose();
                }
            }
        }

        /// <summary>
        ///     Se encarga de manejar los errores que puedan ocurrir al recuperar una cancion del servidor
        /// </summary>
        /// <param name="message">El mensaje que recibe del cliente de canciones</param>
        private void ManejarErrores(string message)
        {
            if (message != "AuntenticacionFallida")
                new MensajeEmergente().MostrarMensajeError(message);
            else
                new MensajeEmergente().MostrarMensajeError("No se logro logear con las credenciales proporcionadas " +
                                                           "en el inicio de sesión, si continua obteniendo este " +
                                                           "mensaje, cierre sesión y vuelva a logearse");
            _waveOutEvent.Stop();
            OnCambioEstadoReproduccion?.Invoke(false);
            _seguidorDeEventosDelReproductor.Stop();
            _estadoReproductor = EstadoReproductor.Detenido;
            if (_bufferCancion != null && _blockAlignedStream != null)
            {
                _bufferCancion.Dispose();
                _blockAlignedStream.Dispose();
            }

            ReproducirSiguienteCancion();
        }

        /// <summary>
        ///     Se encarga de crear el stream de reproduccion e inicializar el reproductor con los bytes recibidos
        /// </summary>
        /// <param name="songBytes">
        ///     Los primeros 64000 bytes de la cancion de los cuales se obtendra la informacion de
        ///     la misma
        /// </param>
        /// <param name="extension">La extension de la cancion a recibir. ejemplo mp3</param>
        private void RecibirPrimerChunkDeCancion(byte[] songBytes, string extension)
        {
            var format = GetWaveFormat(new MemoryStream(songBytes));
            _bufferCancion = new MemoryStream();
            _bufferCancion.Write(songBytes, 0, songBytes.Length);
            _bufferCancion.Position = 0;
            _mp3StreamBuffer = new byte[format.SampleRate];
            _blockAlignedStream =
                new BlockAlignReductionStream(
                    WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(_bufferCancion)));
            _waveOutEvent = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _blockAlignedStream.Position = 0;
            _horaUltimoPaqueteRecibido = DateTime.Now;
            _waveOutEvent.Init(_blockAlignedStream);
            _seguidorDeEventosDelReproductor.Start();
            if (_estadoReproductor == EstadoReproductor.Reproduciendo) _waveOutEvent.Play();
        }

        /// <summary>
        ///     Obtiene el formato de la canciona a partir de un memory stream
        /// </summary>
        /// <param name="initialBytesSong">El memory stream a utilizar para obtener la informacion</param>
        /// <returns>WaveFormat con la informacion de la cancion</returns>
        private WaveFormat GetWaveFormat(MemoryStream initialBytesSong)
        {
            WaveStream tempStream = new Mp3FileReader(initialBytesSong);
            return tempStream.WaveFormat;
        }

        /// <summary>
        ///     Calcula el tiempo promedio en el que tarda en llegar el paquete
        /// </summary>
        private void CalcularTiempoRespuesta()
        {
            _cantidadPaquetesRecividos += 1;
            _tiempoTotalPaquetesRecibidos += DateTime.Now.Subtract(_horaUltimoPaqueteRecibido).Milliseconds;
            _timepoPromedioConexion = _tiempoTotalPaquetesRecibidos / _cantidadPaquetesRecividos;
            _horaUltimoPaqueteRecibido = DateTime.Now;
        }

        /// <summary>
        ///     Agrega los bytes recibidos al stream de bytes donde esta el buffer de la cancion
        /// </summary>
        /// <param name="streamBytes">El stream de bytes</param>
        private void RecivedSongChunk(byte[] streamBytes)
        {
            CalcularTiempoRespuesta();
            var stream = new MemoryStream(streamBytes);
            int bytesRead;
            while ((bytesRead = stream.Read(_mp3StreamBuffer, 0, _mp3StreamBuffer.Length)) > 0)
            {
                var pos = _bufferCancion.Position;
                _bufferCancion.Position = _bufferCancion.Length;
                _bufferCancion.Write(_mp3StreamBuffer, 0, bytesRead);
                _bufferCancion.Position = pos;
            }

            ReanudarReproduccion();
        }

        /// <summary>
        ///     Reaunuda la reproduccion si se pauso
        /// </summary>
        private void ReanudarReproduccion()
        {
            if (_blockAlignedStream.CanRead)
                if (_waveOutEvent.PlaybackState == PlaybackState.Stopped &&
                    _estadoReproductor == EstadoReproductor.Reproduciendo)
                    _waveOutEvent.Play();
        }

        /// <summary>
        ///     Calcula la calidad de la cancion a recuperar a partir del tiempo promedio que le toma al sistema recibir un paquete
        /// </summary>
        /// <param name="tiempoPromedio">el tiempo promedio que le toma al sistema recibir un paquete</param>
        /// <returns>La Calidad de acuerdo al tiempo promedio</returns>
        private Calidad CalcularCalidadAObtenerCancion(long tiempoPromedio)
        {
            Calidad calidad;
            if (tiempoPromedio > 0 && tiempoPromedio <= 150)
                calidad = Calidad.Alta;
            else if (tiempoPromedio > 150 && tiempoPromedio <= 250)
                calidad = Calidad.Media;
            else
                calidad = Calidad.Baja;

            return calidad;
        }

        /// <summary>
        ///     Limpia la cola de reproduccion
        /// </summary>
        public void LimpiarColaDeReproduccion()
        {
            _colaDeReproduccion.LimpiarCola();
            OnActualizacionCola?.Invoke(_colaDeReproduccion.ObtenerProximosElementosEnCola());
        }

        /// <summary>
        ///     Limpia el reproductor
        /// </summary>
        public void LimpiarReproductor()
        {
            DetenerRecepcionDeCancion();
            _waveOutEvent.Stop();
            _estadoReproductor = EstadoReproductor.Detenido;
            _colaDeReproduccion.LimpiarCola();
        }
    }
}