﻿using CommandLine;

namespace AudioMeterEvent
{
    sealed class AudioMeterEvent
    {
        class Options
        {
            [Option("audio-device-id", Required = true)]
            public string AudioDeviceId { get; set; }
        }

        public class UsageException: System.Exception
        {
            public UsageException(string message) : base(message) { }
        }

        public AudioMeterEvent(System.Collections.Generic.IEnumerable<string> args, Logger logger)
        {
            this.logger = logger;
            var helpWriter = new System.IO.StringWriter();
            new Parser(config => config.HelpWriter = helpWriter).ParseArguments<Options>(args)
                .WithParsed<Options>((Options options) => { Run(options); })
                .WithNotParsed<Options>(errors => { throw new UsageException(helpWriter.ToString()); });
        }

        readonly Logger logger;

        void Run(Options options)
        {
            var deviceEnumerator = new MMDeviceAPI.MMDeviceEnumerator();
            MMDeviceAPI.IMMDevice device;
            try
            {
                device = deviceEnumerator.GetDeviceById(options.AudioDeviceId);
            }
            catch (System.Exception exception)
            {
                throw new System.Exception("Unable to get audio device using specified ID", exception);
            }

            new AudioMeter(device.ActivateInterface<EndpointVolume.IAudioMeterInformation>()).SoundDetected += (object sender, System.EventArgs eventArgs) =>
            {
               logger.Log("Sound detected");
            };

            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
        }
    }
}