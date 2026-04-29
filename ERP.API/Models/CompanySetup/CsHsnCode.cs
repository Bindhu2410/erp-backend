namespace ERP.API.Models.CompanySetup
{
    public class CsHsnCode
    {
        public int HsnCodeId { get; set; }
        public int CompanyId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public decimal DefaultGstRate { get; set; }
    }
}
