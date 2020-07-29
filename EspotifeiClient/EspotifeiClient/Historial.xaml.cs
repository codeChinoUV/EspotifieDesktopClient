using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using ManejadorDeArchivos;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MiHistorial.xaml
    /// </summary>
    public partial class MiHistorial
    {
        private List<Cancion> _canciones = new List<Cancion>();
        private int _cantidadDeDiasARecuperar;
        private int _cantidadDeResultadosARecuperar;

        public MiHistorial()
        {
            InitializeComponent();
            DesdeDatePicker.SelectedDate = DateTime.Now.Subtract(TimeSpan.FromDays(10));
            var cantidadDeResultados = (string) ((ComboBoxItem) CantidadDeResultadosComboBox.SelectedItem).Tag;
            _cantidadDeResultadosARecuperar = int.Parse(cantidadDeResultados);
            _cantidadDeDiasARecuperar = DateTime.Now.Subtract(DesdeDatePicker.SelectedDate.Value).Days;
            RecuperarHistorial(_cantidadDeDiasARecuperar, _cantidadDeResultadosARecuperar);
        }

        /// <summary>
        ///     Recupera el historial del usuario actual y lo muestra en pantalla
        /// </summary>
        /// <param name="cantidadDeDias">La cantidad de dias hacia atras, desde hoy de los cuales se obtendra el historial</param>
        /// <param name="cantidadDeResultados">La cantidad de resultados a obtener</param>
        public async void RecuperarHistorial(int cantidadDeDias, int cantidadDeResultados)
        {
            HistorialCancionesListView.IsEnabled = false;
            try
            {
                _canciones = await HistorialClient.GetHistorial(cantidadDeDias, cantidadDeResultados);
                HistorialCancionesListView.ItemsSource = _canciones;
                SinConexionGrid.Visibility = Visibility.Hidden;
                HistorialCancionesListView.Visibility = Visibility.Visible;
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                HistorialCancionesListView.Visibility = Visibility.Hidden;
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

            HistorialCancionesListView.IsEnabled = true;
        }

        /// <summary>
        ///     Recupera las imagenes de los artistas
        /// </summary>
        private async Task ColocarImagenesCanciones(Cancion cancion)
        {
            var clientePortadas = new CoversClient();
            try
            {
                var bitmap = await clientePortadas.GetAlbumCover(cancion.album.id, Calidad.Baja);
                if (bitmap != null)
                    cancion.album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                else
                    cancion.album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
            }
            catch (Exception)
            {
                cancion.album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
            }
        }

        /// <summary>
        ///     Pone a reproducir la cancion seleccionada
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickPlayCancion(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancionAReproducir = _canciones.Find(c => c.id == idCancion);
            await ColocarImagenesCanciones(cancionAReproducir);
            if (cancionAReproducir != null) Player.Player.GetPlayer().EmpezarAReproducirCancion(cancionAReproducir);
        }

        /// <summary>
        ///     Recupera el historial con la nueva cantidad de dias a recuperar
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnDateChanged(object sender, SelectionChangedEventArgs e)
        {
            var nuevaFecha = DesdeDatePicker.SelectedDate;
            if (nuevaFecha != null && nuevaFecha < DateTime.Now)
            {
                _cantidadDeDiasARecuperar = DateTime.Now.Subtract(nuevaFecha.Value).Days;
                RecuperarHistorial(_cantidadDeDiasARecuperar, _cantidadDeResultadosARecuperar);
            }
            else
            {
                new MensajeEmergente().MostrarMensajeAdvertencia("Debe de seleccionar una fecha menor a la actual");
                DesdeDatePicker.SelectedDate = DateTime.Now.Subtract(TimeSpan.FromDays(10));
            }
        }

        /// <summary>
        ///     Cambia la cantidad de resultados a obtener y solcita el historial
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnSelectionChangedCantidadDeResultados(object sender, SelectionChangedEventArgs e)
        {
            var cantidadDeResultados = (string) ((ComboBoxItem) CantidadDeResultadosComboBox.SelectedItem).Tag;
            _cantidadDeResultadosARecuperar = int.Parse(cantidadDeResultados);
            RecuperarHistorial(_cantidadDeDiasARecuperar, _cantidadDeResultadosARecuperar);
        }
    }
}