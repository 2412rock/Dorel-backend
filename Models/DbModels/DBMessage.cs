using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DorelAppBackend.Models.DbModels
{
    public class DBMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ID { get; set; }

        public int SenderId { get; set; }

        public int ReceipientId { get; set; }

        public string Message { get; set; }

        public DateTime SentTime { get; set; }

        public bool? Seen { get; set; }
    }
}
