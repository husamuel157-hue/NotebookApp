using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace NotebookApp
{
    public partial class TaskListPage : ContentPage
    {
        // Global task manager instance
        private readonly GlobalTaskManager _taskManager = GlobalTaskManager.Instance;
        // Current task cycle (Daily/Weekly)
        private readonly string _taskCycle;

        public TaskListPage(string taskCycle)
        {
            InitializeComponent();
            _taskCycle = taskCycle;

            // Set page title
            Title = "Task Management";

            // Bind global data to UI
            BindGlobalTaskData();
            
            // Update task count statistics
            UpdateTaskCounts();
        }

        /// <summary>
        /// Bind global task data to CollectionView
        /// </summary>
        private void BindGlobalTaskData()
        {
            if (_taskCycle == "Daily")
            {
                cvShoppingTasks.ItemsSource = _taskManager.DailyShoppingTasks;
                cvWorkTasks.ItemsSource = _taskManager.DailyWorkTasks;
                
                // Subscribe to collection change events
                _taskManager.DailyShoppingTasks.CollectionChanged += OnShoppingTasksChanged;
                _taskManager.DailyWorkTasks.CollectionChanged += OnWorkTasksChanged;
            }
            else if (_taskCycle == "Weekly")
            {
                cvShoppingTasks.ItemsSource = _taskManager.WeeklyShoppingTasks;
                cvWorkTasks.ItemsSource = _taskManager.WeeklyWorkTasks;
                
                // Subscribe to collection change events
                _taskManager.WeeklyShoppingTasks.CollectionChanged += OnShoppingTasksChanged;
                _taskManager.WeeklyWorkTasks.CollectionChanged += OnWorkTasksChanged;
            }
        }

        /// <summary>
        /// Update task count statistics
        /// </summary>
        private void UpdateTaskCounts()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                int shoppingCount = 0;
                int workCount = 0;

                if (_taskCycle == "Daily")
                {
                    shoppingCount = _taskManager.DailyShoppingTasks.Count;
                    workCount = _taskManager.DailyWorkTasks.Count;
                }
                else if (_taskCycle == "Weekly")
                {
                    shoppingCount = _taskManager.WeeklyShoppingTasks.Count;
                    workCount = _taskManager.WeeklyWorkTasks.Count;
                }

                lblShoppingCount.Text = $"🛒 {shoppingCount} tasks";
                lblWorkCount.Text = $"💼 {workCount} tasks";

                // Control card display/hide
                UpdateCardVisibility(shoppingCount, workCount);
            });
        }

        /// <summary>
        /// Update card display status
        /// </summary>
        private void UpdateCardVisibility(int shoppingCount, int workCount)
        {
            // Shopping tasks list
            cvShoppingTasks.IsVisible = shoppingCount > 0;
            
            // Work tasks list
            cvWorkTasks.IsVisible = workCount > 0;
        }

        /// <summary>
        /// Shopping tasks collection change event handler
        /// </summary>
        private void OnShoppingTasksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTaskCounts();
        }

        /// <summary>
        /// Work tasks collection change event handler
        /// </summary>
        private void OnWorkTasksChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateTaskCounts();
        }

        #region Shopping Tasks CRUD (operate global data + associate date)
        private async void BtnAddShoppingTask_Clicked(object sender, EventArgs e)
        {
            string? taskContent = await DisplayPromptAsync(
                title: $"Add {(_taskCycle == "Daily" ? "Daily" : "Weekly")} Shopping Task",
                message: "Please enter task content:",
                placeholder: "e.g., Buy milk, bread",
                accept: "Confirm",
                cancel: "Cancel",
                maxLength: 100
            );

            // Filter empty input or pure whitespace input
            if (taskContent == null || taskContent.Trim().Length == 0)
            {
                Console.WriteLine("User canceled input or input is empty (shopping task)");
                return;
            }

            // Force update collection on UI thread (avoid async thread issues)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var newTask = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Content = taskContent.Trim(),
                    TaskCycle = _taskCycle,
                    TaskCategory = "Shopping",
                    TaskDate = DateTime.Today // Associate current date
                };

                // Add to corresponding global collection
                if (_taskCycle == "Daily")
                    _taskManager.DailyShoppingTasks.Add(newTask);
                else
                    _taskManager.WeeklyShoppingTasks.Add(newTask);

                // Update task count
                UpdateTaskCounts();
                
                // Debug log
                Console.WriteLine($"Add {_taskCycle} shopping task: {newTask.Content}, date: {newTask.TaskDate:yyyy-MM-dd}");
            });
        }

        private async void BtnDeleteShoppingTask_Clicked(object sender, TappedEventArgs e)
        {
            // Validate trigger and parameters
            if (sender is not Image image || e.Parameter is not Guid taskId)
            {
                Console.WriteLine("Delete shopping task: parameter exception");
                return;
            }

            // Confirmation dialog for deletion
            bool isConfirm = await DisplayAlert(
                title: "Confirm Deletion",
                message: $"Are you sure you want to delete this {(_taskCycle == "Daily" ? "Daily" : "Weekly")} shopping task?",
                accept: "Delete",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Execute deletion operation on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TaskItem? taskToDelete = null;
                if (_taskCycle == "Daily")
                {
                    taskToDelete = _taskManager.DailyShoppingTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToDelete != null) _taskManager.DailyShoppingTasks.Remove(taskToDelete);
                }
                else
                {
                    taskToDelete = _taskManager.WeeklyShoppingTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToDelete != null) _taskManager.WeeklyShoppingTasks.Remove(taskToDelete);
                }

                // Update task count
                UpdateTaskCounts();
                
                // Debug log
                if (taskToDelete != null)
                    Console.WriteLine($"Delete {_taskCycle} shopping task: {taskToDelete.Content}");
                else
                    Console.WriteLine($"Delete {_taskCycle} shopping task: Task with ID {taskId} not found");
            });
        }
        #endregion

        #region Work Tasks CRUD (operate global data + associate date)
        private async void BtnAddWorkTask_Clicked(object sender, EventArgs e)
        {
            string? taskContent = await DisplayPromptAsync(
                title: $"Add {(_taskCycle == "Daily" ? "Daily" : "Weekly")} Work Task",
                message: "Please enter task content:",
                placeholder: "e.g., Complete MAUI page development",
                accept: "Confirm",
                cancel: "Cancel",
                maxLength: 100
            );

            // Filter empty input or pure whitespace input
            if (taskContent == null || taskContent.Trim().Length == 0)
            {
                Console.WriteLine("User canceled input or input is empty (work task)");
                return;
            }

            // Update collection on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var newTask = new TaskItem
                {
                    Id = Guid.NewGuid(),
                    Content = taskContent.Trim(),
                    TaskCycle = _taskCycle,
                    TaskCategory = "Work",
                    TaskDate = DateTime.Today // Associate current date
                };

                // Add to corresponding global collection
                if (_taskCycle == "Daily")
                    _taskManager.DailyWorkTasks.Add(newTask);
                else
                    _taskManager.WeeklyWorkTasks.Add(newTask);

                // Update task count
                UpdateTaskCounts();
                
                // Debug log
                Console.WriteLine($"Add {_taskCycle} work task: {newTask.Content}, date: {newTask.TaskDate:yyyy-MM-dd}");
            });
        }

        private async void BtnDeleteWorkTask_Clicked(object sender, TappedEventArgs e)
        {
            // Validate trigger and parameters
            if (sender is not Image image || e.Parameter is not Guid taskId)
            {
                Console.WriteLine("Delete work task: parameter exception");
                return;
            }

            // Confirmation dialog for deletion
            bool isConfirm = await DisplayAlert(
                title: "Confirm Deletion",
                message: $"Are you sure you want to delete this {(_taskCycle == "Daily" ? "Daily" : "Weekly")} work task?",
                accept: "Delete",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Execute deletion operation on UI thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TaskItem? taskToDelete = null;
                if (_taskCycle == "Daily")
                {
                    taskToDelete = _taskManager.DailyWorkTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToDelete != null) _taskManager.DailyWorkTasks.Remove(taskToDelete);
                }
                else
                {
                    taskToDelete = _taskManager.WeeklyWorkTasks.FirstOrDefault(t => t.Id == taskId);
                    if (taskToDelete != null) _taskManager.WeeklyWorkTasks.Remove(taskToDelete);
                }

                // Update task count
                UpdateTaskCounts();
                
                // Debug log
                if (taskToDelete != null)
                    Console.WriteLine($"Delete {_taskCycle} work task: {taskToDelete.Content}");
                else
                    Console.WriteLine($"Delete {_taskCycle} work task: Task with ID {taskId} not found");
            });
        }
        #endregion
    }
}