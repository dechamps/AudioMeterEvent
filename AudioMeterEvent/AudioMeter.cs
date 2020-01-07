namespace AudioMeterEvent
{
    sealed class AudioMeter : System.IDisposable
    {
        public AudioMeter(EndpointVolume.IAudioMeterInformation audioMeterInformation)
        {
            AudioMeterInformation = audioMeterInformation;
            Timer.AutoReset = true;
            Timer.Elapsed += Timer_Elapsed;
            Timer.Start();
        }

        public event System.EventHandler SoundDetected = delegate {};

        readonly EndpointVolume.IAudioMeterInformation AudioMeterInformation;

        readonly System.Timers.Timer Timer = new System.Timers.Timer(1000);
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