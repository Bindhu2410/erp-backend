using System;
using System.Collections.Generic;

namespace ERP.API.Models.DTOs
{
    public class SalesRepScheduleItemDto
    {
        public string Type { get; set; } = string.Empty; // Meeting, Call, Event
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? With { get; set; }
        public DateTime StartDateTime { get; set; }
        public string? Status { get; set; }
        public string? Label { get; set; } // e.g. Meeting, Demo, Follow-up
        public string? Description { get; set; }
    }

    public class SalesRepScheduleDto
    {
        public List<SalesRepScheduleItemDto> Items { get; set; } = new();
    }
}
