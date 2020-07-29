using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MaterialDesignThemes.Wpf;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
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

        private void ColocarElementosCancion(Cancion cancion)
        {
            if (cancion != null)
            {
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
                duracionSlider.Maximum = cancion.duracion;
                nombreCancionTextBlock.Text = cancion.nombre;
                artistaCacionTextBlock.Text = DarFormatoACreadoresDeContenidoDeCancion(cancion.creadores_de_contenido);
                coverImage.Source = cancion.album.PortadaImagen;
                tiempoTotalTextBlock.Text = cancion.duracionString;
            }
        }

        private void ColocarElementosCancionPersonal(CancionPersonal cancionPersonal)
        {
            if (cancionPersonal != null)
            {
                tiempoActualTextBlock.Text = "00:00";
                duracionSlider.Value = 0;
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
            new RegistrarPlaylist().Show();
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
    }
}