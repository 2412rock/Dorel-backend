namespace DorelAppBackend.Models.Requests
{
    public class SaveMessageReq
    {
        public string ReceipientEmail { get; set; }
        public string Message { get; set; }
    }
}
