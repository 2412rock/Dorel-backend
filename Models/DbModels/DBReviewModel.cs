using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace DorelAppBackend.Models.DbModels
{
    public class DBReviewModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ReviewId { get; set; }
        public int ReviewerUserId { get; set; }

        [NotMapped]
        public string ReviewerName { get; set; }

        public int ReviewedUserId { get; set; }

        public int ServiciuId { get; set; }

        public decimal Rating { get; set; }

        public string ReviewDescription { get; set; }

    }
}
