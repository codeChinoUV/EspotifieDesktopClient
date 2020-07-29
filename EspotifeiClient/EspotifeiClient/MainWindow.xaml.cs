using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
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
            Player.Player.GetPlayer().OnAvanceCancion += RecibirAvanceCancion;
            Player.Player.GetPlayer().OnCambioEstadoReproduccion += RecibirCambioEstadoReproduccion;
        }

        private void RecibirCambioEstadoReproduccion(bool estaReproducciendo)
        {
            if (estaReproducciendo)
            {
                playImage.Kind = PackIconKind.Pause;
            }
            else
            {
                playImage.Kind = PackIconKind.PlayArrow;
            }
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

        private string DarFormatoACreadoresDeContenidoDeCancion(List<CreadorContenido> creadoresContenido)
        {
            var creadoresDeContenido = "";
            if (creadoresContenido != null)
            {
                foreach (var creadorContenido in creadoresContenido)
                {
                    creadoresDeContenido += $"{creadorContenido.nombre}, ";
                }
                if (creadoresDeContenido != "")
                {
                    creadoresDeContenido = creadoresDeContenido.Substring(0, creadoresDeContenido.Length - 2);
                }
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
    }
}