using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using ManejadorDeArchivos;
using MaterialDesignThemes.Wpf;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {

        private int _antiguaCalificacion = 0;
        private int _idCancionActual = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            MenuInicio.SetMainWindow(this);
            PantallaFrame.NavigationService.Navigate(new IniciarSesion());
            Player.Player.GetPlayer().OnIniciaReproduccionCancion += ColocarElementosCancion;
            Player.Player.GetPlayer().OnIniciaReproduccionCancionPersonal += ColocarElementosCancionPersonal;
            Player.Player.GetPlayer().OnAvanceCancion += RecibirAvanceCancion;
            Player.Player.GetPlayer().OnCambioEstadoReproduccion += RecibirCambioEstadoReproduccion;
        }

        private async Task ColocarImagenAlbum(Album album, Calidad calidad)
        {
            var clientePortadas = new CoversClient();
            try
            {
                var bitmap = await clientePortadas.GetAlbumCover(album.id, calidad);
                if (bitmap != null)
                    album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                else
                    album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
            }
            catch (Exception)
            {
                album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
            }
        }
        
        private void RecibirCambioEstadoReproduccion(bool estaReproducciendo)
        {
            if (estaReproducciendo)
                playImage.Kind = PackIconKind.Pause;
            else
                playImage.Kind = PackIconKind.PlayArrow;
        }

        private void RecibirAvanceCancion(double tiempoactual)
        {
            var time = TimeSpan.FromSeconds(tiempoactual);
            duracionSlider.Value = tiempoactual;
            tiempoActualTextBlock.Text = time.ToString("mm':'ss");
        }

        private async void ColocarElementosCancion(Cancion cancion)
        {
            if (cancion != null)
            {
                _idCancionActual = cancion.id;
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
                duracionSlider.Maximum = cancion.duracion;
                nombreCancionTextBlock.Text = cancion.nombre;
                artistaCacionTextBlock.Text = DarFormatoACreadoresDeContenidoDeCancion(cancion.creadores_de_contenido);
                tiempoTotalTextBlock.Text = cancion.duracionString;
                ObtenerCalificacion(cancion.id);
                calificacionRatingBar.Visibility = Visibility.Visible;
                if (cancion.album.PortadaImagen == null)
                {
                    await ColocarImagenAlbum(cancion.album, Calidad.Baja);
                }
                coverImage.Source = cancion.album.PortadaImagen;
            }
        }

        /// <summary>
        /// Coloca la calificación en estrellas de la cancion actual
        /// </summary>
        /// <param name="idCancion">El id de la cancion a recuperar su calificacion</param>
        private async void ObtenerCalificacion(int idCancion)
        {
            calificacionRatingBar.IsEnabled = false;
            try
            {
                _antiguaCalificacion = (await CalificacionClient.GetCalificacion(idCancion)).calificacion_estrellas;
            }
            catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("Verifique su conexión a internet");
            }
            catch (Exception ex)
            {
                if (ex.Message == "NoCalificada")
                {
                    _antiguaCalificacion = 0;
                }else if (ex.Message == "AuntenticacionFallida")
                {
                    new MensajeEmergente().MostrarMensajeError("No se ha podido logear con las credenciales proporcionadas," +
                                                               " si el error ocurre de nuevo, cierre y vuelva a iniciar sesion");
                }
                else
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }

            calificacionRatingBar.Value = _antiguaCalificacion;
            calificacionRatingBar.IsEnabled = true;
        }

        private void ColocarElementosCancionPersonal(CancionPersonal cancionPersonal)
        {
            if (cancionPersonal != null)
            {
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
                calificacionRatingBar.Visibility = Visibility.Collapsed;
                duracionSlider.Maximum = cancionPersonal.duracion;
                nombreCancionTextBlock.Text = cancionPersonal.nombre;
                artistaCacionTextBlock.Text = cancionPersonal.artistas;
                coverImage.Source = (BitmapImage) FindResource("CancionPersonal");
                tiempoTotalTextBlock.Text = cancionPersonal.duracion_string;
            }
        }

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

        private void AbrirMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Collapsed;
            cerrarMenuButton.Visibility = Visibility.Visible;
        }

        private void CerrarMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Visible;
            cerrarMenuButton.Visibility = Visibility.Collapsed;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BuscarListview_Selected(object sender, RoutedEventArgs e)
        {
        }

        private void ListaReproduccionListview_Selected(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new RegistrarPlaylist());
        }

        private void OnSelectedItemArtist(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new Artistas());
        }

        private void OnMiPerfilMouseClick(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new PerfilCreadorDeContenido());
        }

        private void OnClickPlaylists(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new ListasReproduccion());
        }

        private void OnClickPlayButton(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().Play();
        }

        private void OnClickNextButton(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().ReproducirSiguienteCancion();
        }

        private void OnClickCancionAnterior(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().ReproducirCancionAnterior();
        }

        /// <summary>
        ///     Cambia el icono del reproductor y cambia el volumen de reproduccion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        private void OnClickMiLibreriaButton(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new BibliotecaPersonal());
        }

        private void OnClickHistorial(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new MiHistorial());
        }

        /// <summary>
        /// Actualiza o registra la calificacion de una cancion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnValueChangedCalificacion(object sender, RoutedPropertyChangedEventArgs<int> e)
        {
            var calificacion = ((RatingBar)sender).Value;
            calificacionRatingBar.IsEnabled = false;
            try
            {
                if (_antiguaCalificacion == 0)
                {
                    CalificacionClient.AddCalificacion(_idCancionActual, calificacion);
                }
                else
                {
                    CalificacionClient.EditCalificacion(_idCancionActual, calificacion);
                }
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

        private void OnClickCanciones(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new Canciones());
        }
    }
}