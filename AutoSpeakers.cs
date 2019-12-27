namespace AutoSpeakers
{
    class AutoSpeakers
    {
        static void Main()
        {
            var enumerator = new MMDeviceAPILib.MMDeviceEnumerator();
            MMDeviceAPILib.IMMDeviceCollection deviceCollection;
            enumerator.EnumAudioEndpoints(MMDeviceAPILib.EDataFlow.eRender, MMDeviceAPIHelpers.DEVICE_STATE_ACTIVE, out deviceCollection);
            uint deviceCount;
            deviceCollection.GetCount(out deviceCount);
            System.Console.WriteLine("Device count: " + deviceCount);
            for (uint deviceIndex = 0; deviceIndex < deviceCount; ++deviceIndex)
            {
                MMDeviceAPILib.IMMDevice device;
                deviceCollection.Item(deviceIndex, out device);
                MMDeviceAPILib.IPropertyStore propertyStore;
                device.OpenPropertyStore(MMDeviceAPIHelpers.STGM_READ, out propertyStore);

                MMDeviceAPILib.tag_inner_PROPVARIANT propvariant;
                propertyStore.GetValue(MMDeviceAPIHelpers.PKEY_Device_FriendlyName, out propvariant);
                System.Console.WriteLine(MMDeviceAPIHelpers.GetObjectForPropvariant(propvariant).ToString());
            }
            System.Console.ReadLine();
        }
    }
}