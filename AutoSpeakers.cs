namespace AutoSpeakers
{
    static class AutoSpeakers
    {
        static void Main()
        {
            var enumerator = new MMDeviceAPI.MMDeviceEnumerator();
            MMDeviceAPI.IMMDeviceCollection deviceCollection;
            enumerator.EnumAudioEndpoints(MMDeviceAPI.EDataFlow.eRender, MMDeviceAPIHelpers.DEVICE_STATE_ACTIVE, out deviceCollection);
            uint deviceCount;
            deviceCollection.GetCount(out deviceCount);
            System.Console.WriteLine("Device count: " + deviceCount);
            for (uint deviceIndex = 0; deviceIndex < deviceCount; ++deviceIndex)
            {
                MMDeviceAPI.IMMDevice device;
                deviceCollection.Item(deviceIndex, out device);
                MMDeviceAPI.IPropertyStore propertyStore;
                device.OpenPropertyStore(MMDeviceAPIHelpers.STGM_READ, out propertyStore);

                MMDeviceAPI.tag_inner_PROPVARIANT propvariant;
                propertyStore.GetValue(MMDeviceAPIHelpers.PKEY_Device_FriendlyName, out propvariant);
                System.Console.WriteLine(MMDeviceAPIHelpers.GetObjectForPropvariant(propvariant).ToString());
            }
            System.Console.ReadLine();
        }
    }
}