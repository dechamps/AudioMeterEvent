﻿namespace AutoSpeakers
{
    static class MMDeviceAPIHelpers
    {
        public const uint DEVICE_STATE_ACTIVE = 1;
        public const uint STGM_READ = 0;

        // From functiondiscoverykeys_devpkey.h
        public static readonly MMDeviceAPI._tagpropertykey PKEY_Device_FriendlyName = new MMDeviceAPI._tagpropertykey { fmtid = new System.Guid(0xa45c254e, 0xdf1c, 0x4efd, 0x80, 0x20, 0x67, 0xd1, 0x46, 0xa8, 0x50, 0xe0), pid = 14 };

        public static object ToObject(this MMDeviceAPI.tag_inner_PROPVARIANT propvariant)
        {
            var marshalledPropvariant = propvariant.Marshall();
            object variant;
            PropVariantToVariant(ref marshalledPropvariant, out variant);
            PropVariantClear(ref marshalledPropvariant);
            return variant;
        }

        static Propvariant Marshall(this MMDeviceAPI.tag_inner_PROPVARIANT propvariant)
        {
            // Note: it seems unlikely this will work with complicated/exotic PROPVARIANT types.
            var marshalledPropvariant = new Propvariant();
            marshalledPropvariant.vt = propvariant.vt;
            // TODO: it's not clear if this autogenerated field name is stable.
            marshalledPropvariant.p = propvariant.__MIDL____MIDL_itf_mmdeviceapi_0003_00930001.pcVal;
            return marshalledPropvariant;
        }

        [System.Runtime.InteropServices.DllImport(@"propsys.dll", PreserveSig = false)]
        static extern void PropVariantToVariant(ref Propvariant pPropVar, [System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Struct)] out object pVar);

        [System.Runtime.InteropServices.DllImport(@"ole32.dll", PreserveSig = false)]
        static extern void PropVariantClear(ref Propvariant pvar);

        // Unmanaged memory layout for a PROPVARIANT.
        // From https://blogs.msdn.microsoft.com/adamroot/2008/04/11/interop-with-propvariants-in-net/
        // Note that MMDeviceAPI.tag_inner_PROPVARIANT has a completely different memory layout because it is the result of COM interop unmarshalling.
        [System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
        struct Propvariant
        {
            public ushort vt;
            ushort wReserved1;
            ushort wReserved2;
            ushort wReserved3;
            public System.IntPtr p;
            int p2;
        }
    }
}