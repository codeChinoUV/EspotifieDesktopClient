using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class UsuarioClient
    {
        private static readonly int TotalTryes = 2;

        /// <summary>
        ///     Solicita al APIREST registrar un nuevo usuario
        /// </summary>
        /// <param name="user"></param>
        /// <returns>El nuevo usuario registrado</returns>
        /// <exception cref="Exception">Una excepcion que indica si ocurrio un error</exception>
        public static async Task<Usuario> RegisterUsuario(Usuario user)
        {
            Usuario userRegister;
            var path = "/v1/usuario";
            using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, user))
            {
                if (response.IsSuccessStatusCode)
                {
                    userRegister = await response.Content.ReadAsAsync<Usuario>();
                    return userRegister;
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    List<ErrorGeneral> errores;
                    errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                    var error = ProcessBadRequesCode(errores[0].error);
                    throw new Exception(error);
                }
                else
                {
                    ErrorGeneral error;
                    error = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(error.mensaje);
                }
            }
        }

        /// <summary>
        ///     Recupera el usuario logeado y lo asigna a la variable de Usuario
        /// </summary>
        public static async Task GetUser()
        {
            var path = "/v1/usuario";
            var isSuccessfully = false;
            for (var i = 1; i <= TotalTryes; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var usuario = await response.Content.ReadAsAsync<Usuario>();
                        ApiServiceLogin.GetServiceLogin().Usuario = usuario;
                        isSuccessfully = true;
                        break;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else
                    {
                        ErrorGeneral error;
                        error = await response.Content.ReadAsAsync<ErrorGeneral>();
                        throw new Exception(error.mensaje);
                    }
                }

            if (!isSuccessfully) throw new Exception("AuntenticacionFallida");
        }

        /// <summary>
        ///     Procesa los codigos de error para convertirlos a cadenas de informacion para el usuario
        /// </summary>
        /// <param name="badRequestCode">El codigo de error a procesar</param>
        /// <returns>Una cadena que contiene informacion para el usuario a partir de los codigos de error</returns>
        private static string ProcessBadRequesCode(string badRequestCode)
        {
            var response = "Validale que los campos sean correctos";
            if (badRequestCode == "nombre_usuario_en_uso")
                response = "El nombre de usuario ya se encuentra en uso, por favor eliga otro";

            if (badRequestCode == "email_en_uso")
                response = "El correo ya se ha registrado en otra cuenta, intente con otro correo";

            return response;
        }
    }
}