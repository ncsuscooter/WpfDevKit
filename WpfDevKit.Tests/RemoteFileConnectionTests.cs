using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.RemoteFileAccess;

namespace WpfDevKit.Tests.Remote
{
    internal class MockRemoteFileConnection : RemoteFileConnection, IDisposable
    {
        public MockRemoteFileConnection(string path) : base(path, "", "") { }
    }

    internal class MockRemoteFileConnectionFactory : IRemoteFileConnectionFactory
    {
        public IRemoteFileConnection LastConnection { get; private set; }

        public IRemoteFileConnection Create(string path, string user, string password, string domain = default)
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
        public void RemoteFileConnection_Throws_WhenInvalidPath()
        {
            var factory = new RemoteFileConnectionFactory();
            Assert.ThrowsException<OpenRemoteConnectionException>(() =>
            {
                using (var connection = factory.Create("\\\\invalid-server\\share", "user", "pass"))
                {
                }
            });
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void RemoteFileConnection_HandlesCloseFailureGracefully()
        {
            var ex = new CloseRemoteConnectionException("//somepath", "user", 123);
            Assert.AreEqual("//somepath", ex.Path);
            Assert.AreEqual("user", ex.User);
            Assert.AreEqual(123, ex.Win32ErrorCode);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void RemoteFileConnectionFactory_CanBeMockedOrConstructed()
        {
            var factory = new RemoteFileConnectionFactory();
            Assert.IsNotNull(factory);
        }

        [TestMethod]
        [TestCategory("RemoteFileConnection")]
        public void MockRemoteFileConnection_WorksAsExpected()
        {
            var factory = new MockRemoteFileConnectionFactory();
            using (var connection = factory.Create("//mock", "user", "pass"))
            {
                Assert.IsTrue(connection.IsConnected);
                Assert.AreEqual("//mock", connection.Path);
            }

            Assert.IsFalse(factory.LastConnection.IsConnected);
        }
    }
}
