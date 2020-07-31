using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Model;

namespace Api.Rest
{
    public class ListaReproduccionClient
    {
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
    }
}