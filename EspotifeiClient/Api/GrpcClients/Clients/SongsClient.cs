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
    public class SongsClient
    {
        public delegate void PorcentegeUp(float porcentage);

        public event PorcentegeUp OnPorcentageUp;

        public delegate void UploadTerminated();

        public event UploadTerminated OnUploadTerminated;
        public delegate void OnChuckRecived(byte[] bytesSong);

        public delegate void OnRecivedSong(byte[] bytesSong, string extension);

        private const int ChunkSize = 64 * 1000;

        public event OnChuckRecived OnSongChunkRived;

        public event OnRecivedSong OnInitialRecivedSong;

        private const int CounTrys = 2;

        /// <summary>
        /// Solicita al servidor subir una cancion
        /// </summary>
        /// <param name="path">La ruta de la cancion</param>
        /// <param name="idSong">El id de la cancion a subir</param>
        /// <param name="isPersonal">Indica si la cancion personal</param>
        /// <returns>Task</returns>
        /// <exception cref="RpcException">Una excepcion Rcp</exception>
        /// <exception cref="Exception">Una excepcion normal</exception>
        public async Task UploadSong(string path, int idSong, bool isPersonal)
        {
            var channel = new Channel(Configuration.URIGrpcServer, ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);

            var extension = Path.GetExtension(path).Replace(".", "");
            var formatAudio = ConvertExtensionToFormatAudio(extension);
            if (File.Exists(path))
            {
                for (int o = 1; o <= CounTrys; o++)
                {
                    var resquestUploadSong = new SolicitudSubirCancion();
                    resquestUploadSong.InformacionCancion = new InformacionCancion();
                    resquestUploadSong.InformacionCancion.IdCancion = idSong;
                    resquestUploadSong.InformacionCancion.FormatoCancion = formatAudio;
                    resquestUploadSong.TokenAutenticacion = ApiServiceLogin.GetServiceLogin().GetAccessToken();
                    var songBytes = File.ReadAllBytes(path);
                    AsyncClientStreamingCall<SolicitudSubirCancion, RespuestaSolicitudSubirArchivo> call;
                    call = isPersonal ? client.SubirCancionPersonal() : client.SubirCancion();
                    using (call)
                    {
                        try
                        {
                            var totalChunks = songBytes.Length / ChunkSize;
                            var finalBytes = songBytes.Length % ChunkSize;
                            for (var i = 0; i < totalChunks; i++)
                            {
                                resquestUploadSong.Data =
                                    ByteString.CopyFrom(FileManager.SubArray(songBytes, i * ChunkSize, ChunkSize));
                                await call.RequestStream.WriteAsync(resquestUploadSong);
                                OnPorcentageUp?.Invoke(CalculatePercentageUpload(i, totalChunks));
                            }

                            resquestUploadSong.Data =
                                ByteString.CopyFrom(FileManager.SubArray(songBytes, totalChunks, finalBytes));
                            await call.RequestStream.WriteAsync(resquestUploadSong);
                            await call.RequestStream.CompleteAsync();
                        }
                        catch (RpcException ex)
                        {
                            throw new RpcException(ex.Status);
                        }
                        var response = await call.ResponseAsync;
                        if (response.Error == Error.Ninguno)
                        {
                            OnUploadTerminated?.Invoke();
                            return;
                        }else if (response.Error == Error.TokenInvalido || response.Error == Error.TokenFaltante)
                        {
                            ApiServiceLogin.GetServiceLogin().ReLogin();
                        }
                        else
                        {
                            ManageErrorsUploadSong(response.Error);
                        }
                    }
                }
                throw new Exception("AuntenticacionFallida");
            }
        }

        public async void GetSong(int idGetSong, bool isPersonalGetSong)
        {
            var channel = new Channel("ec2-54-160-126-163.compute-1.amazonaws.com:5001", ChannelCredentials.Insecure);
            var client = new Canciones.CancionesClient(channel);

            var request = new SolicitudObtenerCancion();
            var memoryStream = new MemoryStream();
            var position = 0;
            var formatAudio = FormatoAudio.Mp3;
            var error = Error.Ninguno;
            try
            {
                request.IdCancion = idGetSong;
                request.CalidadCancionARecuperar = Calidad.Baja;
                request.TokenAutenticacion =
                    "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJpZF91c3VhcmlvIjoxLCJleHAiOjE1OTUyMjkxOTd9.ynuG-YfgIm1UpLwfLG1at4lxeUREJckFd9cal3tid2g";
                var call = isPersonalGetSong ? client.ObtenerCancionPersonal(request) : client.ObtenerCancion(request);
                using (call)
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var response = call.ResponseStream.Current;
                        memoryStream.Write(response.Data.ToByteArray(), 0, response.Data.Length);
                        position += response.Data.Length;
                        formatAudio = response.FormatoCancion;
                        error = response.Error;
                        if (position == ChunkSize)
                            OnInitialRecivedSong?.Invoke(response.Data.ToByteArray(),
                                ConvertFormatAudioToExtension(formatAudio));
                        else if (position > ChunkSize) OnSongChunkRived?.Invoke(response.Data.ToByteArray());
                    }
                }
            }
            catch (Exception ex)
            {
                var exe = ex;
            }

            var totalSize = memoryStream.ToArray().Length;

            Console.WriteLine(totalSize);
        }

        /// <summary>
        /// Calcula el porcentaje de subida actual
        /// </summary>
        /// <param name="actualChunck">El chunckActual de subida</param>
        /// <param name="totalChunk">El total de Chunks</param>
        /// <returns>El porcentaje de subida</returns>
        private float CalculatePercentageUpload(int actualChunck, int totalChunk)
        {
            float percentage = Convert.ToSingle(actualChunck) / Convert.ToSingle(totalChunk);
            return percentage * 100;
        }
        
        /// <summary>
        /// Convierte un string a un Enum FormatoAudio
        /// </summary>
        /// <param name="extension">El string a convertir</param>
        /// <returns>El FormatoAudio convertido</returns>
        private FormatoAudio ConvertExtensionToFormatAudio(string extension)
        {
            var formatAudio = FormatoAudio.Mp3;
            if (extension == "mp3")
                formatAudio = FormatoAudio.Mp3;
            else if (extension == "m4a")
                formatAudio = FormatoAudio.M4A;
            else if (extension == "flac") formatAudio = FormatoAudio.Flac;

            return formatAudio;
        }

        /// <summary>
        /// Convierte el FormatoAudio a string
        /// </summary>
        /// <param name="audioFormat">El FormatoAudio a convertir</param>
        /// <returns>El string correspondiente</returns>
        private string ConvertFormatAudioToExtension(FormatoAudio audioFormat)
        {
            var formatAudio = "mp3";
            if (audioFormat == FormatoAudio.Mp3)
                formatAudio = "mp3";
            else if (audioFormat == FormatoAudio.M4A)
                formatAudio = "m4a";
            else if (audioFormat == FormatoAudio.Flac) formatAudio = "flac";

            return formatAudio;
        }

        /// <summary>
        /// Maneja los errores que pueda ocurrir al subir una cancion
        /// </summary>
        /// <param name="codigoError">El codigo de error</param>
        /// <exception cref="Exception">La excepcion correspondiente al codigo de error</exception>
        private void ManageErrorsUploadSong(Error codigoError)
        {
            switch (codigoError)
            {
                case Error.Desconocido:
                    throw new Exception("Ocurrio un error en el servidor");
                case Error.UsuarioNoEsDuenoDelRecurso:
                    throw new Exception("No se puede guardar la cancion por que no eres el dueño");
                case Error.CancionInexistente:
                    throw new Exception("No se puede almacenar la cancion debido a que no se almaceno la " +
                                        "informacion de la cancion");
            }
        }
    }
}