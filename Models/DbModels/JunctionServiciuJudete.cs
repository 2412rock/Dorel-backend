using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.DbModels
{
    public class JunctionServiciuJudete
    {
        public int UserID { get; set; }
        public int ServiciuIdID { get; set; }

        public int JudetID { get; set; }

        public string Descriere { get; set; }

    }
}
