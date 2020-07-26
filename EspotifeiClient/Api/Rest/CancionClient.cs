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
        /// Recupera las canciones que pertencen a un album de un creador de contenido
        /// </summary>
        /// <param name="idContentCreator">El id del creador creador de contenido al que pertenece el album </param>
        /// <param name="idAlbum">El id del Album a recuperar sus canciones</param>
        /// <returns>Una Lista de Canciones</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al recuperar el creador de conenidp</exception>
        public static async Task<List<Cancion>> GetSongsFromAlbum(int idContentCreator, int idAlbum)
        {
            var path = $"/v1/creadores-de-contenido/{idContentCreator.ToString()}/albumes/{idAlbum.ToString()}/" +
                       "canciones";
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
        
        /// <summary>
        /// Registra una cancion en el servidor
        /// </summary>
        /// <param name="idAlbum">El id del Album al que pertenece la cancion</param>
        /// <returns>La cancion registrada</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al guardar la cancion</exception>
        public static async Task<Cancion> RegisterSong(int idAlbum, Cancion cancion)
        {
            var path = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, cancion))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var cancionRegistered = await response.Content.ReadAsAsync<Cancion>();
                        var pathAddGenero = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{cancionRegistered.id}/generos";
                        foreach (var genero in cancion.generos)
                            using (var responseAddGenero = await ApiClient.GetApiClient().PostAsJsonAsync(pathAddGenero, genero))
                            {
                                if (responseAddGenero.StatusCode != HttpStatusCode.Created)
                                    throw new Exception("No se pudieron guardar todos los generos, " +
                                                        "puede modificarlos mas adelante");
                            }

                        var pathAddContentCreator = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/" +
                                                    $"{cancionRegistered.id}/creadores-de-contenido";
                        foreach (var contentCreator in cancion.creadores_de_contenido)
                            using (var responseAddGenero = await ApiClient.GetApiClient().PostAsJsonAsync(pathAddContentCreator, contentCreator))
                            {
                                if (responseAddGenero.StatusCode != HttpStatusCode.Created)
                                    throw new Exception("No se pudieron guardar todos los artistas, " +
                                                        "puede modificarlos mas adelante");
                            }
                        return cancionRegistered;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe el album a donde desea agregar la cancion");
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