using CommandLine;

[assembly: System.Reflection.AssemblyTitle("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyDescription("Takes action in response to changes in audio level")]
[assembly: System.Reflection.AssemblyProduct("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyCompany("Etienne Dechamps")]
[assembly: System.Reflection.AssemblyCopyright("Etienne Dechamps <etienne@edechamps.fr>")]

namespace AudioMeterEvent
{
    static class AudioMeterEvent
    {
        class Options
        {
            [Option("audio-device-id", Required = true)]
            public string AudioDeviceId { get; set; }
        }

        static int Main(string[] args)
        {
            return Parser.Default.ParseArguments<Options>(args).MapResult(
                (Options options) => { Run(options); return 0; },
                errors => 1);
        }

        static void Run(Options options)
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
            System.Console.WriteLine(device.Id());
        }
    }
}