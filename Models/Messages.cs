namespace DorelAppBackend.Models
{
    public class Messages
    {
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }

        public string ReceiptEmail { get; set; }

        public string ReceiptName { get; set; }

        public string Message { get; set; } 

        public DateTime SentTime { get; set; }
    }
}
