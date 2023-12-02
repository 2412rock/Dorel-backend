namespace DorelAppBackend.Models.Requests
{
    public class Imagine
    {
        public string FileType { get; set; }

        public string FileExtension { get; set; }
        public string FileContentBase64 { get; set; }
    }
}
