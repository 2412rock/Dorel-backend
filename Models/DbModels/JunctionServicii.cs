namespace DorelAppBackend.Models.DbModels
{
    public class JunctionServicii
    {
        public int UserID { get; set; }
        public int ServiciuIdID { get; set; }

        public DBUserLoginInfoModel User { get; set; }
        public DBServiciuModel Serviciu { get; set; }
    }
}
