using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ERMS.Models
{
    public class TaskItem
    {
        [Key]
        public int TaskID { get; set; }

        [Required(ErrorMessage = "The Project field is required.")]
        public int? ProjectID { get; set; }

        [Required(ErrorMessage = "The Assigned Employee field is required.")]
        public int? AssignedEmployeeID { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }

        [ValidateNever]
        public virtual Project Project { get; set; }

        [ValidateNever]
        public virtual Employee AssignedEmployee { get; set; }
    }
}
