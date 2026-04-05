using Microsoft.Maui.Controls.Shapes;

namespace NotebookApp
{
    public partial class WeeklyTaskPreviewPage : ContentPage
    {
        private readonly GlobalTaskManager _taskManager = GlobalTaskManager.Instance;
        private List<DateTime> _weekDates = new();

        public WeeklyTaskPreviewPage()
        {
            InitializeComponent();
            LoadWeeklyTaskStatus();
            SubscribeToTaskChanges();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            LoadWeeklyTaskStatus();
        }

        private void SubscribeToTaskChanges()
        {
            _taskManager.DailyShoppingTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
            _taskManager.DailyWorkTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
            _taskManager.WeeklyShoppingTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
            _taskManager.WeeklyWorkTasks.CollectionChanged += (s, e) => LoadWeeklyTaskStatus();
        }

        private void LoadWeeklyTaskStatus()
        {
            // 1. Get the date range of this week
            _weekDates = _taskManager.GetThisWeekDates();

            // Set date range display
            var startDate = _weekDates.First();
            var endDate = _weekDates.Last();
            lblWeekRange.Text = $"{startDate:yyyy/M/d} - {endDate:yyyy/M/d}";

            // 2. Render 7-day status
            RenderDay(0, lblWeekDay1, lblDay1, lblStatus1);
            RenderDay(1, lblWeekDay2, lblDay2, lblStatus2);
            RenderDay(2, lblWeekDay3, lblDay3, lblStatus3);
            RenderDay(3, lblWeekDay4, lblDay4, lblStatus4);
            RenderDay(4, lblWeekDay5, lblDay5, lblStatus5);
            RenderDay(5, lblWeekDay6, lblDay6, lblStatus6);
            RenderDay(6, lblWeekDay7, lblDay7, lblStatus7);

            // 3. Update statistical information
            UpdateStatistics();
        }

        private void RenderDay(int index, Label lblWeekDay, Label lblDay, Label lblStatus)
        {
            var date = _weekDates[index];

            // Set the week and date
            lblWeekDay.Text = _taskManager.GetWeekDayName(date);
            lblDay.Text = date.Day.ToString();

            // Get the tasks for that day
            var dailyTasks = _taskManager.DailyShoppingTasks.Concat(_taskManager.DailyWorkTasks)
                .Where(t => t.TaskDate.Date == date.Date)
                .ToList();

            var weeklyTasks = _taskManager.WeeklyShoppingTasks.Concat(_taskManager.WeeklyWorkTasks)
                .Where(t => t.TaskDate.Date == date.Date)
                .ToList();

            var allTasks = dailyTasks.Concat(weeklyTasks).ToList();

            // Set status flag
            if (allTasks.Count == 0)
            {
                lblStatus.Text = "";
                lblStatus.IsVisible = false;
                return;
            }

            lblStatus.IsVisible = true;
            
            var completedCount = allTasks.Count(t => t.IsCompleted);
            var totalCount = allTasks.Count;

            if (completedCount == totalCount)
            {
                // All completed
                lblStatus.Text = "✓";
                lblStatus.TextColor = Color.FromArgb("#F44336"); // red
            }
            else if (completedCount == 0)
            {
                // All incomplete
                lblStatus.Text = "✗";
                lblStatus.TextColor = Color.FromArgb("#212121"); // black
            }
            else
            {
                // partially completed
                lblStatus.Text = "=";
                lblStatus.TextColor = Color.FromArgb("#FFC107"); // yellow
            }
        }

        private void UpdateStatistics()
        {
            // Get all tasks for this week
            var weekTasks = _taskManager.DailyShoppingTasks.Concat(_taskManager.DailyWorkTasks)
                .Concat(_taskManager.WeeklyShoppingTasks).Concat(_taskManager.WeeklyWorkTasks)
                .Where(t => _weekDates.Contains(t.TaskDate.Date))
                .ToList();

            // Calculate the completion rate for this week
            var totalTasks = weekTasks.Count;
            var completedTasks = weekTasks.Count(t => t.IsCompleted);
            var completionRate = totalTasks > 0 ? (double)completedTasks / totalTasks : 0;

            lblCompletionRate.Text = $"{completionRate:P0}";

            // Update progress bar
            var progressWidth = Math.Min(completionRate, 1.0);
            // Use relative width (percentage) instead of absolute width
            progressBarFill.WidthRequest = progressBarContainer.Width * progressWidth;

            // classified statistics
            var shoppingTasks = weekTasks.Where(t => t.TaskCategory == "Shopping").ToList();
            var workTasks = weekTasks.Where(t => t.TaskCategory == "Work").ToList();

            // Shopping task statistics
            var shoppingCompleted = shoppingTasks.Count(t => t.IsCompleted);
            var shoppingTotal = shoppingTasks.Count;
            var shoppingRate = shoppingTotal > 0 ? (double)shoppingCompleted / shoppingTotal : 0;
            lblShoppingStats.Text = $"{shoppingCompleted}/{shoppingTotal} complete ({shoppingRate:P0})";
            shoppingProgressBar.WidthRequest = shoppingProgressBarContainer.Width * shoppingRate;

            // Work task statistics
            var workCompleted = workTasks.Count(t => t.IsCompleted);
            var workTotal = workTasks.Count;
            var workRate = workTotal > 0 ? (double)workCompleted / workTotal : 0;
            lblWorkStats.Text = $"{workCompleted}/{workTotal} complete ({workRate:P0})";
            workProgressBar.WidthRequest = workProgressBarContainer.Width * workRate;
        }
    }
}