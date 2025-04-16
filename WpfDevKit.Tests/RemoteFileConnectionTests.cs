using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.RemoteFileAccess;

namespace WpfDevKit.Tests.RemoteFileAccess
{
    public class MockRemoteFileConnection : IRemoteFileConnection
    {
        public string Path { get; private set; }
        public bool IsConnected { get; private set; }
        public MockRemoteFileConnection(string path) => (Path, IsConnected) = (path, true);
        public void Dispose() => IsConnected = false;
    }

    public class MockRemoteFileConnectionFactory : IRemoteFileConnectionFactory
    {
        public IRemoteFileConnection LastConnection { get; private set; }
        public IRemoteFileConnection Create(string path, string username, string password, string domain = null)
        {
            LastConnection = new MockRemoteFileConnection(path);
            return LastConnection;
        }
    }

    [TestClass]
    public class RemoteFileConnectionTests
    {
        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void OpenRemoteConnectionException_ContainsDetails()
        {
            var ex = new OpenRemoteConnectionException("//server/share", "user1", 1326, 42);
            Assert.AreEqual("//server/share", ex.Data["Path"]);
            Assert.AreEqual("user1", ex.Data["User"]);
            Assert.AreEqual((uint)1326, ex.Data["Error"]);
            Assert.AreEqual((uint)42, ex.Data["Index"]);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void CloseRemoteConnectionException_ContainsDetails()
        {
            var ex = new CloseRemoteConnectionException("//closed/share", 67);
            Assert.AreEqual("//closed/share", ex.Data["Path"]);
            Assert.AreEqual((uint)67, ex.Data["Error"]);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void MockRemoteFileConnection_DisposeMultipleTimes_DoesNotThrow()
        {
            var conn = new MockRemoteFileConnection("//mock/share");
            conn.Dispose();
            conn.Dispose();
            Assert.IsFalse(conn.IsConnected);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void MockRemoteFileConnection_HandlesEmptyPath()
        {
            var conn = new MockRemoteFileConnection("");
            Assert.AreEqual("", conn.Path);
            Assert.IsTrue(conn.IsConnected);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void MockRemoteFileConnection_TracksStateCorrectly()
        {
            var factory = new MockRemoteFileConnectionFactory();
            using (var conn = factory.Create("//mock/share", "user", "pass"))
            {
                Assert.IsTrue(conn.IsConnected);
                Assert.AreEqual("//mock/share", conn.Path);
            }
            Assert.IsFalse(factory.LastConnection.IsConnected);
        }
    }
}
