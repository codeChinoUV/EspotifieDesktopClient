﻿using System;
using System.Collections.Generic;
using System.Net.Http;
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
    ///     Lógica de interacción para Artistas.xaml
    /// </summary>
    public partial class Artistas
    {
        private List<CreadorContenido> _creadoresContenidos;

        public Artistas()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Recupera la busqueda de los creadores de contenido que coincidan con el buscarTextBox
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="keyEventArgs">El evento invocado</param>
        public async void BuscarCreadoresDeContenido(object sender, KeyEventArgs keyEventArgs)
        {
            var cadenaBusqueda = buscarTextBox.Text;
            if (cadenaBusqueda != "")
                try
                {
                    _creadoresContenidos = await CreadorContenidoClient.SearchCreadorContenido(cadenaBusqueda);
                    CreadoresDeContenidoListView.ItemsSource = _creadoresContenidos;
                    SinConexionGrid.Visibility = Visibility.Hidden;
                    CreadoresDeContenidoListView.Visibility = Visibility.Visible;
                    ColocarImagenesArtistas();
                }
                catch (HttpRequestException)
                {
                    CreadoresDeContenidoListView.Visibility = Visibility.Hidden;
                    SinConexionGrid.Visibility = Visibility.Visible;
                }
                catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            else
                _creadoresContenidos = new List<CreadorContenido>();
        }

        /// <summary>
        ///     Recupera las imagenes de los artistas
        /// </summary>
        private async void ColocarImagenesArtistas()
        {
            CreadoresDeContenidoListView.IsEnabled = false;
            var clientePortadas = new CoversClient();
            foreach (var creador in _creadoresContenidos)
                try
                {
                    var bitmap = await clientePortadas.GetContentCreatorCover(creador.id, Calidad.Baja);
                    if (bitmap != null)
                        creador.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                    else
                        creador.PortadaImagen = (BitmapImage) FindResource("ArtistaDesconocidoImagen");
                    CreadoresDeContenidoListView.ItemsSource = null;
                    CreadoresDeContenidoListView.ItemsSource = _creadoresContenidos;
                }
                catch (Exception)
                {
                    creador.PortadaImagen = (BitmapImage) FindResource("ArtistaDesconocidoImagen");
                }

            CreadoresDeContenidoListView.IsEnabled = true;
        }

        /// <summary>
        ///     Cambia a la vista de a ArtistaElemento
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedItem(object sender, MouseButtonEventArgs e)
        {
            var idCreadorContenido = (int) ((Border) sender).Tag;
            var creadorDeContenido = _creadoresContenidos.Find(c => c.id == idCreadorContenido);
            if (creadorDeContenido != null) NavigationService?.Navigate(new ArtistaElementos(creadorDeContenido));
        }
    }
}