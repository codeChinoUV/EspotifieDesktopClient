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
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
