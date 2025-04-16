using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.Hosting;
using WpfDevKit.Logging;

namespace WpfDevKit.Tests.Hosting
{
    [TestClass]
    [TestCategory("TimedHostedService")]
    public class TimedHostedServiceTests
    {
        private sealed class TestService : IntervalService
        {
            private readonly Func<CancellationToken, Task<bool>> _work;
            public List<DateTime> ExecutionTimestamps { get; } = new List<DateTime>();

            public TestService(Func<CancellationToken, Task<bool>> work, ILogService log) : base(log)
            {
                _work = work;
                ExecutionIntervalMilliseconds = 100;
                MinimumRetryMilliseconds = 50;
                MaximumRetryMilliseconds = 1000;
            }

            protected override async Task<bool> DoWorkWithResultAsync(CancellationToken token)
            {
                ExecutionTimestamps.Add(DateTime.UtcNow);
                return await _work(token);
            }

            public Task StartAsync(CancellationToken token) => base.StartAsync(token);
        }

        private class NullLogger : ILogService
        {
            ILogMetrics ILogService.Metrics => throw new NotImplementedException();
            void ILogService.Log(TLogCategory category, Exception exception, Type type, string fileName, string memberName) => throw new NotImplementedException();
            void ILogService.Log(TLogCategory category, string message, string attributes, Type type, string fileName, string memberName) => throw new NotImplementedException();
        }

        [TestMethod]
        public async Task ExecutesOnInterval_CallsDoWork()
        {
            int calls = 0;
            var service = new TestService(async _ => { calls++; return true; }, new NullLogger());

            using (var cts = new CancellationTokenSource(500))
                await service.StartAsync(cts.Token);

            Assert.IsTrue(calls > 2);
        }

        [TestMethod]
        public async Task FailsTriggersRetry_WithBackoff()
        {
            var timestamps = new List<DateTime>();
            int calls = 0;

            var service = new TestService(async _ =>
            {
                timestamps.Add(DateTime.UtcNow);
                calls++;
                return false; // always fail
            }, new NullLogger());

            using (var cts = new CancellationTokenSource(1500))
                await service.StartAsync(cts.Token);

            // Validate retry timestamps grow
            var deltas = new List<TimeSpan>();
            for (int i = 1; i < timestamps.Count; i++)
                deltas.Add(timestamps[i] - timestamps[i - 1]);

            for (int i = 1; i < deltas.Count; i++)
                Assert.IsTrue(deltas[i] >= deltas[i - 1], "Backoff delay did not increase as expected.");
        }

        [TestMethod]
        public async Task TriggerImmediateExecution_WakesEarly()
        {
            int calls = 0;
            var service = new TestService(async _ => { calls++; return true; }, new NullLogger());

            using (var cts = new CancellationTokenSource(1000))
                await service.StartAsync(cts.Token);

            await Task.Delay(100);
            var countBefore = calls;
            service.TriggerImmediateExecution();

            await Task.Delay(100);
            Assert.IsTrue(calls > countBefore);
        }

        [TestMethod]
        public async Task CancelsProperly_ExitsLoop()
        {
            var service = new TestService(async _ => true, new NullLogger());

            using (var cts = new CancellationTokenSource(250))
                await service.StartAsync(cts.Token);

            await Task.Delay(300); // Ensure cleanup completes
            Assert.IsTrue(service.ExecutionTimestamps.Count > 0);
        }
    }
}
