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

            // Set page title and text
            Title = $"{_taskCycle} Task List";
            lblTaskType.Text = $"{_taskCycle} Tasks";

            // Bind global data to UI (core: replace local collection with global collection)
            BindGlobalTaskData();
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
            }
            else if (_taskCycle == "Weekly")
            {
                cvShoppingTasks.ItemsSource = _taskManager.WeeklyShoppingTasks;
                cvWorkTasks.ItemsSource = _taskManager.WeeklyWorkTasks;
            }
        }

        #region Shopping Tasks CRUD (operate global data + associate date)
        private async void BtnAddShoppingTask_Clicked(object sender, EventArgs e)
        {
            string? taskContent = await DisplayPromptAsync(
                title: $"Add {_taskCycle} Shopping Task",
                message: "Please enter task content:",
                placeholder: "e.g.: Buy milk, bread",
                accept: "Confirm",
                cancel: "Cancel",
                maxLength: 100
            );

            // Filter empty input or pure whitespace input
            if (taskContent == null || taskContent.Trim().Length == 0)
            {
                Console.WriteLine("User cancelled input or input is empty (Shopping Task)");
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
                    TaskDate = DateTime.Today // Associate with current date
                };

                // Add to corresponding global collection
                if (_taskCycle == "Daily")
                    _taskManager.DailyShoppingTasks.Add(newTask);
                else
                    _taskManager.WeeklyShoppingTasks.Add(newTask);

                // Debug log
                Console.WriteLine($"Added {_taskCycle} Shopping Task: {newTask.Content}, Date: {newTask.TaskDate:yyyy-MM-dd}");
            });
        }

        private async void BtnDeleteShoppingTask_Clicked(object sender, EventArgs e)
        {
            // Validate button and parameters
            if (sender is not Button btn || btn.CommandParameter is not Guid taskId)
            {
                Console.WriteLine("Delete Shopping Task: Parameter exception");
                return;
            }

            // Confirm dialog to delete
            bool isConfirm = await DisplayAlert(
                title: "Confirm Delete",
                message: $"Do you want to delete this {_taskCycle} Shopping Task?",
                accept: "Delete",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Execute delete operation on UI thread
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

                // Debug log
                if (taskToDelete != null)
                    Console.WriteLine($"Deleted {_taskCycle} Shopping Task: {taskToDelete.Content}");
                else
                    Console.WriteLine($"Delete {_taskCycle} Shopping Task: Task with ID {taskId} not found");
            });
        }
        #endregion

        #region Work Tasks CRUD (operate global data + associate date)
        private async void BtnAddWorkTask_Clicked(object sender, EventArgs e)
        {
            string? taskContent = await DisplayPromptAsync(
                title: $"Add {_taskCycle} Work Task",
                message: "Please enter task content:",
                placeholder: "e.g.: Complete MAUI page development",
                accept: "Confirm",
                cancel: "Cancel",
                maxLength: 100
            );

            // Filter empty input or pure whitespace input
            if (taskContent == null || taskContent.Trim().Length == 0)
            {
                Console.WriteLine("User cancelled input or input is empty (Work Task)");
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
                    TaskDate = DateTime.Today // Associate with current date
                };

                // Add to corresponding global collection
                if (_taskCycle == "Daily")
                    _taskManager.DailyWorkTasks.Add(newTask);
                else
                    _taskManager.WeeklyWorkTasks.Add(newTask);

                // Debug log
                Console.WriteLine($"Added {_taskCycle} Work Task: {newTask.Content}, Date: {newTask.TaskDate:yyyy-MM-dd}");
            });
        }

        private async void BtnDeleteWorkTask_Clicked(object sender, EventArgs e)
        {
            // Validate button and parameters
            if (sender is not Button btn || btn.CommandParameter is not Guid taskId)
            {
                Console.WriteLine("Delete Work Task: Parameter exception");
                return;
            }

            // Confirm dialog to delete
            bool isConfirm = await DisplayAlert(
                title: "Confirm Delete",
                message: $"Do you want to delete this {_taskCycle} Work Task?",
                accept: "Delete",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // Execute delete operation on UI thread
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

                // Debug log
                if (taskToDelete != null)
                    Console.WriteLine($"Deleted {_taskCycle} Work Task: {taskToDelete.Content}");
                else
                    Console.WriteLine($"Delete {_taskCycle} Work Task: Task with ID {taskId} not found");
            });
        }
        #endregion
    }
}