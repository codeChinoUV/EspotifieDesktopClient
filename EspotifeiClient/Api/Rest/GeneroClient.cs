using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Model;

namespace Api.Rest
{
    public class GeneroClient
    {
        /// <summary>
        /// Método del servidor que realiza la petición HTTP para consultar la lista de géneros existentes
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Genero>> GetGeneros()
        {
            List<Genero> generos = new List<Genero>();
            var path = "/v1/generos";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    generos = await response.Content.ReadAsAsync<List<Genero>>();
                    return generos;
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
