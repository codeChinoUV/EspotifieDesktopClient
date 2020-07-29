﻿using Api.Rest;
using Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para ListasReproduccionUsuario.xaml
    /// </summary>
    public partial class ListasReproduccionUsuario : Page
    {
        private List<ListaReproduccion> _listasReproduccion;

        public ListasReproduccionUsuario()
        {
            InitializeComponent();
            ObtenerListasReproduccion();
            
        }

        /// <summary>
        /// Método que inicializa las listas de reproducción del usuario obteniendo sus canciones
        /// </summary>
        /// <returns>Una Task</returns>
        private async Task InicializarListasReproduccion()
        {
            await ObtenerCancionesDeListasReproduccion();
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
                await InicializarListasReproduccion();
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
        /// Método que obtiene las canciones pertenecientes a las listas de reproducción del usuario
        /// </summary>
        /// <returns></returns>
        private async Task ObtenerCancionesDeListasReproduccion()
        {
            if (_listasReproduccion != null)
            {
                var ocurrioExcepcion = false;
                ListaReproduccionListView.Visibility = Visibility.Visible;
                foreach (var playlist in _listasReproduccion)
                    try
                    {
                        playlist.canciones = await CancionClient.GetSongsFromPlaylist(playlist.id);
                        ListaReproduccionListView.ItemsSource = null;
                        ListaReproduccionListView.ItemsSource = _listasReproduccion;
                    } catch (HttpRequestException)
                    {
                        ListaReproduccionListView.Visibility = Visibility.Hidden;
                        new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                        break;
                    } catch (Exception ex)
                    {
                        ocurrioExcepcion = true;
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    } 

                if (ocurrioExcepcion)
                    new MensajeEmergente().MostrarMensajeAdvertencia("No se pudieron recuperar algunas canciones");
            }
        }

        /// <summary>
        /// Método que elimina la lista de reproducción seleccionada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickEliminarPlaylist(object sender, RoutedEventArgs e)
        {
            var confirmacion =
                MensajeEmergente.MostrarMensajeConfirmacion("¿Seguro que desea eliminar la lista de reproducción seleccionada?");
            if (confirmacion)
            {
                ListaReproduccionListView.Visibility = Visibility.Visible;
                int idListaReproduccion = (int) ((Button) sender).Tag;
                if (BuscarListaReproduccion(idListaReproduccion))
                {
                    try
                    {
                        await ListaReproduccionClient.DeteleListaReproduccion(idListaReproduccion);
                        ObtenerListasReproduccion();
                    } catch (HttpRequestException)
                    {
                        ListaReproduccionListView.Visibility = Visibility.Hidden;
                    } catch (Exception ex)
                    {
                        if (ex.Message == "AuntenticacionFallida")
                        {
                            new MensajeEmergente().MostrarMensajeError("No se puede autenticar con las credenciales " +
                                                                       "proporcionadas, se cerrará la sesión");
                            MenuInicio.OcultarMenu();
                            MenuInicio.OcultarReproductor();
                            NavigationService?.Navigate(new IniciarSesion());
                        } else
                        {
                            new MensajeEmergente().MostrarMensajeError(ex.Message);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Método que verifica que la lista de reproducción exista para poder eliminarla
        /// </summary>
        /// <param name="idListaReproduccion"></param>
        /// <returns></returns>
        private bool BuscarListaReproduccion(int idListaReproduccion)
        {
            bool coincide = false;
            foreach (var playlist in _listasReproduccion)
            {
                if (playlist.id == idListaReproduccion)
                {
                    coincide = true;
                    break;
                }
            }
            return coincide;
        }
    }
}
