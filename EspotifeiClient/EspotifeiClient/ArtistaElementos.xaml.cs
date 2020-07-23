using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para ArtistaElementos.xaml
    /// </summary>
    public partial class ArtistaElementos
    {

        private List<Album> _albums;

        public ArtistaElementos()
        {
            InitializeComponent();
        }

        public ArtistaElementos(CreadorContenido creadorContenido)
        {
            InitializeComponent();
            InicializarCreadorDeContenido(creadorContenido);
        }

        private async void InicializarCreadorDeContenido(CreadorContenido creadorContenido)
        {
            PortadaImagen.Source = creadorContenido.PortadaImagen;
            NombreTextBlock.Text = creadorContenido.nombre;
            Biografia.Text = creadorContenido.biografia;
            await RecuperarAlbums(creadorContenido.id);
            await ColocarImagenesAlbumes();
            if (_albums != null)
            {
                AlbumsListView.ItemsSource = _albums;
            }
            else
            {
                AlbumsListView.ItemsSource = new List<Album>();
            }
        }

        private async Task RecuperarAlbums(int idCreadorContenido)
        {
            try
            {
                _albums = await AlbumClient.GetAlbumsFromContentCreator(idCreadorContenido);
            }
            catch (HttpRequestException)
            {
                //Poner en modo sin conexion
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
            }
        }

        private async Task ColocarImagenesAlbumes()
        {
            if (_albums != null)
            {
                var clientePortadas = new CoversClient();
                foreach (var album in _albums)
                {
                    try
                    {
                        var bitmap = await clientePortadas.GetAlbumCover(album.id);
                        if (bitmap != null)
                        {
                            album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                        }
                        else
                        {
                            album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                        }
                    }
                    catch (Exception)
                    {
                        album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                    }
                }
            }
        }

        private void OnClickPlayAlbum(object sender, RoutedEventArgs e)
        {
            var album = (Album) ((Button) sender).Tag;
            var nombre = album.nombre;
        }
    }
}
