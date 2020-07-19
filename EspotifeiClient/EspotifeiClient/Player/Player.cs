using System.IO;
using Api.GrpcClients.Clients;
using Api.GrpcClients.Interfaces;
using NAudio.Wave;

namespace EspotifeiClient.Player
{
    public class Player
    {
        private static readonly Player _player = new Player();
        private WaveOutEvent _waveOutEvent;
        private MemoryStream _songStream;
        private IWaveProvider _provider;
        private ISongsClient _songsClient;
        WaveFormat Format = new WaveFormat(320000,16, 2);
        private BufferedWaveProvider buffer;

        private Player()
        {
            _waveOutEvent = new WaveOutEvent();
            _songStream = new MemoryStream();
            _songsClient = new SongsClient();
            buffer = new BufferedWaveProvider(new Mp3WaveFormat(44100, 2,640000 ,32));
        }

        public static Player getPlayer()
        {
            return _player;
        }

        public void Play(int idSong)
        {
            _songsClient.OnInitialRecivedSong += ReceiveInitialSongChunk;
            _songsClient.OnSongChunkRived += RecivedSongChunk;
            _songsClient.GetSong(idSong, false);
        }

        private void ReceiveInitialSongChunk(byte[] songBytes, string extension)
        {
            //buffer.AddSamples(songBytes, 0, songBytes.Length);
            //var mp3Reader = new Mp3FileReader(new MemoryStream(songBytes));
            //WaveStream wave32 = new WaveChannel32(mp3Reader);
            //var canales = wave32.WaveFormat.Channels;
            //var rater = wave32.WaveFormat.SampleRate;
            //var bis = wave32.WaveFormat.BitsPerSample;
            _waveOutEvent.Init(buffer);
            _waveOutEvent.Play();
        }

        private void RecivedSongChunk(byte[] songBytes)
        {
            buffer.AddSamples(songBytes, 0, songBytes.Length);
        }
    }
}