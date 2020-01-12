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
        public Options() {
            MinimumDuration = System.TimeSpan.FromSeconds(2);
            KeepaliveInterval = System.TimeSpan.FromSeconds(30);
            KeepaliveDuration = System.TimeSpan.FromHours(1);
        }

        [Option("audio-device-id", Required = true, HelpText = "The ID of the audio device to monitor. Use AudioDeviceList.exe to find the device ID.")]
        public string AudioDeviceId { get; set; }

        [Option("minimum-level-db", Default = -60, HelpText = "Peak signal levels lower than this many decibels will be ignored.")]
        public double MinimumLevelDecibels { get; set; }

        [Option("minimum-duration", HelpText = "(Default: 2 seconds) Only consider signals that stay above minimum level for at least this long.")]
        public System.TimeSpan MinimumDuration { get; set; }

        [Option("period", HelpText = "How often to check the meter. If zero (the default), use 10x the Windows audio engine device period.")]
        public System.TimeSpan Period { get; set; }

        [Option("keepalive-interval", HelpText = "(Default: 30 seconds) After sound is detected, keep alive every interval.")]
        public System.TimeSpan KeepaliveInterval { get; set; }

        [Option("keepalive-duration", HelpText = "(Default: 1 hour) After sound is detected, keep alive for this long.")]
        public System.TimeSpan KeepaliveDuration { get; set; }

        [Option("event-uri", HelpText = "Call this URI when events occur (sound detected, keepalive, keepalive expiry, standby/shutdown).")]
        public string Uri { get; set; }

        [Option("sounding-payload", Default = "ON", HelpText = "Send this data via a POST request to event-uri when sounding (including keepalives).")]
        public string SoundingPayload { get; set; }

        [Option("stopped-sounding-payload", Default = "OFF", HelpText = "Send this data via a POST request to event-uri when sounding has stopped (keepalive expiry or standby/shutdown).")]
        public string StoppedSoundingPayload { get; set; }

        [Option("payload-content-type", Default = "text/plain", HelpText = "Content-Type of the payloads to be sent.")]
        public string PayloadContentType { get; set; }

        [Option("http-username", HelpText = "Provide this username in HTTP requests.")]
        public string HttpUsername { get; set; }

        [Option("http-password-file", HelpText = "Provide the password contained in this file in HTTP requests.")]
        public string HttpPasswordFile { get; set; }
        public string HttpPassword { get; private set; }

        [Option("service", Hidden = true)]
        public bool Service { get; set; }

        public class UsageException : System.Exception
        {
            public UsageException(string message) : base(message) { }
        }

        public static Options Parse(System.Collections.Generic.IEnumerable<string> args, bool ignoreUnknownArguments = false)
        {
            var helpWriter = new System.IO.StringWriter();
            var options = new Parser(config => { config.HelpWriter = helpWriter; config.IgnoreUnknownArguments = ignoreUnknownArguments; })
                .ParseArguments<Options>(args).MapResult(
                options => options,
                _ => throw new UsageException(helpWriter.ToString()));
            if (options.HttpPasswordFile != null)
            {
                try
                {
                    options.HttpPassword = System.IO.File.ReadAllText(options.HttpPasswordFile);
                }
                catch (System.Exception exception)
                {
                    throw new System.Exception("Unable to read HTTP password file (" + options.HttpPasswordFile + ")", exception);
                }
            }
            return options;
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
                var audioMeterEvent = CreateAudioMeterEvent(options, new ConsoleLogger());
                audioMeterEvent.Start();

                Microsoft.Win32.SystemEvents.PowerModeChanged += (object sender, Microsoft.Win32.PowerModeChangedEventArgs eventArgs) => {
                    if (eventArgs.Mode == Microsoft.Win32.PowerModes.Suspend) audioMeterEvent.Stop();
                    if (eventArgs.Mode == Microsoft.Win32.PowerModes.Resume) audioMeterEvent.Start();
                };

                var cancelKeyPressed = new System.Threading.ManualResetEventSlim();
                System.Console.CancelKeyPress += (object sender, System.ConsoleCancelEventArgs eventArgs) => {
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
                AudioMeterEvent = CreateAudioMeterEvent(Options, new EventLogLogger(EventLog));
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

        static AudioMeterEvent CreateAudioMeterEvent(Options options, Logger logger)
        {
            var audioMeterEvent = new AudioMeterEvent(
                options.AudioDeviceId,
                new SignalRatio { FieldDecibels = options.MinimumLevelDecibels },
                options.MinimumDuration,
                options.Period,
                options.KeepaliveInterval,
                options.KeepaliveDuration,
                logger);

            if (options.Uri != null)
            {
                var httpClient = new HttpClient(options.PayloadContentType, options.HttpUsername, options.HttpPassword, logger);
                audioMeterEvent.Sounding += (object sender, System.EventArgs eventArgs) => { httpClient.SendHttpRequest(options.Uri, options.SoundingPayload, logger); };
                audioMeterEvent.StoppedSounding += (object sender, System.EventArgs eventArgs) => { httpClient.SendHttpRequest(options.Uri, options.StoppedSoundingPayload, logger); };
            }

            return audioMeterEvent;
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
            // We could almost use RESTRICTED, but not quite - sadly, that seems to break HTTP requests with a weird "No credentials are available in the security package" error from System.Net.SSPIWrapper.AcquireCredentialsHandle().
            service.SetSidType(ServiceManager.ServiceSidType.SERVICE_SID_TYPE_UNRESTRICTED);
            service.SetRequiredPrivileges(new string[] { "SeChangeNotifyPrivilege" });
        }

        static string QuoteCommandLineArgument(string arg)
        {
            // https://stackoverflow.com/a/6040946/172594
            return "\"" + System.Text.RegularExpressions.Regex.Replace(arg, @"(\\+)$", @"$1$1") + "\"";
        }
    }
}