namespace DorelAppBackend.Models.Requests
{
    public class PostReviewRequest
    {
        public int ReviwedUserId { get; set; }

        public int ServiciuId {get;set;}

        public int Rating { get;set;}

        public string Description { get; set; }
    }
}
