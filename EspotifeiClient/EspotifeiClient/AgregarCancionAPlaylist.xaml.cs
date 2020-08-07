using Api.Rest;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para AgregarCancionAPlaylist.xaml
    /// </summary>
    public partial class AgregarCancionAPlaylist : Window
    {
        private List<ListaReproduccion> _listasReproduccion;
        private Cancion _cancion= new Cancion();

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
        /// Método que obtiene las listas de reproducción que el usuario ha creado
        /// </summary>
        private async void ObtenerListasReproduccion()
        {
            try
            {
                _listasReproduccion = await ListaReproduccionClient.GetListaReproduccion();
                ListaReproduccionListView.ItemsSource = _listasReproduccion;
                ColocarImagenesListasReproduccion();
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            } catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }
        }

        /// <summary>
        /// Método que coloca una imagen predefinida a las listas de reproducción del usuario
        /// </summary>
        private void ColocarImagenesListasReproduccion()
        {
            ListaReproduccionListView.IsEnabled = false;
            foreach (var playlist in _listasReproduccion)
            {
                playlist.PortadaImagen = (BitmapImage) FindResource("ListaDesconocida");
            }

            ListaReproduccionListView.ItemsSource = null;
            ListaReproduccionListView.ItemsSource = _listasReproduccion;
            ListaReproduccionListView.IsEnabled = true;
        }

        /// <summary>
        /// 
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
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            } catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }
        }
    }
}
