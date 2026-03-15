namespace NotebookApp
{
    public partial class MainPage : ContentPage
    {
        private readonly GlobalTaskManager _taskManager = GlobalTaskManager.Instance;

        // Property bound to UI triggers (whether all daily tasks are completed)
        public bool IsAllDailyTasksCompleted
        {
            get => _taskManager.GetUncompletedDailyTasks().Count == 0;
        }

        public MainPage()
        {
            InitializeComponent();
            InitializePage();
        }

        private void InitializePage()
        {
            // 1. Date formatting
            var currentDate = DateTime.Now;
            lblCurrentDate.Text = $"{currentDate.Year}/{currentDate.Month}/{currentDate.Day}";

            // 2. Initialize binding context (for triggers)
            BindingContext = this;

            // 3. Initialize today's task display
            UpdateTodayTaskDisplay();

            // 4. Listen for global task changes (auto refresh)
            SubscribeToGlobalTaskChanges();

            // 5. Bind button click events
            BindButtonEvents();
        }

        /// <summary>
        /// Bind all button events
        /// </summary>
        private void BindButtonEvents()
        {
            // Complete tasks button
            btnCompleteTask.Clicked += async (s, e) => await OnCompleteTaskClicked();
            // Task status preview navigation
            btnTaskStatusPreview.Clicked += async (s, e) => await Navigation.PushAsync(new WeeklyTaskPreviewPage());
        }

        /// <summary>
        /// Complete tasks button click logic
        /// </summary>
        private async Task OnCompleteTaskClicked()
        {
            // 1. Check for uncompleted tasks
            var uncompletedTasks = _taskManager.GetUncompletedDailyTasks();
            if (uncompletedTasks.Count == 0)
            {
                await DisplayAlertAsync("Prompt", "All today's tasks have been completed～", "OK");
                return;
            }

            // 2. Confirm dialog to complete all uncompleted tasks
            bool isConfirm = await DisplayAlertAsync(
                title: "Confirm Completion",
                message: $"Do you want to mark all {uncompletedTasks.Count} uncompleted tasks as completed?",
                accept: "Confirm",
                cancel: "Cancel"
            );

            if (!isConfirm) return;

            // 3. Mark tasks as completed
            _taskManager.CompleteAllUnfinishedDailyTasks();

            // 4. Refresh UI and prompt
            UpdateTodayTaskDisplay();
            await DisplayAlertAsync("Success", "All today's tasks have been marked as completed🎉", "OK");
        }

        /// <summary>
        /// Update today's task display (including style refresh)
        /// </summary>
        private void UpdateTodayTaskDisplay()
        {
            lblTodayTask.Text = _taskManager.GetTodayTaskSummary();
            // Force binding refresh (trigger style triggers)
            OnPropertyChanged(nameof(IsAllDailyTasksCompleted));
        }

        /// <summary>
        /// Subscribe to global task changes
        /// </summary>
        private void SubscribeToGlobalTaskChanges()
        {
            _taskManager.DailyShoppingTasks.CollectionChanged += (s, e) => UpdateTodayTaskDisplay();
            _taskManager.DailyWorkTasks.CollectionChanged += (s, e) => UpdateTodayTaskDisplay();
        }
    }
}