using Model;
using System;
using System.Windows;
using System.Net.Http;
using Api.Rest;
using EspotifeiClient.Util;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Api.GrpcClients.Clients;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para RegistrarCreadorContenido.xaml
    /// </summary>
    public partial class RegistrarCreadorContenido
    {
        private string _rutaImagen = "";
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
            /*var creadorContenido = new CreadorContenido
            {
                nombre = nombreCreadorTextbox.Text,
                biografia = biografiaTextbox.Text,
                generos = generosCreador,
                es_grupo = ValidarCheckBoxGrupo(),
            };*/
            return null;
        }

        /// <summary>
        /// Método que contiene el evento para registrar un CreadorContenido al pulsar clic sobre el botón Registrar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickRegistrarCreadorButton(object sender, RoutedEventArgs e)
        {
            if (ValidarTextBoxNombre() && ValidarTextBoxBiografia())
            {
                cancelarButton.IsEnabled = false;
                registrarCreadorButton.IsEnabled = false;
                var creador = CrearCreadorContenido();
                try
                {
                    await CreadorContenidoClient.RegisterCreadorContenido(creador);
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadUserCover(_rutaImagen);
                    }
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

        /// <summary>
        /// Método que consulta los géneros registrados en el servidor
        /// </summary>
        private async void ConsultarGeneros()
        {
            try
            {
                var listaGeneros = await GeneroClient.GetGeneros();
                generosDG.ItemsSource = listaGeneros;
            } catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            }
        }

        /// <summary>
        /// Método que valida el tamaño del campo nombreCreadorTextbox
        /// </summary>
        /// <returns>Verdadero si el TextBox tiene una longitud valida</returns>
        private bool ValidarTextBoxNombre()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 70;
            var nombre = nombreCreadorTextbox.Text;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombre, tamañoMinimo, tamañoMaximo);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de nombre debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo}");
            }
            return esValido;
        }

        /// <summary>
        /// Método que valida el tamaño del campo biografiaTextbox
        /// </summary>
        /// <returns>Verdadero si el TextBox tiene una longitud valida</returns>
        private bool ValidarTextBoxBiografia()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 500;
            var biografia = biografiaTextbox.Text;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(biografia, tamañoMinimo, tamañoMaximo);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de biografía debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo}");
            }
            return esValido;
        }

        /// <summary>
        /// Método que valida si el CheckBox del grupo está activado o no
        /// </summary>
        /// <returns>Verdadero si el CheckBox está activado</returns>
        private bool ValidarCheckBoxGrupo()
        {
            var grupo = false;

            if ((bool) grupoCheckbox.IsChecked)
            {
                grupo = true;
            } else
            {
                return grupo;
            }
            return grupo;
        }

        /// <summary>
        /// Método que contiene el evento para abrir el explorador de archivos y cargar una imagen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAgregarImagenImage(object sender, RoutedEventArgs e)
        {
            BitmapDecoder bitmap;
            var abrirImagen = new OpenFileDialog();
            abrirImagen.Filter = "Archivos png (*.png)|*.png|Archivos jpg (*.jpg)|*.jpg";
            var archivoSeleccionado = abrirImagen.ShowDialog();
            if (archivoSeleccionado != null)
            {
                try
                {
                    if ((bool) archivoSeleccionado)
                        using (var stream = abrirImagen.OpenFile())
                        {
                            bitmap = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.OnLoad);
                            portadaCreadorImage.Source = bitmap.Frames[0];
                            _rutaImagen = abrirImagen.FileName;
                        }
                } catch (Exception)
                {
                    new MensajeEmergente().MostrarMensajeError("Tipo de imagen invalida");
                }
            }
        }
    }
}
