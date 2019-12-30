[assembly: System.Reflection.AssemblyTitle("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyDescription("Takes action in response to changes in audio level")]
[assembly: System.Reflection.AssemblyProduct("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyCompany("Etienne Dechamps")]
[assembly: System.Reflection.AssemblyCopyright("Etienne Dechamps <etienne@edechamps.fr>")]

namespace AudioMeterEvent
{
    static class AudioMeterEventEntryPoint
    {
        static int Main(string[] args)
        {
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
    }
}