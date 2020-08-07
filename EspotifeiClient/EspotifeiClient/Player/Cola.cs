using System.Collections.Generic;
using Model;

namespace EspotifeiClient.Player
{
    public class Cola
    {
        public enum TipoCancionAReproducir
        {
            Cancion,
            CancionPersonal,
            CancionSinConexion,
            Ninguno
        }

        private List<ElementoCola> _colaReproduccion = new List<ElementoCola>();
        private int _posicionCola;
        private int _posicionReproduccion;


        /// <summary>
        ///     Agrega las canciones del album a la cola de reproduccion y borra la cola anterior
        /// </summary>
        /// <param name="album">El album a agregar sus canciones a la cola de reproduccion</param>
        public void AgregarCancionesDeAlbumACola(Album album)
        {
            LimpiarCola();
            foreach (var cancion in album.canciones)
            {
                cancion.album = album;
                _posicionCola += 1;
                var elementoCola = new ElementoCola
                {
                    Cancion = cancion,
                    YaSeReproducio = false,
                    Posicion = _posicionCola
                };
                _colaReproduccion.Add(elementoCola);
            }
        }

        /// <summary>
        ///     Agrega las canciones de los albumes del creador de contenido a la cola de reproduccion
        /// </summary>
        /// <param name="creadorContenido">El creador de contenido a agregar sus canciones</param>
        public void AgregarCancionesDeCreadorDeContenidoACola(CreadorContenido creadorContenido)
        {
            LimpiarCola();
            foreach (var album in creadorContenido.Albums)
            foreach (var cancion in album.canciones)
            {
                cancion.album = album;
                _posicionCola += 1;
                var elementoCola = new ElementoCola
                {
                    Cancion = cancion,
                    YaSeReproducio = false,
                    Posicion = _posicionCola
                };
                _colaReproduccion.Add(elementoCola);
            }
        }

        /// <summary>
        ///     Agrega las canciones de la lista de reproduccion a la cola de reproduccion
        /// </summary>
        /// <param name="listaReproduccion">La lista de reproduccion a agregar sus canciones</param>
        public void AgregarCancionesDeListaDeReproduccionACola(ListaReproduccion listaReproduccion)
        {
            LimpiarCola();
            foreach (var cancion in listaReproduccion.canciones)
            {
                _posicionCola += 1;
                var elementoCola = new ElementoCola
                {
                    Cancion = cancion,
                    YaSeReproducio = false,
                    Posicion = _posicionCola
                };
                _colaReproduccion.Add(elementoCola);
            }
        }

        /// <summary>
        ///     Agrega una cancion a la cola de reproducción
        /// </summary>
        /// <param name="cancion">La cancion a agregar</param>
        public void AgregarCancionACola(Cancion cancion)
        {
            if (!ValidarLaCancionSeEncuentraEnCola(cancion.id))
            {
                _posicionCola += 1;
                var elementoCola = new ElementoCola
                {
                    Cancion = cancion,
                    YaSeReproducio = false,
                    Posicion = _posicionCola
                };
                _colaReproduccion.Add(elementoCola);
            }
        }

        /// <summary>
        ///     Agrega una cancion personal a la cola de reproducción
        /// </summary>
        /// <param name="cancionPersonal">La cancion perosnal a agregar</param>
        public void AgregarCancionPersonalACola(CancionPersonal cancionPersonal)
        {
            if (!ValidarLaCancionPersonalSeEncuentraEnCola(cancionPersonal.id))
            {
                _posicionCola += 1;
                var elementoCola = new ElementoCola
                {
                    CancionPersonal = cancionPersonal,
                    YaSeReproducio = false,
                    Posicion = _posicionCola
                };
                _colaReproduccion.Add(elementoCola);
            }
        }

        /// <summary>
        ///     Agrega una cancion sin conexion a la cola de reproducción
        /// </summary>
        /// <param name="cancionSinConexion">La cancion sin conexion a agregar</param>
        public void AgregarCancionSinConexionACola(CancionSinConexion cancionSinConexion)
        {
            _posicionCola += 1;
            var elementoCola = new ElementoCola
            {
                CancionSinConexion = cancionSinConexion,
                YaSeReproducio = false,
                Posicion = _posicionCola
            };
            _colaReproduccion.Add(elementoCola);
        }

        /// <summary>
        ///     Optiene el tipo de cancion que sigue en la cola de reproducción
        /// </summary>
        /// <returns>TipoCancionAReproducir</returns>
        public TipoCancionAReproducir ObtenerTipoDeCancionSiguiente()
        {
            var tipoCancion = TipoCancionAReproducir.Ninguno;
            if (_colaReproduccion != null)
            {
                var proximoElementoCancion = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion + 1);
                if (proximoElementoCancion != null)
                {
                    if (proximoElementoCancion.Cancion != null)
                        tipoCancion = TipoCancionAReproducir.Cancion;
                    else if (proximoElementoCancion.CancionPersonal != null)
                        tipoCancion = TipoCancionAReproducir.CancionPersonal;
                    else if (proximoElementoCancion.CancionSinConexion != null)
                        tipoCancion = TipoCancionAReproducir.CancionSinConexion;
                }
            }


            return tipoCancion;
        }

        /// <summary>
        ///     Optiene el tipo de cancion anterior en la cola de reproducción
        /// </summary>
        /// <returns>TipoCancionAReproducir</returns>
        public TipoCancionAReproducir ObtenerTipoDeCancionAnterior()
        {
            var tipoCancion = TipoCancionAReproducir.Ninguno;
            if (_colaReproduccion != null)
            {
                var anteriorCancion = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion - 1);
                if (anteriorCancion != null)
                {
                    if (anteriorCancion.Cancion != null)
                        tipoCancion = TipoCancionAReproducir.Cancion;
                    else if (anteriorCancion.CancionPersonal != null)
                        tipoCancion = TipoCancionAReproducir.CancionPersonal;
                    else if (anteriorCancion.CancionSinConexion != null)
                        tipoCancion = TipoCancionAReproducir.CancionSinConexion;
                }
            }

            return tipoCancion;
        }

        /// <summary>
        ///     Regresa la cancion en la cola de reproduccion
        /// </summary>
        /// <param name="anterior">
        ///     Inidica si la cancion a recuperar es la anterior en la cola de reproduccion o es
        ///     la siguiente
        /// </param>
        /// <returns>Una Cancion</returns>
        public Cancion ObtenerCancion(bool anterior)
        {
            Cancion cancion = null;
            if (anterior)
            {
                _posicionReproduccion -= 1;
                var cancionAnterior = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (cancionAnterior != null && cancionAnterior.Cancion != null)
                {
                    cancion = cancionAnterior.Cancion;
                }
            }
            else
            {
                _posicionReproduccion += 1;
                var proximaCancion = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (proximaCancion != null && proximaCancion.Cancion != null)
                {
                    cancion = proximaCancion.Cancion;
                }
            }

            return cancion;
        }

        /// <summary>
        ///     Regresa la cancion personal en la cola de reproduccion
        /// </summary>
        /// <param name="anterior">
        ///     Inidica si la cancion personal a recuperar es la anterior en la cola de
        ///     reproduccion o es la siguiente
        /// </param>
        /// <returns>Una CancionPersonal</returns>
        public CancionPersonal ObtenerCancionPersonal(bool anterior)
        {
            CancionPersonal cancionPersonal = null;
            if (anterior)
            {
                _posicionReproduccion -= 1;
                var cancionAnterior = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (cancionAnterior != null && cancionAnterior.CancionPersonal != null)
                {
                    cancionPersonal = cancionAnterior.CancionPersonal;
                }
            }
            else
            {
                _posicionReproduccion += 1;
                var proximaCancion = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (proximaCancion != null && proximaCancion.CancionPersonal != null)
                {
                    cancionPersonal = proximaCancion.CancionPersonal;
                }
            }

            return cancionPersonal;
        }

        /// <summary>
        ///     Regresa la cancion sin conexion en la cola de reproduccion
        /// </summary>
        /// <param name="anterior">
        ///     Inidica si la cancion sin conexion a recuperar es la anterior en la cola de reproduccion o es
        ///     la siguiente
        /// </param>
        /// <returns>Una CancionSinConexion</returns>
        public CancionSinConexion ObtenerCancionSinConexion(bool anterior)
        {
            CancionSinConexion cancionSinConexion = null;
            if (anterior)
            {
                _posicionReproduccion -= 1;
                var cancionAnterior = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (cancionAnterior != null && cancionAnterior.CancionSinConexion != null)
                    cancionSinConexion = cancionAnterior.CancionSinConexion;
            }
            else
            {
                _posicionReproduccion += 1;
                var proximaCancion = _colaReproduccion.Find(ec => ec.Posicion == _posicionReproduccion);
                if (proximaCancion != null && proximaCancion.CancionSinConexion != null)
                    cancionSinConexion = proximaCancion.CancionSinConexion;
            }

            return cancionSinConexion;
        }

        /// <summary>
        ///     Borra todos los elementos de la cola de reproducción
        /// </summary>
        public void LimpiarCola()
        {
            _colaReproduccion = new List<ElementoCola>();
            _posicionCola = 0;
            _posicionReproduccion = 0;
        }
        
        /// <summary>
        /// Valida si ya existe una cancion en la lista de reproduccion con el id indicado
        /// </summary>
        /// <param name="idCancion">El id de la cancion a buscar</param>
        /// <returns>True si se encuentra, false si no</returns>
        public bool ValidarLaCancionSeEncuentraEnCola(int idCancion)
        {
            var seEncuentra = false;
            foreach (var elementoCola in _colaReproduccion)
            {
                if (elementoCola.Cancion != null)
                {
                    if (elementoCola.Cancion.id == idCancion)
                    {
                        seEncuentra = true;
                        break;
                    }
                }
            }

            return seEncuentra;
        }
        
        /// <summary>
        /// Valida si ya existe una cancio personal en la lista de reproduccion con el id indicado
        /// </summary>
        /// <param name="idCancionPersonal">El id de la cancion personal a buscar</param>
        /// <returns>True si se encuentra, false si no</returns>
        public bool ValidarLaCancionPersonalSeEncuentraEnCola(int idCancionPersonal)
        {
            var seEncuentra = false;
            foreach (var elementoCola in _colaReproduccion)
            {
                if (elementoCola.CancionPersonal != null)
                {
                    if (elementoCola.CancionPersonal.id == idCancionPersonal)
                    {
                        seEncuentra = true;
                        break;
                    }
                }
            }

            return seEncuentra;
        }

        public void EliminarElementoDeCola(int posicion)
        {
            var elementoAElimiar = _colaReproduccion.Find(ec => ec.Posicion == posicion);
            if (elementoAElimiar != null)
            {
                _colaReproduccion.Remove(elementoAElimiar);
                _posicionCola -= 1;
                ReordenarPosicionesColaReproduccion();
            }
        }

        /// <summary>
        /// Vuelve a ordenar el conteo de posicion de los elementos en la cola
        /// </summary>
        private void ReordenarPosicionesColaReproduccion()
        {
            var contador = 1;
            foreach (var elementoCola in _colaReproduccion)
            {
                elementoCola.Posicion = contador;
                contador += 1;
            }
        }

        /// <summary>
        /// Devuelve los elementos de la cola que quedan por reproducir
        /// </summary>
        /// <returns>Una Lista de elementos Cola</returns>
        public List<ElementoCola> ObtenerProximosElementosEnCola()
        {
            var colaRestante = new List<ElementoCola>();
            for (int i = _posicionReproduccion; i < _colaReproduccion.Count; i++)
            {
                colaRestante.Add(_colaReproduccion[i]);
            }

            return colaRestante;
        }
        
    }
}