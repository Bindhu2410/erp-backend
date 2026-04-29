using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class Designation
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        
        [StringLength(100)]
        public string Code { get; set; }
        
        [StringLength(255)]
        public string Name { get; set; }
    }
}