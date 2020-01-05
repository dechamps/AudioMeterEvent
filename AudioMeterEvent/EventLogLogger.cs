namespace AudioMeterEvent
{
    class EventLogLogger : Logger
    {
        public EventLogLogger(System.Diagnostics.EventLog eventLog)
        {
            EventLog = eventLog;
        }

        readonly System.Diagnostics.EventLog EventLog;

        public void Log(string message)
        {
            EventLog.WriteEntry(message);
        }
    }
}