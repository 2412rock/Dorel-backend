using DorelAppBackend.Models.DbModels;

namespace DorelAppBackend.Models.Responses
{
    public class SearchResultResponse
    {
        public string UserName { get; set; }

        public string Descriere { get; set; }

        public string ServiciuName { get; set; }

        public double StarsAverage { get; set; }
    }
}
