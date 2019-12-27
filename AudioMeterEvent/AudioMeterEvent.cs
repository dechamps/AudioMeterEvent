using CommandLine;

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
            System.Console.WriteLine(new MMDeviceAPI.MMDeviceEnumerator()
                .GetDeviceById(options.AudioDeviceId).Id());
        }
    }
}