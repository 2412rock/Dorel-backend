namespace DorelAppBackend.Models.DbModels
{
    public class JunctionJudete
    {
        public int UserID { get; set; }
        public int JudetID { get; set; }

        public DBUserLoginInfoModel User { get; set; }
        public DBJudetModel Judet { get; set; }
    }
}
