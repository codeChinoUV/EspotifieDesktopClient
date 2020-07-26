using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Api.GrpcClients.Clients;
using Api.Rest;
using EspotifeiClient.Util;
using Grpc.Core;
using Microsoft.Win32;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para RegistrarCancion.xaml
    /// </summary>
    public partial class RegistrarCancion
    {

        private List<CreadorContenido> _creadoresDeContenido = new List<CreadorContenido>();
        private List<Genero> _listaGeneros = new List<Genero>();
        private Cancion _cancionRegistrada;
        private Cancion _cancionAEditar;
        private int _idAlbum;
        
        public RegistrarCancion(int idAlbum)
        {
            InitializeComponent();
            _idAlbum = idAlbum;
            ConsultarGeneros();
        }

        public RegistrarCancion(Cancion cancionAEditar, int idAlbum)
        {
            InitializeComponent();
            _cancionAEditar = cancionAEditar;
            _idAlbum = idAlbum;
            ConsultarGeneros();
            LlenarElementosEditarCancion();
        }

        /// <summary>
        /// Regresa la cancion registrada
        /// </summary>
        /// <returns>La cancion registrada</returns>
        private Cancion GetCancionRegistrada()
        {
            return _cancionRegistrada;
        }
        
        /// <summary>
        /// Muestra la ventana para registrar la una cancion
        /// </summary>
        /// <param name="idAlbum">El id del album al que pertenece la cancion</param>
        /// <returns>La cancion registrada</returns>
        public static Cancion MostrarRegistrarCancion(int idAlbum)
        {
            var ventana = new RegistrarCancion(idAlbum);
            ventana.ShowDialog();
            return ventana.GetCancionRegistrada();
        }

        /// <summary>
        /// Muestra la ventana para registrar la una cancion
        /// </summary>
        /// <param name="cancionAEditar">La cancion a editar</param>
        /// <param name="idAlbum">El id del album al que pertenece la cancion</param>
        /// <returns>La cancion registrada</returns>
        public static Cancion MostrarEditarCancion(Cancion cancionAEditar, int idAlbum)
        {
            var ventana = new RegistrarCancion(cancionAEditar, idAlbum);
            ventana.ShowDialog();
            return ventana.GetCancionRegistrada();
        }
        
        /// <summary>
        /// Coloca la informacion de la cancion en los elementos de la pantalla
        /// </summary>
        private void LlenarElementosEditarCancion()
        {
            tituloTextBox.Content = "EDITAR CANCIÓN";
            nombreCancionTextbox.Text = _cancionAEditar.nombre;
            var propioCreadorDeContenido = _cancionAEditar.creadores_de_contenido[0];
            _cancionAEditar.creadores_de_contenido.Remove(propioCreadorDeContenido);
            LlenarListaDeCreadoresDeContenidoSeleccionadoConLosDeCancion(_cancionAEditar.creadores_de_contenido);
        }

        /// <summary>
        /// Coloca en la lista de creadores de contenido los creadores de contenido que ya tiene la cancion
        /// </summary>
        /// <param name="creadorContenidos">La lista de creadores de contenido a agregar</param>
        private void LlenarListaDeCreadoresDeContenidoSeleccionadoConLosDeCancion(
            List<CreadorContenido> creadorContenidos)
        {
            
            foreach (var creadorContenido in creadorContenidos)
            {
                _creadoresDeContenido.Add(creadorContenido);
            }

            CreadorDeContenidoSeleccionadosTabla.ItemsSource = _creadoresDeContenido;
        }

        /// <summary>
        /// Valida si el nombre tiene la longitud permitida
        /// </summary>
        /// <returns>True si es valido o False si no</returns>
        private bool ValidarNombre()
        {
            var cadena = nombreCancionTextbox.Text;
            var tamañoMinimo = 2;
            var tamañoMaximo = 70;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(cadena, tamañoMinimo, tamañoMaximo);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia($"El nombre debe de tener mas de {tamañoMinimo} " +
                                                                 $"caracteres y menos de {tamañoMaximo} caracteres");
            }

            return esValido;
        }

        /// <summary>
        /// Valida si lo por menos hay un genero seleccionado
        /// </summary>
        /// <returns>True si tiene por lo menos un genero seleccionado o False si no</returns>
        private bool ValidarGeneroSeleccionado()
        {
            var tieneGeneroSeleccionado = _listaGeneros.Count > 0;
            if (!tieneGeneroSeleccionado)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia("Debe de seleccionar por lo menos un genero");
            }

            return tieneGeneroSeleccionado;
        }

        /// <summary>
        /// Valida si ya se selecciono una cancion
        /// </summary>
        /// <returns>True si hay una cancion seleccionada o False si no</returns>
        private bool ValidarCancionSeleccionada()
        {
            var cancionSeleccionada = true;
            if (_cancionAEditar == null)
            {
                cancionSeleccionada = ArchivoSeleccionadoText.Text != "";
                if (!cancionSeleccionada)
                {
                    new MensajeEmergente().MostrarMensajeAdvertencia("Debe de seleccionar una cancion");
                }
            }
            
            return cancionSeleccionada;
        }
        
        /// <summary>
        /// Método que consulta los géneros registrados en el servidor
        /// </summary>
        private async void ConsultarGeneros()
        {
            try
            {
                var listaGeneros = await GeneroClient.GetGeneros();
                if (_cancionAEditar != null)
                {
                    listaGeneros = await InicializarEstadoCheckBox(listaGeneros);
                }
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
        /// Recupera los generos de la cancion y marca como seleccionadas los generos que tenga la cancion
        /// </summary>
        private async Task<List<Genero>> InicializarEstadoCheckBox(List<Genero> generos)
        {
            _cancionAEditar.generos = await CancionClient.GetGenerosFromCancion(_cancionAEditar.id, _idAlbum);
            foreach (var genero in _cancionAEditar.generos)
            {
                var index = generos?.FindIndex(g => g.id == genero.id);
                if (index != null)
                {
                    generos[(int)index].seleccionado = true;
                    _listaGeneros.Add(generos[(int)index]);
                }
            }
            return generos;
        }
        
        /// <summary>
        /// Agrega un artista a la lista de artistas de la cancion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarArtista(object sender, RoutedEventArgs e)
        {
            var idCreadorDeContenido = (int)((Button)sender).Tag;
            var creadoresDeContenido = (List<CreadorContenido>)CreadoreContenidoTabla.ItemsSource;
            var creadorDeContenido = creadoresDeContenido.Find(c => c.id == idCreadorDeContenido);
            if(creadorDeContenido != null)
            {
                var creadorDeContenidoEnLista =_creadoresDeContenido.Find(c => c.id==creadorDeContenido.id);
                if(creadorDeContenidoEnLista == null)
                {
                    _creadoresDeContenido.Add(creadorDeContenido);
                    CreadorDeContenidoSeleccionadosTabla.ItemsSource = null;
                    CreadorDeContenidoSeleccionadosTabla.ItemsSource = _creadoresDeContenido;
                }
            }
        }

        /// <summary>
        /// Agrega un genero a la lista de generos de la cancion
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
            _listaGeneros.Add(generoAgregar);
        }

        /// <summary>
        /// Quita un genero de la lista de generos seleccionados
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void QuitarGenero(object sender, RoutedEventArgs e)
        {
            var idGenero = (int) ((CheckBox) sender).Tag;
            var generoAQuitar = _listaGeneros.Find(g => g.id == idGenero);
            if (generoAQuitar != null) _listaGeneros.Remove(generoAQuitar);
        }

        /// <summary>
        /// Abre una ventana de selección de archivos
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
        /// Crea una cancion a partir de los elementos de la pantalla
        /// </summary>
        /// <returns>La Cancion creada</returns>
        private Cancion CrearCancion()
        {
            var cancion = new Cancion
            {
                nombre = nombreCancionTextbox.Text,
                generos = _listaGeneros,
                creadores_de_contenido = _creadoresDeContenido
            };
            return cancion;
        }

        /// <summary>
        /// Actualiza la informacion de la cancion a editar
        /// </summary>
        private void ActualizarCancionEditar()
        {
            _cancionAEditar.nombre = nombreCancionTextbox.Text;
        }

        /// <summary>
        /// Cierra la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnClickCancelarButton(object sender, RoutedEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Deshabilita todos los elementos en pantalla
        /// </summary>
        private void DeshabilitarElementosPantalla()
        {
            cancelarButton.IsEnabled = false;
            registrarAlbumButton.IsEnabled = false;
            subirCancionButton.IsEnabled = false;
            CreadoreContenidoTabla.IsEnabled = false;
            nombreCancionTextbox.IsEnabled = false;
            BuscarTextBox.IsEnabled = false;
            CreadorDeContenidoSeleccionadosTabla.IsEnabled = false;
            GenerosTabla.IsEnabled = false;
        }
        
        /// <summary>
        /// Habilita todos los elementos en pantalla
        /// </summary>
        private void HabilitarElementosPantalla()
        {
            cancelarButton.IsEnabled = true;
            registrarAlbumButton.IsEnabled = true;
            subirCancionButton.IsEnabled = true;
            CreadoreContenidoTabla.IsEnabled = true;
            nombreCancionTextbox.IsEnabled = true;
            BuscarTextBox.IsEnabled = true;
            CreadorDeContenidoSeleccionadosTabla.IsEnabled = true;
            GenerosTabla.IsEnabled = true;
        }

        /// <summary>
        /// Guarda la información de la canción y sube la canción
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickGuardatButton(object sender, RoutedEventArgs e)
        {
            if (_cancionAEditar != null)
            {
                EditarCancion();
            }
            else
            {
                RegistrarNuevaCancion();
            }
        }

        /// <summary>
        /// Edita la informacion de una cancion y sube la cancion al servidor
        /// </summary>
        private async void EditarCancion()
        {
            if (ValidarNombre() && ValidarGeneroSeleccionado() && ValidarCancionSeleccionada())
            {
                DeshabilitarElementosPantalla();
                ActualizarCancionEditar();
                try
                {
                    _cancionRegistrada = await CancionClient.EditSong(_cancionAEditar, _idAlbum, _listaGeneros, _creadoresDeContenido);
                    if (ArchivoSeleccionadoText.Text != "")
                    {
                        try
                        {
                            var clienteCanciones = new SongsClient();
                            clienteCanciones.OnPorcentageUp += SubirPorcentajeAvanza;
                            clienteCanciones.OnUploadTerminated += TerminarSubirCancion;
                            await clienteCanciones.UploadSong(ArchivoSeleccionadoText.Text, _cancionRegistrada.id, false);
                        }
                        catch (RpcException)
                        {
                            new MensajeEmergente().MostrarMensajeError("No se pudo subir la cancion, la puede volver a " +
                                                                       "subir mas adelante");
                            Close();
                        }
                        catch (Exception ex)
                        {
                            if (ex.Message == "AuntenticacionFallida")
                            {
                                new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con el usuario con el " +
                                                                           "cual inicio sesión, se guardo la informacion de la " +
                                                                           "cancion pero no el archivo");
                            }
                            else
                            {
                                new MensajeEmergente().MostrarMensajeError(ex.Message);
                            }
                            Close();
                        }
                    }else
                    {
                        new MensajeEmergente().MostrarMensajeInformacion("La cancion se ha modifico correctamente");
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
        /// Registra la informacion de una cancion y sube la cancion al servidor
        /// </summary>
        private async void RegistrarNuevaCancion()
        {
            if (ValidarNombre() && ValidarGeneroSeleccionado() && ValidarCancionSeleccionada())
            {
                DeshabilitarElementosPantalla();
                var cancionARegistrar = CrearCancion();
                try
                {
                    _cancionRegistrada = await CancionClient.RegisterSong(_idAlbum, cancionARegistrar);
                    try
                    {
                        var clienteCanciones = new SongsClient();
                        clienteCanciones.OnPorcentageUp += SubirPorcentajeAvanza;
                        clienteCanciones.OnUploadTerminated += TerminarSubirCancion;
                        await clienteCanciones.UploadSong(ArchivoSeleccionadoText.Text, _cancionRegistrada.id, false);
                    }
                    catch (RpcException)
                    {
                        new MensajeEmergente().MostrarMensajeError("No se pudo subir la cancion, la puede volver a " +
                                                                   "subir mas adelante");
                        Close();
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message == "AuntenticacionFallida")
                        {
                            new MensajeEmergente().MostrarMensajeError("No se pudo autenticar con el usuario con el " +
                                                                       "cual inicio sesión, se guardo la informacion de la " +
                                                                       "cancion pero no el archivo");
                        }
                        else
                        {
                            new MensajeEmergente().MostrarMensajeError(ex.Message);
                        }
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
        /// Muestra el mensaje de se termino de subir la cancion y cierra la ventana
        /// </summary>
        private void TerminarSubirCancion()
        {
            new MensajeEmergente().MostrarMensajeInformacion("La cancion se ha registrado correctamente");
            Close();
        }

        /// <summary>
        /// Aumenta el porcentaje de la cancion subida y el progress bar
        /// </summary>
        /// <param name="porcentage">El porcentaje</param>
        private void SubirPorcentajeAvanza(float porcentage)
        {
            int porcentajeEntero = (int) porcentage;
            PorcentajeTextBloxk.Text = $"{porcentajeEntero.ToString()}%";
            SubidaProgressbar.Value = porcentage;
        }

        /// <summary>
        /// Solicita al servidor buscar los creadores de contenido que coincida con el nombre
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnKeyUpBuscar(object sender, KeyEventArgs e)
        {
            var cadena = BuscarTextBox.Text;
            if (cadena != "")
            {
                try
                {
                    var creadoresDeContenido = await CreadorContenidoClient.SearchCreadorContenido(cadena);
                    CreadoreContenidoTabla.ItemsSource = creadoresDeContenido;
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor, verifique su conexión a " +
                                                               "internet");
                }
                catch (Exception ex)
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }
            else
            {
                CreadoreContenidoTabla.ItemsSource = null;
                CreadoreContenidoTabla.ItemsSource = new List<CreadorContenido>();
            }
            
        }

        /// <summary>
        /// Quita un Creador de Contenido de la lista de los creadores de contenido
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickQuitarArtista(object sender, RoutedEventArgs e)
        {
            var idCreadorDeContenido = (int)((Button)sender).Tag;
            var creadorDeContenido = _creadoresDeContenido.Find(c => c.id == idCreadorDeContenido);
            _creadoresDeContenido.Remove(creadorDeContenido);
            CreadorDeContenidoSeleccionadosTabla.ItemsSource = null;
            CreadorDeContenidoSeleccionadosTabla.ItemsSource = _creadoresDeContenido;
        }
    }
}