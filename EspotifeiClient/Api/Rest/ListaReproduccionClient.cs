using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class ListaReproduccionClient
    {
        private static readonly int CantidadIntentos = 2;

        /// <summary>
        ///     Tarea que se encarga de recuperar las listas de producción por búsqueda
        /// </summary>
        /// <param name="search">La cadena a buscar</param>
        /// <returns>Una lista de Listas de reproducción</returns>
        /// <exception cref="Exception">Alguna excepcion que ocurrió durante la solicitud</exception>
        public static async Task<List<ListaReproduccion>> SearchListaReproduccion(string search)
        {
            var path = $"/v1/listas-de-reproduccion/buscar/{search}";
            using (var response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var listasReproduccion = await response.Content.ReadAsAsync<List<ListaReproduccion>>();
                    return listasReproduccion;
                }

                ErrorGeneral error;
                error = await response.Content.ReadAsAsync<ErrorGeneral>();
                throw new Exception(error.mensaje);
            }
        }

        /// <summary>
        /// Método que recupera todas las listas de reproducción creadas por un usuario
        /// </summary>
        /// <returns>Una Task</returns>
        public static async Task<List<ListaReproduccion>> GetListaReproduccion()
        {
            var path = $"/v1/listas-de-reproduccion";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var listasReproduccion = await response.Content.ReadAsAsync<List<ListaReproduccion>>();
                    return listasReproduccion;
                } else
                {
                    ErrorGeneral error;
                    error = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(error.mensaje);
                }
            }
        }

        /// <summary>
        /// Método que solicita al servidor eliminar una lista de reproducción
        /// </summary>
        /// <param name="idListaReproduccion">El identificador de la lista de reproducción a eliminar</param>
        /// <returns></returns>
        public static async Task<ListaReproduccion> DeteleListaReproduccion(int idListaReproduccion)
        {
            var path = $"/v1/listas-de-reproduccion/{idListaReproduccion}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().DeleteAsync(path))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var listaReproduccionDeleted = await response.Content.ReadAsAsync<ListaReproduccion>();
                        return listaReproduccionDeleted;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    } else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la lista de reproducción que se desea eliminar");
                    } else
                    {
                        ErrorGeneral error;
                        error = await response.Content.ReadAsAsync<ErrorGeneral>();
                        throw new Exception(error.mensaje);
                    }
                }

            throw new Exception("AuntenticacionFallida");
        }

        /// <summary>
        /// Método que solicita al servidor registrar una lista de reproducción
        /// </summary>
        /// <param name="listaReproduccion">Instancia de ListaReproduccion</param>
        /// <returns>La lista de reproducción creada</returns>
        public static async Task<ListaReproduccion> RegisterListaReproduccion(ListaReproduccion listaReproduccion)
        {
            var path = "/v1/listas-de-reproduccion";
            for (int i = 1; i <= CantidadIntentos; i++)
            {
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, listaReproduccion))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var listaReproduccionRegistered = await response.Content.ReadAsAsync<ListaReproduccion>();
                        return listaReproduccionRegistered;
                    }
                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        List<ErrorGeneral> errores;
                        errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                        throw new Exception(errores[0].mensaje);
                    } else if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    } else
                    {
                        ErrorGeneral error;
                        error = await response.Content.ReadAsAsync<ErrorGeneral>();
                        throw new Exception(error.mensaje);
                    }
                }
            }
            throw new Exception("AuntenticacionFallida");
        }
    }
}