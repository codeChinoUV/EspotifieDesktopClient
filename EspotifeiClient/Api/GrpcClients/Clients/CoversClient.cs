using System;
using System.IO;
using Api.Util;
using Google.Protobuf;
using Grpc.Core;
using ManejadorDeArchivos;

namespace Api.GrpcClients.Clients
{
    public class CoversClient
    {

        private const int ChunkSize = 16 * 1000;

        public void UploadUserCover(string path)
        {
            Channel channel = new Channel("192.168.0.9:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaUsuario();
            UploadCover(path, 0, call);

        }

        public void UploadAlbumCover(string path, int idAlbum)
        {
            Channel channel = new Channel("192.168.0.9:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaAlbum();
            UploadCover(path, idAlbum, call);
        }

        public void UploadContentCreatorCover(string path, int idContentCreator)
        {
            Channel channel = new Channel("192.168.0.9:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var call = client.SubirPortadaCreadorDeContenido();
            UploadCover(path, idContentCreator, call);
        }

        private async void UploadCover(string path, int idCover,
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
                requestUploadCover.TokenAutenticacion =
                    "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoyLCJleHAiOjE1OTQ2MzExNjV9.j9Pjr6x8NRdKFxFOB84T4jNUvEx2NVdbmvG2HTpcFe8";
                var coverBytes = FileManager.ByteArrayFromImageFile(path);
                using (call)
                {
                    try
                    {
                        var totalChunks = coverBytes.Length / ChunkSize;
                        var finalBytes = coverBytes.Length % ChunkSize;
                        for (int i = 0; i < totalChunks; i++)
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
                    catch (Exception ex)
                    {
                        var exe = ex.Message;
                    }

                    var response = await call.ResponseAsync;
                    var error = response;
                }
            }
        }

        private SolicitudObtenerPortada CreateSolicitudObtenerPortada(int idElement, string token)
        {
            var request = new SolicitudObtenerPortada();
            request.IdElementoDePortada = idElement;
            request.CalidadPortadaARecuperar = Calidad.Alta;
            request.TokenAutenticacion = token;
            return request;
        }

        public async void GetAlbumCover(int idAlbum)
        {
            Channel channel = new Channel("192.168.0.10:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request = CreateSolicitudObtenerPortada(idAlbum,
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoyLCJleHAiOjE1OTQ4NjY4NDF9.v-Xfqaw4WT2WFWhv7kJa3yVrJVxIwj3NXBi6HmRz0uI");
            var call = client.ObtenerPortadaAlbum(request);
            GetCover(call);
        }

        public async void GetContentCreatorCover(int idContentCreator)
        {
            Channel channel = new Channel("192.168.0.10:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request = CreateSolicitudObtenerPortada(idContentCreator,
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoyLCJleHAiOjE1OTQ4NjY4NDF9.v-Xfqaw4WT2WFWhv7kJa3yVrJVxIwj3NXBi6HmRz0uI");
            var call = client.ObtenerPortadaCreadorDeContenido(request);
            GetCover(call);
        }

        public async void GetUserCover(int idUser)
        {
            Channel channel = new Channel("192.168.0.10:5001", ChannelCredentials.Insecure);
            var client = new Portadas.PortadasClient(channel);
            var request = CreateSolicitudObtenerPortada(idUser,
                "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoyLCJleHAiOjE1OTQ4NjY4NDF9.v-Xfqaw4WT2WFWhv7kJa3yVrJVxIwj3NXBi6HmRz0uIY");
            var call = client.ObtenerPortadaUsuario(request);
            GetCover(call);
        }

        private async void GetCover(AsyncServerStreamingCall<RespuestaObtenerPortada> call)
        {
            MemoryStream memoryStream = new MemoryStream();
            int position = 0;
            FormatoImagen formatImage = FormatoImagen.Png;
            Error error = Error.Ninguno;
            try
            {
                using (call)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        RespuestaObtenerPortada response = call.ResponseStream.Current;
                        memoryStream.Write(response.Data.ToByteArray(), 0, response.Data.Length);
                        position += response.Data.Length;
                        formatImage = response.FormatoPortada;
                        error = response.Error;
                    }
                }
            }
            catch (Exception ex)
            {
                var exe = ex;
            }
            var error1 = error;
            var totalSize = memoryStream.ToArray().Length;
            Console.WriteLine(totalSize);
        }
        
        
        private FormatoImagen ConvertExtensionToFormatoImagen(string extension)
        {
            var formatImage = FormatoImagen.Png;
            if (extension == "png")
            {
                formatImage = FormatoImagen.Png;
            }else if (extension == "jpg")
            {
                formatImage = FormatoImagen.Jpg;
            }

            return formatImage;
        }
    }
    
    
}