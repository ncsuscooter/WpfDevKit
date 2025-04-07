using System.Runtime.InteropServices;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Defines native methods for accessing remote share connections.
    /// </summary>
    internal static class Imports
    {
        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        internal static extern uint NetUseAdd(string UncServerName,
                                              uint Level,
                                              ref USE_INFO_2 Buf,
                                              out uint ParmError);

        [DllImport("netapi32", CharSet = CharSet.Unicode)]
        internal static extern uint NetUseDel([MarshalAs(UnmanagedType.LPWStr)] string uncServerName,
                                              [MarshalAs(UnmanagedType.LPWStr)] string useName,
                                              TForceCond forceCond);
    }
}
