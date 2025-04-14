using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.Busy;

namespace WpfDevKit.Tests.Busy
{
    [TestClass]
    public class BusyServiceTests
    {
        private IBusyService _service;

        [TestInitialize]
        public void Setup()
        {
            _service = new BusyService();
        }

        [TestMethod]
        [TestCategory("BusyService")]
        public void BusyService_StartStop_ChangesState()
        {
            Assert.IsFalse(_service.IsBusy);

            using (_service.Busy())
            {
                Assert.IsTrue(_service.IsBusy);
            }

            Assert.IsFalse(_service.IsBusy);
        }

        [TestMethod]
        [TestCategory("BusyService")]
        public void BusyService_NestedBusy_IncrementsProperly()
        {
            Assert.IsFalse(_service.IsBusy);

            using (_service.Busy())
            {
                Assert.IsTrue(_service.IsBusy);

                using (_service.Busy())
                {
                    Assert.IsTrue(_service.IsBusy);
                }

                Assert.IsTrue(_service.IsBusy);
            }

            Assert.IsFalse(_service.IsBusy);
        }

        [TestMethod]
        [TestCategory("BusyService")]
        public void BusyService_Event_IsRaised()
        {
            int eventCount = 0;
            _service.IsBusyChanged += () => eventCount++;

            using (_service.Busy())
            {
                Assert.AreEqual(1, eventCount);
            }

            Assert.AreEqual(2, eventCount);
        }

        [TestMethod]
        [TestCategory("BusyService")]
        public void BusyService_ConcurrentAccess_ThreadSafe()
        {
            Parallel.For(0, 100, i =>
            {
                using (_service.Busy())
                {
                    Assert.IsTrue(_service.IsBusy);
                }
            });

            Assert.IsFalse(_service.IsBusy);
        }

        [TestMethod]
        [TestCategory("BusyService")]
        public async Task BusyService_Event_RespectsUIContext()
        {
            SynchronizationContext.SetSynchronizationContext(new TestSyncContext());
            var tcs = new TaskCompletionSource<bool>();

            _service.IsBusyChanged += () =>
            {
                if (_service.IsBusy)
                    tcs.TrySetResult(true);
            };

            using (_service.Busy())
            {
                await Task.WhenAny(tcs.Task, Task.Delay(1000));
            }

            Assert.IsTrue(tcs.Task.IsCompleted);
            Assert.IsTrue(tcs.Task.Result);
        }

        private class TestSyncContext : SynchronizationContext
        {
            public override void Post(SendOrPostCallback d, object state)
            {
                Task.Run(() => d(state));
            }
        }
    }
}
