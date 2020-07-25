using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Model;
using Api.Rest;
using Api.GrpcClients.Clients;
using System.Net.Http;
using ManejadorDeArchivos;
using EspotifeiClient.Util;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para ListasReproduccion.xaml
    /// </summary>
    public partial class ListasReproduccion : Page
    {
        private List<ListaReproduccion> _listasReproduccion;
        public ListasReproduccion()
        {
            InitializeComponent();
        }

        public async void BuscarListasReproduccion(object sender, KeyEventArgs e)
        {
            var cadenaBusqueda = buscarTextBox.Text;
            if (cadenaBusqueda != "")
            {
                try
                {
                    _listasReproduccion = await ListaReproduccionClient.SearchListaReproduccion(cadenaBusqueda);
                    ListasReproduccionListView.ItemsSource = _listasReproduccion;
                    ColocarImagenesListasReproduccion();
                } catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                } catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            } else
            {
                _listasReproduccion = new List<ListaReproduccion>();
            }
        }

        /// <summary>
        /// Método que recupera las imágenes de las listas de reproducción
        /// </summary>
        private async void ColocarImagenesListasReproduccion()
        {
            ListasReproduccionListView.IsEnabled = false;
            var clientePortadas = new CoversClient();
            foreach (var playlist in _listasReproduccion)
            {
                try
                {
                    var bitmap = await clientePortadas.GetContentCreatorCover(playlist.id, Calidad.Baja);
                    if (bitmap != null)
                    {
                        playlist.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                    } else
                    {
                       playlist.PortadaImagen = (BitmapImage) FindResource("ListaDesconocidaImagen");
                    }
                } catch (Exception)
                {
                    playlist.PortadaImagen = (BitmapImage) FindResource("ListaDesconocidaImagen");
                }
            }

            ListasReproduccionListView.ItemsSource = null;
            ListasReproduccionListView.ItemsSource = _listasReproduccion;
            ListasReproduccionListView.IsEnabled = true;
        }

        /*private void OnSelectedItem(object sender, MouseButtonEventArgs e)
        {
            var creadorDeContenido = (CreadorContenido) CreadoresDeContenidoListView.SelectedItem;
            if (creadorDeContenido != null)
            {
                NavigationService?.Navigate(new ArtistaElementos(creadorDeContenido));
            }
        }*/
    }
}
