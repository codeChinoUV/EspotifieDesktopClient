﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Model;

namespace Api.Rest.ApiClient
{
    public class CreadorContenidoClient
    {
        /// <summary>
        /// Método del servidor que realiza la petición HTTP para registrar al CreadorContenido
        /// </summary>
        /// <param name="contentCreator">Variable de tipo de CreadorContenido que contiene su información</param>
        /// <returns>Una variable de tipo CreadorContenido o una excepción si la respuesta de la solicitud es incorrecta</returns>
        public static async Task<CreadorContenido> RegisterCreadorContenido(CreadorContenido contentCreator)
        {
            CreadorContenido contentCreatorRegister = null;
            var path = "/v1/creador-de-contenido";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().PostAsJsonAsync(path, contentCreator))
            {
                if (response.IsSuccessStatusCode)
                {
                    contentCreatorRegister = await response.Content.ReadAsAsync<CreadorContenido>();
                    return contentCreatorRegister;
                } else if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    List<ErrorGeneral> errores;
                    errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                    var error = ProcessBadRequesCode(errores[0].error);
                    throw new Exception(error);
                } else
                {
                    ErrorGeneral error;
                    error = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(error.mensaje);
                }
            }
        }

        /// <summary>
        /// Método que procesa las diferentes solicitudes incorrectas y las devuelve en un mensaje de error
        /// </summary>
        /// <param name="badRequestCode">Variable que contiene el nombre del error de la solicitud</param>
        /// <returns>Variable de tipo string que contiene el mensaje correspondiente al error recibido</returns>
        private static string ProcessBadRequesCode(string badRequestCode)
        {
            var response = "Validale que los campos sean correctos";
            if (badRequestCode == "usuario_tiene_un_creador_de_contenido_registrado")
            {
                response = "El usuario ya tiene registrado un creador de contenido, no puede tener registrado otro";
            }

            if (badRequestCode == "operacion_no_permitida")
            {
                response = "El usuario no tiene permitido realizar la operación solicitada";
            }
            return response;
        }
    }
}