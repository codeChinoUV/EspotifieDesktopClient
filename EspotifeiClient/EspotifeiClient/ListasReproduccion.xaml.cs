using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Api.Rest;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para ListasReproduccion.xaml
    /// </summary>
    public partial class ListasReproduccion : Page
    {
        private List<ListaReproduccion> _listasReproduccion;

        public ListasReproduccion()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Método que obtiene las listas de reproducción que coincidan con los términos de búsqueda
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void BuscarListasReproduccion(object sender, KeyEventArgs e)
        {
            var cadenaBusqueda = buscarTextBox.Text;
            if (cadenaBusqueda != "")
                try
                {
                    _listasReproduccion = await ListaReproduccionClient.SearchListaReproduccion(cadenaBusqueda);
                    ListasReproduccionListView.ItemsSource = _listasReproduccion;
                    ColocarImagenesListasReproduccion();
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            else
                _listasReproduccion = new List<ListaReproduccion>();
        }

        /// <summary>
        ///     Método que recupera las imágenes de las listas de reproducción
        /// </summary>
        private void ColocarImagenesListasReproduccion()
        {
            ListasReproduccionListView.IsEnabled = false;
            foreach (var playlist in _listasReproduccion)
                playlist.PortadaImagen = (BitmapImage) FindResource("ListaReproduccionImagen");

            ListasReproduccionListView.ItemsSource = null;
            ListasReproduccionListView.ItemsSource = _listasReproduccion;
            ListasReproduccionListView.IsEnabled = true;
        }

        /// <summary>
        ///     Método que permite seleccionar una lista de reproducción en específico y navegar a la siguiente pantalla
        ///     para consultar las canciones dentro de esa lista
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItem(object sender, MouseButtonEventArgs e)
        {
            var listaReproduccion = (ListaReproduccion) ListasReproduccionListView.SelectedItem;
            if (listaReproduccion != null)
                NavigationService?.Navigate(new ListaReproduccionElementos(listaReproduccion));
        }
    }
}