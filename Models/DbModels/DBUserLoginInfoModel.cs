using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.DbModels
{
    public class DBUserLoginInfoModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int UserID { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }

        public ICollection<JunctionServicii> JunctionServicii { get; set; } = new List<JunctionServicii>();

        public ICollection<JunctionJudete> JunctionJudete { get; set; } = new List<JunctionJudete>();

    }
}
