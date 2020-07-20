using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Api.GrpcClients.Interfaces;
using Api.Util;
using Google.Protobuf;
using Grpc.Core;
using ManejadorDeArchivos;

namespace Api.GrpcClients.Clients
{
    public class SongsClient
    {
        private const int ChunkSize = 64 * 1000;

        public delegate void OnChuckRecived(byte[] bytesSong);

        public event OnChuckRecived OnSongChunkRived;
        
        public delegate void OnRecivedSong(byte[] bytesSong, string extension);

        public event OnRecivedSong OnInitialRecivedSong;

        public delegate void OnRecivedTerminated();

        public event OnRecivedTerminated OnRecivedTotalSong;

        public int idGetSong { get; set; }
        
        public bool isPersonalGetSong { get; set; }

        public async Task UploadSong(string path, int idSong, bool isPersonal)
        {
            Channel channel = new Channel("ec2-54-160-126-163.compute-1.amazonaws.com:5001", ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);
            
            var extension = Path.GetExtension(path).Replace(".","");
            var formatAudio = ConvertExtensionToFormatAudio(extension);
            if (File.Exists(path))
            {
                var resquestUploadSong = new SolicitudSubirCancion();
                resquestUploadSong.InformacionCancion = new InformacionCancion();
                resquestUploadSong.InformacionCancion.IdCancion = idSong;
                resquestUploadSong.InformacionCancion.FormatoCancion = formatAudio;
                resquestUploadSong.TokenAutenticacion = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoxLCJleHAiOjE1OTUyMTg0ODV9.f7cPSV-HfAk9N3xOLZZeG7twlsAhRX38qpYLc5K5rbw";
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

        public async void GetSong()
        {
            Channel channel = new Channel("ec2-54-160-126-163.compute-1.amazonaws.com:5001", ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);
            
            var request = new SolicitudObtenerCancion();
            MemoryStream memoryStream = new MemoryStream();
            int position = 0;
            FormatoAudio formatAudio = FormatoAudio.Mp3;
            Error error = Error.Ninguno;
            try
            {
                request.IdCancion = idGetSong;
                request.CalidadCancionARecuperar = Calidad.Alta;
                request.TokenAutenticacion =
                    "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoxLCJleHAiOjE1OTUyMjIzNzN9.yypwZ-RRAdcHDDLYTFyoHBn3QUAvX_UhnuXQtTFubMY";
                var call = isPersonalGetSong ? client.ObtenerCancionPersonal(request) : client.ObtenerCancion(request);
                using (call)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        RespuestaObtenerCancion response = call.ResponseStream.Current;
                        memoryStream.Write(response.Data.ToByteArray(), 0, response.Data.Length);
                        position += response.Data.Length;
                        formatAudio = response.FormatoCancion;
                        error = response.Error;
                        if (position == ChunkSize)
                        {
                            OnInitialRecivedSong?.Invoke(response.Data.ToByteArray(), ConvertFormatAudioToExtension(formatAudio));
                        }
                        else if(position > ChunkSize)
                        {
                            OnSongChunkRived?.Invoke(response.Data.ToByteArray());
                        }
                    }
                    OnRecivedTotalSong?.Invoke();
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

        private FormatoAudio ConvertExtensionToFormatAudio(string extension)
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

        private string ConvertFormatAudioToExtension(FormatoAudio audioFormat)
        {
            var formatAudio = "mp3";
            if (audioFormat == FormatoAudio.Mp3)
            {
                formatAudio = "mp3";
            }else if (audioFormat == FormatoAudio.M4A)
            {
                formatAudio = "m4a";
            }else if (audioFormat == FormatoAudio.M4A)
            {
                formatAudio = "flac";
            }

            return formatAudio;
        }
        
        

    }
    
    
}