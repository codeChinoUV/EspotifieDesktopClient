using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using Api.Rest.ApiLogin;
using EspotifeiClient.Util;
using Grpc.Core;
using ManejadorDeArchivos;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para PerfilCreadorDeContenido.xaml
    /// </summary>
    public partial class PerfilCreadorDeContenido
    {
        private List<Album> _albums;
        private CreadorContenido _creadorContenido;

        public PerfilCreadorDeContenido()
        {
            InitializeComponent();
            InicializarCreadorDeContenido();
        }

        /// <summary>
        ///     Obtiene los elementos de un CreadorContenido y muestra sus elementos en la pantalla
        /// </summary>
        private async void InicializarCreadorDeContenido()
        {
            await RecuperarCreadorDeContenido();
            if (_creadorContenido != null)
            {
                PortadaImagen.Source = _creadorContenido.PortadaImagen;
                NombreTextBlock.Text = _creadorContenido.nombre;
                Biografia.Text = _creadorContenido.biografia;
                GenerosDataGrid.ItemsSource = _creadorContenido.generos;
                await InicializarAlbumes();
            }
        }

        /// <summary>
        ///     Recupera y muestra todos los elementos de los albumes
        /// </summary>
        /// <returns></returns>
        private async Task InicializarAlbumes()
        {
            await RecuperarAlbums(_creadorContenido.id);
            AlbumsListView.ItemsSource = _albums;
            await ObtenerCancionesDeAlbumes(_creadorContenido.id);
            AlbumsListView.ItemsSource = null;
            AlbumsListView.ItemsSource = _albums;
            await ColocarImagenesAlbumes();
            await ColocarImagenCreadorDeContenido();
        }

        /// <summary>
        ///     Recupera el creador de contenido del usuario logeado
        /// </summary>
        /// <returns>Task</returns>
        private async Task RecuperarCreadorDeContenido()
        {
            if (ApiServiceLogin.GetServiceLogin().Usuario != null)
                try
                {
                    _creadorContenido = await CreadorContenidoClient.GetCreadorContenidoFromActualUser();
                    SinConexionGrid.Visibility = Visibility.Hidden;
                    AlbumsListView.Visibility = Visibility.Visible;
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
                        MenuInicio.OcultarMenu();
                        MenuInicio.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else if (ex.Message == "SinCreadorDeContenido")
                    {
                        new MensajeEmergente().MostrarMensajeError("No tiene registrado la informacion de su " +
                                                                   "creador de contenido, a continuacion sera " +
                                                                   "redirigido a la pantalla de registro");
                        NavigationService?.Navigate(new RegistrarCreadorContenido(true));
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }
                }
        }

        /// <summary>
        ///     Recupera la imagen del creador de contenido en calidad media y la colca en la portada del creador de
        ///     contenido
        /// </summary>
        private async Task ColocarImagenCreadorDeContenido()
        {
            var clientePortadas = new CoversClient();
            try
            {
                var portada = await clientePortadas.GetContentCreatorCover(_creadorContenido.id, Calidad.Baja);
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
                    MenuInicio.OcultarMenu();
                    MenuInicio.OcultarReproductor();
                    NavigationService?.Navigate(new IniciarSesion());
                }
                else
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }
        }

        /// <summary>
        ///     Obtiene las canciones de los albumes del creador de contenido
        /// </summary>
        /// <param name="idCreadorDeContenido">El id del creador de contenido al que pertenecen los albumes</param>
        /// <returns>Una Task</returns>
        private async Task ObtenerCancionesDeAlbumes(int idCreadorDeContenido)
        {
            if (_albums != null)
            {
                var ocurrioExcepcion = false;
                SinConexionGrid.Visibility = Visibility.Hidden;
                AlbumsListView.Visibility = Visibility.Visible;
                foreach (var album in _albums)
                    try
                    {
                        album.canciones = await CancionClient.GetSongsFromAlbum(idCreadorDeContenido, album.id);
                    }
                    catch (HttpRequestException)
                    {
                        SinConexionGrid.Visibility = Visibility.Visible;
                        AlbumsListView.Visibility = Visibility.Hidden;
                        break;
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "AuntenticacionFallida")
                        {
                            new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                                       "proporcionadas, se cerrara la sesion");
                            MenuInicio.OcultarMenu();
                            MenuInicio.OcultarReproductor();
                            NavigationService?.Navigate(new IniciarSesion());
                        }
                        else
                        {
                            ocurrioExcepcion = true;
                        }
                    }

                if (ocurrioExcepcion)
                    new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
            }
        }

        /// <summary>
        ///     Recupera la imagen dOnClikEditarAlbumoca
        /// </summary>
        /// <returns></returns>
        private async Task ColocarImagenesAlbumes()
        {
            if (_albums != null)
                foreach (var album in _albums)
                    await ColocarImagenAlbum(album, Calidad.Baja);
            AlbumsListView.ItemsSource = null;
            AlbumsListView.ItemsSource = _albums;
        }

        /// <summary>
        ///     Coloca la portada a un Abum y actualiza la ListView de los Albumes
        /// </summary>
        /// <param name="album">El Album a obtener y colocar su portada</param>
        /// <param name="calidad">La calidad de la imagen a recuperar</param>
        /// <returns>Task</returns>
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
            cancion.album = BuscarAlbumDeCancion(idCancion);
            Player.Player.GetPlayer().EmpezarAReproducirCancion(cancion);
        }

        /// <summary>
        ///     Abre una ventana para el registro de un Album y si se actualiza recupera su imagen y actualiza el item source
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickAgregarAlbumButton(object sender, RoutedEventArgs e)
        {
            var albumRegistrado = RegistrarAlbum.MostrarRegistrarAlbum();
            if (albumRegistrado != null)
            {
                _albums.Add(albumRegistrado);
                await ColocarImagenAlbum(albumRegistrado, Calidad.Baja);
                AlbumsListView.ItemsSource = null;
                AlbumsListView.ItemsSource = _albums;
            }
        }

        /// <summary>
        ///     Cambia a la pagina Registrar creador de contenido
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickEditarPerfilButton(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegistrarCreadorContenido(_creadorContenido));
        }

        /// <summary>
        ///     Muestra la ventana de edicion de Album
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickEditarAlbum(object sender, RoutedEventArgs e)
        {
            AlbumsListView.IsEnabled = false;
            var idAlbum = (int) ((Button) sender).Tag;
            var alalbumAEditar = _albums.Find(a => a.id == idAlbum);
            await ColocarImagenAlbum(alalbumAEditar, Calidad.Media);
            if (alalbumAEditar != null)
            {
                var albumEditado = RegistrarAlbum.EditarAlbum(alalbumAEditar);
                AlbumsListView.IsEnabled = true;
                if (albumEditado != null) await InicializarAlbumes();
            }
        }

        /// <summary>
        ///     Muestra la ventana de registro de cancion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickAgregarCancion(object sender, RoutedEventArgs e)
        {
            var idAlbum = (int) ((Button) sender).Tag;
            var cancionRegistrada = RegistrarCancion.MostrarRegistrarCancion(idAlbum);
            if (cancionRegistrada != null) await InicializarAlbumes();
        }

        /// <summary>
        ///     Elimina la cancion seleccionada
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickEliminarCancion(object sender, RoutedEventArgs e)
        {
            var confirmacion =
                MensajeEmergente.MostrarMensajeConfirmacion("¿Seguro que desea eliminar la cancion seleccionada?");
            if (confirmacion)
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                AlbumsListView.Visibility = Visibility.Visible;
                var idCancion = (int) ((Button) sender).Tag;
                var album = BuscarAlbumDeCancion(idCancion);
                try
                {
                    await CancionClient.DeteleCancion(idCancion, album.id);
                    await InicializarAlbumes();
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
                                                                   "proporcionadas, se cerrara la sesion");
                        MenuInicio.OcultarMenu();
                        MenuInicio.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }
                }
            }
        }

        private async void OnClickEditarCancion(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var album = BuscarAlbumDeCancion(idCancion);
            var cancion = BuscarCancionEnAlbumes(idCancion);
            if (album != null && cancion != null)
            {
                var cancionEditada = RegistrarCancion.MostrarEditarCancion(cancion, album.id);
                if (cancionEditada != null) await InicializarAlbumes();
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
        ///     Manda a la cola de reproduccion las canciones del creador de contenido
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickPlayCreadorDeContenido(object sender, RoutedEventArgs e)
        {
            _creadorContenido.Albums = _albums;
            Player.Player.GetPlayer().AñadirCancionesDeCreadorDeContenidoACola(_creadorContenido);
        }

        /// <summary>
        /// Agrega la cancion a la cola de reproducción
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
        /// Manda a reproducir la radio de la cancion seleccionada
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
                    MenuInicio.OcultarMenu();
                    MenuInicio.OcultarReproductor();
                    NavigationService?.Navigate(new IniciarSesion());
                }
                else
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }
        }

        private void OnClickAgregarAPlaylist(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}