namespace DorelAppBackend.Models.Requests
{
    public class SaveMessageReq
    {
        public int ReceipientId { get; set; }
        public string Message { get; set; }
    }
}
