namespace AudioMeterEvent
{
    public class SerializedTaskQueue
    {
        System.Threading.Tasks.Task LastTask = System.Threading.Tasks.Task.CompletedTask;

        // Runs `action`. If called multiple times, actions are called in strict sequential order.
        public void Enqueue(System.Action action)
        {
            LastTask = LastTask.ContinueWith((System.Threading.Tasks.Task previousTask) => { action(); });
        }
    }
}