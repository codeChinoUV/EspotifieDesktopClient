using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
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
    ///     Lógica de interacción para RegistrarCreadorContenido.xaml
    /// </summary>
    public partial class RegistrarCreadorContenido
    {
        private readonly List<Genero> _listaGenero = new List<Genero>();
        private readonly bool _regresarAPerfilCreador;
        private CreadorContenido _creadorContenidoAEditar;
        private string _rutaImagen = "";

        public RegistrarCreadorContenido(bool regresarAPerfilCreador = false)
        {
            InitializeComponent();
            _regresarAPerfilCreador = regresarAPerfilCreador;
            ConsultarGeneros();
        }

        public RegistrarCreadorContenido(CreadorContenido creadorContenidoAEditar)
        {
            InitializeComponent();
            _creadorContenidoAEditar = creadorContenidoAEditar;
            ColocarElementosCreadorDeContenidoEditar();
            ConsultarGeneros();
        }

        /// <summary>
        ///     Coloca todos los elementos del creador de contenido a editar en la pantalla
        /// </summary>
        private void ColocarElementosCreadorDeContenidoEditar()
        {
            tituloLabel.Content = "EDICIÓN DE CREADOR";
            if (_creadorContenidoAEditar != null)
            {
                portadaCreadorImage.Source = _creadorContenidoAEditar.PortadaImagen;
                nombreCreadorTextbox.Text = _creadorContenidoAEditar.nombre;
                biografiaTextbox.Text = _creadorContenidoAEditar.biografia;
                grupoCheckbox.IsChecked = _creadorContenidoAEditar.es_grupo;
            }
        }

        /// <summary>
        ///     Marca como seleccionadas los generos que tenga el creador de contenido
        /// </summary>
        private List<Genero> InicializarEstadoCheckBox(List<Genero> generos)
        {
            foreach (var genero in _creadorContenidoAEditar.generos)
            {
                var index = generos?.FindIndex(g => g.id == genero.id);
                if (index != null)
                {
                    generos[(int) index].seleccionado = true;
                    _listaGenero.Add(generos[(int) index]);
                }
            }

            return generos;
        }

        /// <summary>
        ///     Actualiza los campos del creador de contenido igualandolos con la informacion en los campos
        /// </summary>
        private void ActualizarCreadorDeContenidoAPartirDeCampos()
        {
            _creadorContenidoAEditar.nombre = nombreCreadorTextbox.Text;
            _creadorContenidoAEditar.biografia = biografiaTextbox.Text;
            _creadorContenidoAEditar.es_grupo = ValidarCheckBoxGrupo();
        }

        /// <summary>
        ///     Método que crea un CreadorContenido a partir de su información
        /// </summary>
        /// <returns>Variable de tipo CreadorContenido</returns>
        private CreadorContenido CrearCreadorContenido()
        {
            var creadorContenido = new CreadorContenido
            {
                nombre = nombreCreadorTextbox.Text,
                biografia = biografiaTextbox.Text,
                generos = _listaGenero,
                es_grupo = ValidarCheckBoxGrupo()
            };
            return creadorContenido;
        }

        /// <summary>
        ///     Edita o registra un creador de contenido dependiendo de la pantalla mostrada
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickRegistrarCreadorButton(object sender, RoutedEventArgs e)
        {
            if (_creadorContenidoAEditar != null)
                EditarCreadorDeContenido();
            else
                RegistrarCreadorDeContenido();
        }

        /// <summary>
        ///     Registra en el servidor un creador de contendo
        /// </summary>
        private async void RegistrarCreadorDeContenido()
        {
            if (ValidarTextBoxNombre() && ValidarTextBoxBiografia())
            {
                cancelarButton.IsEnabled = false;
                registrarCreadorButton.IsEnabled = false;
                var creador = CrearCreadorContenido();
                var registrado = false;
                try
                {
                    creador = await CreadorContenidoClient.RegisterCreadorContenido(creador);
                    registrado = true;
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadContentCreatorCover(_rutaImagen, creador.id);
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
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                                   "proporcionadas, se cerrara la sesion");
                        MainWindow.OcultarMenu();
                        MainWindow.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeAdvertencia(ex.Message);
                    }
                }

                if (registrado)
                {
                    if (_regresarAPerfilCreador)
                        NavigationService?.Navigate(new PerfilCreadorDeContenido());
                    else
                        NavigationService?.Navigate(new Canciones());
                }

                cancelarButton.IsEnabled = true;
                registrarCreadorButton.IsEnabled = true;
            }
        }

        /// <summary>
        ///     Edita la informacion de un creador de contenido en base a lo que hay en los campos
        /// </summary>
        private async void EditarCreadorDeContenido()
        {
            if (ValidarTextBoxNombre() && ValidarTextBoxBiografia())
            {
                cancelarButton.IsEnabled = false;
                registrarCreadorButton.IsEnabled = false;
                ActualizarCreadorDeContenidoAPartirDeCampos();
                var editado = false;
                try
                {
                    _creadorContenidoAEditar =
                        await CreadorContenidoClient.EditCreadorContenido(_creadorContenidoAEditar, _listaGenero);
                    editado = true;
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadContentCreatorCover(_rutaImagen, _creadorContenidoAEditar.id);
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
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                                   "proporcionadas, se cerrara la sesion");
                        MainWindow.OcultarMenu();
                        MainWindow.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeAdvertencia(ex.Message);
                    }
                }

                if (editado) NavigationService?.Navigate(new PerfilCreadorDeContenido());
                cancelarButton.IsEnabled = true;
                registrarCreadorButton.IsEnabled = true;
            }
        }

        /// <summary>
        ///     Método que consulta los géneros registrados en el servidor
        /// </summary>
        private async void ConsultarGeneros()
        {
            try
            {
                var listaGeneros = await GeneroClient.GetGeneros();
                if (_creadorContenidoAEditar != null) listaGeneros = InicializarEstadoCheckBox(listaGeneros);
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
        ///     Método que valida el tamaño del campo nombreCreadorTextbox
        /// </summary>
        /// <returns>Verdadero si el TextBox tiene una longitud valida</returns>
        private bool ValidarTextBoxNombre()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 70;
            var nombre = nombreCreadorTextbox.Text;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombre, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de nombre debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo}");
            return esValido;
        }

        /// <summary>
        ///     Método que valida el tamaño del campo biografiaTextbox
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
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de biografía debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo}");
            return esValido;
        }

        /// <summary>
        ///     Método que valida si el CheckBox del grupo está activado o no
        /// </summary>
        /// <returns>Verdadero si el CheckBox está activado</returns>
        private bool ValidarCheckBoxGrupo()
        {
            var grupo = false;

            if (grupoCheckbox.IsChecked != null) grupo = (bool) grupoCheckbox.IsChecked;
            return grupo;
        }

        /// <summary>
        ///     Método que contiene el evento para abrir el explorador de archivos y cargar una imagen
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarImagenImage(object sender, RoutedEventArgs e)
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
                            portadaCreadorImage.Source = bitmap.Frames[0];
                            _rutaImagen = abrirImagen.FileName;
                        }
                }
                catch (Exception)
                {
                    new MensajeEmergente().MostrarMensajeError("Tipo de imagen invalida");
                }
        }

        /// <summary>
        ///     Agrega el genero seleccionado a la lista de generos del creador de contenido
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void AgregarGenero(object sender, RoutedEventArgs e)
        {
            var idGenero = (int) ((CheckBox) sender).Tag;
            var generoAgregar = new Genero
            {
                id = idGenero
            };
            _listaGenero.Add(generoAgregar);
        }

        /// <summary>
        ///     Quita el genero seleccionado de la lista de generos
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void QuitarGenero(object sender, RoutedEventArgs e)
        {
            var idGenero = (int) ((CheckBox) sender).Tag;
            var generoAQuitar = _listaGenero.Find(g => g.id == idGenero);
            if (generoAQuitar != null) _listaGenero.Remove(generoAQuitar);
        }

        /// <summary>
        ///     Navega a la pagina de Menu de inicio si se cancela la operacion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickCancelarButton(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new Canciones());
        }
    }
}