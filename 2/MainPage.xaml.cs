using Microsoft.Maui.Controls.Shapes;

namespace NotebookApp
{
    public partial class MainPage : ContentPage
    {
        private readonly GlobalTaskManager _taskManager = GlobalTaskManager.Instance;
        private List<TaskItem> _dailyTasks = new();
        private List<TaskItem> _weeklyTasks = new();

        public MainPage()
        {
            InitializeComponent();
            InitializePage();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshTasks();
        }

        private void InitializePage()
        {
            // Set current date
            var currentDate = DateTime.Now;
            lblCurrentDate.Text = $"{currentDate.Year}/{currentDate.Month}/{currentDate.Day} {GetEnglishWeekDay(currentDate.DayOfWeek)}";

            // Listen for task changes
            SubscribeToTaskChanges();
        }

        private string GetEnglishWeekDay(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Monday",
                DayOfWeek.Tuesday => "Tuesday",
                DayOfWeek.Wednesday => "Wednesday",
                DayOfWeek.Thursday => "Thursday",
                DayOfWeek.Friday => "Friday",
                DayOfWeek.Saturday => "Saturday",
                DayOfWeek.Sunday => "Sunday",
                _ => ""
            };
        }

        private void SubscribeToTaskChanges()
        {
            _taskManager.DailyShoppingTasks.CollectionChanged += (s, e) => RefreshTasks();
            _taskManager.DailyWorkTasks.CollectionChanged += (s, e) => RefreshTasks();
            _taskManager.WeeklyShoppingTasks.CollectionChanged += (s, e) => RefreshTasks();
            _taskManager.WeeklyWorkTasks.CollectionChanged += (s, e) => RefreshTasks();
        }

        private void RefreshTasks()
        {
            // Get today's tasks: only show TaskCycle="Daily" tasks
            _dailyTasks = _taskManager.DailyShoppingTasks
                .Concat(_taskManager.DailyWorkTasks)
                .Where(t => t.TaskCycle == "Daily" && t.TaskDate.Date == DateTime.Today.Date)
                .ToList();

            // Get weekly tasks: only show TaskCycle="Weekly" tasks
            var weekDates = _taskManager.GetThisWeekDates();
            _weeklyTasks = _taskManager.WeeklyShoppingTasks
                .Concat(_taskManager.WeeklyWorkTasks)
                .Where(t => t.TaskCycle == "Weekly" && weekDates.Contains(t.TaskDate.Date))
                .ToList();

            // Update statistics
            UpdateStats();

            // Update task lists
            UpdateTaskLists();
        }

        private void UpdateStats()
        {
            // Today's task statistics
            var dailyCompleted = _dailyTasks.Count(t => t.IsCompleted);
            var dailyTotal = _dailyTasks.Count;
            lblDailyStats.Text = $"{dailyCompleted}/{dailyTotal} Completed";

            // Weekly task statistics
            var weeklyCompleted = _weeklyTasks.Count(t => t.IsCompleted);
            var weeklyTotal = _weeklyTasks.Count;
            lblWeeklyStats.Text = $"{weeklyCompleted}/{weeklyTotal} Completed";
        }

        private void UpdateTaskLists()
        {
            // Clear containers
            dailyTasksContainer.Children.Clear();
            weeklyTasksContainer.Children.Clear();

            // Display today's tasks
            if (_dailyTasks.Count == 0)
            {
                dailyTasksContainer.Children.Add(new Label
                {
                    Text = "No tasks for today",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = 150
                });
            }
            else
            {
                foreach (var task in _dailyTasks)
                {
                    dailyTasksContainer.Children.Add(CreateTaskItemView(task));
                }
            }

            // Display weekly tasks
            if (_weeklyTasks.Count == 0)
            {
                weeklyTasksContainer.Children.Add(new Label
                {
                    Text = "No tasks for this week",
                    FontSize = 14,
                    TextColor = Colors.Gray,
                    HorizontalOptions = LayoutOptions.Center,
                    VerticalOptions = LayoutOptions.Center,
                    HeightRequest = 150
                });
            }
            else
            {
                foreach (var task in _weeklyTasks)
                {
                    weeklyTasksContainer.Children.Add(CreateTaskItemView(task));
                }
            }
        }

        private View CreateTaskItemView(TaskItem task)
        {
            // Main container
            var container = new Border
            {
                BackgroundColor = Colors.White,
                Stroke = Color.FromArgb("#E0E0E0"),
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(6) },
                HeightRequest = 36,
                Padding = new Thickness(0)
            };

            var mainLayout = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },   // Status button
                    new ColumnDefinition { Width = GridLength.Auto },   // Category label
                    new ColumnDefinition { Width = GridLength.Star },   // Task content
                    new ColumnDefinition { Width = GridLength.Auto }    // Complete button
                },
                ColumnSpacing = 8,
                VerticalOptions = LayoutOptions.Center
            };

            // Status button
            var statusButton = new Button
            {
                WidthRequest = 20,
                HeightRequest = 20,
                CornerRadius = 10,
                BackgroundColor = task.IsCompleted ? Color.FromArgb("#4CAF50") : Colors.White,
                Text = task.IsCompleted ? "✓" : "✗",
                TextColor = task.IsCompleted ? Colors.White : Color.FromArgb("#F44336"),
                FontSize = 12,
                Padding = 0,
                Margin = 0
            };

            statusButton.Clicked += (s, e) => ToggleTaskCompletion(task);

            // Category label (moved after status button)
            var categoryBorder = new Border
            {
                BackgroundColor = task.TaskCategory == "Shopping" ? 
                    Color.FromArgb("#FF9800") : Color.FromArgb("#2196F3"),
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(4) },
                Padding = new Thickness(6, 2),
                HeightRequest = 20,
                WidthRequest = 45,
                VerticalOptions = LayoutOptions.Center
            };

            var categoryLabel = new Label
            {
                Text = task.TaskCategory == "Shopping" ? "Shopping" : "Work",
                FontSize = 11,
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };

            categoryBorder.Content = categoryLabel;

            // Task content
            var contentLabel = new Label
            {
                Text = task.Content,
                FontSize = 14,
                TextColor = Colors.Black,
                VerticalOptions = LayoutOptions.Center,
                LineBreakMode = LineBreakMode.TailTruncation,
                Margin = new Thickness(8, 0, 8, 0)
            };

            // Complete button (only shown when task is not completed)
            var completeButton = new Button
            {
                Text = "Complete",
                FontSize = 12,
                TextColor = Colors.White,
                BackgroundColor = Color.FromArgb("#2196F3"),
                CornerRadius = 4,
                Padding = new Thickness(10, 4),
                HeightRequest = 28,
                WidthRequest = 60,
                VerticalOptions = LayoutOptions.Center,
                IsVisible = !task.IsCompleted  // Only shown when not completed
            };

            completeButton.Clicked += (s, e) => 
            {
                task.IsCompleted = true;
                RefreshTasks();
            };

            // Add to Grid
            mainLayout.Add(statusButton, 0, 0);
            mainLayout.Add(categoryBorder, 1, 0);
            mainLayout.Add(contentLabel, 2, 0);
            mainLayout.Add(completeButton, 3, 0);

            container.Content = mainLayout;

            return container;
        }

        private void ToggleTaskCompletion(TaskItem task)
        {
            task.IsCompleted = !task.IsCompleted;
            RefreshTasks();
        }

        private new async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await DisplayAlert(title, message, cancel);
        }

        /// <summary>
        /// Complete today's tasks in batch
        /// </summary>
        private async void BtnCompleteAllDaily_Clicked(object sender, TappedEventArgs e)
        {
            // Check if there are tasks for today
            if (_dailyTasks.Count == 0)
            {
                await DisplayAlertAsync("Prompt", "No tasks for today need to be completed", "OK");
                return;
            }

            // Check if all tasks have been completed
            var uncompletedTasks = _dailyTasks.Where(t => !t.IsCompleted).ToList();
            if (uncompletedTasks.Count == 0)
            {
                await DisplayAlertAsync("Prompt","All tasks for today have been completed","OK");
                return;
            }

            // Confirmation dialog
            bool isConfirm = await DisplayAlert(
                title: "Confirm batch completion",
                message: $"Are you sure you want to complete {uncompletedTasks.Count} tasks for today in batch?",
                accept: "Confirm",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Complete today's tasks in batch
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                foreach (var task in uncompletedTasks)
                {
                    task.IsCompleted = true;
                }

                // Refresh task list
                RefreshTasks();

                // Prompt completion
                await DisplayAlertAsync("Completed", $"Batch completed {uncompletedTasks.Count} tasks for today", "OK");
            });
        }

        /// <summary>
        /// Complete this week's tasks in batch
        /// </summary>
        private async void BtnCompleteAllWeekly_Clicked(object sender, TappedEventArgs e)
        {
            // Check if there are any tasks for this week
            if (_weeklyTasks.Count == 0)
            {
                await DisplayAlertAsync("Prompt", "No tasks for this week need to be completed", "OK");
                return;
            }

            // Check whether all tasks have been completed
            var uncompletedTasks = _weeklyTasks.Where(t => !t.IsCompleted).ToList();
            if (uncompletedTasks.Count == 0)
            {
                await DisplayAlertAsync("Prompt", "All tasks for this week have been completed", "OK");
                return;
            }

            // Confirmation dialog
            bool isConfirm = await DisplayAlert(
                title: "Confirm batch completion",
                message: $"Are you sure you want to complete {uncompletedTasks.Count} tasks for this week in batch?",
                accept: "Confirm",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Complete this week's tasks in batch
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                foreach (var task in uncompletedTasks)
                {
                    task.IsCompleted = true;
                }
                
                // Refresh task list
                RefreshTasks();
                
                // Prompt completion
                await DisplayAlertAsync("Completed", $"Batch completed {uncompletedTasks.Count} tasks for this week", "OK");
            });
        }
    }
}