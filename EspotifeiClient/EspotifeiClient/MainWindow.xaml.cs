using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.ManejadorDeCancionesSinConexion;
using EspotifeiClient.ManejoUsuarios;
using EspotifeiClient.Util;
using ManejadorDeArchivos;
using MaterialDesignThemes.Wpf;
using Model;
using Model.Enum;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private static MainWindow _mainWindow;
        private int _antiguaCalificacion;
        private Cancion _cancionActual;
        private int _idCancionActual;

        public MainWindow()
        {
            InitializeComponent();
            SetMainWindow(this);
            OcultarElementosSinConexion();
            Closed += OnClose;
            PantallaFrame.NavigationService.Navigate(new IniciarSesion());
            Player.Player.GetPlayer().OnIniciaReproduccionCancion += ColocarElementosCancion;
            Player.Player.GetPlayer().OnIniciaReproduccionCancionPersonal += ColocarElementosCancionPersonal;
            Player.Player.GetPlayer().OnAvanceCancion += RecibirAvanceCancion;
            Player.Player.GetPlayer().OnCambioEstadoReproduccion += RecibirCambioEstadoReproduccion;
        }

        private void OcultarElementosSinConexion()
        {
            calificacionRatingBar.Visibility = Visibility.Collapsed;
            InicarRadioButton.Visibility = Visibility.Collapsed;
            AgregarAPlaylistButton.Visibility = Visibility.Collapsed;
            DescargarButton.Visibility = Visibility.Collapsed;
        }

        private void MostrarElementosSinConexion()
        {
            calificacionRatingBar.Visibility = Visibility.Visible;
            InicarRadioButton.Visibility = Visibility.Visible;
            AgregarAPlaylistButton.Visibility = Visibility.Visible;
            DescargarButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Revisa si el manejador de archivos no esta almancenando una cancion y guarda la informacion de los usuarios
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClose(object sender, EventArgs e)
        {
            while (!ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion().SePuedeCerrarLaApp())
                Thread.Sleep(1000);
            ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion().TerminarDeDescargarCanciones();
            ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().GuardarInformacionUsuarios();
        }

        /// <summary>
        ///     Recupera la portada del album de la cancion que se va a reproducir y se lo coloca
        /// </summary>
        /// <param name="album">El album a colocar la imagen</param>
        /// <param name="calidad">La calidad de la iamgen a recuperar</param>
        /// <returns>Task</returns>
        private async Task ColocarImagenAlbum(Album album, Calidad calidad)
        {
            var clientePortadas = new CoversClient();
            try
            {
                MostrarElementosSinConexion();
                var bitmap = await clientePortadas.GetAlbumCover(album.id, calidad);
                if (bitmap != null)
                    album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                else
                    album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
            }
            catch (Exception)
            {
                album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                OcultarElementosSinConexion();
            }
        }

        /// <summary>
        ///     Coloca la imagen del reproductor en pausa o play dependiendo del estado del reproductor
        /// </summary>
        /// <param name="estaReproducciendo">Indica si el reproductor se encuentra reproduciendo</param>
        private void RecibirCambioEstadoReproduccion(bool estaReproducciendo)
        {
            if (estaReproducciendo)
                playImage.Kind = PackIconKind.Pause;
            else
                playImage.Kind = PackIconKind.PlayArrow;
        }

        /// <summary>
        ///     Actualiza la posicion del slider de reproduccion
        /// </summary>
        /// <param name="tiempoactual">El tiempo actual de reproduccion</param>
        private void RecibirAvanceCancion(double tiempoactual)
        {
            var time = TimeSpan.FromSeconds(tiempoactual);
            duracionSlider.Value = tiempoactual;
            tiempoActualTextBlock.Text = time.ToString("mm':'ss");
        }

        /// <summary>
        ///     Coloca en el reproduccion la informacion de la cancion que se encuentra reproduciendo
        /// </summary>
        /// <param name="cancion">La cancion a colocar su informacion</param>
        private async void ColocarElementosCancion(Cancion cancion)
        {
            if (cancion != null)
            {
                _cancionActual = cancion;
                MostrarElementosSinConexion();
                _idCancionActual = cancion.id;
                ObtenerCalificacion(cancion.id);
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
                duracionSlider.Maximum = cancion.duracion;
                nombreCancionTextBlock.Text = cancion.nombre;
                artistaCacionTextBlock.Text = DarFormatoACreadoresDeContenidoDeCancion(cancion.creadores_de_contenido);
                tiempoTotalTextBlock.Text = cancion.duracionString;
                calificacionRatingBar.Visibility = Visibility.Visible;
                if (cancion.album.PortadaImagen == null) await ColocarImagenAlbum(cancion.album, Calidad.Baja);
                coverImage.Source = cancion.album.PortadaImagen;
            }
        }

        /// <summary>
        ///     Coloca la calificación en estrellas de la cancion actual
        /// </summary>
        /// <param name="idCancion">El id de la cancion a recuperar su calificacion</param>
        private async void ObtenerCalificacion(int idCancion)
        {
            calificacionRatingBar.IsEnabled = false;
            calificacionRatingBar.Visibility = Visibility.Visible;
            try
            {
                _antiguaCalificacion = (await CalificacionClient.GetCalificacion(idCancion)).calificacion_estrellas;
            }
            catch (HttpRequestException)
            {
                OcultarElementosSinConexion();
            }
            catch (Exception ex)
            {
                if (ex.Message == "NoCalificada")
                    _antiguaCalificacion = 0;
                else if (ex.Message == "AuntenticacionFallida")
                    new MensajeEmergente().MostrarMensajeError(
                        "No se ha podido logear con las credenciales proporcionadas," +
                        " si el error ocurre de nuevo, cierre y vuelva a iniciar sesion");
                else
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
            }

            calificacionRatingBar.Value = _antiguaCalificacion;
            calificacionRatingBar.IsEnabled = true;
        }

        /// <summary>
        ///     Coloca la informacion de una cancion personal en los elementos del reproductor
        /// </summary>
        /// <param name="cancionPersonal">La cancion personal a colocar su informacion</param>
        private void ColocarElementosCancionPersonal(CancionPersonal cancionPersonal)
        {
            if (cancionPersonal != null)
            {
                _cancionActual = null;
                _idCancionActual = 0;
                OcultarElementosSinConexion();
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
                duracionSlider.Maximum = cancionPersonal.duracion;
                nombreCancionTextBlock.Text = cancionPersonal.nombre;
                artistaCacionTextBlock.Text = cancionPersonal.artistas;
                coverImage.Source = (BitmapImage) FindResource("CancionPersonal");
                tiempoTotalTextBlock.Text = cancionPersonal.duracion_string;
            }
        }

        /// <summary>
        ///     Crea unn string con los nombres de los creadores de contenido de la cancion
        /// </summary>
        /// <param name="creadoresContenido">Los creadores de contenido de los cuales se creara el string</param>
        /// <returns>Un string con los nombres de los creadores de contenido</returns>
        private string DarFormatoACreadoresDeContenidoDeCancion(List<CreadorContenido> creadoresContenido)
        {
            var creadoresDeContenido = "";
            if (creadoresContenido != null)
            {
                foreach (var creadorContenido in creadoresContenido)
                    creadoresDeContenido += $"{creadorContenido.nombre}, ";
                if (creadoresDeContenido != "")
                    creadoresDeContenido = creadoresDeContenido.Substring(0, creadoresDeContenido.Length - 2);
            }

            return creadoresDeContenido;
        }

        /// <summary>
        ///     Pausa o reanuda la reproduccion de la cancion actual
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayButton(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().Play();
        }

        /// <summary>
        ///     Reproduce la siguiente cancion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickNextButton(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().ReproducirSiguienteCancion();
        }

        /// <summary>
        ///     Reproduce la cancion anterior
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickCancionAnterior(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().ReproducirCancionAnterior();
        }

        /// <summary>
        ///     Cambia el icono del reproductor y cambia el volumen de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnValueChangedVolumen(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var volumen = (int) ((Slider) sender).Value;
            if (volumen == 0)
                VolumenImage.Kind = PackIconKind.VolumeMute;
            else
                VolumenImage.Kind = PackIconKind.VolumeHigh;
            Player.Player.GetPlayer().ActualizarVolumen(volumen);
        }

        /// <summary>
        ///     Coloca el volumen de reproduccion a 0 o a 100 dependiendo del valor del slider
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickVolumenButton(object sender, RoutedEventArgs e)
        {
            var volumen = (int) VolumenSlider.Value;
            if (volumen == 0)
            {
                Player.Player.GetPlayer().ActualizarVolumen(100);
                VolumenImage.Kind = PackIconKind.VolumeHigh;
                VolumenSlider.Value = 100;
            }
            else
            {
                Player.Player.GetPlayer().ActualizarVolumen(0);
                VolumenImage.Kind = PackIconKind.VolumeMute;
                VolumenSlider.Value = 0;
            }
        }

        /// <summary>
        ///     Actualiza o registra la calificacion de una cancion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnValueChangedCalificacion(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            var calificacion = ((RatingBar) sender).Value;
            calificacionRatingBar.IsEnabled = false;
            try
            {
                if (_antiguaCalificacion == 0)
                    CalificacionClient.AddCalificacion(_idCancionActual, calificacion);
                else
                    CalificacionClient.EditCalificacion(_idCancionActual, calificacion);
                _antiguaCalificacion = calificacion;
            }
            catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se ha podido guardar la calificación, verifique su " +
                                                           "conexión a internet e intentelo nuevamente");
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }

            calificacionRatingBar.IsEnabled = true;
        }

        private async void OnClickIniciarRadio(object sender, RoutedEventArgs e)
        {
            if (_cancionActual != null)
            {
                List<Cancion> radio;
                try
                {
                    MostrarElementosSinConexion();
                    radio = await CancionClient.GetRadioFromSong(_cancionActual.id);
                    Player.Player.GetPlayer().AñadirRadioAListaDeReproduccion(radio);
                }
                catch (HttpRequestException)
                {
                    OcultarElementosSinConexion();
                }
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                                   "proporcionadas, se cerra la sesion");
                        OnClickCerrarSesion(null, null);
                        PantallaFrame.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError("Ocurrio un error y no se puede iniciar la radio");
                    }
                }
            }
        }

        private void OnClickDescargar(object sender, RoutedEventArgs e)
        {
            if (_cancionActual != null)
                ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion()
                    .AgregarCancionSinConexion(_cancionActual);
        }

        public void OnClickCerrarSesion(object sender, RoutedEventArgs routedEventArgs)
        {
            LimpiarReproductor();
            LimpiarElementosReproductor();
            ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().CerrarSesionUsuario();
            OcultarMenu();
            OcultarReproductor();
            PantallaFrame.Navigate(new IniciarSesion());
        }

        private void LimpiarReproductor()
        {
            _antiguaCalificacion = 0;
            _idCancionActual = 0;
            Player.Player.GetPlayer().LimpiarReproductor();
        }

        private void LimpiarElementosReproductor()
        {
            OcultarElementosSinConexion();
            tiempoActualTextBlock.Text = "00:00";
            duracionSlider.Value = 0;
            duracionSlider.Maximum = 100;
            nombreCancionTextBlock.Text = "";
            artistaCacionTextBlock.Text = "";
            tiempoTotalTextBlock.Text = "00:00";
            calificacionRatingBar.Visibility = Visibility.Collapsed;
            coverImage.Source = null;
        }

        /// <summary>
        ///     Despliega el menu
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void AbrirMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Collapsed;
            cerrarMenuButton.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Cierra el menu
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void CerrarMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Visible;
            cerrarMenuButton.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Cambia la pagina a la de artistas
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnSelectedItemArtist(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new Artistas());
        }

        /// <summary>
        ///     Cambia a la pagina de perfil creador de contenido
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnMiPerfilMouseClick(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new PerfilCreadorDeContenido());
        }

        /// <summary>
        ///     Cambia a la pagina de listas de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlaylists(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new ListasReproduccion());
        }

        /// <summary>
        ///     Cambia a la pagina de biblioteca personal
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickMiLibreriaButton(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new BibliotecaPersonal());
        }

        /// <summary>
        ///     Cambia a la pagina de historial
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickHistorial(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new MiHistorial());
        }

        /// <summary>
        ///     Cambia a la pagina de canciones
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickCanciones(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new Canciones());
        }

        /// <summary>
        ///     Cambia a la pagina de cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickColaReproduccion(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new ColaDeReproduccion());
        }

        /// <summary>
        ///     Cambia a la pagina de canciones sin conexion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickCancionesDescargadas(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new CancionesSinConexion());
        }

        /// <summary>
        ///     Muestra el item de mi perfil
        /// </summary>
        public static void MostrarElementoMiPerfil()
        {
            if (_mainWindow != null)
                if (ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado() != null)
                    if (ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado()
                        .tipo_usuario == TipoUsuario.CreadorDeContenido)
                        _mainWindow.MiPerfilItem.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Muestra el menu
        /// </summary>
        public static void MostrarMenu()
        {
            if (_mainWindow != null) _mainWindow.GridMenu.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Oculta el menu
        /// </summary>
        public static void OcultarMenu()
        {
            if (_mainWindow != null) _mainWindow.GridMenu.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        ///     Muestra el reproductor
        /// </summary>
        public static void MostrarReproductor()
        {
            if (_mainWindow != null) _mainWindow.Reproductor.Visibility = Visibility.Visible;
        }

        /// <summary>
        ///     Oculta el reproductor
        /// </summary>
        public static void OcultarReproductor()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Reproductor.Visibility = Visibility.Collapsed;
                _mainWindow.tiempoActualTextBlock.Text = "00:00";
                _mainWindow.duracionSlider.Value = 0;
                _mainWindow.duracionSlider.Maximum = 100;
                _mainWindow.nombreCancionTextBlock.Text = "";
                _mainWindow.artistaCacionTextBlock.Text = "";
                _mainWindow.tiempoTotalTextBlock.Text = "00:00";
                _mainWindow.calificacionRatingBar.Visibility = Visibility.Collapsed;
                _mainWindow.coverImage.Source = null;
                _mainWindow.calificacionRatingBar.Visibility = Visibility.Collapsed;
                _mainWindow.InicarRadioButton.Visibility = Visibility.Collapsed;
                _mainWindow.AgregarAPlaylistButton.Visibility = Visibility.Collapsed;
                _mainWindow.DescargarButton.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///     Establece la pantalla principal
        /// </summary>
        /// <param name="mainWindow"></param>
        public static void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
    }
}