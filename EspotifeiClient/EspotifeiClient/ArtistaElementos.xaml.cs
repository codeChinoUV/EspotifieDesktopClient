using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.ManejadorDeCancionesSinConexion;
using EspotifeiClient.Util;
using Grpc.Core;
using ManejadorDeArchivos;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para ArtistaElementos.xaml
    /// </summary>
    public partial class ArtistaElementos
    {
        private List<Album> _albums;
        private CreadorContenido _creadorContenido;

        public ArtistaElementos()
        {
            InitializeComponent();
        }

        public ArtistaElementos(CreadorContenido creadorContenido)
        {
            InitializeComponent();
            InicializarCreadorDeContenido(creadorContenido);
        }

        /// <summary>
        ///     Obtiene los elementos de un CreadorContenido y muestra sus elementos en la pantalla
        /// </summary>
        /// <param name="creadorContenido">El creador de contenido a mostrar</param>
        private async void InicializarCreadorDeContenido(CreadorContenido creadorContenido)
        {
            _creadorContenido = creadorContenido;
            PortadaImagen.Source = creadorContenido.PortadaImagen;
            NombreTextBlock.Text = creadorContenido.nombre;
            Biografia.Text = creadorContenido.biografia;
            await RecuperarAlbums(creadorContenido.id);
            ColocarImagenesAlbumes();
            ColocarIamgenCreadorDeContenido();
        }

        /// <summary>
        ///     Recupera la imagen del creador de contenido en calidad media y la colca en la portada del creador de
        ///     contenido
        /// </summary>
        private async void ColocarIamgenCreadorDeContenido()
        {
            var clientePortadas = new CoversClient();
            try
            {
                var portada = await clientePortadas.GetContentCreatorCover(_creadorContenido.id, Calidad.Media);
                if (portada != null)
                {
                    _creadorContenido.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(portada);
                    PortadaImagen.Source = null;
                    PortadaImagen.Source = _creadorContenido.PortadaImagen;
                }
            }
            catch (RpcException)
            {
                PortadaImagen.Source = _creadorContenido.PortadaImagen;
            }
        }

        /// <summary>
        ///     Recupera del ApiRest los albumes del creador de contenido
        /// </summary>
        /// <param name="idCreadorContenido">El id del creador de contenido a recuperar sus albumes</param>
        /// <returns>Un Task</returns>
        private async Task RecuperarAlbums(int idCreadorContenido)
        {
            try
            {
                _albums = await AlbumClient.GetAlbumsFromContentCreator(idCreadorContenido);
                SinConexionGrid.Visibility = Visibility.Hidden;
                AlbumsListView.Visibility = Visibility.Visible;
                if (_albums == null)
                    _albums = new List<Album>();
                AlbumsListView.ItemsSource = _albums;
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                AlbumsListView.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                if (ex.Message == "AuntenticacionFallida")
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                               "proporcionadas, se cerra la sesion");
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

        /// <summary>
        ///     Recupera la imagen del Album y la coloca
        /// </summary>
        private async void ColocarImagenesAlbumes()
        {
            if (_albums != null)
            {
                var clientePortadas = new CoversClient();
                foreach (var album in _albums)
                    try
                    {
                        var bitmap = await clientePortadas.GetAlbumCover(album.id, Calidad.Baja);
                        if (bitmap != null)
                            album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                        else
                            album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                    }
                    catch (Exception)
                    {
                        album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                    }

                AlbumsListView.ItemsSource = null;
                AlbumsListView.ItemsSource = _albums;
            }
        }

        /// <summary>
        ///     Coloca en la cola de reproduccion el album entero
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento indicado</param>
        private void OnClickPlayAlbum(object sender, RoutedEventArgs e)
        {
            var idAlbum = (int) ((Button) sender).Tag;
            var album = _albums.Find(a => a.id == idAlbum);
            if (album != null) Player.Player.GetPlayer().AñadirCancionesDeAlbumACola(album);
        }

        /// <summary>
        ///     Pone a reproducir la cancion seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickPlayCancion(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancion = BuscarCancionEnAlbumes(idCancion);
            var album = BuscarAlbumDeCancion(idCancion);
            cancion.album = album;
            if (album != null) Player.Player.GetPlayer().EmpezarAReproducirCancion(cancion);
        }

        /// <summary>
        ///     Añade las canciones del creador de contenido a la cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayCreadorContenidoButto(object sender, RoutedEventArgs e)
        {
            _creadorContenido.Albums = _albums;
            Player.Player.GetPlayer().AñadirCancionesDeCreadorDeContenidoACola(_creadorContenido);
        }

        /// <summary>
        ///     Agrega la cancion a la cola de reproducción
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarACola(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancion = BuscarCancionEnAlbumes(idCancion);
            if (cancion != null)
            {
                cancion.album = BuscarAlbumDeCancion(idCancion);
                Player.Player.GetPlayer().AñadirCancionAColaDeReproduccion(cancion);
            }
        }

        /// <summary>
        ///     Busca la cancion con el idCancion dentro de los Albums
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar</param>
        /// <returns>La cancion del id cancion</returns>
        private Cancion BuscarCancionEnAlbumes(int idCancion)
        {
            Cancion cancionCoincide = null;
            foreach (var album in _albums)
            {
                var cancion = album.canciones.Find(c => c.id == idCancion);
                if (cancion != null)
                {
                    cancionCoincide = cancion;
                    break;
                }
            }

            return cancionCoincide;
        }

        /// <summary>
        ///     Busca el album en donde se encuentra contenido la cancion con el idCancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar su album</param>
        /// <returns>El album de la cancion</returns>
        private Album BuscarAlbumDeCancion(int idCancion)
        {
            Album albumDeCancion = null;
            foreach (var album in _albums)
            {
                var cancion = album.canciones.Find(c => c.id == idCancion);
                if (cancion != null)
                {
                    albumDeCancion = album;
                    break;
                }
            }

            return albumDeCancion;
        }

        /// <summary>
        ///     Manda a reproducir la radio de la cancion seleccionada
        /// </summary>
        /// <param name="sender">El objeto que invoco el eventp</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickEmpezarRadio(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            List<Cancion> radio;
            try
            {
                radio = await CancionClient.GetRadioFromSong(idCancion);
                SinConexionGrid.Visibility = Visibility.Hidden;
                AlbumsListView.Visibility = Visibility.Visible;
                Player.Player.GetPlayer().AñadirRadioAListaDeReproduccion(radio);
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                AlbumsListView.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                if (ex.Message == "AuntenticacionFallida")
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                               "proporcionadas, se cerra la sesion");
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

        /// <summary>
        /// Agrega una cancion a una lista de reproduccion
        /// </summary>
        /// <param name="sender">EL objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarAPlaylist(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            if (idCancion != 0)
            {
                new AgregarCancionAPlaylist(idCancion).ShowDialog();
            }
        }

        /// <summary>
        ///     Coloca una cancion a la cola de descargas
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickDescargar(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancion = BuscarCancionEnAlbumes(idCancion);
            if (cancion != null)
            {
                cancion.album = BuscarAlbumDeCancion(idCancion);
                ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion().AgregarCancionSinConexion(cancion);
            }
        }
    }
}