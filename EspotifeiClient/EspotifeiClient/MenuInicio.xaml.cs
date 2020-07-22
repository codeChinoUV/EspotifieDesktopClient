
using System.Windows;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para MenuInicio.xaml
    /// </summary>
    public partial class MenuInicio
    {
        public MenuInicio()
        {
            InitializeComponent();
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
            new IniciarSesion();
        }

        private void ListaReproduccionListview_Selected(object sender, RoutedEventArgs e)
        {
            new RegistrarPlaylist().Show();
        }

        private void AbrirMenuButton_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
