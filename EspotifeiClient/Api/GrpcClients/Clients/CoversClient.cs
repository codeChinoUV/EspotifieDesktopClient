using System;
using System.IO;
using System.Threading.Tasks;
using Api.Rest.ApiLogin;
using Api.Util;
using Google.Protobuf;
using Grpc.Core;
using ManejadorDeArchivos;

namespace Api.GrpcClients.Clients
{
    public class CoversClient
    {
        private const int ChunkSize = 16 * 1000;
        private const int TryNumbres = 2;

        public async void UploadUserCover(string path)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaUsuario();
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    var response = await UploadCover(path, 0, call);
                    if (response.Error == Error.TokenInvalido || response.Error == Error.TokenFaltante)
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    else if (response.Error == Error.Ninguno)
                        break;
                    else
                        ErrorHandlerUploadCover(response);
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
        }

        public async void UploadAlbumCover(string path, int idAlbum)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaAlbum();
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    var response = await UploadCover(path, idAlbum, call);
                    if (response.Error == Error.TokenInvalido || response.Error == Error.TokenFaltante)
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    else if (response.Error == Error.Ninguno)
                        break;
                    else
                        ErrorHandlerUploadCover(response);
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
        }

        public async void UploadContentCreatorCover(string path, int idContentCreator)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaCreadorDeContenido();
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    var response = await UploadCover(path, idContentCreator, call);
                    if (response.Error == Error.TokenInvalido || response.Error == Error.TokenFaltante)
                        await ApiServiceLogin.GetServiceLogin().ReLogin();
                    else if (response.Error == Error.Ninguno)
                        break;
                    else
                        ErrorHandlerUploadCover(response);
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
        }

        private async Task<RespuestaSolicitudSubirArchivo> UploadCover(string path, int idCover,
            AsyncClientStreamingCall<SolicitudSubirPortada, RespuestaSolicitudSubirArchivo> call)
        {
            var extension = Path.GetExtension(path).Replace(".", "");
            var formatImage = ConvertExtensionToFormatoImagen(extension);
            if (File.Exists(path))
            {
                var requestUploadCover = new SolicitudSubirPortada();
                requestUploadCover.InformacionPortada = new InformacionPortada();
                requestUploadCover.InformacionPortada.IdElementoDePortada = idCover;
                requestUploadCover.InformacionPortada.FormatoImagen = formatImage;
                requestUploadCover.TokenAutenticacion = ApiServiceLogin.GetServiceLogin().GetAccessToken();
                var coverBytes = FileManager.ByteArrayFromImageFile(path);
                using (call)
                {
                    var totalChunks = coverBytes.Length / ChunkSize;
                    var finalBytes = coverBytes.Length % ChunkSize;
                    try
                    {
                        for (var i = 0; i < totalChunks; i++)
                        {
                            requestUploadCover.Data =
                                ByteString.CopyFrom(FileManager.SubArray(coverBytes, i * ChunkSize, ChunkSize));
                            await call.RequestStream.WriteAsync(requestUploadCover);
                        }

                        requestUploadCover.Data =
                            ByteString.CopyFrom(FileManager.SubArray(coverBytes, totalChunks, finalBytes));
                        await call.RequestStream.WriteAsync(requestUploadCover);
                        await call.RequestStream.CompleteAsync();
                    }
                    catch (RpcException ex)
                    {
                        if (ex.StatusCode != StatusCode.OK) throw new RpcException(ex.Status, ex.Message);
                    }

                    return await call.ResponseAsync;
                }
            }

            throw new Exception("No se encontro el archivo en la ruta especifica");
        }

        private SolicitudObtenerPortada CreateSolicitudObtenerPortada(int idElement, string token, Calidad calidad)
        {
            var request = new SolicitudObtenerPortada();
            request.IdElementoDePortada = idElement;
            request.CalidadPortadaARecuperar = calidad;
            request.TokenAutenticacion = token;
            return request;
        }

        public async Task<MemoryStream> GetAlbumCover(int idAlbum, Calidad calidad)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request =
                CreateSolicitudObtenerPortada(idAlbum, ApiServiceLogin.GetServiceLogin().GetAccessToken(), calidad);
            var call = client.ObtenerPortadaAlbum(request);
            MemoryStream cover = null;
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    cover = await GetCover(call);
                    break;
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
                catch (Exception)
                {
                    await ApiServiceLogin.GetServiceLogin().ReLogin();
                }

            return cover;
        }

        public async Task<MemoryStream> GetContentCreatorCover(int idContentCreator, Calidad calidad)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request = CreateSolicitudObtenerPortada(idContentCreator,
                ApiServiceLogin.GetServiceLogin().GetAccessToken(), calidad);
            var call = client.ObtenerPortadaCreadorDeContenido(request);
            MemoryStream cover = null;
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    cover = await GetCover(call);
                    break;
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
                catch (Exception)
                {
                    await ApiServiceLogin.GetServiceLogin().ReLogin();
                }

            return cover;
        }

        public async Task<MemoryStream> GetUserCover(int idUser, Calidad calidad)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request = CreateSolicitudObtenerPortada(idUser,
                ApiServiceLogin.GetServiceLogin().GetAccessToken(), calidad);
            var call = client.ObtenerPortadaUsuario(request);
            MemoryStream cover = null;
            for (var i = 1; i <= TryNumbres; i++)
                try
                {
                    cover = await GetCover(call);
                    break;
                }
                catch (RpcException ex)
                {
                    throw new RpcException(ex.Status, ex.Message);
                }
                catch (Exception)
                {
                    await ApiServiceLogin.GetServiceLogin().ReLogin();
                }

            return cover;
        }

        private async Task<MemoryStream> GetCover(AsyncServerStreamingCall<RespuestaObtenerPortada> call)
        {
            var memoryStream = new MemoryStream();
            var error = Error.Ninguno;
            try
            {
                using (call)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var response = call.ResponseStream.Current;
                        memoryStream.Write(response.Data.ToByteArray(), 0, response.Data.Length);
                        error = response.Error;
                    }
                }
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode != StatusCode.OK) throw new RpcException(ex.Status, ex.Message);
            }

            if (error != Error.Ninguno && (error == Error.TokenFaltante || error == Error.TokenInvalido))
                ErrorHandlerGetCover(error);
            else if (error != Error.Ninguno) memoryStream = null;

            return memoryStream;
        }

        private FormatoImagen ConvertExtensionToFormatoImagen(string extension)
        {
            var formatImage = FormatoImagen.Png;
            if (extension == "png")
                formatImage = FormatoImagen.Png;
            else if (extension == "jpg") formatImage = FormatoImagen.Jpg;

            return formatImage;
        }

        /// <summary>
        ///     Se encarga de manejar las posibles excepciones que puedan ocurrir al subir o solicitar una portada
        /// </summary>
        /// <param name="respuesta"></param>
        /// <exception cref="Exception"></exception>
        private void ErrorHandlerUploadCover(RespuestaSolicitudSubirArchivo respuesta)
        {
            switch (respuesta.Error)
            {
                case Error.Desconocido:
                    throw new Exception("Ocurrio un error en el servidor y no se pudo guardar la imagen");
                case Error.OperacionNoPermitida:
                    throw new Exception("No tiene permiso a realizar la operacion solicitada");
                case Error.AlbumInexistente:
                    throw new Exception("No existe el album al que desea agregar la portada");
            }
        }

        /// <summary>
        ///     Se encarga de manejar los errores ocurridos al recuperar una cancion
        /// </summary>
        /// <param name="error">El error ocurrido</param>
        /// <exception cref="Exception">La excepcion que indica el tipo de error ocurrido</exception>
        private void ErrorHandlerGetCover(Error error)
        {
            switch (error)
            {
                case Error.TokenFaltante:
                case Error.TokenInvalido:
                    throw new Exception();
            }
        }
    }
}