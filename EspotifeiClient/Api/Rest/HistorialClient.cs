using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Model;

namespace Api.Rest
{
    public class HistorialClient
    {
        private static readonly int CantidadIntentos = 2;

        /// <summary>
        ///     Recupera el historial del usuario actual
        /// </summary>
        /// <param name="cantidadDeDias">
        ///     La cantidad de dias a partir de la fecha actual de los cuales se obtendra
        ///     el historial
        /// </param>
        /// <param name="cantidadDeResultados">La cantidad de resultados a obtener</param>
        /// <returns>Una Lista de Canciones</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al recuperar el creador de conenidp</exception>
        public static async Task<List<Cancion>> GetHistorial(int cantidadDeDias, int cantidadDeResultados)
        {
            var path = $"/v1/historial-reproduccion/canciones?cantidad={cantidadDeResultados}&pagina=1&" +
                       $"ultimos_dias_a_obtener={cantidadDeDias}";
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