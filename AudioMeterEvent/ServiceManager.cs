namespace AudioMeterEvent
{
    // https://blogs.msdn.microsoft.com/anlynes/2006/07/30/using-net-code-to-set-a-windows-service-to-automatically-restart-on-failure/

    sealed class ServiceManager : System.IDisposable
    {
        public ServiceManager()
        {
            SCManagerHandle = OpenSCManager(null, null, ServiceControlAccessRights.SC_MANAGER_CONNECT);
            if (SCManagerHandle == System.IntPtr.Zero)
                throw new System.ComponentModel.Win32Exception("Unable to open Service Control Manager");
        }

        System.IntPtr SCManagerHandle = System.IntPtr.Zero;

        public sealed class Service : System.IDisposable
        {
            public Service(ServiceManager serviceManager, string serviceName)
            {
                ServiceHandle = OpenService(serviceManager.SCManagerHandle, serviceName, ServiceAccessRights.SERVICE_CHANGE_CONFIG);
                if (ServiceHandle == System.IntPtr.Zero)
                    throw new System.ComponentModel.Win32Exception("Unable to open Service: " + serviceName);
            }

            public void Dispose()
            {
                if (ServiceHandle != System.IntPtr.Zero && CloseServiceHandle(ServiceHandle) == 0)
                    throw new System.ComponentModel.Win32Exception("Unable to close Service");
                ServiceHandle = System.IntPtr.Zero;
            }

            ~Service()
            {
                Dispose();
            }

            System.IntPtr ServiceHandle;

            public void SetSidType(ServiceSidType sidType)
            {
                var serviceSidInfo = new SERVICE_SID_INFO { ServiceSidType = sidType };
                if (ChangeServiceConfig2WithServiceSidInfo(ServiceHandle, ServiceConfig2InfoLevel.SERVICE_CONFIG_SERVICE_SID_INFO, ref serviceSidInfo) == 0)
                    throw new System.ComponentModel.Win32Exception("Unable to set service SID type");
            }

            public void SetRequiredPrivileges(System.Collections.Generic.IEnumerable<string> privileges)
            {
                var requiredPrivileges = new System.Text.StringBuilder();
                foreach (var privilege in System.Linq.Enumerable.Append(privileges, ""))
                {
                    requiredPrivileges.Append(privilege);
                    requiredPrivileges.Append('\0');
                }
                var requiredPrivilegesInfo = new SERVICE_REQUIRED_PRIVILEGES_INFO { RequiredPrivileges = System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(requiredPrivileges.ToString()) };
                try
                {
                    if (ChangeServiceConfig2WithRequiredPrivilegesInfo(ServiceHandle, ServiceConfig2InfoLevel.SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO, ref requiredPrivilegesInfo) == 0)
                        throw new System.ComponentModel.Win32Exception("Unable to set service required privileges");
                }
                finally
                {
                    System.Runtime.InteropServices.Marshal.FreeHGlobal(requiredPrivilegesInfo.RequiredPrivileges);
                }
            }
        }

        public void Dispose()
        {
            if (SCManagerHandle != System.IntPtr.Zero && CloseServiceHandle(SCManagerHandle) == 0)
                throw new System.ComponentModel.Win32Exception("Unable to close Service Control Manager");
            SCManagerHandle = System.IntPtr.Zero;
        }

        ~ServiceManager()
        {
            Dispose();
        }

        [System.Flags]
        enum ServiceControlAccessRights : int
        {
            SC_MANAGER_CONNECT = 1,
        }

        [System.Flags]
        enum ServiceAccessRights : int
        {
            SERVICE_CHANGE_CONFIG = 2,
        }

        enum ServiceConfig2InfoLevel : int
        {
            SERVICE_CONFIG_SERVICE_SID_INFO = 5,
            SERVICE_CONFIG_REQUIRED_PRIVILEGES_INFO = 6,
        }

        public enum ServiceSidType : uint
        {
            SERVICE_SID_TYPE_NONE = 0,
            SERVICE_SID_TYPE_RESTRICTED = 3,
            SERVICE_SID_TYPE_UNRESTRICTED = 1,
        }

        struct SERVICE_SID_INFO
        {
            public ServiceSidType ServiceSidType;
        }

        struct SERVICE_REQUIRED_PRIVILEGES_INFO
        {
            public System.IntPtr RequiredPrivileges;
        }

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        static extern System.IntPtr OpenSCManager(
            string machineName,
            string databaseName,
            ServiceControlAccessRights desiredAccess);

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        static extern int CloseServiceHandle(System.IntPtr hSCObject);

        [System.Runtime.InteropServices.DllImport("advapi32.dll", SetLastError = true)]
        static extern System.IntPtr OpenService(
            System.IntPtr hSCManager,
            string serviceName,
            ServiceAccessRights desiredAccess);

        [System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2", SetLastError = true)]
        static extern int ChangeServiceConfig2WithServiceSidInfo(
            System.IntPtr hService,
            ServiceConfig2InfoLevel dwInfoLevel,
            ref SERVICE_SID_INFO lpInfo);
        [System.Runtime.InteropServices.DllImport("advapi32.dll", EntryPoint = "ChangeServiceConfig2", SetLastError = true)]
        static extern int ChangeServiceConfig2WithRequiredPrivilegesInfo(
            System.IntPtr hService,
            ServiceConfig2InfoLevel dwInfoLevel,
            ref SERVICE_REQUIRED_PRIVILEGES_INFO lpInfo);
    }
}