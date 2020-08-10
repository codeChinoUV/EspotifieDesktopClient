using System;
using System.Net.Http;
using System.Windows;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using Grpc.Core;
using Microsoft.Win32;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para RegistrarCancionPersonal.xaml
    /// </summary>
    public partial class RegistrarCancionPersonal
    {
        private CancionPersonal _cancionRegistrada;

        public RegistrarCancionPersonal()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Regresa la cancion personal registrada
        /// </summary>
        /// <returns>La cancion personal registrada</returns>
        private CancionPersonal GetCancionPersonalRegistrada()
        {
            return _cancionRegistrada;
        }

        /// <summary>
        ///     Muestra la ventana para registrar la una cancion personal
        /// </summary>
        /// <returns>La cancion personal registrada</returns>
        public static CancionPersonal MostrarRegistrarCancionPersonal()
        {
            var ventana = new RegistrarCancionPersonal();
            ventana.ShowDialog();
            return ventana.GetCancionPersonalRegistrada();
        }

        /// <summary>
        ///     Valida si el nombre tiene la longitud permitida
        /// </summary>
        /// <returns>True si es valido o False si no</returns>
        private bool ValidarNombre()
        {
            var cadena = nombreCancionTextbox.Text;
            var tamañoMinimo = 3;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(cadena, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia($"El nombre debe de tener mas de {tamañoMinimo} " +
                                                                 $"caracteres y menos de {tamañoMaximo} caracteres");

            return esValido;
        }

        /// <summary>
        ///     Valida si el campo artistas tiene la longitud permitida
        /// </summary>
        /// <returns>True si es valido o False si no</returns>
        private bool ValidarArtistas()
        {
            var cadena = artistasCancionTextbox.Text;
            var tamañoMinimo = 3;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(cadena, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de artistas debe de tener mas de {tamañoMinimo} " +
                    $"caracteres y menos de {tamañoMaximo} caracteres");

            return esValido;
        }

        /// <summary>
        ///     Valida si el album tiene la longitud permitida
        /// </summary>
        /// <returns>True si es valido o False si no</returns>
        private bool ValidarAlbum()
        {
            var cadena = albumCancionTextbox.Text;
            var tamañoMinimo = 3;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(cadena, tamañoMinimo, tamañoMaximo);
            if (!esValido)
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo del album debe de tener mas de {tamañoMinimo} " +
                    $"caracteres y menos de {tamañoMaximo} caracteres");

            return esValido;
        }

        /// <summary>
        ///     Valida si ya se selecciono una cancion
        /// </summary>
        /// <returns>True si hay una cancion seleccionada o False si no</returns>
        private bool ValidarCancionSeleccionada()
        {
            var cancionSeleccionada = ArchivoSeleccionadoText.Text != "";
            if (!cancionSeleccionada)
                new MensajeEmergente().MostrarMensajeAdvertencia("Debe de seleccionar una cancion");
            return cancionSeleccionada;
        }

        /// <summary>
        ///     Abre una ventana de selección de archivos
        /// </summary>
        /// <param name="sender">El objeto invocado</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickSeleccionarCancion(object sender, RoutedEventArgs e)
        {
            var abrirImagen = new OpenFileDialog();
            abrirImagen.Filter = "Archivos mp3 (*.mp3)|*.mp3|Archivos flac (*.flac)|*.flac";
            var archivoSeleccionado = abrirImagen.ShowDialog();
            if (archivoSeleccionado != null)
                if ((bool) archivoSeleccionado)
                {
                    ArchivoSeleccionadoText.Text = abrirImagen.FileName;
                    ArchivoSeleccionadoText.Visibility = Visibility.Visible;
                }
        }

        /// <summary>
        ///     Crea una cancion personal a partir de los elementos de la pantalla
        /// </summary>
        /// <returns>La CancionPersonal creada</returns>
        private CancionPersonal CrearCancionPersonal()
        {
            var cancion = new CancionPersonal
            {
                nombre = nombreCancionTextbox.Text,
                album = albumCancionTextbox.Text,
                artistas = artistasCancionTextbox.Text
            };
            return cancion;
        }

        /// <summary>
        ///     Cierra la ventana
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnClickCancelarButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        ///     Deshabilita todos los elementos en pantalla
        /// </summary>
        private void DeshabilitarElementosPantalla()
        {
            cancelarButton.IsEnabled = false;
            registrarAlbumButton.IsEnabled = false;
            subirCancionButton.IsEnabled = false;
            artistasCancionTextbox.IsEnabled = false;
            nombreCancionTextbox.IsEnabled = false;
            albumCancionTextbox.IsEnabled = false;
        }

        /// <summary>
        ///     Habilita todos los elementos en pantalla
        /// </summary>
        private void HabilitarElementosPantalla()
        {
            cancelarButton.IsEnabled = true;
            registrarAlbumButton.IsEnabled = true;
            subirCancionButton.IsEnabled = true;
            artistasCancionTextbox.IsEnabled = true;
            nombreCancionTextbox.IsEnabled = true;
            albumCancionTextbox.IsEnabled = true;
        }

        /// <summary>
        ///     Guarda la información de la canción personal y sube la canción personal
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickGuardatButton(object sender, RoutedEventArgs e)
        {
            RegistrarNuevaCancion();
        }

        /// <summary>
        ///     Registra la informacion de una cancion personal y sube la cancion al servidor
        /// </summary>
        private async void RegistrarNuevaCancion()
        {
            if (ValidarNombre() && ValidarArtistas() && ValidarAlbum() && ValidarCancionSeleccionada())
            {
                DeshabilitarElementosPantalla();
                var cancionARegistrar = CrearCancionPersonal();
                try
                {
                    _cancionRegistrada = await BibliotecaPersonalClient.RegisterSong(cancionARegistrar);
                    try
                    {
                        var clienteCanciones = new SongsClient();
                        clienteCanciones.OnPorcentageUp += SubirPorcentajeAvanza;
                        clienteCanciones.OnUploadTerminated += TerminarSubirCancion;
                        await clienteCanciones.UploadSong(ArchivoSeleccionadoText.Text, _cancionRegistrada.id, true);
                    }
                    catch (RpcException)
                    {
                        new MensajeEmergente().MostrarMensajeError(
                            "No se pudo subir la cancion personal, la puede volver a " +
                            "subir mas adelante");
                        Close();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "AuntenticacionFallida")
                            new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con el usuario con el " +
                                                                       "cual inicio sesión, se guardo la informacion de la " +
                                                                       "cancion pero no el archivo");
                        else
                            new MensajeEmergente().MostrarMensajeError(ex.Message);
                        Close();
                    }
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor, porfavor verifique " +
                                                               "su conexión a internet");
                }
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con el usuario con el " +
                                                                   "cual inicio sesión, no se ha guardado la canción");
                        Close();
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }
                }

                HabilitarElementosPantalla();
            }
        }

        /// <summary>
        ///     Muestra el mensaje de se termino de subir la cancion y cierra la ventana
        /// </summary>
        private void TerminarSubirCancion()
        {
            new MensajeEmergente().MostrarMensajeInformacion("La cancion se ha registrado correctamente");
            Close();
        }

        /// <summary>
        ///     Aumenta el porcentaje de la cancion subida y el progress bar
        /// </summary>
        /// <param name="porcentage">El porcentaje</param>
        private void SubirPorcentajeAvanza(float porcentage)
        {
            var porcentajeEntero = (int) porcentage;
            PorcentajeTextBloxk.Text = $"{porcentajeEntero.ToString()}%";
            SubidaProgressbar.Value = porcentage;
        }
    }
}