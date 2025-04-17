using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Hosting;

namespace WpfDevKit.Tests.Hosting
{
    [TestClass]
    public class HostTests
    {
        private class TestService : HostedService
        {
            public bool Started { get; private set; }
            public bool Stopped { get; private set; }

            protected override async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Debug.WriteLine($"[HOST TESTS] HostedService '{GetType().FullName}' executing.");
                Started = true;
                while (!cancellationToken.IsCancellationRequested)
                    await Task.Delay(10, cancellationToken);
            }

            public override Task StopAsync(CancellationToken cancellationToken)
            {
                Stopped = true;
                return base.StopAsync(cancellationToken);
            }
        }

        private class FailingService : HostedService
        {
            protected override Task ExecuteAsync(CancellationToken _) => throw new InvalidOperationException("Boom!");
        }

        private class SlowStoppingService : HostedService
        {
            public bool WasCancelled { get; private set; }

            protected override async Task ExecuteAsync(CancellationToken cancellationToken)
            {
                Debug.WriteLine($"[HOST TESTS] HostedService '{GetType().FullName}' executing.");
                while (!cancellationToken.IsCancellationRequested)
                    await Task.Delay(50, cancellationToken);
            }

            public override async Task StopAsync(CancellationToken cancellationToken)
            {
                try
                {
                    // Simulate long-running stop
                    await Task.Delay(5000, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    WasCancelled = true;
                }
            }
        }

        [TestMethod]
        public async Task HostedService_StartsAndStopsCorrectly()
        {
            var testService = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(_ => testService);
            var host = new Host(services.Build());

            await host.StartAsync();
            Assert.IsTrue(testService.Started, "Service should be marked as started.");

            await host.StopAsync();
            Assert.IsTrue(testService.Stopped, "Service should be marked as stopped.");
        }

        [TestMethod]
        public void Host_Dispose_CleansUpServices()
        {
            var testService = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(_ => testService);
            var host = new Host(services.Build());

            host.Start();
            host.Dispose();
            Assert.IsTrue(testService.Stopped, "Dispose should stop hosted services.");
        }

        [TestMethod]
        public void HostBuilder_BuildsHostWithServices()
        {
            var builder = HostBuilder.CreateHostBuilder();
            builder.Services.AddSingleton<IHostedService, TestService>();
            var host = builder.Build();

            Assert.IsNotNull(host);
            Assert.IsNotNull(host.Services.GetService(typeof(IHostedService)));
        }

        [TestMethod]
        public async Task Host_StartsAndStops_MultipleServices()
        {
            var svc1 = new TestService();
            var svc2 = new TestService();
            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(_ => svc1);
            services.AddSingleton<IHostedService>(_ => svc2);

            var host = new Host(services.Build());
            
            await host.StartAsync();
            await Task.Delay(150);
            Console.WriteLine($"svc1.Started = {svc1.Started} - Expected: true");
            Console.WriteLine($"svc2.Started = {svc2.Started} - Expected: true");

            await host.StopAsync();
            await Task.Delay(150);
            Console.WriteLine($"svc1.Stopped = {svc1.Stopped} - Expected: true");
            Console.WriteLine($"svc2.Stopped = {svc2.Stopped} - Expected: true");

            Assert.IsTrue(svc1.Started && svc1.Stopped);
            Assert.IsTrue(svc2.Started && svc2.Stopped);
        }

        [TestMethod]
        public async Task Host_StartAsync_HandlesServiceFailures()
        {
            var good = new TestService();
            var bad = new FailingService();

            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(_ => good);
            services.AddSingleton<IHostedService>(_ => bad);
            services.AddSingleton<InternalLogger>(_ => new InternalLogger()); // Optional stub

            var host = new Host(services.Build());

            await host.StartAsync();
            await Task.Delay(100); // Allow time for services to start

            Console.WriteLine($"good.Started = {good.Started} - Expected: true");

            Assert.IsTrue(good.Started); // Confirm others still ran
        }


        [TestMethod]
        public async Task Host_PublicApi_StartStop()
        {
            var builder = HostBuilder.CreateHostBuilder();
            builder.Services.AddSingleton<IHostedService, TestService>();

            IHost host = builder.Build();
            await host.StartAsync();
            await host.StopAsync();
        }

        [TestMethod]
        public void Host_DisposeTwice_IsIdempotent()
        {
            var services = new ServiceCollection();
            services.AddSingleton<IHostedService, TestService>();
            var host = new Host(services.Build());

            host.Dispose();
            host.Dispose(); // Should not throw
        }

        [TestMethod]
        public async Task StopAsync_Timeout_CancelsGracefully()
        {
            var service = new SlowStoppingService();
            var services = new ServiceCollection();
            services.AddSingleton<IHostedService>(_ => service);
            var host = new Host(services.Build());

            await host.StartAsync();

            using (var cts = new CancellationTokenSource(100)) // short timeout
            {
                await host.StopAsync(cts.Token);
            }

            Assert.IsTrue(service.WasCancelled, "Service should have been cancelled due to timeout.");
        }
    }
}
