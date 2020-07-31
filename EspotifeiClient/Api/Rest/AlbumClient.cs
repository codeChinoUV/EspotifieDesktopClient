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
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
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

        /// <summary>
        ///     Solicita al API registrar un album
        /// </summary>
        /// <param name="album"></param>
        /// <returns>El Album registrado</returns>
        /// <exception cref="Exception">Alguna excepcion que puede ocurrir al mandar la solicitud al servidor</exception>
        public static async Task<Album> RegisterAlbum(Album album)
        {
            var path = "/v1/creador-de-contenido/albumes";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, album))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var albumRegistered = await response.Content.ReadAsAsync<Album>();
                        return albumRegistered;
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        List<ErrorGeneral> errores;
                        errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                        throw new Exception(errores[0].mensaje);
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
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

        /// <summary>
        ///     Solicita al API editar un album
        /// </summary>
        /// <param name="idAlbum">El id del album a editar</param>
        /// <param name="album"></param>
        /// <returns>El Album editado</returns>
        /// <exception cref="Exception">Alguna excepcion que puede ocurrir al mandar la solicitud al servidor</exception>
        public static async Task<Album> EditAlbum(int idAlbum, Album album)
        {
            var path = $"/v1/creador-de-contenido/albumes/{idAlbum}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PutAsJsonAsync(path, album))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var albumEdited = await response.Content.ReadAsAsync<Album>();
                        return albumEdited;
                    }

                    if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        List<ErrorGeneral> errores;
                        errores = await response.Content.ReadAsAsync<List<ErrorGeneral>>();
                        throw new Exception(errores[0].mensaje);
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
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