using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class CancionClient
    {
        private static readonly int CantidadIntentos = 2;

        /// <summary>
        ///     Recupera las canciones que pertencen a un album de un creador de contenido
        /// </summary>
        /// <param name="idContentCreator">El id del creador creador de contenido al que pertenece el album </param>
        /// <param name="idAlbum">El id del Album a recuperar sus canciones</param>
        /// <returns>Una Lista de Canciones</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al recuperar el creador de conenidp</exception>
        public static async Task<List<Cancion>> GetSongsFromAlbum(int idContentCreator, int idAlbum)
        {
            var path = $"/v1/creadores-de-contenido/{idContentCreator.ToString()}/albumes/{idAlbum.ToString()}/" +
                       "canciones/";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var canciones = await response.Content.ReadAsAsync<List<Cancion>>();
                        return canciones;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe el album indicado");
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