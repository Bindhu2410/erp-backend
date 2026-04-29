using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class SalesDemoWithItemsResponse
    {
        public int? Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        public int? UserId { get; set; }
        public DateTime DemoDate { get; set; }
        public string Status { get; set; }
        public int? AddressId { get; set; }
        public string? OpportunityId { get; set; }
        public string DemoContact { get; set; }
        public string DemoName { get; set; }
        public string? CustomerName { get; set; }
        public string DemoApproach { get; set; }
        public string DemoOutcome { get; set; }
        public string DemoFeedback { get; set; }
        public string Comments { get; set; }
         public string? LeadId { get; set; }
         public string? ContactMobileNum { get; set; }
        
        public List<int>? PresenterIds { get; set; }
        public List<string>? PresenterNames { get; set; }
         public object? Address { get; set; }
        /// <summary>
        /// Time of the demo (HH:mm:ss)
        /// </summary>
        public TimeSpan? DemoTime { get; set; }
          public ERP.API.Models.DTOs.SalesAddressDto? CustomerAddress { get; set; }
        public List<SalesDemoItemResponse> Items { get; set; } = new List<SalesDemoItemResponse>();

        /// <summary>
        /// Maps ItemId to checklist names for each item in the demo
        /// </summary>
        public Dictionary<int, List<string>> ChecklistNamesByItemId { get; set; } = new();

        /// <summary>
        /// Lead address fields (pincode, area, state, district, city, door_no, street, landmark) from sales_lead
        /// </summary>
        public object? LeadAddress { get; set; }
    }
}
