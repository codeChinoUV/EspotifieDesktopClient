﻿using System;
using System.Collections.Generic;
using System.Linq;
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
using Model;
using Api.Rest;
using System.Net.Http;
using Api.GrpcClients.Clients;
using ManejadorDeArchivos;
using EspotifeiClient.Util;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para ListaReproduccionElementos.xaml
    /// </summary>
    public partial class ListaReproduccionElementos : Page
    {
        private ListaReproduccion _listaReproduccion;
        private Album _album;

        public ListaReproduccionElementos()
        {
            InitializeComponent();
        }

        public ListaReproduccionElementos(ListaReproduccion listaReproduccion)
        {
            InitializeComponent();
            InicializarListaReproduccion(listaReproduccion);
        }

        /// <summary>
        /// Método que obtiene los elementos de una ListaReproduccion y muestra sus elementos en la pantalla
        /// </summary>
        /// <param name="listaReproduccion">La lista de reproducción a mostrar</param>
        private async void InicializarListaReproduccion(ListaReproduccion listaReproduccion)
        {
            _listaReproduccion = listaReproduccion;
            PortadaImagen.Source = listaReproduccion.PortadaImagen;
            NombreTextBlock.Text = listaReproduccion.nombre;
            DescripcionTextBlock.Text = listaReproduccion.descripcion;
            MinutosTextBlock.Text = "Duración en minutos: "+listaReproduccion.duracion_total.ToString();
            await ObtenerCancionesDeListasReproduccion(_listaReproduccion.id);
            await ColocarImagenesAlbumes();
        }

        /// <summary>
        /// Método que recupera las canciones que pertenecen a una determinada ListaReproduccion
        /// </summary>
        /// <param name="idListaReproduccion">Identificador de lista de reproducción seleccionada</param>
        /// <returns></returns>
        private async Task ObtenerCancionesDeListasReproduccion(int idListaReproduccion)
        {
            var ocurrioExcepcion = false;
            try
            {
                _listaReproduccion.canciones = await CancionClient.GetSongsFromPlaylist(idListaReproduccion);
                CancionesListView.ItemsSource = _listaReproduccion.canciones;
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            } catch (Exception ex)
            {
                ocurrioExcepcion = true;
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }

            if (ocurrioExcepcion)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
            }
        }

        /// <summary>
        /// Método que recupera las portadas del servidor y las asigna a su canción correspondiente
        /// </summary>
        /// <returns></returns>
        private async Task ColocarImagenesAlbumes()
        {
            if (_listaReproduccion.canciones != null)
            {
                var clientePortadas = new CoversClient();
                foreach (var playlist in _listaReproduccion.canciones)
                {
                    try
                    {
                        var bitmap = await clientePortadas.GetAlbumCover(playlist.album.id, Calidad.Baja);
                        if (bitmap != null)
                        {
                            playlist.album.PortadaImagen = ImagenUtil.CrearBitmapDeMemory(bitmap);
                        } else
                        {
                           playlist.album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                        }
                        CancionesListView.ItemsSource = null;
                        CancionesListView.ItemsSource = _listaReproduccion.canciones;
                    } catch (Exception)
                    {
                        playlist.album.PortadaImagen = (BitmapImage) FindResource("AlbumDesconocido");
                    }
                }
            }

        }
    }
}