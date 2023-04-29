namespace UPSMonitorService
{
    internal static class TaskExtensions
    {
        public static void FireAndForget(this Task task)
        {
            if(!task.IsCompleted || task.IsFaulted)
            {
                _ = FinishAndForget(task);
            }
        }

        private static async Task FinishAndForget(Task task)
        {
            try
            {
                await task.ConfigureAwait(false);
            }
            catch
            { }
        }
    }
}
