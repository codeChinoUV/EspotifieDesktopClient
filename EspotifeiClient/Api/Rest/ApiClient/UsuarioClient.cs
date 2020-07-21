using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Model;

namespace Api.Rest.ApiClient
{
    public class UsuarioClient
    {
        public static async Task<Usuario> RegisterUsuario(Usuario user)
        {
            Usuario userRegister = null;
            var path = "/v1/usuario";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().PostAsJsonAsync(path, user))
            {
                if (response.IsSuccessStatusCode)
                {
                    userRegister = await response.Content.ReadAsAsync<Usuario>();
                    return userRegister;
                }
                else if(response.StatusCode == HttpStatusCode.BadRequest)
                {
                    List<ErrorGeneral> errores;
                    var stringError = "Valida que los los campos sean correctos";
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

        private static string ProcessBadRequesCode(string badRequestCode)
        {
            var response = "Validale que los campos sean correctos";
            if (badRequestCode == "nombre_usuario_en_uso")
            {
                response = "El nombre de usuario ya se encuentra en uso, por favor eliga otro";
            }

            if (badRequestCode == "email_en_uso")
            {
                response = "El correo ya se ha registrado en otra cuenta, intente con otro correo";
            }

            return response;
        }
    }
}