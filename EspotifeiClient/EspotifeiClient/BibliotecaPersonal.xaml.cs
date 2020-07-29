using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Api.Rest;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para BibliotecaPersonal.xaml
    /// </summary>
    public partial class BibliotecaPersonal
    {

        private List<CancionPersonal> _cancionesPersonales = new List<CancionPersonal>();

        public BibliotecaPersonal()
        {
            InitializeComponent();
            CargarCancionesEnBibliotecaPersonal();
        }

        /// <summary>
        /// Recupera las canciones de la bilioteca personal y las muestra
        /// </summary>
        private async void CargarCancionesEnBibliotecaPersonal()
        {
            try
            {
                _cancionesPersonales = await BibliotecaPersonalClient.GetCancionesPersonales();
                ColocarInformacionBibliotecaPersonal();
                ListViewCancionesPersonales.ItemsSource = _cancionesPersonales;
                ListViewCancionesPersonales.Visibility = Visibility.Visible;
                SinConexionGrid.Visibility = Visibility.Hidden;
            }
            catch (HttpRequestException)
            {
                ListViewCancionesPersonales.Visibility = Visibility.Hidden;
                SinConexionGrid.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                if (ex.Message == "AuntenticacionFallida")
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                               "proporcionadas, se cerra la sesion");
                    MenuInicio.OcultarMenu();
                    MenuInicio.OcultarReproductor();
                    NavigationService?.Navigate(new IniciarSesion());
                }
                else
                {
                    new MensajeEmergente().MostrarMensajeError(ex.Message);
                }
            }
        }

        /// <summary>
        /// Cola la informacion de la biblioteca personal en sus respectivos campos
        /// </summary>
        private void ColocarInformacionBibliotecaPersonal()
        {
            float tiempoTotal = 0;
            foreach (var cancionPersonal in _cancionesPersonales)
            {
                tiempoTotal += cancionPersonal.duracion;
            }

            var time = TimeSpan.FromSeconds(tiempoTotal);
            DuracionTotal.Text = time.ToString("mm':'ss");
            CantidadDeCanciones.Text = _cancionesPersonales.Count.ToString();
        }

        /// <summary>
        /// Reproduce una cancion de la lista de canciones de la biblioteca personal
        /// </summary>
        /// <param name="sender">El objeto invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayCancionPersonal(object sender, RoutedEventArgs e)
        {
            if (_cancionesPersonales.Count >= 250)
            {
                 new MensajeEmergente().MostrarMensajeAdvertencia("Solo tiene permitido un maximo de 250 canciones" +
                                                                  " en su biblioteca personal");   
            }
            else
            {
                int idCancion = (int) ((Button) sender).Tag;
                var cancionAReproducir = BuscarCancionPersonal(idCancion);
                if (cancionAReproducir != null)
                {
                    Player.Player.GetPlayer().EmpezarAReproducirCancionPersonal(cancionAReproducir);
                }
            }
            
        }

        /// <summary>
        /// Elimina la cancion seleccionada
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickEliminarCancion(object sender, RoutedEventArgs e)
        {
            var confirmacion =
                MensajeEmergente.MostrarMensajeConfirmacion("¿Seguro que desea eliminar la cancion seleccionada?");
            if (confirmacion)
            {
                SinConexionGrid.Visibility = Visibility.Hidden;
                ListViewCancionesPersonales.Visibility = Visibility.Visible;
                int idCancion = (int) ((Button) sender).Tag;
                try
                {
                    await BibliotecaPersonalClient.DeteleCancion(idCancion);
                    CargarCancionesEnBibliotecaPersonal();
                }
                catch (HttpRequestException)
                {
                    SinConexionGrid.Visibility = Visibility.Visible;
                    ListViewCancionesPersonales.Visibility = Visibility.Hidden;
                }
                catch (Exception ex)
                {
                    if (ex.Message == "AuntenticacionFallida")
                    {
                        new MensajeEmergente().MostrarMensajeError("No se puede autentican con las credenciales " +
                                                                   "proporcionadas, se cerrara la sesion");
                        MenuInicio.OcultarMenu();
                        MenuInicio.OcultarReproductor();
                        NavigationService?.Navigate(new IniciarSesion());
                    }
                    else
                    {
                        new MensajeEmergente().MostrarMensajeError(ex.Message);
                    }
                }
            }
        }

        /// <summary>
        /// Muestra la ventana de AgregarCancionPersonal y recarga la pantalla si el registro fue exitoso
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarCancion(object sender, RoutedEventArgs e)
        {
            var cancionRegistrada = RegistrarCancionPersonal.MostrarRegistrarCancionPersonal();
            if (cancionRegistrada != null)
            {
                CargarCancionesEnBibliotecaPersonal();
            }
        }

        /// <summary>
        /// Coloca en la cola de reproducción todas las canciones en la biblioteca personal
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayBibliotecaPersonal(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().AñadirBibliotecaPersonalACola(_cancionesPersonales);
        }

        /// <summary>
        /// Busca una cancion personal en la lista de canciones por su id
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar</param>
        /// <returns>La cancion que coincide con el id</returns>
        private CancionPersonal BuscarCancionPersonal(int idCancion)
        {
            CancionPersonal cancion = _cancionesPersonales.Find(c => c.id == idCancion);
            return cancion;
        }
    }
}
