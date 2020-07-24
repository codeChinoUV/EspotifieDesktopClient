using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class AlbumClient
    {
        private static readonly int CantidadIntentos = 2;

        /// <summary>
        ///     Recupera los albumes de un creador de contenido
        /// </summary>
        /// <param name="idContentCreator">El id del creador de contenido a obtener sus albumes</param>
        /// <returns>Una Lista de Albumes</returns>
        /// <exception cref="Exception">Alguna excepción que ocurra al obtener los albumes</exception>
        public static async Task<List<Album>> GetAlbumsFromContentCreator(int idContentCreator)
        {
            var path = $"/v1/creadores-de-contenido/{idContentCreator.ToString()}/albumes";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var albumes = await response.Content.ReadAsAsync<List<Album>>();
                        return albumes;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe el artista indicado");
                    }
                    else
                    {
                        ErrorGeneral error;
                        error = await response.Content.ReadAsAsync<ErrorGeneral>();
                        throw new Exception(error.mensaje);
                    }
                }

            throw new Exception("AuntenticacionFallida");
        }
    }
}