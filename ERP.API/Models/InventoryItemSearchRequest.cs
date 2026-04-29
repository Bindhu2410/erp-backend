using System.ComponentModel;

namespace ERP.API.Models
{
    public class InventoryItemSearchRequest
    {
         [DefaultValue(null)]
        public int? Id { get; set; } = null;

         [DefaultValue("sales_leads.date_created")]
        public string? Make { get; set; }=null;

         [DefaultValue("sales_leads.date_created")]
    public string? Model { get; set; }=null;

     [DefaultValue("sales_leads.date_created")]
    public string? Product { get; set; }=null;

     [DefaultValue("sales_leads.date_created")]
    public string? Category { get; set; }=null;

     [DefaultValue("sales_leads.date_created")]
public string? ItemCode { get; set; }=null;

 [DefaultValue("sales_leads.date_created")]
public string? ItemName { get; set; }=null;
    }
}
