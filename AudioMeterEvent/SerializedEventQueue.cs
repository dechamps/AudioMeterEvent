namespace AudioMeterEvent
{
    public class SerializedEventQueue : System.IDisposable
    {
        System.Threading.CancellationTokenSource LastCancellationTokenSource = new System.Threading.CancellationTokenSource();
        System.Threading.Tasks.Task LastTask = System.Threading.Tasks.Task.CompletedTask;
        
        // Runs `action`. If called multiple times, actions are called in strict sequential order.
        // If a previously enqueued action has not started running, it will never be called.
        // (In other words, new actions render older ones obsolete.)
        public void EnqueueEvent(System.Action action)
        {
            LastCancellationTokenSource.Cancel();
            LastCancellationTokenSource.Dispose();
            LastTask = LastTask.ContinueWith(
                (System.Threading.Tasks.Task previousTask) => { action(); },
                (LastCancellationTokenSource = new System.Threading.CancellationTokenSource()).Token,
                System.Threading.Tasks.TaskContinuationOptions.LazyCancellation,
                System.Threading.Tasks.TaskScheduler.Current
            );
        }

        public void Wait()
        {
            LastTask.Wait();
        }

        public void Dispose()
        {
            LastCancellationTokenSource.Dispose();
        }
    }
}