using System;
using System.Threading.Tasks;
using Api.GrpcClients.Clients;

namespace Api.GrpcClients.Interfaces
{
    public interface ISongsClient
    {
        Task UploadSong(string path, int idSong, bool isPersonal);

        void GetSong(int idSong, bool isPersonal);

        event SongsClient.OnRecivedSong OnInitialRecivedSong;
        
        event SongsClient.OnChuckRecived OnSongChunkRived;
    }
}