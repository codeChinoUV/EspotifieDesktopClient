using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Model;

namespace Api.Rest
{
    public class GeneroClient
    {

        /// <summary>
        ///     Método del servidor que realiza la petición HTTP para consultar la lista de géneros existentes
        /// </summary>
        /// <returns>Lista de géneros musicales</returns>
        public static async Task<List<Genero>> GetGeneros()
        {
            var generos = new List<Genero>();
            var path = "/v1/generos";
            using (var response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    generos = await response.Content.ReadAsAsync<List<Genero>>();
                    return generos;
                }

                ErrorGeneral error;
                error = await response.Content.ReadAsAsync<ErrorGeneral>();
                throw new Exception(error.mensaje);
            }
        }

        /// <summary>
        /// Recupera las canciones un genero
        /// </summary>
        /// <param name="idGenero">El id del genero a recuperar sus canciones</param>
        /// <param name="cantidad">La cantidad de canciones a recuperar</param>
        /// <returns>Una Lista de canciones</returns>
        public static async Task<List<Cancion>> GetCancionesOfGenero(int idGenero, int cantidad)
        {
            var path = $"/v1/generos/{idGenero}/canciones?cantidad={cantidad}&pagina=1";
            using (var response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var canciones = await response.Content.ReadAsAsync<List<Cancion>>();
                    return canciones;
                }
                else if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    throw new Exception("Ocurrio un error y no se pueden recuperar las canciones");
                }
                else
                {
                    ErrorGeneral error;
                    error = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(error.mensaje);
                }
            }
        }
    }
}