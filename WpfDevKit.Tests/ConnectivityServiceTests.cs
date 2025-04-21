using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.Connectivity;
using WpfDevKit.DependencyInjection;

namespace WpfDevKit.Tests.Connectivity
{
    [TestClass]
    public class ConnectivityServiceTests
    {
        private class TestOptions : ConnectivityServiceOptions
        {
            private int _attempts = 0;
            private readonly Func<int, bool> _condition;

            public TestOptions(Func<int, bool> connectionCondition)
            {
                _condition = connectionCondition;
                ValidateConnectionAsync = async (ct) =>
                {
                    await Task.Delay(10, ct);
                    _attempts++;
                    return _condition(_attempts);
                };

                GetStatus = _ => "TestStatus";
                MinimumRetryMilliseconds = 10;
                MaximumRetryMilliseconds = 500;
                ExecutionIntervalMilliseconds = 500;
            }
        }

        [TestMethod]
        public async Task ConnectivityService_SuccessfulConnection_ImmediatelyConnected()
        {
            var options = new Options<ConnectivityServiceOptions>(new TestOptions(attempt => true));
            var service = new ConnectivityService(options);

            using (var cts = new CancellationTokenSource(250))
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(100); // Exit BEFORE token times out
            }

            Console.WriteLine($"State = '{service.State}' - Expected: Connected");
            Assert.AreEqual(TConnectivityState.Connected, service.State);

            Console.WriteLine($"LastSuccessfulConnection.HasValue = {service.LastSuccessfulConnection.HasValue} - Expected: true");
            Assert.IsTrue(service.LastSuccessfulConnection.HasValue);

            Console.WriteLine($"Attempts = {service.Attempts} - Expected: 0");
            Assert.AreEqual(0, service.Attempts);
        }

        [TestMethod]
        public async Task ConnectivityService_AlwaysFails_RetriesIncrement()
        {
            var options = new Options<ConnectivityServiceOptions>(new TestOptions(attempt => false)
            {
                ExecutionIntervalMilliseconds = 100
            });
            var service = new ConnectivityService(options);

            using (var cts = new CancellationTokenSource(250))
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(500); // Exit AFTER token times out
            }

            Console.WriteLine($"State = '{service.State}' - Expected: Disconnected");
            Assert.AreEqual(TConnectivityState.Disconnected, service.State);

            Console.WriteLine($"Attempts = {service.Attempts} - Expected: >1");
            Assert.IsTrue(service.Attempts > 1);

            Console.WriteLine($"LastSuccessfulConnection.HasValue = {service.LastSuccessfulConnection.HasValue} - Expected: false");
            Assert.IsFalse(service.LastSuccessfulConnection.HasValue);
        }

        [TestMethod]
        public async Task ConnectivityService_ManualTrigger_OverridesDelay()
        {
            var callCount = 0;
            var options = new Options<ConnectivityServiceOptions>(new TestOptions(_ =>
            {
                callCount++;
                return false;
            }));

            var service = new ConnectivityService(options);

            using (var cts = new CancellationTokenSource(250))
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(100); // Exit BEFORE token times out

                var countBefore = callCount;
                service.TriggerImmediateExecution();
                await Task.Delay(400); // Exit AFTER token times out

                Console.WriteLine($"Call count after trigger = {callCount} - Expected: > {countBefore}");
                Assert.IsTrue(callCount > countBefore);
            }
        }

        [TestMethod]
        public async Task ConnectivityService_StatusChanged_EventFires()
        {
            var options = new Options<ConnectivityServiceOptions>(new TestOptions(attempt => attempt == 2));
            var service = new ConnectivityService(options);

            int changeCount = 0;
            service.StatusChanged += (s, e) => changeCount++;

            using (var cts = new CancellationTokenSource(250))
            {
                await service.StartAsync(cts.Token);
                await Task.Delay(500); // Exit AFTER token times out
            }

            Console.WriteLine($"StatusChanged event count = {changeCount} - Expected: >= 2");
            Assert.IsTrue(changeCount >= 2);
        }
    }
}
