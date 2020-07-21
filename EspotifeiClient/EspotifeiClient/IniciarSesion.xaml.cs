using System;
using System.Net.Http;
using System.Windows;
using Api.Rest.Login;
using Model;


namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para IniciarSesion.xaml
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
                Login login = new Login
                {
                    User = usuarioTextbox.Text,
                    Password = contraseniaPasswordbox.Password
                };
                try
                {
                    await ApiServiceLogin.GetServiceLogin().Login(login);
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
                MessageBox.Show("Debe de ingresar los campos de usuario y contraseña", "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
            
            
            
        }
    }
}
