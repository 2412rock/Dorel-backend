using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.DbModels
{
    public class DBJudetModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int JudetID { get; set; }

        public string Name { get; set; }
    }
}
