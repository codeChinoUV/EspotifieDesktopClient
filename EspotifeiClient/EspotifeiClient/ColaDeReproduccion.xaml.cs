using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using EspotifeiClient.Player;

namespace EspotifeiClient
{
    public partial class ColaDeReproduccion
    {

        private List<ElementoCola> _colaReproduccion;
        
        public ColaDeReproduccion()
        {
            InitializeComponent();
            InicializarInformacionCancion();
        }

        /// <summary>
        /// Recupera los elementos de la cola de reproduccion y coloca su informacion en los elementos de la intefaz grafica
        /// </summary>
        private void InicializarInformacionCancion()
        {
            _colaReproduccion = Player.Player.GetPlayer().RecuperarElementosRestantesEnCola();
            ListViewColaReproduccion.ItemsSource = _colaReproduccion;
            Player.Player.GetPlayer().OnActualizacionCola += ActualizacionCola;
            CantidadCanciones.Text = _colaReproduccion.Count.ToString();
            TiempoReproduccion.Text = CalcularTiempoTotalReproduccion(_colaReproduccion);
        }

        /// <summary>
        /// Cacula el tiempo total de reproduccion
        /// </summary>
        /// <param name="cola">La cola de reproduccion de donde tomara las canciones para calcular su tiempo total</param>
        /// <returns>El tiempo total en string </returns>
        private string CalcularTiempoTotalReproduccion(List<ElementoCola> cola)
        {
            var tiempoTotal = 0.0f;
            foreach (var elementoCola in cola)
            {
                tiempoTotal += elementoCola.Duracion;
            }
            var time = TimeSpan.FromSeconds(tiempoTotal);
            return time.ToString("hh':'mm':'ss");
        }
        
        /// <summary>
        /// Actualiza los elementos en pantalla cuando recibe el evento
        /// </summary>
        /// <param name="elementosCola">Los elementos en la cola de reproduccion actualizados</param>
        private void ActualizacionCola(List<ElementoCola> elementosCola)
        {
            _colaReproduccion = elementosCola;
            ListViewColaReproduccion.ItemsSource = null;
            ListViewColaReproduccion.ItemsSource = _colaReproduccion;
            CantidadCanciones.Text = _colaReproduccion.Count.ToString();
            TiempoReproduccion.Text = CalcularTiempoTotalReproduccion(_colaReproduccion);
        }

        /// <summary>
        /// Solicita al reproductor eliminar una cancion de la cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickEliminarCancion(object sender, RoutedEventArgs e)
        {
            var posicion = (int) ((Button) sender).Tag;
            Player.Player.GetPlayer().EliminarElementoDeColaReproduccion(posicion);
        }

        /// <summary>
        /// Solicita al reproductor limpiar la cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickLimpiarCola(object sender, RoutedEventArgs e)
        {
            Player.Player.GetPlayer().LimpiarColaDeReproduccion();
        }
    }
}