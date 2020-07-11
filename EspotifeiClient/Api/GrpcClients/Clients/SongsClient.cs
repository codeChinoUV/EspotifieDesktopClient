using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Api.GrpcClients.Interfaces;
using Api.Util;
using Google.Protobuf;
using Grpc.Core;
using ManejadorDeArchivos;

namespace Api.GrpcClients.Clients
{
    public class SongsClient : ISongsClient
    {
        private const int ChunkSize = 16 * 1000;
        
        public async Task UploadSong(string path, int idSong, bool isPersonal)
        {
            Channel channel = new Channel("192.168.0.4:5001", ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);
            
            var extension = Path.GetExtension(path).Replace(".","");
            var formatAudio = ConvertExtensionToFormatoAudio(extension);
            if (File.Exists(path))
            {
                var resquestUploadSong = new SolicitudSubirCancion();
                resquestUploadSong.InformacionCancion = new InformacionCancion();
                resquestUploadSong.InformacionCancion.IdCancion = idSong;
                resquestUploadSong.InformacionCancion.FormatoCancion = formatAudio;
                resquestUploadSong.TokenAutenticacion = "eyJ0eXAiOiJKV1QiLhCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoyLCJleHAiOjE1OTQ0MzA5MjB9.L1RXIEGWir63NS7P4KSbCSTnX8VRtM6_uZz8FN1k7D0";
                resquestUploadSong.InformacionCancion.Sha256 = FileManager.Sha256CheckSum(path);
                var songBytes = File.ReadAllBytes(path);
                AsyncClientStreamingCall<SolicitudSubirCancion, RespuestaSolicitudSubirArchivo> call = null; 
                call = isPersonal ? client.SubirCancionPersonal() : client.SubirCancion();
                using (call)
                {
                    try
                    {
                        var totalChunks = songBytes.Length / ChunkSize;
                        var finalBytes = songBytes.Length % ChunkSize;
                        for (int i = 0; i < totalChunks; i++)
                        {
                            resquestUploadSong.Data =
                                ByteString.CopyFrom(FileManager.SubArray(songBytes, i*ChunkSize, ChunkSize));
                            await call.RequestStream.WriteAsync(resquestUploadSong);
                        }
                        resquestUploadSong.Data = ByteString.CopyFrom(FileManager.SubArray(songBytes, totalChunks, finalBytes));
                        await call.RequestStream.WriteAsync(resquestUploadSong);
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
        
        
        
        
        private FormatoAudio ConvertExtensionToFormatoAudio(string extension)
        {
            var formatAudio = FormatoAudio.Mp3;
            if (extension == "mp3")
            {
                formatAudio = FormatoAudio.Mp3;
            }else if (extension == "m4a")
            {
                formatAudio = FormatoAudio.M4A;
            }else if (extension == "flac")
            {
                formatAudio = FormatoAudio.Flac;
            }

            return formatAudio;
        }
        
    }
    
    
}