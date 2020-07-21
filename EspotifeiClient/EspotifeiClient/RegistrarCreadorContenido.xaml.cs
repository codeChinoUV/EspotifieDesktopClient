using Model;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Api.Rest.ApiClient;
using System.Net.Http;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para RegistrarCreadorContenido.xaml
    /// </summary>
    public partial class RegistrarCreadorContenido : Page
    {
        public RegistrarCreadorContenido()
        {
            InitializeComponent();
        }

        private CreadorContenido CrearCreadorContenido()
        {
            List<String> generosCreador = new List<String>();
            generosCreador.Add("Pop");
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
    }
}
