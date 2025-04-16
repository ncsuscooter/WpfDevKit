using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.Connectivity;

namespace WpfDevKit.Tests.Connectivity
{
    [TestClass]
    public class ConnectivityServiceTests
    {
        private IConnectivityService connectivityService;

        [TestInitialize]
        public void Init()
        {
            var options = new ConnectivityServiceOptions
            {
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(50);
                    return await Task.FromResult(true);
                }
            };
            connectivityService = new ConnectivityService(options);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_SetsInitialState()
        {
            await connectivityService.StartAsync(CancellationToken.None);
            await Task.Delay(100);
            Assert.IsTrue(connectivityService.IsConnected);
            Assert.AreEqual("connected", connectivityService.Status);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_ChangeEvent_Fires()
        {
            bool raised = false;
            connectivityService.StatusChanged += () => raised = true;
            await connectivityService.StartAsync(CancellationToken.None);
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
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(50, ct);
                    return await Task.FromResult(++attempts >= 3);
                }
            };
            var service = new ConnectivityService(options);
            await service.StartAsync(CancellationToken.None);
            var sw = Stopwatch.StartNew();
            while (!service.IsConnected && sw.Elapsed < TimeSpan.FromSeconds(5))
                await Task.Delay(50);

            Assert.IsTrue(service.IsConnected, $"Expected connection after retries (attempts: {attempts})");
            Assert.IsTrue(attempts >= 3);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_CancelStopsMonitoring()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel();

            await connectivityService.StartAsync(cts.Token);
            Assert.IsFalse(connectivityService.IsConnected); // Likely short-circuited
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_Failure_ThrowsAndContinues()
        {
            int calls = 0;
            var options = new ConnectivityServiceOptions
            {
                Host = "failhost",
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(50);
                    calls++;
                    if (calls == 1)
                        throw new InvalidOperationException("Simulated failure");
                    return await Task.FromResult(true);
                }
            };
            var service = new ConnectivityService(options);
            await service.StartAsync(CancellationToken.None);
            var sw = Stopwatch.StartNew();
            while (!service.IsConnected && sw.Elapsed < TimeSpan.FromSeconds(5))
                await Task.Delay(50);
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
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(50);
                    return await Task.FromResult(false);
                }
            };

            var service = new ConnectivityService(options);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await service.StartAsync(cts.Token);
            var sw = Stopwatch.StartNew();
            while (!service.IsConnected && sw.Elapsed < TimeSpan.FromSeconds(5))
                await Task.Delay(50);
            Assert.IsFalse(service.IsConnected);
        }

        [TestMethod]
        [TestCategory("ConnectivityService")]
        public async Task ConnectivityService_TimeoutOnIsReadyAsync_DoesNotCrash()
        {
            var options = new ConnectivityServiceOptions
            {
                Host = "timeouthost",
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(5000, ct); // Simulates long delay
                    return true;
                }
            };

            var service = new ConnectivityService(options);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
            await service.StartAsync(cts.Token);
            var sw = Stopwatch.StartNew();
            while (!service.IsConnected && sw.Elapsed < TimeSpan.FromSeconds(5))
                await Task.Delay(50);
            Assert.IsFalse(service.IsConnected);
        }
    }
}
