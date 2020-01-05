namespace AudioMeterEvent
{
    sealed class AudioMeterEvent
    {
        public AudioMeterEvent(string audioDeviceId, Logger logger)
        {
            this.logger = logger;

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

            new AudioMeter(device.ActivateInterface<EndpointVolume.IAudioMeterInformation>()).SoundDetected += (object sender, System.EventArgs eventArgs) =>
            {
               logger.Log("Sound detected");
            };
            logger.Log("Audio meter monitoring started");
        }

        readonly Logger logger;
    }
}