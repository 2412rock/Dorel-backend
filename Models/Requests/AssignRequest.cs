namespace DorelAppBackend.Models.Requests
{
    public class AssignRequest
    {
        public string UserEmail { get; set; }
        public string[] Servicii { get; set; }
        public string[] Judete { get; set; }
    }
}
