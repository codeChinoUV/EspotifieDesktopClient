using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using Grpc.Core;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para Artistas.xaml
    /// </summary>
    public partial class Artistas
    {

        private List<CreadorContenido> _creadoresContenidos;
        
        public Artistas()
        {
            InitializeComponent();
        }

        public async void BuscarCreadoresDeContenido(object sender, KeyEventArgs keyEventArgs)
        {
            var cadenaBusqueda = buscarTextBox.Text;
            var clientePortadas = new CoversClient();
            if (cadenaBusqueda != "")
            {
                try
                {
                    _creadoresContenidos = await CreadorContenidoClient.SearchCreadorContenido(cadenaBusqueda);
                    foreach (var creador in _creadoresContenidos)
                    {
                        try
                        {
                            var bitmap = await clientePortadas.GetContentCreatorCover(creador.id);
                            if (bitmap != null)
                            {
                                creador.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                            }
                            else
                            {
                                creador.PortadaImagen = (BitmapImage) FindResource("ArtistaDesconocidoImagen");
                            }
                        }
                        catch (Exception)
                        {
                            creador.PortadaImagen = (BitmapImage) FindResource("ArtistaDesconocidoImagen");
                        }
                    }

                    CreadoresDeContenidoListView.ItemsSource = _creadoresContenidos;
                }
                catch (HttpRequestException)
                {
                    //Colocar en modo sin conexion
                }
                catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }
            else
            {
                _creadoresContenidos = new List<CreadorContenido>();
            }
        }

        private void OnSelectedItem(object sender, MouseButtonEventArgs e)
        {
            var creadorDeContenido = (CreadorContenido) CreadoresDeContenidoListView.SelectedItem;
            if (creadorDeContenido != null)
            {
                NavigationService.Navigate(new ArtistaElementos(creadorDeContenido));
            }
        }
    }
}
