using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace Api.Rest.ApiClient
{
    public class GeneroClient
    {
        /// <summary>
        /// Método del servidor que realiza la petición HTTP para consultar la lista de géneros existentes
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Genero>> GetGenero()
        {
            List<Genero> listaGeneros = new List<Genero>();
            var path = "/v1/generos";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    listaGeneros = await response.Content.ReadAsAsync<List<Genero>>();
                    return listaGeneros;
                } else
                {
                    ErrorGeneral error;
                    error = await response.Content.ReadAsAsync<ErrorGeneral>();
                    throw new Exception(error.mensaje);
                }
            }

        }
    }
}
