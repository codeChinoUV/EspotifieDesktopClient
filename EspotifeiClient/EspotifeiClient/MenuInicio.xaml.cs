
using System.Windows;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para MenuInicio.xaml
    /// </summary>
    public partial class MenuInicio
    {

        private static MainWindow _mainWindow;
        
        public MenuInicio()
        {
            InitializeComponent();
            MostrarMenu();
            MostrarReproductor();
        }

        public static void MostrarMenu()
        {
            if (_mainWindow != null)
            {
                _mainWindow.GridMenu.Visibility = Visibility.Visible;
            }
        }

        public static void OcultarMenu()
        {
            if (_mainWindow != null)
            {
                _mainWindow.GridMenu.Visibility = Visibility.Collapsed;
            }
        }

        public static void MostrarReproductor()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Reproductor.Visibility = Visibility.Visible;
            }
        }

        public static void OcultarReproductor()
        {
            if (_mainWindow != null)
            {
                _mainWindow.Reproductor.Visibility = Visibility.Collapsed;
            }
        }
        
        public static void SetMainWindow(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }
        
    }
}
