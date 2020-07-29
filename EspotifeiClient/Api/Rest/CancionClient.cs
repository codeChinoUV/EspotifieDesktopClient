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
                    } else
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
        /// <param name="cancion">La cancion a registrar</param>
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

        /// <summary>
        /// Edita una cancion en el servidor
        /// </summary>
        /// <param name="cancion">La cancion a editar</param>
        /// <param name="idAlbum">El id del Album al que pertenece la cancion</param>
        /// <param name="actualsGeneros">La lista de generos actuales de la cancion</param>
        /// <param name="actualsCreadores">la lista de creadores actuales de la cancion</param>
        /// <returns>La cancion registrada</returns>
        /// <exception cref="Exception">Alguna excepcion que pueda ocurrir al guardar la cancion</exception>
        public static async Task<Cancion> EditSong(Cancion cancion, int idAlbum, List<Genero> actualsGeneros,
            List<CreadorContenido> actualsCreadores)
        {
            var path = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{cancion.id}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().PutAsJsonAsync(path, cancion))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var cancionEdited = await response.Content.ReadAsAsync<Cancion>();
                        await AddNewGeneroToSong(cancionEdited.id, idAlbum, cancion.generos, actualsGeneros);
                        await DeleteOldGeneroToSong(cancionEdited.id, idAlbum, cancion.generos, actualsGeneros);
                        await AddNewCreadoresToSong(cancionEdited.id, idAlbum, cancion.creadores_de_contenido,
                            actualsCreadores);
                        await DeleteOldCreadoresToSong(cancionEdited.id, idAlbum, cancion.creadores_de_contenido,
                            actualsCreadores);
                        return cancionEdited;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la cancion que se desea editar");
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
        /// Solicita al servidor eliminar una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a eliminar</param>
        /// <param name="idAlbum">El id del album al que pertenece la canciónn</param>
        /// <returns>La cancion eliminada</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        public static async Task<Cancion> DeteleCancion(int idCancion, int idAlbum)
        {
            var path = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{idCancion}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().DeleteAsync(path))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var cancionDeleted = await response.Content.ReadAsAsync<Cancion>();
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
        /// Método que solicita al servidor eliminar una canción de una palylist
        /// </summary>
        /// <param name="idListaReproduccion">El id de la lista de reproducción a la que pertenece la canción</param>
        /// <param name="idCancion">El id de la canción a eliminar</param>
        /// <returns></returns>
        public static async Task<Cancion> DeteleCancionPlaylist(int idListaReproduccion, int idCancion)
        {
            var path = $"/v1/listas-de-reproduccion/{idListaReproduccion}/canciones/{idCancion}";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().DeleteAsync(path))
                {
                    if (response.StatusCode == HttpStatusCode.Accepted)
                    {
                        var cancionDeleted = await response.Content.ReadAsAsync<Cancion>();
                        return cancionDeleted;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    } else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la cancion que se desea eliminar");
                    } else
                    {
                        ErrorGeneral error;
                        error = await response.Content.ReadAsAsync<ErrorGeneral>();
                        throw new Exception(error.mensaje);
                    }
                }

            throw new Exception("AuntenticacionFallida");
        }

        /// <summary>
        /// Agrega los nuevos generos a una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a la que se le agregaran los generos</param>
        /// <param name="idAlbum">El album al que pertenece la cancion</param>
        /// <param name="cancionGeneros">Los generos que tenia la cancion</param>
        /// <param name="actualsGeneros">Los generos que ahora la cancion</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        private static async Task AddNewGeneroToSong(int idCancion, int idAlbum, List<Genero> cancionGeneros,
            List<Genero> actualsGeneros)
        {
            var pathAddGenero = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{idCancion}/generos";
            var generosToAdd = CalculateGenerosToAdd(cancionGeneros, actualsGeneros);
            foreach (var genero in generosToAdd)
            {
                using (HttpResponseMessage response =
                    await ApiClient.GetApiClient().PostAsJsonAsync(pathAddGenero, genero))
                {
                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception("No se pudieron agregar los nuevos generos");
                    }
                }
            }
        }
        
        /// <summary>
        /// Agrega los nuevos generos a una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a la que se le agregaran los generos</param>
        /// <param name="idAlbum">El album al que pertenece la cancion</param>
        /// <param name="cancionGeneros">Los generos que tenia la cancion</param>
        /// <param name="actualsGeneros">Los generos que ahora tiene la cancion</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        private static async Task DeleteOldGeneroToSong(int idCancion, int idAlbum, List<Genero> cancionGeneros,
            List<Genero> actualsGeneros)
        {
            var generosToAdd = CalculateGenerosToDelete(cancionGeneros, actualsGeneros);
            foreach (var genero in generosToAdd)
            {
                var pathToDeleteGenero =
                    $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{idCancion}/generos/{genero.id}";
                using (HttpResponseMessage response =
                    await ApiClient.GetApiClient().DeleteAsync(pathToDeleteGenero))
                {
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        throw new Exception("No se pudieron quitar los viejos generos");
                    }
                }
            }
        }
        
        /// <summary>
        /// Agrega los nuevos creadores de contenido a una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a la que se le agregaran los creadores de contenido</param>
        /// <param name="idAlbum">El album al que pertenece la cancion</param>
        /// <param name="cancionCreadores">Los creadores de contenido que tenia la cancion</param>
        /// <param name="actualsCreadores">Los creadores de contenido que ahora tiene la cancion</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        private static async Task AddNewCreadoresToSong(int idCancion, int idAlbum,
            List<CreadorContenido> cancionCreadores, List<CreadorContenido> actualsCreadores)
        {
            var pathAddCreador = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/" +
                                 $"{idCancion}/creadores-de-contenido";
            var creadoresToAdd = CalculateCreadoresDeContenidoToAdd(cancionCreadores, actualsCreadores);
            foreach (var creadorDeContenido in creadoresToAdd)
            {
                using (HttpResponseMessage response =
                    await ApiClient.GetApiClient().PostAsJsonAsync(pathAddCreador, creadorDeContenido))
                {
                    if (response.StatusCode != HttpStatusCode.Created)
                    {
                        throw new Exception("No se pudieron agregar los nuevos artistas");
                    }
                }
            }
        }
        
        /// <summary>
        /// Quita los antiguos creadores de contenido de la cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a la que se le quitaran los creadores de contenido</param>
        /// <param name="idAlbum">El album al que pertenece la cancion</param>
        /// <param name="cancionCreadores">Los creadores de contenido que tenia la cancion</param>
        /// <param name="actualsCreadores">Los creadores de conenido que ahora tiene la cancion</param>
        /// <returns>Task</returns>
        /// <exception cref="Exception">Una excepcion que pueda ocurrir</exception>
        private static async Task DeleteOldCreadoresToSong(int idCancion, int idAlbum,
            List<CreadorContenido> cancionCreadores, List<CreadorContenido> actualsCreadores)
        {
            var creadoresToAdd = CalculateCreadoresDeContenidoToDelete(cancionCreadores, actualsCreadores);
            foreach (var creadorContenido in creadoresToAdd)
            {
                var pathToDeleteGenero = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{idCancion}/" +
                                         $"creadores-de-contenido/{creadorContenido.id}";
                using (HttpResponseMessage response =
                    await ApiClient.GetApiClient().DeleteAsync(pathToDeleteGenero))
                {
                    if (response.StatusCode != HttpStatusCode.Accepted)
                    {
                        throw new Exception("No se pudieron quitar los antiguos artistas");
                    }
                }
            }
        }
        
        /// <summary>
        /// Calcula los Generos a eliminar en la edicion de un creador de contenido
        /// </summary>
        /// <param name="last">La lista con los generos que tenia el creador de contenido</param>
        /// <param name="actual">La lista con los generos que tendra el creador de contenido</param>
        /// <returns>Una lista de generos</returns>
        private static List<Genero> CalculateGenerosToDelete(List<Genero> last, List<Genero> actual)
        {
            var listGenerosToDetele = new List<Genero>();
            foreach (var genero in last)
            {
                var isGenero = false;
                foreach (var actualGenero in actual)
                {
                    if (genero.id == actualGenero.id)
                    {
                        isGenero = true;
                        break;
                    } 
                }

                if (!isGenero)
                {
                    listGenerosToDetele.Add(genero);
                }
            }

            return listGenerosToDetele;
        }

        /// <summary>
        /// Calcula los Generos a agregar en la edicion de una cancion
        /// </summary>
        /// <param name="last">La lista con los generos que tenia la cancion</param>
        /// <param name="actual">La lista con los generos que tendra la cancion</param>
        /// <returns>Una lista de generos</returns>
        private static List<Genero> CalculateGenerosToAdd(List<Genero> last, List<Genero> actual)
        {
            var listaGenerosToAdd = new List<Genero>();
            foreach (var actualGenero in actual)
            {
                var isGenero = false;
                foreach (var genero in last)
                {
                    if (genero.id == actualGenero.id)
                    {
                        isGenero = true;
                        break;
                    } 
                }

                if (!isGenero)
                {
                    listaGenerosToAdd.Add(actualGenero);
                }
            }

            return listaGenerosToAdd;
        }
        
        /// <summary>
        /// Calcula los Creadores de contenido a agregar en la edicion de una cancion
        /// </summary>
        /// <param name="last">La lista con los creadores de contenido que tenia la cancion</param>
        /// <param name="actual">La lista con los creadores de contenido que tendra la cancion</param>
        /// <returns>Una lista de creadores de contenido</returns>
        private static List<CreadorContenido> CalculateCreadoresDeContenidoToAdd(List<CreadorContenido> last, 
            List<CreadorContenido> actual)
        {
            var listaCreadorContenidoToAdd = new List<CreadorContenido>();
            foreach (var actualCreadorContenido in actual)
            {
                var isCreador = false;
                foreach (var creadorContenido in last)
                {
                    if (creadorContenido.id == actualCreadorContenido.id)
                    {
                        isCreador = true;
                        break;
                    } 
                }

                if (!isCreador)
                {
                    listaCreadorContenidoToAdd.Add(actualCreadorContenido);
                }
            }

            return listaCreadorContenidoToAdd;
        }
        
        /// <summary>
        /// Calcula los Creadores de contenido a eliminar en la edicion de una cancion
        /// </summary>
        /// <param name="last">La lista con los creadores de contenido que tenia la cancion</param>
        /// <param name="actual">La lista con los creadores de contenido que tendra la cancion</param>
        /// <returns>Una lista de creadores de contenido</returns>
        private static List<CreadorContenido> CalculateCreadoresDeContenidoToDelete(List<CreadorContenido> last, 
            List<CreadorContenido> actual)
        {
            var listCreadoresToDetele = new List<CreadorContenido>();
            foreach (var creadorContenido in last)
            {
                var isGenero = false;
                foreach (var actualCreador in actual)
                {
                    if (creadorContenido.id == actualCreador.id)
                    {
                        isGenero = true;
                        break;
                    } 
                }

                if (!isGenero)
                {
                    listCreadoresToDetele.Add(creadorContenido);
                }
            }

            return listCreadoresToDetele;
        }

        /// <summary>
        /// Recupera los generos que pertenecen a una cancion
        /// </summary>
        /// <param name="idCancion">El id de la cancion a recuperar sus generos</param>
        /// <param name="idAlbum">El id del album al que pertenece la cancion</param>
        /// <returns>Una lista de Generos</returns>
        /// <exception cref="Exception">Alguna excepción que pueda ocurrir</exception>
        public static async Task<List<Genero>> GetGenerosFromCancion(int idCancion, int idAlbum)
        {
            var path = $"/v1/creador-de-contenido/albumes/{idAlbum}/canciones/{idCancion}/generos";
            for (var i = 1; i <= CantidadIntentos; i++)
                using (var response = await ApiClient.GetApiClient().GetAsync(path))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var generosFromSong = await response.Content.ReadAsAsync<List<Genero>>();
                        return generosFromSong;
                    }
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        ApiServiceLogin.GetServiceLogin().ReLogin();
                    }
                    else if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        throw new Exception("No existe la cancion indicada");
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

        public static async Task<List<Cancion>> GetSongsFromPlaylist(int idPlaylist)
        {
            var path = $"/v1/listas-de-reproduccion/{idPlaylist.ToString()}/canciones";
            using (HttpResponseMessage response = await ApiClient.GetApiClient().GetAsync(path))
            {
                if (response.IsSuccessStatusCode)
                {
                    var canciones = await response.Content.ReadAsAsync<List<Cancion>>();
                    return canciones;
                } else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    throw new Exception("No existe la lista de reproducci�n indicada");
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