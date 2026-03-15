using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;

namespace NotebookApp
{
    public partial class WeeklyTaskPreviewPage : ContentPage
    {
        private readonly GlobalTaskManager _taskManager = GlobalTaskManager.Instance;
        private List<DateTime> _weekDates;

        public WeeklyTaskPreviewPage()
        {
            InitializeComponent();
            LoadWeeklyTaskStatus();
            SubscribeToTaskChanges();
        }

        /// <summary>
        /// Load weekly task status (priority to show week sequence, show status only when there are tasks)
        /// </summary>
        private void LoadWeeklyTaskStatus()
        {
            // 1. Get date list from Monday to Sunday of current week
            _weekDates = _taskManager.GetThisWeekDates();

            // 2. Render week sequence and status for 7 days (core fix)
            RenderDay(0, lblWeekDay1, lblDay1, lblStatus1);
            RenderDay(1, lblWeekDay2, lblDay2, lblStatus2);
            RenderDay(2, lblWeekDay3, lblDay3, lblStatus3);
            RenderDay(3, lblWeekDay4, lblDay4, lblStatus4);
            RenderDay(4, lblWeekDay5, lblDay5, lblStatus5);
            RenderDay(5, lblWeekDay6, lblDay6, lblStatus6);
            RenderDay(6, lblWeekDay7, lblDay7, lblStatus7);
        }

        /// <summary>
        /// Render single day: Priority to show week day + date (week sequence), show status marker only when there are tasks
        /// </summary>
        private void RenderDay(int index, Label lblWeekDay, Label lblDay, Label lblStatus)
        {
            var date = _weekDates[index];
            var dailyTasks = _taskManager.DailyShoppingTasks.Concat(_taskManager.DailyWorkTasks)
                .Where(t => t.TaskDate.Date == date.Date)
                .ToList();

            // ========== Mandatory: Week sequence (week day + date) ==========
            lblWeekDay.Text = _taskManager.GetWeekDayName(date); // Mon/Tue...
            lblDay.Text = date.Day.ToString();                   // Date number (1-31)

            // ========== Show on demand: Status marker (only show when there are tasks) ==========
            if (dailyTasks.Count == 0)
            {
                lblStatus.IsVisible = false; // No tasks: hide status marker
                return;
            }

            // Has tasks: calculate status and show marker
            var status = _taskManager.GetTaskStatusForDate(date);
            lblStatus.IsVisible = true;
            switch (status)
            {
                case TaskStatus.Completed: // Completed: Red √
                    lblStatus.Text = "√";
                    lblStatus.TextColor = Colors.Red;
                    break;
                case TaskStatus.Uncompleted: // Uncompleted: Black ×
                    lblStatus.Text = "×";
                    lblStatus.TextColor = Colors.Black;
                    break;
                case TaskStatus.InProgress: // Theoretically not triggered (InProgress only when no tasks)
                    lblStatus.Text = "=";
                    lblStatus.TextColor = Colors.Yellow;
                    break;
            }

            // Highlight today: date text in blue
            if (date.Date == DateTime.Today.Date)
            {
                lblDay.TextColor = Colors.Blue;
            }
        }

        /// <summary>
        /// Refresh page when tasks change
        /// </summary>
        private void SubscribeToTaskChanges()
        {
            _taskManager.DailyShoppingTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
            _taskManager.DailyWorkTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
        }
    }
}