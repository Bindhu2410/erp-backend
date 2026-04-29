using System;

namespace ERP.API.Models.DTOs
{
    public class DemoCardsDto
    {
        public long DemoRequested { get; set; }
        public long DemoScheduled { get; set; }
        public long DemoCompleted { get; set; }
        public long DemoCancelled { get; set; }
    }
}
