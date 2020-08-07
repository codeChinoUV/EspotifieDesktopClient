using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Api.Rest;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para AgregarCancionAPlaylist.xaml
    /// </summary>
    public partial class AgregarCancionAPlaylist : Window
    {
        private readonly Cancion _cancion = new Cancion();
        private List<ListaReproduccion> _listasReproduccion;

        public AgregarCancionAPlaylist()
        {
            InitializeComponent();
        }

        public AgregarCancionAPlaylist(int idCancion)
        {
            InitializeComponent();
            _cancion.id = idCancion;
            ObtenerListasReproduccion();
        }

        /// <summary>
        ///     Método que obtiene las listas de reproducción que el usuario ha creado
        /// </summary>
        private async void ObtenerListasReproduccion()
        {
            try
            {
                _listasReproduccion = await ListaReproduccionClient.GetListaReproduccion();
                ListaReproduccionListView.ItemsSource = _listasReproduccion;
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
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickAgregarCancionAPlaylist(object sender, RoutedEventArgs e)
        {
            var idPlaylist = (int) ((Button) sender).Tag;
            try
            {
                var cancion = await ListaReproduccionClient.RegisterCancionAListaReproduccion(idPlaylist, _cancion.id);
                MessageBox.Show("Canción agregada a la playlist");
            }
            catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }
        }
    }
}