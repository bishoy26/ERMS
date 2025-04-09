using System;
using System.ComponentModel.DataAnnotations;

namespace ERMS.Models
{
    public class TaskItem
    {
        [Key]
        public int TaskID { get; set; }

        [Required(ErrorMessage = "The Project field is required.")]
        public int? ProjectID { get; set; }  // Nullable

        [Required(ErrorMessage = "The Assigned Employee field is required.")]
        public int? AssignedEmployeeID { get; set; }  // Nullable

        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? DueDate { get; set; }

        // Navigation properties
        public virtual Project Project { get; set; }
        public virtual Employee AssignedEmployee { get; set; }
    }
}
