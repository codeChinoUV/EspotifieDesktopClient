using System;
using System.Collections.Generic;
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
    }
}