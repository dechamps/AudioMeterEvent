namespace AutoSpeakers
{
    static class AutoSpeakers
    {
        static void Main()
        {
            foreach (var device in new MMDeviceAPI.MMDeviceEnumerator()
                .GetDeviceCollection(MMDeviceAPI.EDataFlow.eRender, MMDeviceAPIHelpers.DEVICE_STATE_ACTIVE)
                .GetDevices())
                System.Console.WriteLine(device
                    .GetPropertyStore(MMDeviceAPIHelpers.STGM_READ)
                    .Get(MMDeviceAPIHelpers.PKEY_Device_FriendlyName));
            System.Console.ReadLine();
        }
    }
}