using Model;
using System;
using System.Windows;
using System.Net.Http;
using Api.Rest;
using EspotifeiClient.Util;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Api.GrpcClients.Clients;
using System.Collections.Generic;
using System.Windows.Controls;
using Grpc.Core;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para RegistrarCreadorContenido.xaml
    /// </summary>
    public partial class RegistrarCreadorContenido
    {
        private string _rutaImagen = "";
        List<Genero> listaGenero = new List<Genero>();
        
        public RegistrarCreadorContenido()
        {
            InitializeComponent();
            DataContext = this;
            ConsultarGeneros();
        }
        
        /// <summary>
        /// Método que crea un CreadorContenido a partir de su información
        /// </summary>
        /// <returns>Variable de tipo CreadorContenido</returns>
        private CreadorContenido CrearCreadorContenido()
        { 
            var creadorContenido = new CreadorContenido
            {
                nombre = nombreCreadorTextbox.Text,
                biografia = biografiaTextbox.Text,
                generos = listaGenero,
                es_grupo = ValidarCheckBoxGrupo(),
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
            if (ValidarTextBoxNombre() && ValidarTextBoxBiografia())
            {
                cancelarButton.IsEnabled = false;
                registrarCreadorButton.IsEnabled = false;
                var creador = CrearCreadorContenido();
                var registrado = false;
                try
                {
                    var creadorContenido = await CreadorContenidoClient.RegisterCreadorContenido(creador);
                    registrado = true;
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadContentCreatorCover(_rutaImagen, creadorContenido.id);
                    }
                } catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (RpcException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se pudo guardar la imagen de portada, puede subirla " +
                                                               "mas adelante");
                }
                catch (Exception exception)
                {
                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }

                if (registrado)
                {
                    NavigationService?.Navigate(new MenuInicio());
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
                GenerosTabla.ItemsSource = listaGeneros;

            }
            catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            }
            catch (Exception ex)
            {
                new MensajeEmergente().MostrarMensajeError(ex.Message);
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
            bool esValido;
            var tamañoMinimo = 5;
            var tamañoMaximo = 500;
            var biografia = biografiaTextbox.Text;
            esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(biografia, tamañoMinimo, tamañoMaximo);
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

            if (grupoCheckbox.IsChecked != null)
            {
               grupo = (bool)grupoCheckbox.IsChecked;
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

        /// <summary>
        /// Método que permite agregar géneros a una lista al momento de seleccionar un checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AgregarGenero(object sender, RoutedEventArgs e)
        {
            var idGenero = (int) ((CheckBox) sender).Tag;
            var generoAgregar = new Genero
            {
                id = idGenero
            };
            listaGenero.Add(generoAgregar);
        }

        /// <summary>
        /// Método que permite quitar géneros de una lista al deseleccionar un checkbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void QuitarGenero(object sender, RoutedEventArgs e)
        {
            var idGenero = (int) ((CheckBox) sender).Tag;
            var generoAQuitar = listaGenero.Find(g => g.id == idGenero);
            if (generoAQuitar != null)
            {
                listaGenero.Remove(generoAQuitar);
            }
        }
    }
}
