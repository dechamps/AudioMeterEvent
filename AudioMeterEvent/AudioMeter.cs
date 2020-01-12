namespace AudioMeterEvent
{
    sealed class AudioMeter : System.IDisposable
    {
        public AudioMeter(EndpointVolume.IAudioMeterInformation audioMeterInformation, SignalRatio minimumLevel, System.TimeSpan minimumDuration, System.TimeSpan Period)
        {
            AudioMeterInformation = audioMeterInformation;
            MinimumLevel = minimumLevel;
            MinimumDuration = minimumDuration;
            Timer = new System.Timers.Timer(Period.TotalMilliseconds)
            {
                AutoReset = true,
            };
            Timer.Elapsed += Timer_Elapsed;
            // TODO: we could make this more efficient when no audio is playing by using IAudioSessionNotification and IAudioSessionEvents, so that we don't poll when no audio session is active.
            Timer.Start();
        }

        public sealed class SoundDetectedEventArgs : System.EventArgs {
            public SignalRatio PeakLevel { get; set; }
        }

        public event System.EventHandler<SoundDetectedEventArgs> SoundDetected = delegate {};

        readonly SignalRatio MinimumLevel;

        readonly EndpointVolume.IAudioMeterInformation AudioMeterInformation;

        readonly System.Timers.Timer Timer;
        public void Dispose()
        {
            Timer.Dispose();
        }

        readonly System.TimeSpan MinimumDuration;
        readonly object CurrentDurationMutex = new object();
        System.TimeSpan CurrentDuration;
        void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs elapsedEventArgs)
        {
            AudioMeterInformation.GetPeakValue(out var peakFactor);
            var peakLevel = new SignalRatio { Factor = peakFactor };
            if (peakLevel < MinimumLevel)
            {
                lock (CurrentDurationMutex) CurrentDuration = System.TimeSpan.Zero;
            }
            lock (CurrentDurationMutex)
            {
                CurrentDuration += System.TimeSpan.FromMilliseconds(Timer.Interval);
                if (CurrentDuration < MinimumDuration) return;
            }
            SoundDetected(this, new SoundDetectedEventArgs { PeakLevel = peakLevel });
        }
    }
}