namespace ERP.API.Models
{
    public class StatusCommentsResponse
    {
        public string Status { get; set; }
        public string Comments { get; set; }
        public int? AssignedTo { get; set; }
    }
}
