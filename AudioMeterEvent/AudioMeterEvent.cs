namespace AudioMeterEvent
{
    sealed class AudioMeterEvent : System.IDisposable
    {
        public AudioMeterEvent(
            string audioDeviceId,
            SignalRatio minimumLevel,
            System.TimeSpan minimumDuration,
            System.TimeSpan period,
            System.TimeSpan keepaliveInterval,
            System.TimeSpan keepaliveDuration,
            Logger logger)
        {
            MinimumLevel = minimumLevel;
            MinimumDuration = minimumDuration;
            KeepaliveInterval = keepaliveInterval;
            KeepaliveDuration = keepaliveDuration;
            Logger = logger;

            var deviceEnumerator = new MMDeviceAPI.MMDeviceEnumerator();
            MMDeviceAPI.IMMDevice device;
            try
            {
                device = deviceEnumerator.GetDeviceById(audioDeviceId);
            }
            catch (System.Exception exception)
            {
                throw new System.Exception("Unable to get audio device using specified ID", exception);
            }
            IAudioMeterInformation = device.ActivateInterface<EndpointVolume.IAudioMeterInformation>();

            Period = period;
            if (Period <= System.TimeSpan.Zero)
            {
                Period = new System.TimeSpan(minimumDuration.Ticks / 8);
                device.ActivateInterface<AudioClient.IAudioClient>().GetDevicePeriod(out var defaultDevicePeriod, out var minimumDevicePeriodTicks);
                var minimumDevicePeriod = new System.TimeSpan(minimumDevicePeriodTicks);
                if (Period < minimumDevicePeriod) Period = minimumDevicePeriod;
            }
            Logger.Log("Using minimum level: " + MinimumLevel + ", minimum duration: " + MinimumDuration + ", period: " + Period + ", keepalive interval: " + KeepaliveInterval + ", keepalive duration: " + KeepaliveDuration);
        }

        public event System.EventHandler Sounding = delegate { };
        public event System.EventHandler StoppedSounding = delegate { };
        readonly SerializedEventQueue EventQueue = new SerializedEventQueue();

        readonly SignalRatio MinimumLevel;
        readonly System.TimeSpan MinimumDuration;
        readonly System.TimeSpan KeepaliveInterval;
        readonly System.TimeSpan KeepaliveDuration;
        readonly System.TimeSpan Period;
        readonly Logger Logger;
        readonly EndpointVolume.IAudioMeterInformation IAudioMeterInformation;
        readonly object Mutex = new object();
        AudioMeter AudioMeter;
        KeepaliveTimers CurrentKeepaliveTimers;

        public void Start()
        {
            if (AudioMeter != null)
            {
                Logger.Log("Attempted to Start an already started AudioMeterEvent");
                return;
            }
            Logger.Log("Starting audio meter monitoring");
            var audioMeter = new AudioMeter(IAudioMeterInformation, MinimumLevel, MinimumDuration, Period);
            lock (Mutex)
            {
                AudioMeter = audioMeter;
            }
            AudioMeter.SoundDetected += AudioMeter_SoundDetected;
        }

        public void Stop()
        {
            if (AudioMeter == null)
            {
                Logger.Log("Attempted to Stop an already stopped AudioMeterEvent");
                return;
            }
            AudioMeter audioMeter;
            KeepaliveTimers keepaliveTimers;
            lock (Mutex)
            {
                audioMeter = AudioMeter;
                AudioMeter = null;

                Logger.Log("Stopped audio meter monitoring");
                if (CurrentKeepaliveTimers != null) EventQueue.EnqueueEvent(() => { StoppedSounding(this, System.EventArgs.Empty); });

                keepaliveTimers = CurrentKeepaliveTimers;
                CurrentKeepaliveTimers = null;
            }
            audioMeter.Dispose();
            if (keepaliveTimers != null) keepaliveTimers.Dispose();
        }

        void AudioMeter_SoundDetected(object sender, AudioMeter.SoundDetectedEventArgs eventArgs)
        {
            bool initial;
            var keepaliveTimers = new KeepaliveTimers(KeepaliveInterval, KeepaliveDuration);
            lock (Mutex)
            {
                if (sender != AudioMeter) return;  // Don't race against Stop()

                initial = CurrentKeepaliveTimers == null;
                if (!initial)
                    keepaliveTimers = CurrentKeepaliveTimers;
                else
                {
                    Logger.Log("Sounding (initial, " + eventArgs.PeakLevel + ")");
                    EventQueue.EnqueueEvent(() => { Sounding(this, System.EventArgs.Empty); });
                    CurrentKeepaliveTimers = keepaliveTimers;
                }
            }

            if (!initial)
            {
                keepaliveTimers.ResetDuration();
                return;
            }
            keepaliveTimers.IntervalElapsed += KeepaliveTimers_IntervalElapsed;
            keepaliveTimers.DurationElapsed += KeepaliveTimers_DurationElapsed;
            keepaliveTimers.Start();
        }

        void KeepaliveTimers_IntervalElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (Mutex)
            {
                if (sender != CurrentKeepaliveTimers) return;

                Logger.Log("Sounding (keepalive)");
                EventQueue.EnqueueEvent(() => { Sounding(this, System.EventArgs.Empty); });
            }
        }

        void KeepaliveTimers_DurationElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            KeepaliveTimers keepaliveTimers;
            lock (Mutex)
            {
                if (sender != CurrentKeepaliveTimers) return;
                keepaliveTimers = CurrentKeepaliveTimers;
                CurrentKeepaliveTimers = null;

                Logger.Log("Expiry");
                EventQueue.EnqueueEvent(() => { StoppedSounding(this, System.EventArgs.Empty); });
            }
            keepaliveTimers.Dispose();
        }

        public void Dispose()
        {
            EventQueue.Dispose();
        }

        sealed class KeepaliveTimers : System.IDisposable
        {
            readonly System.Timers.Timer IntervalTimer;
            readonly System.Timers.Timer DurationTimer;

            public KeepaliveTimers(System.TimeSpan interval, System.TimeSpan duration)
            {
                IntervalTimer = new System.Timers.Timer(interval.TotalMilliseconds) { AutoReset = true };
                IntervalTimer.Elapsed += IntervalTimer_Elapsed;

                DurationTimer = new System.Timers.Timer(duration.TotalMilliseconds);
                DurationTimer.Elapsed += DurationTimer_Elapsed;
            }

            public event System.Timers.ElapsedEventHandler IntervalElapsed;
            public event System.Timers.ElapsedEventHandler DurationElapsed;

            void IntervalTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                IntervalElapsed(this, e);
            }

            void DurationTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
            {
                DurationElapsed(this, e);
            }

            public void Start()
            {
                IntervalTimer.Start();
                DurationTimer.Start();
            }

            public void ResetDuration()
            {
                DurationTimer.Interval = DurationTimer.Interval;
            }

            public void Dispose()
            {
                IntervalTimer.Dispose();
                DurationTimer.Dispose();
            }
        }
    }
}