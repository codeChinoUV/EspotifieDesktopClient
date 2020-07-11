using System;
using System.Threading.Tasks;

namespace Api.GrpcClients.Interfaces
{
    public interface ISongsClient
    {
        Task UploadSong(string path, int idSong, bool isPersonal);
    }
}