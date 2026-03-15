namespace NotebookApp
{
    public partial class TaskConfigPage : ContentPage
    {
        public TaskConfigPage()
        {
            InitializeComponent();
        }

        // Daily task card click event: Navigate to daily task list
        private async void TapDailyTask(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new TaskListPage("Daily"));
        }

        // Weekly task card click event: Navigate to weekly task list
        private async void TapWeeklyTask(object sender, TappedEventArgs e)
        {
            await Navigation.PushAsync(new TaskListPage("Weekly"));
        }
    }
}