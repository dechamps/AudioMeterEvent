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

        public static Options Parse(System.Collections.Generic.IEnumerable<string> args, bool ignoreUnknownArguments = false)
        {
            var helpWriter = new System.IO.StringWriter();
            return new Parser(config => { config.HelpWriter = helpWriter; config.IgnoreUnknownArguments = ignoreUnknownArguments; })
                .ParseArguments<Options>(args).MapResult(
                options => options,
                _ => throw new UsageException(helpWriter.ToString()));
        }
    }

    static class AudioMeterEventEntryPoint
    {
        static int Main(string[] argsArray)
        {
            System.Collections.Generic.IEnumerable<string> args = argsArray;
            bool consoleMode = GetConsoleMode(ref args);
            
            Options options;
            try
            {
                options = Options.Parse(args);
            } catch (Options.UsageException usageException)
            {
                System.Console.Error.WriteLine(usageException.Message);
                return 1;
            }

            if (consoleMode)
                new AudioMeterEvent(options.AudioDeviceId, new ConsoleLogger());
            else
                System.ServiceProcess.ServiceBase.Run(new Service(options));
            
            return 0;
        }

        sealed class Service : System.ServiceProcess.ServiceBase
        {
            public Service(Options options)
            {
                Options = options;
            }

            readonly Options Options;

            protected override void OnStart(string[] args)
            {
                new AudioMeterEvent(Options.AudioDeviceId, new ConsoleLogger());
            }
        }

        static bool GetConsoleMode(ref System.Collections.Generic.IEnumerable<string> args)
        {
            if (args.Take(1).SequenceEqual(new string[] { "service" }))
            {
                args = args.Skip(1);
                return false;
            }
            return true;
        }
    }

    [System.ComponentModel.RunInstaller(true)]
    public class Installer : System.Configuration.Install.Installer
    {
        public Installer()
        {
            Installers.Add(new System.ServiceProcess.ServiceProcessInstaller
            {
                Account = System.ServiceProcess.ServiceAccount.LocalService
            });
            Installers.Add(new System.ServiceProcess.ServiceInstaller
            {
                ServiceName = "AudioMeterEvent",
                Description = "Raises events based on audio device meter levels.",
                ServicesDependedOn = new string[] { "Audiosrv" }
            });
        }

        protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
        {
            var args = new System.Collections.Generic.List<string>();
            foreach (System.Collections.DictionaryEntry keyValue in Context.Parameters)
            {
                if (keyValue.Value == null) continue;
                var key = keyValue.Key.ToString();
                // When "--foo=bar" is specified, InstallUtil only strips the first "-", resulting in "-foo=bar". If that happens, get rid of the leading dash.
                if (key.StartsWith("-")) key = key.Substring(1);
                args.Add("--" + key);
                args.Add(keyValue.Value.ToString());
            }
            Context.Parameters["assemblypath"] = QuoteCommandLineArgument(Context.Parameters["assemblypath"]) + " service " + Parser.Default.FormatCommandLine(
                Options.Parse(args,
                    // Ignore InstallUtil parameters such as "assemblypath"
                    ignoreUnknownArguments: true));
        }

        static string QuoteCommandLineArgument(string arg)
        {
            // https://stackoverflow.com/a/6040946/172594
            return "\"" + System.Text.RegularExpressions.Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
        }
    }
}