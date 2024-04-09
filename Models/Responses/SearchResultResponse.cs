using DorelAppBackend.Models.DbModels;
using DorelAppBackend.Models.Requests;

namespace DorelAppBackend.Models.Responses
{
    public class SearchResultResponse
    {
        public string UserName { get; set; }

        public int UserId { get; set; }

        public string Descriere { get; set; }

        public string ServiciuName { get; set; }

        public int ServiciuId { get; set; } 

        public int JudetId { get; set; }

        public string JudetName { get; set; }

        public double StarsAverage { get; set; }

        public Imagine ImagineCover { get; set; }
    }
}
