using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Model;

namespace EspotifeiClient.ManejoUsuarios
{
    public class ManejadorDeUsuariosLogeados
    {
        private List<Usuario> _usuariosLogeados;
        private static ManejadorDeUsuariosLogeados _manejadorDeUsuariosLogeados = new ManejadorDeUsuariosLogeados();

        /// <summary>
        /// Devuelve la instancia del singleton
        /// </summary>
        /// <returns>La instancia del singleton</returns>
        public static ManejadorDeUsuariosLogeados GetManejadorDeUsuariosLogeados()
        {
            return _manejadorDeUsuariosLogeados;
        }
        
        private ManejadorDeUsuariosLogeados()
        {
            var fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "Espotifei");
            Directory.CreateDirectory(fileName);
            fileName = Path.Combine(fileName, "usuarios.spfei");
            try
            {
                using (var stream = File.Open(fileName, FileMode.Open))
                {
                    var formateadorBinario = new BinaryFormatter();
                    _usuariosLogeados = (List<Usuario>) formateadorBinario.Deserialize(stream);
                }
            }
            catch (Exception)
            {
                _usuariosLogeados = new List<Usuario>();
            }
            
        }
        
        /// <summary>
        /// Devuelve al usuario que tiene la sesion iniciada
        /// </summary>
        /// <returns>El usuario logeado</returns>
        public Usuario ObtenerUsuarioLogeado()
        {
            Usuario usuarioLogeado = null;
            foreach (var usuario in _usuariosLogeados)
            {
                if (usuario.sesion_iniciada)
                {
                    usuarioLogeado = usuario;
                    break;
                }
            }

            return usuarioLogeado;
        }

        /// <summary>
        /// Agrega a la lista de usuarios un usuario que se haya logeado
        /// </summary>
        /// <param name="usuario">El usuario a agregar</param>
        public void InicioSesionUsuario(Usuario usuario)
        {
            var usuarioAlmacenado = _usuariosLogeados.Find(u => u.nombre_usuario == usuario.nombre_usuario);
            if (usuarioAlmacenado != null)
            {
                var posicion = _usuariosLogeados.IndexOf(usuarioAlmacenado);
                usuario.sesion_iniciada = true;
                usuario.canciones_pendientes_descarga = _usuariosLogeados[posicion].canciones_pendientes_descarga;
                usuario.canciones_sin_conexion = _usuariosLogeados[posicion].canciones_sin_conexion;
                _usuariosLogeados[posicion] = usuario;
            }
            else
            {
                usuario.sesion_iniciada = true;
                _usuariosLogeados.Add(usuario);
            }
        }

        /// <summary>
        /// Cierra la sesion de un usuario con la sesion abierta
        /// </summary>
        public void CerrarSesionUsuario()
        {
            var usuario = ObtenerUsuarioLogeado();
            usuario.sesion_iniciada = false;
        }

        /// <summary>
        /// Serializa la informacion de los usuarios en un archivo
        /// </summary>
        public void GuardarInformacionUsuarios()
        {
            var fileName = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData), "Espotifei");
            fileName = Path.Combine(fileName, "usuarios.spfei");
            try
            {
                using (var stream = File.Open(fileName, FileMode.Create))
                {
                    var formateadorBinario = new BinaryFormatter();
                    formateadorBinario.Serialize(stream, _usuariosLogeados);
                }
            }
            catch (Exception)
            {
                //No hacer nada
            }
        }
        
    }
    
}