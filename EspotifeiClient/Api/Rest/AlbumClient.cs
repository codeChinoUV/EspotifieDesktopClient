using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class AlbumClient
    {

        private static readonly int CantidadIntentos = 2;
        
        public static async Task<List<Album>> GetAlbumsFromContentCreator(int idContentCreator)
        {
            var path = $"/v1/creadores-de-contenido/{idContentCreator.ToString()}/albumes";
            for (int i = 1; i <= CantidadIntentos; i++)
            {
                using (HttpResponseMessage response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var albumes = await response.Content.ReadAsAsync<List<Album>>();
                        return albumes;
                    } else if(response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if(response.StatusCode == HttpStatusCode.NotFound)
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
            }
            throw new Exception("AuntenticacionFallida");
        }
    }
    
}