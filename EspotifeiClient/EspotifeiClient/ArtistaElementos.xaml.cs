﻿using System;
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
using Grpc.Core;
using ManejadorDeArchivos;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para ArtistaElementos.xaml
    /// </summary>
    public partial class ArtistaElementos
    {
        private CreadorContenido _creadorContenido;
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

        /// <summary>
        /// Obtiene los elementos de un CreadorContenido y muestra sus elementos en la pantalla
        /// </summary>
        /// <param name="creadorContenido">El creador de contenido a mostrar</param>
        private async void InicializarCreadorDeContenido(CreadorContenido creadorContenido)
        {
            _creadorContenido = creadorContenido;
            PortadaImagen.Source = creadorContenido.PortadaImagen;
            NombreTextBlock.Text = creadorContenido.nombre;
            Biografia.Text = creadorContenido.biografia;
            await RecuperarAlbums(creadorContenido.id);
            await ObtenerCancionesDeAlbumes(creadorContenido.id);
            await ColocarImagenesAlbumes();
            await ColocarIamgenCreadorDeContenido();
        }

        /// <summary>
        /// Recupera la imagen del creador de contenido en calidad media y la colca en la portada del creador de
        /// contenido
        /// </summary>
        private async Task ColocarIamgenCreadorDeContenido()
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
        /// Recupera del ApiRest los albumes del creador de contenido
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
                if (_albums != null)
                {
                    AlbumsListView.ItemsSource = _albums;
                }
                else
                {
                    AlbumsListView.ItemsSource = new List<Album>();
                }
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
        /// Obtiene las canciones de los albumes del creador de contenido
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
                {
                    try
                    {
                        album.canciones = await CancionClient.GetSongsFromAlbum(idCreadorDeContenido, album.id);
                        AlbumsListView.ItemsSource = null;
                        AlbumsListView.ItemsSource = _albums;
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
                                                                       "proporcionadas, se cerra la sesion");
                            MenuInicio.OcultarMenu();
                            MenuInicio.OcultarReproductor();
                            NavigationService?.Navigate(new IniciarSesion());
                        }
                        else
                        {
                            ocurrioExcepcion = true;
                        }
                    }
                }

                if (ocurrioExcepcion)
                {
                    new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
                }
            }
        }

        /// <summary>
        /// Recupera la imagen del Album y la coloca
        /// </summary>
        /// <returns></returns>
        private async Task ColocarImagenesAlbumes()
        {
            if (_albums != null)
            {
                var clientePortadas = new CoversClient();
                foreach (var album in _albums)
                {
                    try
                    {
                        var bitmap = await clientePortadas.GetAlbumCover(album.id, Calidad.Baja);
                        if (bitmap != null)
                        {
                            album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                        }
                        else
                        {
                            album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                        }
                        AlbumsListView.ItemsSource = null;
                        AlbumsListView.ItemsSource = _albums;
                    }
                    catch (Exception)
                    {
                        album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                    }
                }
            }
        }

        /// <summary>
        /// Coloca en la cola de reproduccion el album entero
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento indicado</param>
        private void OnClickPlayAlbum(object sender, RoutedEventArgs e)
        {
            var album = (int) ((Button) sender).Tag;
            //TODO Mandar a la cola todo el album
        }

        /// <summary>
        /// Pone a reproducir la cancion seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickPlayCancion(object sender, RoutedEventArgs e)
        {
            //TODO Mandar a reproducir la cancion y borrar la cola
            var idCancion = (int) ((Button) sender).Tag;
        }
        
    }
}
