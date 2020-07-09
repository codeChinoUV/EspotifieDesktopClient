using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
            openFile.Multiselect = true;
            if (openFile.ShowDialog()==System.Windows.Forms.DialogResult.OK)
            {
                files = openFile.SafeFileNames;
                paths = openFile.FileNames;
                for (int contador = 0; contador < files.Length; contador++)
                {
                    cancionesListbox.Items.Add(files[contador]);
                }
            }
        }

        private void CancionesListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            axMoviePlayer.FileName = paths[cancionesListbox.SelectedIndex];
            axMoviePlayer.Play();
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
            //axMoviePlayer.FileName = paths[cancionesListbox.SelectedIndex];
            axMoviePlayer.Pause();
        }

        private void Proxima_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                axMoviePlayer.FileName = paths[cancionesListbox.SelectedIndex + 1];
                cancionesListbox.SelectedIndex = cancionesListbox.SelectedIndex + 1;
                axMoviePlayer.Play();
            } catch (IndexOutOfRangeException)
            {

            }
            
        }

        private void Anterior_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                axMoviePlayer.FileName = paths[cancionesListbox.SelectedIndex - 1];
                cancionesListbox.SelectedIndex = cancionesListbox.SelectedIndex - 1;
                axMoviePlayer.Play();
            } catch (IndexOutOfRangeException)
            {

            }
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            axMoviePlayer.Play();
        }

        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
