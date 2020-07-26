using System.Windows;
using System.Windows.Input;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            MenuInicio.SetMainWindow(this);
            PantallaFrame.NavigationService.Navigate(new IniciarSesion());
        }

        private void AbrirMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Collapsed;
            cerrarMenuButton.Visibility = Visibility.Visible;
        }

        private void CerrarMenuButton_Click(object sender, RoutedEventArgs e)
        {
            abrirMenuButton.Visibility = Visibility.Visible;
            cerrarMenuButton.Visibility = Visibility.Collapsed;
        }

        private void CerrarButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void BuscarListview_Selected(object sender, RoutedEventArgs e)
        {
            
        }

        private void ListaReproduccionListview_Selected(object sender, RoutedEventArgs e)
        {
            new RegistrarPlaylist().Show();
        }

        private void OnSelectedItemArtist(object sender, RoutedEventArgs e)
        {
            PantallaFrame.Navigate(new Artistas());
        }

        private void OnMiPerfilMouseClick(object sender, MouseButtonEventArgs e)
        {
            PantallaFrame.Navigate(new PerfilCreadorDeContenido());
        }
    }
}