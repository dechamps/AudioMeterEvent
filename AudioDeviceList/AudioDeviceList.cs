[assembly: System.Reflection.AssemblyTitle("AudioDeviceList")]
[assembly: System.Reflection.AssemblyDescription("List all active audio devices on the system along with their WASAPI IDs")]
[assembly: System.Reflection.AssemblyProduct("AudioMeterEvent")]
[assembly: System.Reflection.AssemblyCompany("Etienne Dechamps")]
[assembly: System.Reflection.AssemblyCopyright("Etienne Dechamps <etienne@edechamps.fr>")]

namespace AudioMeterEvent
{
    static class AudioDeviceList
    {
        static void Main()
        {
            System.Console.WriteLine("Render devices:");
            DisplayDevices(MMDeviceAPI.EDataFlow.eRender);
            System.Console.WriteLine("Capture devices:");
            DisplayDevices(MMDeviceAPI.EDataFlow.eCapture);
        }

        static void DisplayDevices(MMDeviceAPI.EDataFlow dataFlow)
        {
            foreach (var device in new MMDeviceAPI.MMDeviceEnumerator()
               .GetDeviceCollection(dataFlow, MMDeviceAPIHelpers.DEVICE_STATE_ACTIVE)
               .GetDevices())
                System.Console.WriteLine(device.Id() + " " + device
                    .GetPropertyStore(MMDeviceAPIHelpers.STGM_READ)
                    .Get(MMDeviceAPIHelpers.PKEY_Device_FriendlyName));
        }
    }
}