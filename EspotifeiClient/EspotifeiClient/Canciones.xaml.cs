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
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para Canciones.xaml
    /// </summary>
    public partial class Canciones
    {
        private List<Cancion> _canciones;

        public Canciones()
        {
            InitializeComponent();
            InicializarCanciones();
            MainWindow.MostrarMenu();
            MainWindow.MostrarReproductor();
            MainWindow.MostrarElementoMiPerfil();
        }

        /// <summary>
        ///     Recupera los generos disponibles y las primeras 5 canciones de cada genero
        /// </summary>
        private async void InicializarCanciones()
        {
            buscarTextBox.IsEnabled = false;
            var cantidadDeCancionesPorGeneros = 5;
            _canciones = new List<Cancion>();
            try
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                CancionesListView.Visibility = Visibility.Visible;
                var generos = await GeneroClient.GetGeneros();
                foreach (var genero in generos)
                {
                    var cancionDeGeneros =
                        await GeneroClient.GetCancionesOfGenero(genero.id, cantidadDeCancionesPorGeneros);
                    _canciones.AddRange(cancionDeGeneros);
                }

                CancionesListView.ItemsSource = _canciones;
                ColocarImagenesCanciones();
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                CancionesListView.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }

            buscarTextBox.IsEnabled = true;
        }

        /// <summary>
        ///     Recupera las imagenes de las canciones
        /// </summary>
        private async void ColocarImagenesCanciones()
        {
            CancionesListView.IsEnabled = false;
            var clientePortadas = new CoversClient();
            foreach (var cancion in _canciones)
                try
                {
                    var bitmap = await clientePortadas.GetAlbumCover(cancion.album.id, Calidad.Baja);
                    if (bitmap != null)
                        cancion.album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                    else
                        cancion.album.PortadaImagen = (BitmapImage) FindResource("Cancion");
                    CancionesListView.ItemsSource = null;
                    CancionesListView.ItemsSource = _canciones;
                }
                catch (Exception)
                {
                    cancion.album.PortadaImagen = (BitmapImage) FindResource("ArtistaDesconocidoImagen");
                }

            CancionesListView.IsEnabled = true;
        }

        /// <summary>
        ///     Busca las canciones que coincide con el texto introducido en el TextBox Buscar
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void BuscarCancionTextBox(object sender, KeyEventArgs e)
        {
            var cadenaBusqueda = buscarTextBox.Text;
            if (cadenaBusqueda != "")
                try
                {
                    _canciones = await CancionClient.SearchCanciones(cadenaBusqueda);
                    CancionesListView.ItemsSource = _canciones;
                    SinConexionGrid.Visibility = Visibility.Hidden;
                    CancionesListView.Visibility = Visibility.Visible;
                    ColocarImagenesCanciones();
                }
                catch (HttpRequestException)
                {
                    CancionesListView.Visibility = Visibility.Hidden;
                    SinConexionGrid.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            else
                _canciones = new List<Cancion>();
        }

        /// <summary>
        ///     Te dirige a la pantalla de ArtistaElementos del artista de la cancion seleccionada
        /// </summary>
        /// <param name="sender">El Objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickArtista(object sender, MouseButtonEventArgs e)
        {
            var idCancion = (int) ((TextBlock) sender).Tag;
            var cancion = BuscarCancionPorId(idCancion);
            if (cancion != null && cancion.creadores_de_contenido[0] != null)
                NavigationService?.Navigate(new ArtistaElementos(cancion.creadores_de_contenido[0]));
        }

        /// <summary>
        ///     Reproduce la cancion seleccionada
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlay(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancion = BuscarCancionPorId(idCancion);
            if (cancion != null) Player.Player.GetPlayer().EmpezarAReproducirCancion(cancion);
        }

        /// <summary>
        ///     Busca en la lista de canciones por el id de la cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar</param>
        /// <returns>La cancion que coincide con el id</returns>
        private Cancion BuscarCancionPorId(int idCancion)
        {
            var cancion = _canciones.Find(c => c.id == idCancion);
            return cancion;
        }
    }
}