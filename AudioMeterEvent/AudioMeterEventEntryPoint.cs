using System.Linq;

[assembly: System.Reflection.AssemblyTitle("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyDescription("Takes action in response to changes in audio level")]
[assembly: System.Reflection.AssemblyProduct("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyCompany("Etienne Dechamps")]
[assembly: System.Reflection.AssemblyCopyright("Etienne Dechamps <etienne@edechamps.fr>")]

namespace AudioMeterEvent
{
    static class AudioMeterEventEntryPoint
    {
        static int Main(string[] argsArray)
        {
            System.Collections.Generic.IEnumerable<string> args = argsArray;
            if (GetConsoleMode(ref args)) {
                System.ServiceProcess.ServiceBase.Run(new Service(args));
                return 0;
            }

            try
            {
                new AudioMeterEvent(args, new ConsoleLogger());
            } catch (AudioMeterEvent.UsageException usageException)
            {
                System.Console.Error.WriteLine(usageException.Message);
                return 1;
            }
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
                new AudioMeterEvent(args, new ConsoleLogger());
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