using System;
using System.Net.Http;
using System.Windows;
using Api.Rest;
using EspotifeiClient.Util;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para RegistrarPlaylist.xaml
    /// </summary>
    public partial class RegistrarPlaylist : Window
    {
        private ListaReproduccion _listaReproduccionRegistrada;

        public RegistrarPlaylist()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Valida si la longitud del campo nombre tiene la longitud adecuada
        /// </summary>
        /// <returns>True si es valida o False si no</returns>
        private bool ValidarTamañoNombre()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombreTextbox.Text, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia("El campo de Nombre debe de tener más de " +
                                                                 $"{tamañoMinimo} y menos de {tamañoMaximo} caracteres");

            return esValido;
        }

        /// <summary>
        ///     Valida si la longitud del campo descripcion tiene la longitud adecuada
        /// </summary>
        /// <returns>True si es valida o False si no</returns>
        private bool ValidarTamañoDescripcion()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 300;
            var esValido =
                ValidacionDeCadenas.ValidarTamañoDeCadena(descripcionTextbox.Text, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia("El campo de Descripción debe de tener más de " +
                                                                 $"{tamañoMinimo} y menos de {tamañoMaximo} caracteres");

            return esValido;
        }

        /// <summary>
        ///     Crea una lista de reproducción a partir de la informacion de los campos
        /// </summary>
        /// <returns>Un Album</returns>
        private ListaReproduccion CrearListaReproduccion()
        {
            var listaReproduccion = new ListaReproduccion
            {
                nombre = nombreTextbox.Text,
                descripcion = descripcionTextbox.Text
            };
            return listaReproduccion;
        }

        /// <summary>
        ///     Método que contiene el evento de botón para registrar una lista de reproducción
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRegistrarPlaylistButton(object sender, RoutedEventArgs e)
        {
            RegistrarNuevaListaReproduccion();
        }

        /// <summary>
        ///     Método que contiene el evento de botón para cerrar la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickCancelarPlaylistButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Registra la informacion de una ListaReproduccion
        /// </summary>
        private async void RegistrarNuevaListaReproduccion()
        {
            if (ValidarTamañoNombre() && ValidarTamañoDescripcion())
            {
                cancelarPlaylistButton.IsEnabled = false;
                registrarPlaylistButton.IsEnabled = false;
                var listaReproduccion = CrearListaReproduccion();
                var listaReproduccionRegistrada = false;
                try
                {
                    _listaReproduccionRegistrada =
                        await ListaReproduccionClient.RegisterListaReproduccion(listaReproduccion);
                    listaReproduccionRegistrada = true;
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (Exception exception)
                {
                    if (exception.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con las credenciales " +
                                                                   "con las que se inició sesión ");
                        Close();
                    }

                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }

                if (listaReproduccionRegistrada) Close();
                cancelarPlaylistButton.IsEnabled = true;
                registrarPlaylistButton.IsEnabled = true;
            }
        }
    }
}