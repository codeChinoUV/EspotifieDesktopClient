using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Api.GrpcClients.Clients;
using Api.GrpcClients.Interfaces;
using NAudio.Wave;

namespace EspotifeiClient.Player
{
    public class Player
    {
        private static readonly Player _player = new Player();
        private WaveOutEvent _waveOutEvent;
        private IWaveProvider _provider;
        private SongsClient _songsClient;
        private bool playingStarted = false;
        private BufferedWaveProvider provider;
        private WaveFormat format;
        private byte[] buffer;
        private MemoryStream totalBuffer;
        private byte[] mp3StreamBuffer;
        private Mp3FileReader _mp3FileReader;
        int bytesRead = 0, totalBytes=0;
        long contador = 0;
        long skipTo = 0;

        private Player()
        {
            _waveOutEvent = new WaveOutEvent();
            _songsClient = new SongsClient();
        }

        public static Player getPlayer()
        {
            return _player;
        }

        public void Play(int idSong)
        {
            _songsClient.OnInitialRecivedSong += ReceiveInitialSongChunk;
            _songsClient.OnSongChunkRived += RecivedSongChunk;
            _songsClient.OnRecivedTotalSong += TermintedRecivedSong;
            _songsClient.isPersonalGetSong = false;
            _songsClient.idGetSong = idSong;
            var recivedSongThread = new Thread(_songsClient.GetSong);
            recivedSongThread.Start();
        }

        private void TermintedRecivedSong()
        {
            var totalByes = totalBuffer.Length;
            var totalsbytesMp3 = provider.BufferedBytes;
            var playerThread = new Thread(_waveOutEvent.Play);
            playerThread.Start();
        }

        private void ReceiveInitialSongChunk(byte[] songBytes, string extension)
        {
            format = GetWaveFormat(new MemoryStream(songBytes));
            totalBuffer = new MemoryStream();
            totalBuffer.Write(songBytes, 0, songBytes.Length);
            totalBuffer.Position -= songBytes.Length;
            mp3StreamBuffer = new byte[format.SampleRate];
            provider = new BufferedWaveProvider(format);
            provider.BufferDuration = TimeSpan.FromHours(3);
            _mp3FileReader = new Mp3FileReader(totalBuffer);
            //buffer.AddSamples(songBytes, 0, songBytes.Length);
            //var mp3Reader = new Mp3FileReader(new MemoryStream(songBytes));
            //WaveStream wave32 = new WaveChannel32(mp3Reader);
            //var canales = wave32.WaveFormat.Channels;
            //var rater = wave32.WaveFormat.SampleRate;
            //var bis = wave32.WaveFormat.BitsPerSample;
            _waveOutEvent.Init(provider);
        }

        //private void RecivedSongChunk(byte[] songBytes)
        //{
        //    buffer.AddSamples(songBytes, 0, songBytes.Length);
        //}

        private WaveFormat GetWaveFormat(MemoryStream initialBytesSong)
        {
            WaveStream tempStream = new Mp3FileReader(initialBytesSong);
            return tempStream.WaveFormat;
        }
        
        private void RecivedSongChunk(byte[] streamBytes)
        {
            totalBuffer.Write(streamBytes, 0, streamBytes.Length);
            totalBuffer.Position -= streamBytes.Length;
            int cantidadChunks = streamBytes.Length / mp3StreamBuffer.Length;
            while ((bytesRead = _mp3FileReader.Read(mp3StreamBuffer, 0, mp3StreamBuffer.Length)) > 0)
            {
                provider.AddSamples(mp3StreamBuffer, 0, bytesRead);
            }
            
        }
        
    }
}