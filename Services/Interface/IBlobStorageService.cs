namespace DorelAppBackend.Services.Interface
{
    public interface IBlobStorageService
    {
        public Task UploadImage(string fileName, string fileExtension, string fileType, string fileContentBase64);
    }
}
