using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Api.Rest;
using Api.Rest.ApiLogin;
using EspotifeiClient.ManejoUsuarios;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para IniciarSesion.xaml
    /// </summary>
    public partial class IniciarSesion
    {
        public IniciarSesion()
        {
            InitializeComponent();
            IniciarSesionLocal();
        }

        /// <summary>
        /// Verifica si existe un usuario almacenado e inicia sesion con el usuario
        /// </summary>
        private async void IniciarSesionLocal()
        {
            var usuarioLogeado = ObtenerUsuarioLogeado();
            if (usuarioLogeado != null)
            {
                await InciarSesion(usuarioLogeado, true);
            }
        }

        /// <summary>
        /// Obtiene el usuario que tiene la sesion inciada
        /// </summary>
        /// <returns>El objeto para logearse con el usuario que tiene la sesion iniciada</returns>
        private Login ObtenerUsuarioLogeado()
        {
            Login loginUsuario = null; 
            var usuarioLogeado = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            if (usuarioLogeado != null)
            {
                loginUsuario = usuarioLogeado.login;
            }

            return loginUsuario;
        }

        /// <summary>
        /// Inicia sesion con las credenciales indicadas
        /// </summary>
        /// <param name="login">El objeto que contiene las credenciales del usuario</param>
        /// <param name="desdeLocal">Inidica si se iniciara sesion con las credenciales proporcionadas por el usuario
        /// o con credenciales almacenadas</param>
        /// <returns>Task</returns>
        private async Task InciarSesion(Login login, bool desdeLocal)
        {
            try
            {
                await ApiServiceLogin.GetServiceLogin().Login(login);
                var usuario = await UsuarioClient.GetUser();
                usuario.login = login;
                ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().InicioSesionUsuario(usuario);
                NavigationService?.Navigate(new Canciones());
            }
            catch (HttpRequestException)
            {
                if (!desdeLocal)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }else
                {
                    NavigationService?.Navigate(new Canciones());
                }
            }
            catch (Exception exception)
            {
                if (exception.Message == "AuntenticacionFallida")
                    new MensajeEmergente().MostrarMensajeError("No se pudo iniciar sesión, intentelo nuevamente");
                new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
            }
        }

        /// <summary>
        /// Recupera las credenciales del usuario, verifica que sean validas e inicia sesion con ellas
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private async void OnClickIngresarButton(object sender, RoutedEventArgs e)
        {
            if (contraseniaPasswordbox.Password != "" && usuarioTextbox.Text != "")
            {
                ingresarButton.IsEnabled = false;
                var login = new Login
                {
                    User = usuarioTextbox.Text,
                    Password = contraseniaPasswordbox.Password
                };
                await InciarSesion(login, false);
                ingresarButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Debe de ingresar los campos de usuario y contraseña", "", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        /// <summary>
        /// Cambia la pagina a la de registrar
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickRegistrar(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new RegistrarUsuario());
        }
    }
}