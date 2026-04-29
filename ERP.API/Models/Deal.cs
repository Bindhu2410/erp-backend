using System;

namespace ERP.API.Models
{
    public class Deal
    {
        public int Id { get; set; }
        public int UserCreated { get; set; }
        public DateTime DateCreated { get; set; }
        public int UserUpdated { get; set; }
        public DateTime DateUpdated { get; set; }
        public string? Status { get; set; }
        public string? DealName { get; set; }
        public double Amount { get; set; }
        public double ExpectedRevenue { get; set; }
        public string? DealAge { get; set; }
        public string? DealFor { get; set; }
        public DateTime CloseDate { get; set; }
        public bool IsActive { get; set; }
        public string? Comments { get; set; }
        public int OpportunitiesId { get; set; }
        public int SalesRepresentativeId { get; set; }
        public int TerritoryId { get; set; }
        public int AreaId { get; set; }
        public int CityId { get; set; }
        public int DistrictId { get; set; }
        public int StateId { get; set; }
        public int PincodeId { get; set; }
    }
}
