using System.Windows;
using Api.Rest.ApiLogin;
using Model.Enum;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MenuInicio.xaml
    /// </summary>
    public partial class MenuInicio
    {
        private static MainWindow _mainWindow;

        public MenuInicio()
        {
            InitializeComponent();
            MostrarMenu();
            MostrarReproductor();
            MostrarElementoMiPerfil();
        }

        /// <summary>
        /// Muestra el item de mi perfil
        /// </summary>
        private void MostrarElementoMiPerfil()
        {
            if (_mainWindow != null)
            {
                if (ApiServiceLogin.GetServiceLogin().Usuario != null)
                {
                    if (ApiServiceLogin.GetServiceLogin().Usuario.tipo_usuario == TipoUsuario.CreadorDeContenido)
                    {
                        _mainWindow.MiPerfilItem.Visibility = Visibility.Visible;
                    }
                }
            }
        }

        public static void MostrarMenu()
        {
            if (_mainWindow != null) _mainWindow.GridMenu.Visibility = Visibility.Visible;
        }

        public static void OcultarMenu()
        {
            if (_mainWindow != null) _mainWindow.GridMenu.Visibility = Visibility.Collapsed;
        }

        public static void MostrarReproductor()
        {
            if (_mainWindow != null) _mainWindow.Reproductor.Visibility = Visibility.Visible;
        }

        public static void OcultarReproductor()
        {
            if (_mainWindow != null) _mainWindow.Reproductor.Visibility = Visibility.Collapsed;
        }

        public static void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
    }
}