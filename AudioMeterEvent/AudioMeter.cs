namespace AudioMeterEvent
{
    sealed class AudioMeter : System.IDisposable
    {
        public AudioMeter(EndpointVolume.IAudioMeterInformation audioMeterInformation)
        {
            this.audioMeterInformation = audioMeterInformation;
            timer.AutoReset = true;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        public event System.EventHandler SoundDetected = delegate {};

        readonly EndpointVolume.IAudioMeterInformation audioMeterInformation;

        readonly System.Timers.Timer timer = new System.Timers.Timer(1000);
        public void Dispose()
        {
            timer.Dispose();
        }

        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs elapsedEventArgs)
        {
            audioMeterInformation.GetPeakValue(out var peakFactor);
            if (peakFactor <= 0) return;
            SoundDetected(this, System.EventArgs.Empty);
        }
    }
}