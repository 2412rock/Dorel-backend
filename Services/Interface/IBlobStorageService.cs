using DorelAppBackend.Models.Requests;

namespace DorelAppBackend.Services.Interface
{
    public interface IBlobStorageService
    {
        public string GetFileName(int userID, int serviciuId, int pictureIndex);
        public Task UploadImage(string fileName, string fileType, string fileContentBase64);

        public Task<Imagine> DownloadImage(string fileName);

        public Task DeleteImage(string fileName);
    }
}
