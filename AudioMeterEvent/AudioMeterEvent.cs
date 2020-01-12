﻿namespace AudioMeterEvent
{
    sealed class AudioMeterEvent
    {
        public AudioMeterEvent(string audioDeviceId, SignalRatio minimumLevel, System.TimeSpan minimumDuration, System.TimeSpan period, Logger logger)
        {
            MinimumLevel = minimumLevel;
            MinimumDuration = minimumDuration;
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
                device.ActivateInterface<AudioClient.IAudioClient>().GetDevicePeriod(out var defaultDevicePeriod, out var minimumDevicePeriod);
                Period = new System.TimeSpan(defaultDevicePeriod * 10);
            }
            Logger.Log("Using minimum level: " + MinimumLevel + ", minimum duration: " + MinimumDuration + ", period: " + Period);
        }

        readonly SignalRatio MinimumLevel;
        readonly System.TimeSpan MinimumDuration;
        readonly Logger Logger;
        readonly System.TimeSpan Period;
        readonly EndpointVolume.IAudioMeterInformation IAudioMeterInformation;
        readonly object Mutex = new object();
        AudioMeter AudioMeter;

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
            Logger.Log("Stopping audio meter monitoring");
            AudioMeter audioMeter;
            lock (Mutex)
            {
                audioMeter = AudioMeter;
                AudioMeter = null;
            }
            audioMeter.Dispose();
        }

        void AudioMeter_SoundDetected(object sender, AudioMeter.SoundDetectedEventArgs eventArgs)
        {
            lock (Mutex)
            {
                if (sender != AudioMeter) return;  // Don't race against Stop()
            }
            Logger.Log("Sound detected: " + eventArgs.PeakLevel);
        }
    }
}