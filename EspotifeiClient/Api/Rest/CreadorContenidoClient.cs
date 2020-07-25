using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class CreadorContenidoClient
    {
        private static int CantidadIntentos = 2;
        
        /// <summary>
        ///     Método del servidor que realiza la petición HTTP para registrar al CreadorContenido
        /// </summary>
        /// <param name="contentCreator">Variable de tipo de CreadorContenido que contiene su información</param>
        /// <returns>Una variable de tipo CreadorContenido o una excepción si la respuesta de la solicitud es incorrecta</returns>
        public static async Task<CreadorContenido> RegisterCreadorContenido(CreadorContenido contentCreator)
        {
            CreadorContenido contentCreatorRegister;
            var path = "/v1/creador-de-contenido";
            for (int i = 1; i <= CantidadIntentos; i++)
            {
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, contentCreator))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        contentCreatorRegister = await response.Content.ReadAsAsync<CreadorContenido>();
                        path = "/v1/creador-de-contenido/generos";
                        foreach (var genero in contentCreator.generos)
                            using (var responseAddGenero = await ApiClient.GetApiClient().PostAsJsonAsync(path, genero))
                            {
                                if (!responseAddGenero.IsSuccessStatusCode)
                                    throw new Exception("No se pudieron guardar todos los generos, " +
                                                        "puede modificarlos mas adelante");
                            }

                        return contentCreatorRegister;
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
            throw new Exception("AuntenticacionFallida"); 
        }

        /// <summary>
        /// Realiza las peticiones al servidor para modificar la información de un creador de contenido 
        /// </summary>
        /// <param name="contentCreator">Variable de tipo de CreadorContenido que contiene su información</param>
        /// <param name="actuals">La lista de generos actualoizada del creador de contenido</param>
        /// <returns>Una variable de tipo CreadorContenido o una excepción si la respuesta de la solicitud es incorrecta</returns>
        public static async Task<CreadorContenido> EditCreadorContenido(CreadorContenido contentCreator, List<Genero> actuals)
        {
            CreadorContenido contentCreatorRegister;
            var path = "/v1/creador-de-contenido";
            for (int i = 1; i <= CantidadIntentos; i++)
            {
                using (var response = await ApiClient.GetApiClient().PutAsJsonAsync(path, contentCreator))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        contentCreatorRegister = await response.Content.ReadAsAsync<CreadorContenido>();
                        var generosToDelete = CalculateGenerosToDelete(contentCreator.generos, actuals);
                        foreach (var genero in generosToDelete)
                        {
                            path = $"/v1/creador-de-contenido/generos/{genero.id}";
                            using (var responseAddGenero = await ApiClient.GetApiClient().DeleteAsync(path))
                            {
                                if (!responseAddGenero.IsSuccessStatusCode)
                                    throw new Exception("No se pudieron modificar todos los generos");
                            }
                        }
                        var generosToAdd = CalculateGenerosToAdd(contentCreator.generos, actuals);
                        foreach (var genero in generosToAdd)
                        {
                            path = "/v1/creador-de-contenido/generos";
                            using (var responseAddGenero = await ApiClient.GetApiClient().PostAsJsonAsync(path, genero))
                            {
                                if (!responseAddGenero.IsSuccessStatusCode)
                                    throw new Exception("No se pudieron modificar todos los generos");
                            }
                        }
                        return contentCreatorRegister;
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        List<ErrorGeneral> errores;
                        errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                        var error = ProcessBadRequesCode(errores[0].error);
                        throw new Exception(error);
                    }
                    else if(response.StatusCode == HttpStatusCode.Unauthorized)
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
            }
            throw new Exception("AuntenticacionFallida"); 
        }

        /// <summary>
        /// Calcula los Generos a eliminar en la edicion de un creador de contenido
        /// </summary>
        /// <param name="last">La lista con los generos que tenia el creador de contenido</param>
        /// <param name="actual">La lista con los generos que tendra el creador de contenido</param>
        /// <returns>Una lista de generos</returns>
        private static List<Genero> CalculateGenerosToDelete(List<Genero> last, List<Genero> actual)
        {
            var listGenerosToDetele = new List<Genero>();
            foreach (var genero in last)
            {
                var isGenero = false;
                foreach (var actualGenero in actual)
                {
                    if (genero.id == actualGenero.id)
                    {
                        isGenero = true;
                        break;
                    } 
                }

                if (!isGenero)
                {
                    listGenerosToDetele.Add(genero);
                }
            }

            return listGenerosToDetele;
        }

        /// <summary>
        /// Calcula los Generos a agregar en la edicion de un creador de contenido
        /// </summary>
        /// <param name="last">La lista con los generos que tenia el creador de contenido</param>
        /// <param name="actual">La lista con los generos que tendra el creador de contenido</param>
        /// <returns>Una lista de generos</returns>
        private static List<Genero> CalculateGenerosToAdd(List<Genero> last, List<Genero> actual)
        {
            var listaGenerosToAdd = new List<Genero>();
            foreach (var actualGenero in actual)
            {
                var isGenero = false;
                foreach (var genero in last)
                {
                    if (genero.id == actualGenero.id)
                    {
                        isGenero = true;
                        break;
                    } 
                }

                if (!isGenero)
                {
                    listaGenerosToAdd.Add(actualGenero);
                }
            }

            return listaGenerosToAdd;
        }
        
        /// <summary>
        ///     Recupera los creadores de contenido por busqueda
        /// </summary>
        /// <param name="search">La cadena a buscar</param>
        /// <returns>Una lista de Creadores de contenido</returns>
        /// <exception cref="Exception">Alguna excepcion que ocurrio en la solicitud</exception>
        public static async Task<List<CreadorContenido>> SearchCreadorContenido(string search)
        {
            var path = $"/v1/creadores-de-contenido/buscar/{search}";
            using (var response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var creadores = await response.Content.ReadAsAsync<List<CreadorContenido>>();
                    return creadores;
                }

                ErrorGeneral error;
                error = await response.Content.ReadAsAsync<ErrorGeneral>();
                throw new Exception(error.mensaje);
            }
        }

        /// <summary>
        /// Recupera el creador de contenido del usuario logeado
        /// </summary>
        /// <returns>El creador de contenido del usuario logeado</returns>
        /// <exception cref="Exception">Alguna excepcion que ocurrio en la solicitud</exception>
        public static async Task<CreadorContenido> GetCreadorContenidoFromActualUser()
        {
            var path = "/v1/creador-de-contenido";
            for (int i = 1; i <= CantidadIntentos; i++)
            {
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var creadores = await response.Content.ReadAsAsync<CreadorContenido>();
                        return creadores;
                    }
                    else if(response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("SinCreadorDeContenido");
                    }else if (response.StatusCode == HttpStatusCode.Unauthorized)
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
            }
            
            throw new Exception("AuntenticacionFallida"); 
        }
        
        /// <summary>
        ///     Método que procesa las diferentes solicitudes incorrectas y las devuelve en un mensaje de error
        /// </summary>
        /// <param name="badRequestCode">Variable que contiene el nombre del error de la solicitud</param>
        /// <returns>Variable de tipo string que contiene el mensaje correspondiente al error recibido</returns>
        private static string ProcessBadRequesCode(string badRequestCode)
        {
            var response = "Validale que los campos sean correctos";
            if (badRequestCode == "usuario_tiene_un_creador_de_contenido_registrado")
                response = "El usuario ya tiene registrado un creador de contenido, no puede tener registrado otro";

            if (badRequestCode == "operacion_no_permitida")
                response = "El usuario no tiene permitido realizar la operación solicitada";
            return response;
        }
        
    }
}