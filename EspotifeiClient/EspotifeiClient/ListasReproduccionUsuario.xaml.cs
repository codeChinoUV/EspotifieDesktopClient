using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Api.Rest;
using MaterialDesignThemes.Wpf;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para ListasReproduccionUsuario.xaml
    /// </summary>
    public partial class ListasReproduccionUsuario
    {
        private List<ListaReproduccion> _listasReproduccion;

        public ListasReproduccionUsuario()
        {
            InitializeComponent();
            ObtenerListasReproduccion();
        }
        

        /// <summary>
        ///     Método que obtiene las listas de reproducción que el usuario ha creado
        /// </summary>
        private async void ObtenerListasReproduccion()
        {
            try
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                ListaReproduccionListView.Visibility = Visibility.Visible;
                AgregarPlaylistButton.Visibility = Visibility.Visible;
                _listasReproduccion = await ListaReproduccionClient.GetListaReproduccion();
                ListaReproduccionListView.ItemsSource = _listasReproduccion;
                ColocarImagenesListasReproduccion();
                await ObtenerCancionesDeListasReproduccion();
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                ListaReproduccionListView.Visibility = Visibility.Hidden;
                AgregarPlaylistButton.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }
        }

        /// <summary>
        ///     Método que coloca una imagen predefinida a las listas de reproducción del usuario
        /// </summary>
        private void ColocarImagenesListasReproduccion()
        {
            ListaReproduccionListView.IsEnabled = false;
            foreach (var playlist in _listasReproduccion)
                playlist.PortadaImagen = (BitmapImage) FindResource("ListaDesconocida");

            ListaReproduccionListView.ItemsSource = null;
            ListaReproduccionListView.ItemsSource = _listasReproduccion;
            ListaReproduccionListView.IsEnabled = true;
        }

        /// <summary>
        ///     Método que obtiene las canciones pertenecientes a las listas de reproducción del usuario
        /// </summary>
        /// <returns></returns>
        private async Task ObtenerCancionesDeListasReproduccion()
        {
            if (_listasReproduccion != null)
            {
                var ocurrioExcepcion = false;
                ListaReproduccionListView.Visibility = Visibility.Visible;
                foreach (var playlist in _listasReproduccion)
                    try
                    {
                        SinConexionGrid.Visibility = Visibility.Hidden;
                        ListaReproduccionListView.Visibility = Visibility.Visible;
                        AgregarPlaylistButton.Visibility = Visibility.Visible;
                        playlist.canciones = await CancionClient.GetSongsFromPlaylist(playlist.id);
                        ListaReproduccionListView.ItemsSource = null;
                        ListaReproduccionListView.ItemsSource = _listasReproduccion;
                    }
                    catch (HttpRequestException)
                    {
                        SinConexionGrid.Visibility = Visibility.Visible;
                        ListaReproduccionListView.Visibility = Visibility.Hidden;
                        AgregarPlaylistButton.Visibility = Visibility.Hidden;
                        break;
                    }
                    catch (Exception ex)
                    {
                        ocurrioExcepcion = true;
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }

                if (ocurrioExcepcion)
                    new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
            }
        }

        /// <summary>
        ///     Método que elimina la lista de reproducción seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickEliminarPlaylist(object sender, RoutedEventArgs e)
        {
            var confirmacion =
                MensajeEmergente.MostrarMensajeConfirmacion(
                    "¿Seguro que desea eliminar la lista de reproducción seleccionada?");
            if (confirmacion)
            {
                ListaReproduccionListView.Visibility = Visibility.Visible;
                var idListaReproduccion = (int) ((Button) sender).Tag;
                if (BuscarListaReproduccion(idListaReproduccion))
                    try
                    {
                        SinConexionGrid.Visibility = Visibility.Hidden;
                        ListaReproduccionListView.Visibility = Visibility.Visible;
                        AgregarPlaylistButton.Visibility = Visibility.Visible;
                        await ListaReproduccionClient.DeteleListaReproduccion(idListaReproduccion);
                        ObtenerListasReproduccion();
                    }
                    catch (HttpRequestException)
                    {
                        SinConexionGrid.Visibility = Visibility.Visible;
                        ListaReproduccionListView.Visibility = Visibility.Hidden;
                        AgregarPlaylistButton.Visibility = Visibility.Hidden;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "AuntenticacionFallida")
                        {
                            new MensajeEmergente().MostrarMensajeError("No se puede autenticar con las credenciales " +
                                                                       "proporcionadas, se cerrará la sesión");
                            MainWindow.OcultarMenu();
                            MainWindow.OcultarReproductor();
                            NavigationService?.Navigate(new IniciarSesion());
                        }
                        else
                        {
                            new MensajeEmergente().MostrarMensajeError(ex.Message);
                        }
                    }
            }
        }

        /// <summary>
        ///     Método que elimina una canción seleccionada de la lista de reproducción en la que se encuentra el usuario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickEliminarCancionPlaylist(object sender, RoutedEventArgs e)
        {
            var confirmacion =
                MensajeEmergente.MostrarMensajeConfirmacion("¿Seguro que desea eliminar la canción seleccionada?");
            if (confirmacion)
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                ListaReproduccionListView.Visibility = Visibility.Visible;
                AgregarPlaylistButton.Visibility = Visibility.Visible;
                var idCancion = (int) ((Button) sender).Tag;
                var playlist = BuscarListaReproduccionDeCancion(idCancion);
                try
                {
                    await CancionClient.DeteleCancionPlaylist(playlist.id, idCancion);
                    ObtenerListasReproduccion();
                }
                catch (HttpRequestException)
                {
                    SinConexionGrid.Visibility = Visibility.Visible;
                    ListaReproduccionListView.Visibility = Visibility.Hidden;
                    AgregarPlaylistButton.Visibility = Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se puede autenticar con las credenciales " +
                                                                   "proporcionadas, se cerrará la sesión");
                        MainWindow.OcultarMenu();
                        MainWindow.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        ///     Método que verifica que la lista de reproducción exista para poder eliminarla
        /// </summary>
        /// <param name="idListaReproduccion">id de la lista de reproducción a buscar</param>
        /// <returns>true si la encuentra</returns>
        private bool BuscarListaReproduccion(int idListaReproduccion)
        {
            var coincide = false;
            foreach (var playlist in _listasReproduccion)
                if (playlist.id == idListaReproduccion)
                {
                    coincide = true;
                    break;
                }

            return coincide;
        }

        /// <summary>
        ///     Método que verifica que exista la canción seleccionada en la lista de reproducción
        /// </summary>
        /// <param name="idCancion">id de la canción a buscar</param>
        /// <returns>La instancia de ListaReproduccion con ese id</returns>
        private ListaReproduccion BuscarListaReproduccionDeCancion(int idCancion)
        {
            ListaReproduccion playlistDeCancion = null;
            foreach (var playlist in _listasReproduccion)
            {
                var cancion = playlist.canciones.Find(c => c.id == idCancion);
                if (cancion != null)
                {
                    playlistDeCancion = playlist;
                    break;
                }
            }

            return playlistDeCancion;
        }

        /// <summary>
        ///     Método de evento del botón que redirige a la pantalla RegistrarPlaylist.xaml
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAgregarPlaylist(object sender, RoutedEventArgs e)
        {
            new RegistrarPlaylist().ShowDialog();
            ObtenerListasReproduccion();
        }

        /// <summary>
        ///     Método que añade y reproduce las canciones de una determinada playlist
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickPlayListaReproduccionButton(object sender, RoutedEventArgs e)
        {
            var idListaReproduccion = (int) ((Button) sender).Tag;
            var listaReproduccion = _listasReproduccion.Find(a => a.id == idListaReproduccion);
            if (listaReproduccion != null)
                Player.Player.GetPlayer().AñadirCancionesDeListaDeReproduccionACola(listaReproduccion);
        }
    }
}