using System;
using System.Net.Http;
using System.Windows;
using System.Windows.Media.Imaging;
using Api.GrpcClients.Clients;
using Api.Rest;
using Api.Rest.ApiLogin;
using EspotifeiClient.Util;
using Grpc.Core;
using Microsoft.Win32;
using Model;
using Model.Enum;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para RegistrarUsuario.xaml
    /// </summary>
    public partial class RegistrarUsuario
    {
        private string _rutaImagen = "";
        
        public RegistrarUsuario()
        {
            InitializeComponent();
            LlenarComboBoxTiposUsuario();
            tipoUsuarioCombobox.SelectedItem = tipoUsuarioCombobox.Items[1];
        }

        /// <summary>
        /// Coloca en el ComboBox la descripcion de los Enum TipoUsuario
        /// </summary>
        private void LlenarComboBoxTiposUsuario()
        {
            tipoUsuarioCombobox.Items.Add("Creador de contenido");
            tipoUsuarioCombobox.Items.Add("Consumidor de musica");
        }

        /// <summary>
        /// Convierte un string a un TipoUsuario
        /// </summary>
        /// <param name="tipoUsuario">El string a convertir</param>
        /// <returns>El tipo de usuario</returns>
        private TipoUsuario ConvertirStringATipoUsuario(string tipoUsuario)
        {
            TipoUsuario tipoUsuarioConvertido;
            if (tipoUsuario == "Creador de contenido")
            {
                tipoUsuarioConvertido = TipoUsuario.CreadorDeContenido;
            }
            else
            {
                tipoUsuarioConvertido = TipoUsuario.ConsumidorDeMusica;
            }

            return tipoUsuarioConvertido;
        }
        
        /// <summary>
        /// Crea un nuevo usuario a partir de la información de los campos en la pagina
        /// </summary>
        /// <returns>Un objeto de tipo usuario</returns>
        private Usuario CrearUsuario()
        {
            var usuario = new Usuario
            {
                nombre = nombreTextbox.Text,
                contrasena = contraseniaPasswordbox.Password,
                correo_electronico = correoTextbox.Text,
                nombre_usuario = usuarioTextbox.Text,
                tipo_usuario = ConvertirStringATipoUsuario((String) tipoUsuarioCombobox.SelectedItem)
            };
            return usuario;
        }

        /// <summary>
        /// Valida el tamaño del campo TextBoxNombre
        /// </summary>
        /// <returns>Verdadero si el TextBox tiene una longitud valida</returns>
        private bool ValidarTextBoxNombre()
        {
            var tamañoMinimo = 5;
            var tamañoMaximo = 70;
            var nombre = nombreTextbox.Text;
            var esValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombre, tamañoMinimo, tamañoMaximo);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de nombre debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo}");
            }
            return esValido;
        }

        /// <summary>OnClickAgregarImagenButtontamaño del TextBox del Usuario tiene una longitud valida y si es alfanumerico
        /// </summary>
        /// <returns>True si cumple con las condiciones o False si no</returns>
        private bool ValidarTextBoxNombreUsuario()
        {
            var esValido = true;
            var tamañoMinimo = 5;
            var tamañoMaximo = 20;
            var nombreUsuario = usuarioTextbox.Text;
            var tamañoValido = ValidacionDeCadenas.ValidarTamañoDeCadena(nombreUsuario, tamañoMinimo, tamañoMaximo);
            var esAlfanumerica = ValidacionDeCadenas.ValidarCadenaEsAlfanumerica(nombreUsuario);
            if (!tamañoValido || !esAlfanumerica)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de usuario debe de tener mas de {tamañoMinimo} caracteres y menos de {tamañoMaximo} y " +
                    $"solo puede contener letras y numero");
                esValido = false;
            }

            return esValido;
        }

        /// <summary>
        /// Valida si la contraseña del PasswordBox tiene un formato valido
        /// </summary>
        /// <returns>True si el formato es valido o False si no</returns>
        private bool ValidarTextBoxContraseña()
        {
            var esValido = ValidacionDeCadenas.ValidarContraseña(contraseniaPasswordbox.Password);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia(
                    $"El campo de contraseña debe de tener mas de 8 caracteres y menos de 16. Debe de contener por lo " +
                    $"menos una letra mayuscula, un numero y un simbolo");
            }

            return esValido;
        }

        /// <summary>
        /// Valida si el correo del CorreoTextBox tiene un formato valido
        /// </summary>
        /// <returns>True si el formato es valido o False si no</returns>
        private bool ValidarTextBoxCorreo()
        {
            var esValido = ValidacionDeCadenas.ValidarCorreoElectronico(correoTextbox.Text);
            if (!esValido)
            {
                new MensajeEmergente().MostrarMensajeAdvertencia("Por favor introduzca un correo electronico valido");
            }

            return esValido;
        }
        
        /// <summary>
        /// Registra un nuevo usuario
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnClickRegistrarButton(object sender, RoutedEventArgs e)
        {
            if (ValidarTextBoxNombre() && ValidarTextBoxCorreo() && ValidarTextBoxNombreUsuario() && 
                ValidarTextBoxContraseña())
            {
                cancelarButton.IsEnabled = false;
                registrarUsuarioButton.IsEnabled = false; 
                var usuario = CrearUsuario();
                bool usuarioRegistrado = false;
                try
                {
                    await UsuarioClient.RegisterUsuario(usuario);
                    usuarioRegistrado = true;
                    var usuarioLogin = new Login
                    {
                        Password = usuario.contrasena,
                        User = usuario.nombre_usuario
                    };
                    await ApiServiceLogin.GetServiceLogin().Login(usuarioLogin);
                    if (_rutaImagen != "")
                    {
                        var clientePortadas = new CoversClient();
                        clientePortadas.UploadUserCover(_rutaImagen);
                    }
                }
                catch (HttpRequestException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se puede conectar al servidor");
                }
                catch (RpcException)
                {
                    new MensajeEmergente().MostrarMensajeError("No se pudo guardar la imagen de portada, puede subirla " +
                                                               "mas adelante");
                }
                catch (Exception exception)
                {
                    new MensajeEmergente().MostrarMensajeAdvertencia(exception.Message);
                }

                if (usuarioRegistrado)
                {
                    if (usuario.tipo_usuario == TipoUsuario.CreadorDeContenido)
                    {
                        PageManager.ChangePage<RegistrarCreadorContenido>();
                    }
                    else
                    {
                        PageManager.ChangePage<MenuInicio>();
                    }
                }
                cancelarButton.IsEnabled = true;
                registrarUsuarioButton.IsEnabled = true; 
            }
        }

        /// <summary>
        /// Abre un menu para seleccionar una imagen
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
            {
                try
                {
                    if ((bool) archivoSeleccionado)
                        using (var stream = abrirImagen.OpenFile())
                        {
                            bitmap = BitmapDecoder.Create(stream, BitmapCreateOptions.PreservePixelFormat,
                                BitmapCacheOption.OnLoad);
                            imagenPerfil.Source = bitmap.Frames[0];
                            _rutaImagen = abrirImagen.FileName;
                        }
                }
                catch (Exception)
                {
                    new MensajeEmergente().MostrarMensajeError("Tipo de imagen invalida");
                }
            }
        }
        
        
    }
}
