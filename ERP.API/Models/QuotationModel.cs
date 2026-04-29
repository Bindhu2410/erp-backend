using System;
using System.Collections.Generic;
using System.Linq;

namespace ERP.API.Models
{
    // Clean, single definition for QuotationModel and QuotationItem.
    public class QuotationModel
    {
        public string QuotationNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.UtcNow;

        // Company details
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string CompanyPhone { get; set; } = string.Empty;
        public string CompanyEmail { get; set; } = string.Empty;
        public string CompanyLogoUrl { get; set; } = string.Empty; // absolute url or data URI

        // Customer / Bill To
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;

        // Visual / template options
        public string? PrimaryColor { get; set; } = "#0B5FFF"; // default blue
        public string? SecondaryColor { get; set; } = "#FFD54F"; // accent

        // Table of contents (optional) - list of section names
        public List<string> TableOfContents { get; set; } = new List<string>();

        // Terms & conditions (can contain HTML)
        public string TermsAndConditionsHtml { get; set; } = string.Empty;

        // Custom footer HTML (optional)
        public string FooterHtml { get; set; } = string.Empty;

        // Optional fields used by the template
        public string Subject { get; set; } = string.Empty;
        public string RepresentativeName { get; set; } = string.Empty;

        public List<QuotationItem> Items { get; set; } = new List<QuotationItem>();

        public decimal SubTotal => Items.Sum(i => i.Total);
        public decimal TaxAmount { get; set; }
        public decimal Total => SubTotal + TaxAmount;
    }

    public class QuotationItem
    {
        public string Description { get; set; } = string.Empty;
        public decimal Qty { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public decimal Total => Qty * UnitPrice;
    }
}