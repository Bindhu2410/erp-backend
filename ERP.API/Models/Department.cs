using System;
using System.ComponentModel.DataAnnotations;

namespace ERP.API.Models
{
    public class Department
    {
        public int Id { get; set; }
        public int? UserCreated { get; set; }
        public DateTime? DateCreated { get; set; }
        public int? UserUpdated { get; set; }
        public DateTime? DateUpdated { get; set; }
        
        [StringLength(255)]
        public string Name { get; set; }
        
        [StringLength(255)]
        public string HeadOfDepartment { get; set; }
    }
}