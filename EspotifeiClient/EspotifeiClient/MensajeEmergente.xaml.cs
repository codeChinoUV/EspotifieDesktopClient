using System.Windows;
using System.Windows.Media.Imaging;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para MensajeEmergenteVentana.xaml
    /// </summary>
    public partial class MensajeEmergente
    {
        private bool _acepto;

        public MensajeEmergente()
        {
            InitializeComponent();
        }

        public MensajeEmergente(string mensaje)
        {
            InitializeComponent();
            MensajeTextBlock.Text = "Confirmacion";
            CuerpoMensajeTextBlock.Text = mensaje;
            MensajeImagen.Source = (BitmapImage) FindResource("ConfirmacionImagen");
            cancelarButton.Visibility = Visibility.Visible;
        }

        public bool GetResultado()
        {
            return _acepto;
        }

        /// <summary>
        ///     Muestra un mensaje de confirmacion
        /// </summary>
        /// <param name="cuerpoMensaje">El mensaje a mostrar</param>
        /// <returns>True si se acepto o False si se cancelo</returns>
        public static bool MostrarMensajeConfirmacion(string cuerpoMensaje)
        {
            var ventana = new MensajeEmergente(cuerpoMensaje);
            ventana.ShowDialog();
            return ventana.GetResultado();
        }

        /// <summary>
        ///     Muestra un mensaje de advertencia
        /// </summary>
        /// <param name="cuerpoMensaje">El mensaje a mostrar</param>
        public void MostrarMensajeAdvertencia(string cuerpoMensaje)
        {
            MensajeTextBlock.Text = "Advertencia";
            CuerpoMensajeTextBlock.Text = cuerpoMensaje;
            MensajeImagen.Source = (BitmapImage) FindResource("AlertaImagen");
            ShowDialog();
        }

        /// <summary>
        ///     Muestra un mensaje de error
        /// </summary>
        /// <param name="cuerpoMensaje">El mensaje a mostrar</param>
        public void MostrarMensajeError(string cuerpoMensaje)
        {
            MensajeTextBlock.Text = "Error";
            CuerpoMensajeTextBlock.Text = cuerpoMensaje;
            MensajeImagen.Source = (BitmapImage) FindResource("ErrorImagen");
            ShowDialog();
        }

        /// <summary>
        ///     Muestra un mensaje de informacion
        /// </summary>
        /// <param name="cuerpoMensaje">El mensaje a mostrar</param>
        public void MostrarMensajeInformacion(string cuerpoMensaje)
        {
            MensajeTextBlock.Text = "Información";
            CuerpoMensajeTextBlock.Text = cuerpoMensaje;
            MensajeImagen.Source = (BitmapImage) FindResource("InformacionImagen");
            ShowDialog();
        }

        /// <summary>
        ///     Coloca el valor de aceptado y cierra la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickAceptarButton(object sender, RoutedEventArgs e)
        {
            _acepto = true;
            Close();
        }

        /// <summary>
        ///     Coloca el valor de no aceptado y cierra la ventana
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClickCancelarButton(object sender, RoutedEventArgs e)
        {
            _acepto = false;
            Close();
        }
    }
}