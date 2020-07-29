using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class BibliotecaPersonalClient
    {
        private static readonly int CantidadIntentos = 2;

        /// <summary>
        ///     Registra una cancion personal en el servidor
        /// </summary>
        /// <param name="cancionPersonal">La cancion personal a registrar</param>
        /// <returns>La cancion personal registrada</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al guardar la cancion</exception>
        public static async Task<CancionPersonal> RegisterSong(CancionPersonal cancionPersonal)
        {
            var path = "/v1/biblioteca-personal/canciones";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, cancionPersonal))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var cancionRegistered = await response.Content.ReadAsAsync<CancionPersonal>();
                        return cancionRegistered;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
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
        ///     Solicita al servidor eliminar una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a eliminar</param>
        /// <param name="idAlbum">El id del album al que pertenece la canciónn</param>
        /// <returns>La cancion eliminada</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        public static async Task<CancionPersonal> DeteleCancion(int idCancion)
        {
            var path = $"/v1/biblioteca-personal/canciones/{idCancion}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().DeleteAsync(path))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var cancionDeleted = await response.Content.ReadAsAsync<CancionPersonal>();
                        return cancionDeleted;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la cancion que se desea eliminar");
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
        ///     Recupera las canciones que existen en la biblioteca personal del usuario actual
        /// </summary>
        /// <returns>Una lista de canciones personales</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al recuperar las canciones</exception>
        public static async Task<List<CancionPersonal>> GetCancionesPersonales()
        {
            var path = "/v1/biblioteca-personal/canciones?cantidad=250&pagina=1";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var canciones = await response.Content.ReadAsAsync<List<CancionPersonal>>();
                        return canciones;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
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