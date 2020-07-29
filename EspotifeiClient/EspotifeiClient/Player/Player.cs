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
        public delegate void IniciaReproduccionCancion(Cancion cancion);

        public event IniciaReproduccionCancion OnIniciaReproduccionCancion;
        
        public delegate void IniciaReproduccionCancionPersonal(CancionPersonal cancionPersonal);

        public event IniciaReproduccionCancionPersonal OnIniciaReproduccionCancionPersonal;

        public delegate void AvanzaCancion(double tiempoActual);

        public delegate void CambiaEstadoReproudccion(bool estaReproducciendo);

        public event CambiaEstadoReproudccion OnCambioEstadoReproduccion;
        
        public event AvanzaCancion OnAvanceCancion;

        private DispatcherTimer _seguidorDeEventosDelReproductor;
        
        private static readonly Player _player = new Player();
        private Cola _colaDeReproduccion = new Cola();
        private byte[] _mp3StreamBuffer;
        private MemoryStream _songBuffer;
        private SongsClient _songsClient;
        private WaveOut _waveOutEvent;
        private WaveStream _blockAlignedStream;
        private long _timepoPromedioConexion;
        private DateTime _horaUltimoPaqueteRecibido;
        private long _cantidadPaquetesRecividos;
        private long _tiempoTotalPaquetesRecibidos;
        private float _duracionTotalDeCancionEnReproduccion;
        private EstadoReproductor _estadoReproductor = EstadoReproductor.Detenido; 
        
        public static Player GetPlayer()
        {
            return _player;
        }
        
        private Player()
        {
            _waveOutEvent = new WaveOut();
            var desiredLatency = 1000;
            _waveOutEvent.DesiredLatency = desiredLatency;
            _seguidorDeEventosDelReproductor = new DispatcherTimer();
            _seguidorDeEventosDelReproductor.Tick += SeguidorDeTiempoReproduccion;
            _seguidorDeEventosDelReproductor.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        /// <summary>
        /// Actualiza el volumen del reproductor
        /// </summary>
        /// <param name="volumenActualizado">El nuevo volumen del reproductor</param>
        public void ActualizarVolumen(int volumenActualizado)
        {
            float volumen = Convert.ToSingle(volumenActualizado / Convert.ToSingle(100));
            _waveOutEvent.Volume = volumen;
        }

        private void SeguidorDeTiempoReproduccion(object sender, EventArgs e)
        {
            OnAvanceCancion?.Invoke(_blockAlignedStream.CurrentTime.TotalSeconds);
            if (_blockAlignedStream.CurrentTime.TotalSeconds >= _duracionTotalDeCancionEnReproduccion)
            {
                _seguidorDeEventosDelReproductor.Stop();
                _waveOutEvent.Stop();
                _estadoReproductor = EstadoReproductor.Detenido;
                OnCambioEstadoReproduccion?.Invoke(false);
                ReproducirSiguienteCancion();
            }
        }
        
        /// <summary>
        /// Pausa, Reproduce o Inicia la siguiente cancion dependiento de _estadoReproductor
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
            else if(_estadoReproductor == EstadoReproductor.Pausado)
            {
                _waveOutEvent.Play();
                _seguidorDeEventosDelReproductor.Start();
                OnCambioEstadoReproduccion?.Invoke(true);
                _estadoReproductor = EstadoReproductor.Reproduciendo;
            }else if (_estadoReproductor == EstadoReproductor.Detenido)
            {
                ReproducirSiguienteCancion();
            }
        }
        
        /// <summary>
        /// Empieza la reproduccion de la siguiente cancion en la cola
        /// </summary>
        public void ReproducirSiguienteCancion()
        {
            if (_colaDeReproduccion.ObtenerTipoDeCancionSiguiente() != Cola.TipoCancionAReproducir.Ninguno)
            {
                switch (_colaDeReproduccion.ObtenerTipoDeCancionSiguiente())
                {
                    case Cola.TipoCancionAReproducir.Cancion:
                        var proximaCancion = _colaDeReproduccion.ObtenerCancion(false);
                        if (proximaCancion != null)
                        {
                            EmpezarAReproducirCancion(proximaCancion);
                        }
                        break;
                    case Cola.TipoCancionAReproducir.CancionPersonal:
                        var proximaCancionPersonal = _colaDeReproduccion.ObtenerCancionPersonal(false);
                        if (proximaCancionPersonal != null)
                        {
                            EmpezarAReproducirCancionPersonal(proximaCancionPersonal);
                        }
                        break;
                }
            }
        }
        
        /// <summary>
        /// Reproduce la cancion anterior de la cola o reinicia la reproduccion de la actual si el tiempo de reproduccion
        /// es menor o igual a 10 segundos
        /// </summary>
        public void ReproducirCancionAnterior()
        {
            if (_blockAlignedStream.CurrentTime.TotalSeconds <= 10 )
            {
                switch (_colaDeReproduccion.ObtenerTipoDeCancionAnterior())
                {
                    case Cola.TipoCancionAReproducir.Cancion:
                        var cancionAnterior = _colaDeReproduccion.ObtenerCancion(true);
                        if (cancionAnterior != null)
                        {
                            EmpezarAReproducirCancion(cancionAnterior);
                        }
                        break;
                    case Cola.TipoCancionAReproducir.CancionPersonal:
                        var cancionPersonalAnterior = _colaDeReproduccion.ObtenerCancionPersonal(true);
                        if (cancionPersonalAnterior != null)
                        {
                            EmpezarAReproducirCancionPersonal(cancionPersonalAnterior);
                        }
                        break;
                }
            }
            else
            {
                _blockAlignedStream.Seek(0, SeekOrigin.Begin);
                if (_estadoReproductor != EstadoReproductor.Reproduciendo)
                {
                    _waveOutEvent.Play();
                    if (_estadoReproductor == EstadoReproductor.Detenido)
                    {
                        _seguidorDeEventosDelReproductor.Start();
                        _estadoReproductor = EstadoReproductor.Reproduciendo;
                    }
                    OnCambioEstadoReproduccion?.Invoke(true);
                }
            }
        }


        /// <summary>
        /// Añade un Album a la cola de reproduccion
        /// </summary>
        /// <param name="album">El album a colocar en la cola de reproduccion sus canciones</param>
        public void AñadirCancionesDeAlbumACola(Album album)
        {
            _colaDeReproduccion.AgregarCancionesDeAlbumACola(album);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno)
            {
                ReproducirSiguienteCancion();
            }
        }
        
        /// <summary>
        /// Añade las canciones del creador de contenido a la cola de reproduccion
        /// </summary>
        /// <param name="creadorContenido">El creador de contenido a agregar a la cola de reproduccion sus canciones</param>
        public void AñadirCancionesDeCreadorDeContenidoACola(CreadorContenido creadorContenido)
        {
            _colaDeReproduccion.AgregarCancionesDeCreadorDeContenidoACola(creadorContenido);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno)
            {
                ReproducirSiguienteCancion();
            }
        }

        /// <summary>
        /// Añade las canciones de la lista de reproduccion a la cola
        /// </summary>
        /// <param name="listaReproduccion">La lista de canciones a agregar a la cola de reproduccion</param>
        public void AñadirCancionesDeListaDeReproduccionACola(ListaReproduccion listaReproduccion)
        {
            _colaDeReproduccion.AgregarCancionesDeListaDeReproduccionACola(listaReproduccion);
            var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
            if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno)
            {
                ReproducirSiguienteCancion();
            }
        }

        public void AñadirBibliotecaPersonalACola(List<CancionPersonal> cancionesPersonales)
        {
           
            if (cancionesPersonales != null)
            {
                _colaDeReproduccion.LimpiarCola();
                foreach (var cancionPersonal in cancionesPersonales)
                {
                    _colaDeReproduccion.AgregarCancionPersonalACola(cancionPersonal);
                }
                var tipoCancion = _colaDeReproduccion.ObtenerTipoDeCancionSiguiente();
                if (tipoCancion != Cola.TipoCancionAReproducir.Ninguno)
                {
                    ReproducirSiguienteCancion();
                }
            }
        }
        
        /// <summary>
        /// Añade una cancion a la cola de reproduccion
        /// </summary>
        /// <param name="cancion">La cancion a agregar</param>
        public void AñadirCancionAColaDeReproduccion(Cancion cancion)
        {
            _colaDeReproduccion.AgregarCancionACola(cancion);
        }

        /// <summary>
        /// Añade una cancion personal a la cola de reproduccion
        /// </summary>
        /// <param name="cancionPersonal">La cancion perosnal a agregar</param>
        public void AñadirCancionPersonalAColaDeReproduccion(CancionPersonal cancionPersonal)
        {
            _colaDeReproduccion.AgregarCancionPersonalACola(cancionPersonal);
        }

        /// <summary>
        /// Añade una cancion sin conexion a la cola de reproduccion
        /// </summary>
        /// <param name="cancionSinConexion"></param>
        public void AñadirCancionSinConexionAColaDeReproduccion(CancionSinConexion cancionSinConexion)
        {
            _colaDeReproduccion.AgregarCancionSinConexionACola(cancionSinConexion);
        }

        /// <summary>
        /// Empieza la reproduccion de una cancion
        /// </summary>
        /// <param name="cancion">La cancion a reproducir</param>
        public void EmpezarAReproducirCancion(Cancion cancion)
        {
            OnIniciaReproduccionCancion?.Invoke(cancion);
            OnCambioEstadoReproduccion?.Invoke(true);
            _estadoReproductor = EstadoReproductor.Reproduciendo;
            _duracionTotalDeCancionEnReproduccion = cancion.duracion;
            ReproducirCancion(cancion.id, false);
        }
        
        /// <summary>
        /// Empieza la reproduccion de una cancion personal sin afectar la cola de reproduccion
        /// </summary>
        /// <param name="cancionPersonal">La cancion personal a reproducir</param>
        public void EmpezarAReproducirCancionPersonal(CancionPersonal cancionPersonal)
        {
            OnIniciaReproduccionCancionPersonal?.Invoke(cancionPersonal);
            OnCambioEstadoReproduccion?.Invoke(true);
            _estadoReproductor = EstadoReproductor.Reproduciendo;
            _duracionTotalDeCancionEnReproduccion = cancionPersonal.duracion;
            ReproducirCancion(cancionPersonal.id, true);
        }

        /// <summary>
        /// Inicializa el cliente de canciones para solicitar la cancion con el idSong
        /// </summary>
        /// <param name="idSong">El id de la cancion a recuperar</param>
        /// <param name="isPersonal">Indica si la cancion es personal</param>
        private void ReproducirCancion(int idSong, bool isPersonal)
        {
            if (_songsClient != null)
            {
                _songsClient.OnSongChunkRived -= RecivedSongChunk;
                _songsClient.OnInitialRecivedSong -= RecibirPrimerChunkDeCancion;
                _songsClient.OnErrorRaised -= ManejarErrores;
                _songsClient.StopGetSong();
                _songsClient = null;
                _waveOutEvent.Stop();
                _seguidorDeEventosDelReproductor.Stop();
            }
            _songsClient = new SongsClient();
            _songsClient.OnInitialRecivedSong += RecibirPrimerChunkDeCancion;
            _songsClient.OnSongChunkRived += RecivedSongChunk;
            _songsClient.OnErrorRaised += ManejarErrores;
            var calidadARecuperar = CalcularCalidadAObtenerCancion(_timepoPromedioConexion);
            _songsClient.GetSong(idSong, calidadARecuperar, isPersonal);
        }

        /// <summary>
        /// Se encarga de manejar los errores que puedan ocurrir al recuperar una cancion del servidor
        /// </summary>
        /// <param name="message">El mensaje que recibe del cliente de canciones</param>
        private void ManejarErrores(string message)
        {
            if (message != "AuntenticacionFallida")
            {
                new MensajeEmergente().MostrarMensajeError(message);
            }
            else
            {
                new MensajeEmergente().MostrarMensajeError("No se logro logear con las credenciales proporcionadas " +
                                                           "en el inicio de sesión, si continua obteniendo este " +
                                                           "mensaje, cierre sesión y vuelva a logearse");
            }
            _waveOutEvent.Stop();
            OnCambioEstadoReproduccion?.Invoke(false);
            _seguidorDeEventosDelReproductor.Stop();
            _estadoReproductor = EstadoReproductor.Detenido;
            ReproducirSiguienteCancion();
        }
        
        /// <summary>
        /// Se encarga de crear el stream de reproduccion e inicializar el reproductor con los bytes recibidos
        /// </summary>
        /// <param name="songBytes">Los primeros 64000 bytes de la cancion de los cuales se obtendra la informacion de
        /// la misma</param>
        /// <param name="extension">La extension de la cancion a recibir. ejemplo mp3</param>
        private void RecibirPrimerChunkDeCancion(byte[] songBytes, string extension)
        {
            var format = GetWaveFormat(new MemoryStream(songBytes));
            _songBuffer = new MemoryStream();
            _songBuffer.Write(songBytes, 0, songBytes.Length);
            _songBuffer.Position = 0;
            _mp3StreamBuffer = new byte[format.SampleRate];
            _blockAlignedStream =
                new BlockAlignReductionStream(
                    WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(_songBuffer)));
            _waveOutEvent = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _blockAlignedStream.Position = 0;
            _waveOutEvent.Init(_blockAlignedStream);
            _seguidorDeEventosDelReproductor.Start();
            if (_estadoReproductor == EstadoReproductor.Reproduciendo)
            {
                _waveOutEvent.Play();
            }
        }

        /// <summary>
        /// Obtiene el formato de la canciona a partir de un memory stream
        /// </summary>
        /// <param name="initialBytesSong">El memory stream a utilizar para obtener la informacion</param>
        /// <returns>WaveFormat con la informacion de la cancion</returns>
        private WaveFormat GetWaveFormat(MemoryStream initialBytesSong)
        {
            WaveStream tempStream = new Mp3FileReader(initialBytesSong);
            return tempStream.WaveFormat;
        }

        /// <summary>
        /// Calcula el tiempo promedio en el que tarda en llegar el paquete
        /// </summary>
        private void CalcularTiempoRespuesta()
        {
            _cantidadPaquetesRecividos += 1;
            _tiempoTotalPaquetesRecibidos += DateTime.Now.Subtract(_horaUltimoPaqueteRecibido).Milliseconds;
            _timepoPromedioConexion = _tiempoTotalPaquetesRecibidos / _cantidadPaquetesRecividos;
            _horaUltimoPaqueteRecibido = DateTime.Now;
        }

        /// <summary>
        /// Agrega los bytes recibidos al stream de bytes donde esta el buffer de la cancion
        /// </summary>
        /// <param name="streamBytes">El stream de bytes</param>
        private void RecivedSongChunk(byte[] streamBytes)
        {
            CalcularTiempoRespuesta();
            var stream = new MemoryStream(streamBytes);
            int bytesRead;
            while ((bytesRead = stream.Read(_mp3StreamBuffer, 0, _mp3StreamBuffer.Length)) > 0)
            {
                var pos = _songBuffer.Position;
                _songBuffer.Position = _songBuffer.Length;
                _songBuffer.Write(_mp3StreamBuffer, 0, bytesRead);
                _songBuffer.Position = pos;
            }
            ReanudarReproduccion();
        }
 
        /// <summary>
        /// Reaunuda la reproduccion si se pauso
        /// </summary>
        private void ReanudarReproduccion()
        {
            if (_blockAlignedStream.CanRead)
            {
                if (_waveOutEvent.PlaybackState == PlaybackState.Stopped && _estadoReproductor == EstadoReproductor.Reproduciendo)
                {
                    _waveOutEvent.Play();
                }    
            }
        }

        /// <summary>
        /// Calcula la calidad de la cancion a recuperar a partir del tiempo promedio que le toma al sistema recibir un paquete
        /// </summary>
        /// <param name="tiempoPromedio">el tiempo promedio que le toma al sistema recibir un paquete</param>
        /// <returns>La Calidad de acuerdo al tiempo promedio</returns>
        private Calidad CalcularCalidadAObtenerCancion(long tiempoPromedio)
        {
            Calidad calidad;
            if (tiempoPromedio > 0 && tiempoPromedio <= 150)
            {
                calidad = Calidad.Alta;
            }else if (tiempoPromedio > 150 && tiempoPromedio <= 250)
            {
                calidad = Calidad.Media;
            }
            else
            {
                calidad = Calidad.Baja;
            }

            return calidad;
        } 
    }
}