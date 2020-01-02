using System.Linq;
using CommandLine;

[assembly: System.Reflection.AssemblyTitle("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyDescription("Takes action in response to changes in audio level")]
[assembly: System.Reflection.AssemblyProduct("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyCompany("Etienne Dechamps")]
[assembly: System.Reflection.AssemblyCopyright("Etienne Dechamps <etienne@edechamps.fr>")]

namespace AudioMeterEvent
{
    class Options
    {
        [Option("audio-device-id", Required = true)]
        public string AudioDeviceId { get; set; }

        public class UsageException : System.Exception
        {
            public UsageException(string message) : base(message) { }
        }

        public static Options Parse(System.Collections.Generic.IEnumerable<string> args)
        {
            var helpWriter = new System.IO.StringWriter();
            return new Parser(config => config.HelpWriter = helpWriter).ParseArguments<Options>(args).MapResult(
                options => options,
                _ => throw new UsageException(helpWriter.ToString()));
        }
    }

    static class AudioMeterEventEntryPoint
    {
        static int Main(string[] argsArray)
        {
            System.Collections.Generic.IEnumerable<string> args = argsArray;
            if (GetConsoleMode(ref args)) {
                System.ServiceProcess.ServiceBase.Run(new Service(args));
                return 0;
            }

            Options options;
            try
            {
                options = Options.Parse(args);
            } catch (Options.UsageException usageException)
            {
                System.Console.Error.WriteLine(usageException.Message);
                return 1;
            }
            new AudioMeterEvent(options.AudioDeviceId, new ConsoleLogger());
            return 0;
        }

        sealed class Service : System.ServiceProcess.ServiceBase
        {
            public Service(System.Collections.Generic.IEnumerable<string> args)
            {
                this.args = args;
            }

            readonly System.Collections.Generic.IEnumerable<string> args;

            protected override void OnStart(string[] args)
            {
                var options = Options.Parse(args);
                new AudioMeterEvent(options.AudioDeviceId, new ConsoleLogger());
            }
        }

        static bool GetConsoleMode(ref System.Collections.Generic.IEnumerable<string> args)
        {
            if (args.Take(1).SequenceEqual(new string[] { "service" }))
            {
                args = args.Skip(1);
                return true;
            }
            return false;
        }
    }
}