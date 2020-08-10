using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using EspotifeiClient.ManejadorDeCancionesSinConexion;
using EspotifeiClient.ManejoUsuarios;
using Model;

namespace EspotifeiClient
{
    /// <summary>
    ///     Lógica de interacción para CancionesSinConexion.xaml
    /// </summary>
    public partial class CancionesSinConexion
    {
        private List<CancionSinConexion> _cancionesDescargadas = new List<CancionSinConexion>();
        private List<CancionSinConexion> _cancionesPendientes = new List<CancionSinConexion>();

        public CancionesSinConexion()
        {
            InitializeComponent();
            ObtenerCancionesSinConexion();
            ColocarInformacionCanciones();
            ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion().OnActualizacionElementosCola +=
                ObtenerCancionesSinConexion;
        }

        /// <summary>
        ///     Colaca la informacion general de las canciones descarfas en los elementos de la pantalla
        /// </summary>
        private void ColocarInformacionCanciones()
        {
            var tiempoTotal = 0.0f;
            if (_cancionesDescargadas != null)
                foreach (var cancionSinConexion in _cancionesDescargadas)
                    tiempoTotal += cancionSinConexion.duracion;
            var tiempo = TimeSpan.FromSeconds(tiempoTotal);
            CantidadDeCanciones.Text = _cancionesDescargadas?.Count.ToString();
            DuracionTotal.Text = tiempo.ToString("hh':'mm':'ss");
        }

        /// <summary>
        ///     Recupera las canciones sin conexion del usuario y las pendientes de descarga y las coloca en los text view
        /// </summary>
        private void ObtenerCancionesSinConexion()
        {
            var usuarioActual = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
            var cancionDescargando = ManejadorCancionesSinConexion.GetManejadorDeCancionesSinConexion()
                .GetCancionDescargando();
            _cancionesPendientes = new List<CancionSinConexion>();
            if (cancionDescargando != null) _cancionesPendientes.Add(cancionDescargando);
            _cancionesPendientes.AddRange(usuarioActual.canciones_pendientes_descarga);
            _cancionesDescargadas = usuarioActual.canciones_sin_conexion;
            ListViewCancionesDescargadas.ItemsSource = null;
            ListViewCancionesDescargadas.ItemsSource = _cancionesDescargadas;
            ListViewCancionesSinConexionPendientes.ItemsSource = null;
            ListViewCancionesSinConexionPendientes.ItemsSource = _cancionesPendientes;
            ColocarInformacionCanciones();
        }

        /// <summary>
        ///     Empieza a reproducir una cancion sin conexion sin afectar la cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayCancionSinConexion(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancionSinConexion = BuscarCancionSinConexion(idCancion);
            if (cancionSinConexion != null)
                Player.Player.GetPlayer().EmpezarAReproducirCancionSinConexion(cancionSinConexion);
        }

        /// <summary>
        ///     Se encarga de agregar la cancion a la cola de reproduccion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickAgregarACola(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancionSinConexion = BuscarCancionSinConexion(idCancion);
            if (cancionSinConexion != null)
                Player.Player.GetPlayer().AñadirCancionSinConexionAColaDeReproduccion(cancionSinConexion);
        }

        /// <summary>
        ///     Elimina la cancion de las canciones sin conexion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickEliminarCancion(object sender, RoutedEventArgs e)
        {
            var idCancion = (int) ((Button) sender).Tag;
            var cancion = BuscarCancionSinConexion(idCancion);
            if (cancion != null)
            {
                if (File.Exists(cancion.ruta_cancion)) File.Delete(cancion.ruta_cancion);
                var usuario = ManejadorDeUsuariosLogeados.GetManejadorDeUsuariosLogeados().ObtenerUsuarioLogeado();
                usuario.canciones_sin_conexion.Remove(cancion);
                ObtenerCancionesSinConexion();
            }
        }

        /// <summary>
        ///     Coloca en la cola de reproduccion todas las canciones sin conexion
        /// </summary>
        /// <param name="sender">El objeto que invoco el evento</param>
        /// <param name="e">El evento invocado</param>
        private void OnClickPlayTodasLasCanciones(object sender, RoutedEventArgs e)
        {
            if (_cancionesDescargadas != null)
                Player.Player.GetPlayer().AñadirTodasLasCancionesSinConexionACola(_cancionesDescargadas);
        }

        /// <summary>
        ///     Busca una cancion sin conexion en la lista de canciones por su id
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar</param>
        /// <returns>La cancion a la cual pertenece el id</returns>
        private CancionSinConexion BuscarCancionSinConexion(int idCancion)
        {
            CancionSinConexion cancion = null;
            if (_cancionesDescargadas != null) cancion = _cancionesDescargadas.Find(cs => cs.id == idCancion);

            return cancion;
        }
    }
}