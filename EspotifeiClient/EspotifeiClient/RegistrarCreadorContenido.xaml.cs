﻿using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Api.Rest.ApiClient;
using System.Net.Http;

namespace EspotifeiClient
{
    public partial class RegistrarCreadorContenido : Page
    {
        public RegistrarCreadorContenido()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Método que crea un CreadorContenido a partir de su información
        /// </summary>
        /// <returns>Variable de tipo CreadorContenido</returns>
        private CreadorContenido CrearCreadorContenido()
        {
            bool grupo = false;

            if ((bool)grupoCheckbox.IsChecked)
            {
                grupo = true;
            }

            var creadorContenido = new CreadorContenido
            {
                nombre = nombreCreadorTextbox.Text,
                biografia = biografiaTextbox.Text,
                generos = generosCreador,
                es_grupo = grupo,
            };
            return creadorContenido;
        }

        /// <summary>
        /// Método que contiene el evento para registrar un CreadorContenido al pulsar clic sobre el botón Registrar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickRegistrarCreadorButton(object sender, RoutedEventArgs e)
        {
            cancelarButton.IsEnabled = false;
            registrarCreadorButton.IsEnabled = false;
            var creador = CrearCreadorContenido();
            try
            {
                await CreadorContenidoClient.RegisterCreadorContenido(creador);
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            } catch (Exception exception)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
            }
            cancelarButton.IsEnabled = true;
            registrarCreadorButton.IsEnabled = true;
        }

        /// <summary>
        /// Método que consulta los géneros registrados en el servidor
        /// </summary>
        private async void ConsultarGeneros()
        {
            try
            {
                var listaGeneros = await GeneroClient.GetGenero();
                generosDG.ItemsSource = listaGeneros;
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            }

         
        }
    }
}
