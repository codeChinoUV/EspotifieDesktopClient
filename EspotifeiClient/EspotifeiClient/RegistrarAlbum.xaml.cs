using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using Grpc.Core;
using Microsoft.Win32;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para RegistrarAlbum.xaml
    /// </summary>
    public partial class RegistrarAlbum
    {

        private Album _albumRegistrado;
        private Album _albumEditar;
        private string _rutaImagen = "";
        
        public RegistrarAlbum()
        {
            InitializeComponent();
        }

        public RegistrarAlbum(Album album)
        {
            InitializeComponent();
            _albumEditar = album;
            LlenarCamposEditarAlbum();
        }
        
        /// <summary>
        /// Coloca los campos con la informacion del album a editar
        /// </summary>
        private void LlenarCamposEditarAlbum()
        {
            tituloLabel.Content = "EDITAR ALBUM";
            nombreTextbox.Text = _albumEditar.nombre;
            anioLanzamientoTextbox.Text = _albumEditar.anio_lanzamiento;
            imagenAlbum.Source = _albumEditar.PortadaImagen;
        }
        
        /// <summary>
        /// Regresa el album registrado
        /// </summary>
        /// <returns>Un Album</returns>
        public Album GetAlbumRegistrado()
        {
            return _albumRegistrado;
        }
        
        /// <summary>
        /// Muestra la pantalla para registrar un Album y regresa el album registrado
        /// </summary>
        /// <returns>El album registrado</returns>
        public static Album MostrarRegistrarAlbum()
        {
            var ventana = new RegistrarAlbum();
            ventana.ShowDialog();
            return ventana.GetAlbumRegistrado();
        }

        /// <summary>
        /// Muestra la ventana para editar un Album y regresa el album editado
        /// </summary>
        /// <param name="albumAEditar">El album a editar</param>
        /// <returns>El album editado</returns>
        public static Album EditarAlbum(Album albumAEditar)
        {
            var ventana = new RegistrarAlbum(albumAEditar);
            ventana.ShowDialog();
            return ventana.GetAlbumRegistrado();
        }
        
        /// <summary>
        /// Muestra una ventana para la seleccion de la portada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAgregarImagenButton(object sender, RoutedEventArgs e)
        {
            BitmapDecoder bitmap;
            var abrirImagen = new OpenFileDialog();
            abrirImagen.Filter = "Archivos png (*.png)|*.png|Archivos jpg (*.jpg)|*.jpg";
            var archivoSeleccionado = abrirImagen.ShowDialog();
            if (archivoSeleccionado != null)
                try
                {
                    if ((bool) archivoSeleccionado)
                        using (var stream = abrirImagen.OpenFile())
                        {
                            bitmap = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.OnLoad);
                            imagenAlbum.Source = bitmap.Frames[0];
                            _rutaImagen = abrirImagen.FileName;
                        }
                }
                catch (Exception)
                {
                    new MensajeEmergente().MostrarMensajeError("Tipo de imagen invalida");
                }
        }

        /// <summary>
        /// Valida si la longitud del campo nombre tiene la longitud adecuada
        /// </summary>
        /// <returns>True si es valida o False si no</returns>
        private bool ValidarTamañoNombre()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombreTextbox.Text, tamañoMinimo, tamañoMaximo);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia($"El campo de Nombre debe de tener mas de " +
                                                                 $"{tamañoMinimo} y menos de {tamañoMaximo} caracteres");
            }

            return esValido;
        }

        /// <summary>
        /// Valida que el campo de año de lanzamiento contengo un año valido
        /// </summary>
        /// <returns>True si el año es valido o False si no</returns>
        private bool ValidarAño()
        {
            var esValido = true;
            try
            {
                var año = Int32.Parse(anioLanzamientoTextbox.Text);
                if (año <= 1000 || año >= 3000)
                {
                    esValido = false;
                }
            }
            catch (FormatException)
            {
                esValido = false;
            }

            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia("El año de lanzamiento debe ser un numero que se " +
                                                                 "encuentre entre el año 1001 y 2999");
            }

            return esValido;
        }

        /// <summary>
        /// Crea un album a partir de la informacion de los campos
        /// </summary>
        /// <returns>Un Album</returns>
        private Album CrearAlbum()
        {
            var album = new Album
            {
                nombre = nombreTextbox.Text,
                anio_lanzamiento = anioLanzamientoTextbox.Text
            };
            return album;
        }
        
        /// <summary>
        /// Cierra la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickCancelarButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Edita o registra el album dependiendo de la pantalla invocada
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickRegistrarAlbumButton(object sender, RoutedEventArgs e)
        {
            if (_albumEditar != null)
            {
                EditarAlbum();
            }
            else
            {
                RegistrarNuevoAlbum();
            }
        }

        /// <summary>
        /// Registra la informacion de un Album y su portada
        /// </summary>
        private async void RegistrarNuevoAlbum()
        {
            if (ValidarTamañoNombre() && ValidarAño())
            {
                cancelarButton.IsEnabled = false;
                registrarAlbumButton.IsEnabled = false;
                var album = CrearAlbum();
                var albumRegistrado = false;
                try
                {
                    _albumRegistrado = await AlbumClient.RegisterAlbum(album);
                    albumRegistrado = true;
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadAlbumCover(_rutaImagen, _albumRegistrado.id );
                    }
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (RpcException)
                {
                    new MensajeEmergente().MostrarMensajeError(
                        "No se pudo guardar la imagen de portada, puede subirla " +
                        "mas adelante");
                }
                catch (Exception exception)
                {
                    if (exception.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con las credenciales " +
                                                                   "con las que se inicio sesion ");
                        Close();
                    }
                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }
                if (albumRegistrado)
                {
                    Close();
                }
                cancelarButton.IsEnabled = true;
                registrarAlbumButton.IsEnabled = true;
            }
        }

        /// <summary>
        /// Edita un album y guarda su portada
        /// </summary>
        private async void EditarAlbum()
        {
            if (ValidarTamañoNombre() && ValidarAño())
            {
                cancelarButton.IsEnabled = false;
                registrarAlbumButton.IsEnabled = false;
                var album = CrearAlbum();
                var albumEditado = false;
                try
                {
                    _albumRegistrado = await AlbumClient.EditAlbum(_albumEditar.id, album);
                    albumEditado = true;
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadAlbumCover(_rutaImagen, _albumRegistrado.id );
                    }
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (RpcException)
                {
                    new MensajeEmergente().MostrarMensajeError(
                        "No se pudo guardar la imagen de portada, puede subirla " +
                        "mas adelante");
                }
                catch (Exception exception)
                {
                    if (exception.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con las credenciales " +
                                                                   "con las que se inicio sesion ");
                        Close();
                    }
                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }
                if (albumEditado)
                {
                    Close();
                }
                cancelarButton.IsEnabled = true;
                registrarAlbumButton.IsEnabled = true;
            }
        }
    }
}