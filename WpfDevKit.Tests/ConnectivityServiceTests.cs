using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.Connectivity;

namespace WpfDevKit.Tests.Connectivity
{
    [TestClass]
    public class ConnectivityServiceTests
    {
        private ConnectivityService _service;

        [TestInitialize]
        public void Init()
        {
            var options = new ConnectivityServiceOptions
            {
                Host = "localhost",
                ConnectingStatusMessage = "connecting...",
                ConnectedStatusMessage = "connected",
                IsReadyAsync = async (ct) => await Task.FromResult(true)
            };

            _service = new ConnectivityService(options);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_SetsInitialState()
        {
            await _service.StartAsync(CancellationToken.None);
            Assert.IsTrue(_service.IsConnected);
            Assert.AreEqual("connected", _service.Status);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_ChangeEvent_Fires()
        {
            bool raised = false;
            _service.ConnectionChanged += () => raised = true;
            await _service.StartAsync(CancellationToken.None);
            Assert.IsTrue(raised);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_Failure_RetriesUntilSuccess()
        {
            int attempts = 0;
            var options = new ConnectivityServiceOptions
            {
                Host = "retryhost",
                ConnectingStatusMessage = "connecting...",
                ConnectedStatusMessage = "connected",
                IsReadyAsync = async (ct) => await Task.FromResult(++attempts >= 3)
            };

            var retryingService = new ConnectivityService(options);
            await retryingService.StartAsync(CancellationToken.None);
            Assert.IsTrue(retryingService.IsConnected);
            Assert.IsTrue(attempts >= 3);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_CancelStopsMonitoring()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await _service.StartAsync(cts.Token);
            Assert.IsFalse(_service.IsConnected); // Likely short-circuited
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_Failure_ThrowsAndContinues()
        {
            int calls = 0;
            var options = new ConnectivityServiceOptions
            {
                Host = "failhost",
                ConnectingStatusMessage = "connecting...",
                ConnectedStatusMessage = "connected",
                IsReadyAsync = async (ct) =>
                {
                    calls++;
                    if (calls == 1)
                        throw new InvalidOperationException("Simulated failure");
                    return true;
                }
            };

            var service = new ConnectivityService(options);
            await service.StartAsync(CancellationToken.None);

            Assert.IsTrue(service.IsConnected);
            Assert.IsTrue(calls >= 2);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_AlwaysFalseReadiness_NeverConnects()
        {
            var options = new ConnectivityServiceOptions
            {
                Host = "neverhost",
                ConnectingStatusMessage = "connecting...",
                ConnectedStatusMessage = "connected",
                IsReadyAsync = async (ct) => await Task.FromResult(false)
            };

            var service = new ConnectivityService(options);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await service.StartAsync(cts.Token);

            Assert.IsFalse(service.IsConnected);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_TimeoutOnIsReadyAsync_DoesNotCrash()
        {
            var options = new ConnectivityServiceOptions
            {
                Host = "timeouthost",
                ConnectingStatusMessage = "connecting...",
                ConnectedStatusMessage = "connected",
                IsReadyAsync = async (ct) =>
                {
                    await Task.Delay(5000, ct); // Simulates long delay
                    return true;
                }
            };

            var service = new ConnectivityService(options);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await service.StartAsync(cts.Token);

            Assert.IsFalse(service.IsConnected);
        }
    }
}
