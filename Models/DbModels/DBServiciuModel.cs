using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.DbModels
{
    public class DBServiciuModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ServiciuIdID { get; set; }

        public string Name { get; set; }
    }
}
