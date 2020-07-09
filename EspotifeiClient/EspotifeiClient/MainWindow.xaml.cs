using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

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
