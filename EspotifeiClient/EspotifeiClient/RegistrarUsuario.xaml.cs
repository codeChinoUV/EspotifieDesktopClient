using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Api.Rest.ApiClient;
using Model;
using Model.Enum;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para RegistrarUsuario.xaml
    /// </summary>
    public partial class RegistrarUsuario : Page
    {
        public RegistrarUsuario()
        {
            InitializeComponent();
        }

        private Usuario CrearUsuario()
        {
            var usuario = new Usuario
            {
                nombre = nombreTextbox.Text,
                contrasena = contraseniaPasswordbox.Password,
                correo_electronico = correoTextbox.Text,
                nombre_usuario = usuarioTextbox.Text,
                tipo_usuario = (TipoUsuario) tipoUsuarioCombobox.SelectedItem
            };
            return usuario;
        }
        
        private async void OnClickRegistrarButton(object sender, RoutedEventArgs e)
        {
            cancelarButton.IsEnabled = false;
            registrarUsuarioButton.IsEnabled = false; 
            var usuario = CrearUsuario();
            try
            {
                await UsuarioClient.RegisterUsuario(usuario);
            }
            catch (HttpRequestException)
            {
                new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
            }
            catch (Exception exception)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
            }
            cancelarButton.IsEnabled = true;
            registrarUsuarioButton.IsEnabled = true; 
        }
    }
}
