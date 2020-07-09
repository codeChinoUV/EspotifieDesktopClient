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
    public class CancionesClient : ISongsClient
    {
        private const int ChunkSize = 16 * 1000;
        
        public void UploadSong(String path, int idSong)
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);
            
            var extension = Path.GetExtension(path).Replace(".","");
            var formatAudio = ConvertExtensionToFormatoAudio(extension);
            if (File.Exists(path))
            {
                var resquestUploadSong = new SolicitudSubirCancion();
                resquestUploadSong.InformacionCancion.IdCancion = idSong;
                resquestUploadSong.InformacionCancion.FormatoCancion = formatAudio;
                resquestUploadSong.TokenAutenticacion = "";
                resquestUploadSong.InformacionCancion.Sha256 = FileManager.Sha256CheckSum(path);
                var fileStream = new FileStream(path, FileMode.Open);
                var songBytes = new byte[Convert.ToInt32(fileStream.Length.ToString())];
                using (var call = client.SubirCancion())
                {
                    for (int i = 0; i < songBytes.Length; i += ChunkSize)
                    {
                        resquestUploadSong.Data = ByteString.CopyFrom(FileManager.SubArray(songBytes, i, ChunkSize));
                        call.RequestStream.WriteAsync(resquestUploadSong);
                        if (i + ChunkSize > songBytes.Length)
                        {
                            var bytesLength = songBytes.Length - i - 1;
                            resquestUploadSong.Data = ByteString.CopyFrom(FileManager.SubArray(songBytes, i, bytesLength));
                            call.RequestStream.WriteAsync(resquestUploadSong);
                            break;
                        }
                    }

                    call.RequestStream.CompleteAsync();
                    var response = call.ResponseAsync;
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