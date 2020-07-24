using System.IO;
using System.Threading;
using Api.GrpcClients.Clients;
using NAudio.Wave;

namespace EspotifeiClient.Player
{
    public class Player
    {
        private static readonly Player _player = new Player();
        private byte[] _mp3StreamBuffer;
        private MemoryStream _songBuffer;
        private readonly SongsClient _songsClient;
        private WaveOut _waveOutEvent;

        private Player()
        {
            _waveOutEvent = new WaveOut();
            _songsClient = new SongsClient();
        }

        public static Player GetPlayer()
        {
            return _player;
        }

        public void Play(int idSong)
        {
            _songsClient.OnInitialRecivedSong += ReceiveInitialSongChunk;
            _songsClient.OnSongChunkRived += RecivedSongChunk;
            _songsClient.isPersonalGetSong = false;
            _songsClient.idGetSong = idSong;
            var recivedSongThread = new Thread(_songsClient.GetSong);
            recivedSongThread.Start();
        }


        private void ReceiveInitialSongChunk(byte[] songBytes, string extension)
        {
            var format = GetWaveFormat(new MemoryStream(songBytes));
            _songBuffer = new MemoryStream();
            _songBuffer.Write(songBytes, 0, songBytes.Length);
            _songBuffer.Position = 0;
            _mp3StreamBuffer = new byte[format.SampleRate];
            WaveStream blockAlignedStream =
                new BlockAlignReductionStream(
                    WaveFormatConversionStream.CreatePcmStream(new Mp3FileReader(_songBuffer)));
            _waveOutEvent = new WaveOut(WaveCallbackInfo.FunctionCallback());
            _waveOutEvent.Init(blockAlignedStream);
            _waveOutEvent.Play();
        }

        private WaveFormat GetWaveFormat(MemoryStream initialBytesSong)
        {
            WaveStream tempStream = new Mp3FileReader(initialBytesSong);
            return tempStream.WaveFormat;
        }

        private void RecivedSongChunk(byte[] streamBytes)
        {
            var stream = new MemoryStream(streamBytes);
            int bytesRead;
            while ((bytesRead = stream.Read(_mp3StreamBuffer, 0, _mp3StreamBuffer.Length)) > 0)
            {
                var pos = _songBuffer.Position;
                _songBuffer.Position = _songBuffer.Length;
                _songBuffer.Write(_mp3StreamBuffer, 0, bytesRead);
                _songBuffer.Position = pos;
            }
        }
    }
}