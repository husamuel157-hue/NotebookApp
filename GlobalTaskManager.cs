using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace NotebookApp
{
    /// <summary>
    /// Task status enumeration (corresponding to color markers)
    /// </summary>
    public enum TaskStatus
    {
        Completed,   // Completed (red√)
        Uncompleted, // Uncompleted (black×)
        InProgress   // In Progress (yellow=)
    }

    /// <summary>
    /// Global task data manager (added weekly status calculation)
    /// </summary>
    public class GlobalTaskManager
    {
        // Singleton instance
        private static readonly Lazy<GlobalTaskManager> _instance = new(() => new GlobalTaskManager());
        public static GlobalTaskManager Instance => _instance.Value;
        private GlobalTaskManager() { }

        // Global task collections
        public ObservableCollection<TaskItem> DailyShoppingTasks { get; } = new();
        public ObservableCollection<TaskItem> DailyWorkTasks { get; } = new();
        public ObservableCollection<TaskItem> WeeklyShoppingTasks { get; } = new();
        public ObservableCollection<TaskItem> WeeklyWorkTasks { get; } = new();

        #region New: Core methods for weekly task status
        /// <summary>
        /// Get date list from Monday to Sunday of current week (in order)
        /// </summary>
        public List<DateTime> GetThisWeekDates()
        {
            var today = DateTime.Today;
            // Calculate Monday of current week (adjust if Sunday is first day of week, here Monday is first)
            var monday = today.AddDays(-(int)today.DayOfWeek + 1);
            if ((int)today.DayOfWeek == 0) // Special handling for Sunday
                monday = today.AddDays(-6);

            // Generate date list from Monday to Sunday
            var weekDates = new List<DateTime>();
            for (int i = 0; i < 7; i++)
            {
                weekDates.Add(monday.AddDays(i));
            }
            return weekDates;
        }

        /// <summary>
        /// Get overall task status for specified date
        /// </summary>
        public TaskStatus GetTaskStatusForDate(DateTime date)
        {
            // Filter all daily tasks for specified date
            var dailyTasks = DailyShoppingTasks.Concat(DailyWorkTasks)
                .Where(t => t.TaskDate.Date == date.Date)
                .ToList();

            if (dailyTasks.Count == 0)
                return TaskStatus.InProgress; // No tasks = In Progress (yellow=)

            // All tasks completed = Completed; Has uncompleted = Uncompleted
            bool allCompleted = dailyTasks.All(t => t.IsCompleted);
            return allCompleted ? TaskStatus.Completed : TaskStatus.Uncompleted;
        }

        /// <summary>
        /// Get week day name for date (Monday/Tuesday...Sunday)
        /// </summary>
        public string GetWeekDayName(DateTime date)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => "Mon",
                DayOfWeek.Tuesday => "Tue",
                DayOfWeek.Wednesday => "Wed",
                DayOfWeek.Thursday => "Thu",
                DayOfWeek.Friday => "Fri",
                DayOfWeek.Saturday => "Sat",
                DayOfWeek.Sunday => "Sun",
                _ => ""
            };
        }
        #endregion

        // Original methods (retained and adapted for TaskDate)
        public List<TaskItem> GetUncompletedDailyTasks()
        {
            var allDailyTasks = DailyShoppingTasks.Concat(DailyWorkTasks).ToList();
            return allDailyTasks.Where(t => !t.IsCompleted).ToList();
        }

        public void CompleteAllUnfinishedDailyTasks()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var task in DailyShoppingTasks.Where(t => !t.IsCompleted))
                    task.IsCompleted = true;
                foreach (var task in DailyWorkTasks.Where(t => !t.IsCompleted))
                    task.IsCompleted = true;
            });
        }

        public string GetTodayTaskSummary()
        {
            var allDailyTasks = DailyShoppingTasks.Concat(DailyWorkTasks)
                .Where(t => t.TaskDate.Date == DateTime.Today.Date)
                .ToList();

            if (allDailyTasks.Count == 0)
                return "No tasks for today, go to Task Configuration to add some～";

            var completedTasks = allDailyTasks.Where(t => t.IsCompleted).ToList();
            var uncompletedTasks = allDailyTasks.Where(t => !t.IsCompleted).ToList();

            if (uncompletedTasks.Count == 0)
                return "All today's tasks have been completed🎉";

            var taskContents = uncompletedTasks.Take(2).Select(t => t.Content).ToList();
            var summary = string.Join("; ", taskContents);
            if (uncompletedTasks.Count > 2)
                summary += $" and {uncompletedTasks.Count} other uncompleted tasks";
            return summary;
        }
    }
}