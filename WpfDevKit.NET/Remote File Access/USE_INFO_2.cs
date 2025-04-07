using System.Runtime.InteropServices;

namespace WpfDevKit.RemoteFileAccess
{
    /// <summary>
    /// Represents the input structure for a remote file share connection.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct USE_INFO_2
    {
        public string ui2_local;
        public string ui2_remote;
        public string ui2_password;
        public uint ui2_status;
        public uint ui2_asg_type;
        public uint ui2_refcount;
        public uint ui2_usecount;
        public string ui2_username;
        public string ui2_domainname;
    }
}
