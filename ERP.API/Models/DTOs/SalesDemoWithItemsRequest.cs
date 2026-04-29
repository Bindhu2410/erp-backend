using System.Collections.Generic;
using ERP.API.Models.DTOs;

namespace ERP.API.Models.DTOs
{
    public class SalesDemoWithItemsRequest
    {
        public CreateSalesDemoDto Demo { get; set; }
        // Now matches Opportunity API: Items is a list of SalesDemoItemRequest
        public List<SalesDemoItemRequest> Items { get; set; }

        /// <summary>
        /// List of checklist items and their is_active status for this demo
        /// </summary>
        public List<ChecklistSelectionDto> Checklists { get; set; }

        /// <summary>
        /// Time of the demo (HH:mm:ss)
        /// </summary>
        public TimeSpan? DemoTime => Demo?.DemoTime;
    }

    /// <summary>
    /// Represents a checklist item and its selection status for a demo
    /// </summary>
    public class ChecklistSelectionDto
    {
        /// <summary>
        /// The unique ID of the checklist item (from demo_checklists_items.id)
        /// </summary>

        /// <summary>
        /// The name of the checklist item
        /// </summary>
        public string? ChecklistName { get; set; }

        /// <summary>
        /// Whether this checklist item is active/selected for the demo
        /// </summary>
        public bool IsActive { get; set; }
    }
}
