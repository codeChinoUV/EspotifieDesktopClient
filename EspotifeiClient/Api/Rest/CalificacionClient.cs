using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class CalificacionClient
    {
        public static int CantidadIntentos = 2;

        /// <summary>
        ///     Califica una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a calificar</param>
        /// <param name="calificacionEstrellas">La calificacion</param>
        /// <returns>La calificacion agregada</returns>
        /// <exception cref="Exception">Alguna excepcion que puede ocurrir</exception>
        public static async Task<Calificacion> AddCalificacion(int idCancion, int calificacionEstrellas)
        {
            var path = $"/v1/canciones/{idCancion}/calificaciones";
            var calificacion = new Calificacion
            {
                calificacion_estrellas = calificacionEstrellas
            };
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PostAsJsonAsync(path, calificacion))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var calificacionAdded = await response.Content.ReadAsAsync<Calificacion>();
                        return calificacionAdded;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la cancion que desea calificar");
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
        ///     Recupera la calificacion de una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a recuperar su calificacion</param>
        /// <returns>La calificacion de la cancion</returns>
        /// <exception cref="Exception">Alguna excepcion que puede ocurrir</exception>
        public static async Task<Calificacion> GetCalificacion(int idCancion)
        {
            var path = $"/v1/canciones/{idCancion}/calificaciones";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var calificacionAdded = await response.Content.ReadAsAsync<Calificacion>();
                        return calificacionAdded;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("NoCalificada");
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
        ///     Edita la calificacion de una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a editar su calificacion</param>
        /// <param name="cantidadEstrellas">La cantidad de estrellas con la que se calificara la cancion</param>
        /// <returns>La calificacion editada</returns>
        /// <exception cref="Exception">Alguna excepcion que puede ocurrir</exception>
        public static async Task<Calificacion> EditCalificacion(int idCancion, int cantidadEstrellas)
        {
            var path = $"/v1/canciones/{idCancion}/calificaciones";
            var calificacion = new Calificacion
            {
                calificacion_estrellas = cantidadEstrellas
            };
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PutAsJsonAsync(path, calificacion))
                {
                    if (response.StatusCode == HttpStatusCode.Created)
                    {
                        var calificacionAdded = await response.Content.ReadAsAsync<Calificacion>();
                        return calificacionAdded;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("NoCalificada");
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