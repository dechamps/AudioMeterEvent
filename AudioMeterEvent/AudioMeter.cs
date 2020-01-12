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
            // TODO: we could make this more efficient when no audio is playing by using IAudioSessionNotification and IAudioSessionEvents, so that we don't poll when no audio session is active.
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