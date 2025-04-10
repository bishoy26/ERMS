using System;
using System.Collections.Generic;

namespace ERMS.Models
{
    public class Project
    {
        public int ProjectID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        // New property to link a project to its Manager.
        public string ManagerId { get; set; }

        public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
