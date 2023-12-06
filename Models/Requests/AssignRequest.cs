namespace DorelAppBackend.Models.Requests
{
    public class AssignRequest
    {
        public string[] Servicii { get; set; }
        public string[] Judete { get; set; }
        public ServiciiImaginiDescriere[] ServiciiAndImagini { get; set; }
    }
}
