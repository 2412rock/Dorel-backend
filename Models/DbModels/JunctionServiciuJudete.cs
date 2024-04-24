using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DorelAppBackend.Models.DbModels
{
    public class JunctionServiciuJudete
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }
        public int UserID { get; set; }
        public int ServiciuIdID { get; set; }

        public int JudetID { get; set; }

        public string Descriere { get; set; }

        public decimal? Rating { get; set; }

        public bool Ofer { get; set; }
    }
}
