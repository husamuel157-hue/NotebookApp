using System;
using System.Collections.Generic;
using System.Text;

namespace NotebookApp
{
    /// <summary>
    /// Task data model (associated with a specific date)
    /// </summary>
    public class TaskItem
    {
        // Unique identifier
        public Guid Id { get; set; }
        // Task content
        public string Content { get; set; } = string.Empty;
        // Task cycle (Daily/Weekly)
        public string TaskCycle { get; set; } = string.Empty; // "Daily" | "Weekly"
        // Task category (Shopping/Work)
        public string TaskCategory { get; set; } = string.Empty; // "Shopping" | "Work"
        // Task completion status
        public bool IsCompleted { get; set; } = false;
        // Task associated date (linked to a specific day)
        public DateTime TaskDate { get; set; } = DateTime.Today; // Default to current day
    }
}