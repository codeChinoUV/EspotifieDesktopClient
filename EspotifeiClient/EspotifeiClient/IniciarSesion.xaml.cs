using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Controls;
using Api.Rest.ApiClient;
using Model;


namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para IniciarSesion.xaml
    /// </summary>
    public partial class IniciarSesion : Page
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
                    MessageBox.Show("Ocurrio un error al conectar al servidor", "", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
