namespace AudioMeterEvent
{
    sealed class AudioMeter : System.IDisposable
    {
        public AudioMeter(EndpointVolume.IAudioMeterInformation audioMeterInformation, System.TimeSpan Period)
        {
            AudioMeterInformation = audioMeterInformation;
            Timer = new System.Timers.Timer(Period.TotalMilliseconds)
            {
                AutoReset = true,
            };
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        public event System.EventHandler SoundDetected = delegate {};

        readonly EndpointVolume.IAudioMeterInformation AudioMeterInformation;

        readonly System.Timers.Timer Timer;
        public void Dispose()
        {
            Timer.Dispose();
        }

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs elapsedEventArgs)
        {
            AudioMeterInformation.GetPeakValue(out var peakFactor);
            if (peakFactor <= 0) return;
            SoundDetected(this, System.EventArgs.Empty);
        }
    }
}