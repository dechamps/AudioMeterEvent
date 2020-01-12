namespace AudioMeterEvent
{
    public class SerializedTaskQueue
    {
        readonly object Mutex = new object();
        readonly System.Collections.Concurrent.ConcurrentQueue<System.Action> Tasks = new System.Collections.Concurrent.ConcurrentQueue<System.Action>();

        public void Enqueue(System.Action action)
        {
            Tasks.Enqueue(action);
            System.Threading.Tasks.Task.Run(() =>
            {
                lock (Mutex)
                {
                    if (!Tasks.TryDequeue(out var action))
                        throw new System.Exception("SerializedTaskQueue is unexpectedly empty");
                    action();
                }
            });
        }
    }
}