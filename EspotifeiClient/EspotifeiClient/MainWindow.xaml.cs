using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Api.GrpcClients.Clients;
using Api.GrpcClients.Interfaces;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        String[] paths, files;

        private void ExaminarButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.Multiselect = false;
            if (openFile.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                
                var path = openFile.FileName;
                SongsClient client = new SongsClient();
                Player.Player.GetPlayer().Play(1);
                //client.UploadSong(openFile.FileName, 8, false);
                //client.UploadSong(openFile.FileName, 1, false);
            }
        }

        private void CancionesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            nombreCancionTextblock.Text = DelimitarCadena();
        }

        private string DelimitarCadena()
        {
            string cadena = files[cancionesListbox.SelectedIndex];
            char delimitador = '.';
            string[] valores = cadena.Split(delimitador);
            string cadenaFinal = valores[0];
            return cadenaFinal;

        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Proxima_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var x = 0;
            } catch (IndexOutOfRangeException)
            {

            }
            
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                
            } catch (IndexOutOfRangeException)
            {

            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
