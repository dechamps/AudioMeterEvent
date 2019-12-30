namespace AudioMeterEvent
{
    class ConsoleLogger : Logger
    {
        public void Log(string message)
        {
            System.Console.WriteLine(message);
        }
    }
}