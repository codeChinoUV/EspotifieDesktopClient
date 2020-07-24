using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;
using Api.Rest.ApiLogin;
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
        }


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
                try
                {
                    await ApiServiceLogin.GetServiceLogin().Login(login);
                    NavigationService?.Navigate(new MenuInicio());
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (Exception exception)
                {
                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }

                ingresarButton.IsEnabled = true;
            }
            else
            {
                MessageBox.Show("Debe de ingresar los campos de usuario y contraseña", "", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private void OnClickRegistrar(object sender, MouseButtonEventArgs e)
        {
            NavigationService?.Navigate(new RegistrarUsuario());
        }
    }
}