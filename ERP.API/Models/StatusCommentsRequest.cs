namespace ERP.API.Models
{
    public class StatusCommentsRequest
    {
        public string Status { get; set; }
        public string Comments { get; set; }
        public int? AssignedTo { get; set; }
    }
}
