using System;

namespace Api.GrpcClients.Interfaces
{
    public interface ISongsClient
    {
        void UploadSong(String path, int idSong);
    }
}