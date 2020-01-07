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
        [Option("audio-device-id", Required = true, HelpText = "The ID of the audio device to monitor. Use AudioDeviceList.exe to find the device ID.")]
        public string AudioDeviceId { get; set; }

        [Option("service", Hidden = true)]
        public bool Service { get; set; }

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

    static class ServiceInfo
    {
        public const string Name = "AudioMeterEvent";
    }

    public static class AudioMeterEventEntryPoint
    {
        static int Main(string[] args)
        {
            Options options;
            try
            {
                options = Options.Parse(args);
            } catch (Options.UsageException usageException)
            {
                System.Console.Error.WriteLine(usageException.Message);
                return 1;
            }

            if (options.Service)
                System.ServiceProcess.ServiceBase.Run(new Service(options));
            else
            {
                var audioMeterEvent = new AudioMeterEvent(options.AudioDeviceId, new ConsoleLogger());
                audioMeterEvent.Start();

                Microsoft.Win32.SystemEvents.PowerModeChanged += (object sender, Microsoft.Win32.PowerModeChangedEventArgs eventArgs) => {
                    if (eventArgs.Mode == Microsoft.Win32.PowerModes.Suspend) audioMeterEvent.Stop();
                    if (eventArgs.Mode == Microsoft.Win32.PowerModes.Resume) audioMeterEvent.Start();
                };

                var cancelKeyPressed = new System.Threading.ManualResetEventSlim();
                System.Console.CancelKeyPress += (object sender, System.ConsoleCancelEventArgs eventArgs) => {
                    audioMeterEvent.Stop();
                    cancelKeyPressed.Set();
                    eventArgs.Cancel = true;
                };
                cancelKeyPressed.Wait();
                audioMeterEvent.Stop();
            }

            return 0;
        }

        sealed class Service : System.ServiceProcess.ServiceBase
        {
            public Service(Options options)
            {
                ServiceName = ServiceInfo.Name;
                AutoLog = true;
                CanHandlePowerEvent = true;
                Options = options;
            }

            readonly Options Options;
            AudioMeterEvent AudioMeterEvent;

            protected override void OnStart(string[] args)
            {
                AudioMeterEvent = new AudioMeterEvent(Options.AudioDeviceId, new EventLogLogger(EventLog));
                AudioMeterEvent.Start();
            }

            protected override bool OnPowerEvent(System.ServiceProcess.PowerBroadcastStatus powerStatus)
            {
                if (powerStatus == System.ServiceProcess.PowerBroadcastStatus.Suspend) AudioMeterEvent.Stop();
                if (powerStatus == System.ServiceProcess.PowerBroadcastStatus.ResumeSuspend) AudioMeterEvent.Start();
                return base.OnPowerEvent(powerStatus);
            }

            protected override void OnStop()
            {
                AudioMeterEvent.Stop();
            }
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
                ServiceName = ServiceInfo.Name,
                Description = "Raises events based on audio device meter levels.",
                ServicesDependedOn = new string[] { "Audiosrv" }
            });
        }

        protected override void OnBeforeInstall(System.Collections.IDictionary savedState)
        {
            var args = new System.Collections.Generic.List<string>(new string[] { "--service" });
            foreach (System.Collections.DictionaryEntry keyValue in Context.Parameters)
            {
                if (keyValue.Value == null) continue;
                var key = keyValue.Key.ToString();
                // When "--foo=bar" is specified, InstallUtil only strips the first "-", resulting in "-foo=bar". If that happens, get rid of the leading dash.
                if (key.StartsWith("-")) key = key.Substring(1);
                args.Add("--" + key);
                args.Add(keyValue.Value.ToString());
            }
            Context.Parameters["assemblypath"] = QuoteCommandLineArgument(Context.Parameters["assemblypath"]) + " " + Parser.Default.FormatCommandLine(
                Options.Parse(args,
                    // Ignore InstallUtil parameters such as "assemblypath"
                    ignoreUnknownArguments: true),
                configuration => configuration.ShowHidden = true);

            base.OnBeforeInstall(savedState);
        }

        protected override void OnAfterInstall(System.Collections.IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            using var serviceManager = new ServiceManager();
            using var service = new ServiceManager.Service(serviceManager, ServiceInfo.Name);
            service.SetSidType(ServiceManager.ServiceSidType.SERVICE_SID_TYPE_RESTRICTED);
            service.SetRequiredPrivileges(new string[] { "SeChangeNotifyPrivilege" });
        }

        static string QuoteCommandLineArgument(string arg)
        {
            // https://stackoverflow.com/a/6040946/172594
            return "\"" + System.Text.RegularExpressions.Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
        }
    }
}