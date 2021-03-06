﻿using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Api.Rest;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para ListaReproduccionElementos.xaml
    /// </summary>
    public partial class ListaReproduccionElementos
    {
        private ListaReproduccion _listaReproduccion;

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
        ///     Método que obtiene los elementos de una ListaReproduccion y muestra sus elementos en la pantalla
        /// </summary>
        /// <param name="listaReproduccion">La lista de reproducción a mostrar</param>
        private async void InicializarListaReproduccion(ListaReproduccion listaReproduccion)
        {
            _listaReproduccion = listaReproduccion;
            PortadaImagen.Source = listaReproduccion.PortadaImagen;
            NombreTextBlock.Text = listaReproduccion.nombre;
            DescripcionTextBlock.Text = listaReproduccion.descripcion;
            MinutosTextBlock.Text = "Duración: " + listaReproduccion.duracion;
            await ObtenerCancionesDeListasReproduccion(_listaReproduccion.id);
        }

        /// <summary>
        ///     Método que recupera las canciones que pertenecen a una determinada ListaReproduccion
        /// </summary>
        /// <param name="idListaReproduccion">Identificador de lista de reproducción seleccionada</param>
        /// <returns></returns>
        private async Task ObtenerCancionesDeListasReproduccion(int idListaReproduccion)
        {
            var ocurrioExcepcion = false;
            try
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                CancionesListView.Visibility = Visibility.Visible;
                _listaReproduccion.canciones = await CancionClient.GetSongsFromPlaylist(idListaReproduccion);
                CancionesListView.ItemsSource = _listaReproduccion.canciones;
            }
            catch (HttpRequestException)
            {
                SinConexionGrid.Visibility = Visibility.Visible;
                CancionesListView.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                ocurrioExcepcion = true;
                new MensajeEmergente().MostrarMensajeError(ex.Message);
            }

            if (ocurrioExcepcion)
                new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
        }

        /// <summary>
        ///     Método que añade las canciones del creador de contenido a la cola de reproduccion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickPlayListaReproduccionButton(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().AñadirCancionesDeListaDeReproduccionACola(_listaReproduccion);
        }
    }
}