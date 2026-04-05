using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Maui.Dispatching;

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
        private GlobalTaskManager() 
        {
            InitializeTestData();
        }

        // Global task collections
        public ObservableCollection<TaskItem> DailyShoppingTasks { get; } = new();
        public ObservableCollection<TaskItem> DailyWorkTasks { get; } = new();
        public ObservableCollection<TaskItem> WeeklyShoppingTasks { get; } = new();
        public ObservableCollection<TaskItem> WeeklyWorkTasks { get; } = new();

        private void InitializeTestData()
        {
        }

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

        public void CompleteAllUnfinishedWeeklyTasks()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var task in WeeklyShoppingTasks.Where(t => !t.IsCompleted))
                    task.IsCompleted = true;
                foreach (var task in WeeklyWorkTasks.Where(t => !t.IsCompleted))
                    task.IsCompleted = true;
            });
        }

        public int GetDailyTaskCompletionCount()
        {
            var allDailyTasks = DailyShoppingTasks.Concat(DailyWorkTasks).ToList();
            return allDailyTasks.Count(t => t.IsCompleted);
        }

        public int GetWeeklyTaskCompletionCount()
        {
            var allWeeklyTasks = WeeklyShoppingTasks.Concat(WeeklyWorkTasks).ToList();
            return allWeeklyTasks.Count(t => t.IsCompleted);
        }

        public int GetTotalDailyTasks()
        {
            return DailyShoppingTasks.Count + DailyWorkTasks.Count;
        }

        public int GetTotalWeeklyTasks()
        {
            return WeeklyShoppingTasks.Count + WeeklyWorkTasks.Count;
        }

        public double GetDailyCompletionRate()
        {
            var total = GetTotalDailyTasks();
            if (total == 0) return 0;
            return (double)GetDailyTaskCompletionCount() / total;
        }

        public double GetWeeklyCompletionRate()
        {
            var total = GetTotalWeeklyTasks();
            if (total == 0) return 0;
            return (double)GetWeeklyTaskCompletionCount() / total;
        }
    }
}