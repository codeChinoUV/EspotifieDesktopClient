﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EspotifeiClient
{
    /// <summary>
    /// Lógica de interacción para BarraNavegacion.xaml
    /// </summary>
    public partial class BarraNavegacion : Page
    {
        public BarraNavegacion()
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
