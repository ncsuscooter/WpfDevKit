using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfDevKit.Tests
{
    [TestClass]
    public class UtilityTests
    {
        public TestContext TestContext { get; set; }

        public enum TestEnum
        {
            Default,
            [System.ComponentModel.Description("DescriptiveName")] WithDescription
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_ByName_Works()
        {
            var result = "WithDescription".ToEnum<TestEnum>(returnDefault: false);
            Assert.AreEqual(TestEnum.WithDescription, result);
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_ByDescription_Works()
        {
            var result = "DescriptiveName".ToEnum<TestEnum>();
            Assert.AreEqual(TestEnum.WithDescription, result);
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_Invalid_ReturnsDefault()
        {
            var result = "InvalidValue".ToEnum<TestEnum>(returnDefault: true);
            Assert.AreEqual(default(TestEnum), result);
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_Invalid_Throws()
        {
            Assert.ThrowsException<InvalidOperationException>(() => "InvalidValue".ToEnum<TestEnum>(returnDefault: false));
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_CaseMismatch_ReturnsDefault()
        {
            var result = "withdescription".ToEnum<TestEnum>(returnDefault: true);
            Assert.AreEqual(TestEnum.WithDescription, result); // Case-insensitive match expected
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_EmptyString_ReturnsDefault()
        {
            var result = "".ToEnum<TestEnum>(returnDefault: true);
            Assert.AreEqual(default(TestEnum), result);
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_NullString_ReturnsDefault()
        {
            string input = null;
            var result = input.ToEnum<TestEnum>(returnDefault: true);
            Assert.AreEqual(default(TestEnum), result);
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_NullString_Throws()
        {
            string input = null;
            Assert.ThrowsException<ArgumentNullException>(() => input.ToEnum<TestEnum>(returnDefault: false));
        }

        [TestMethod]
        [TestCategory("EnumExtensions")]
        public void EnumExtensions_ToEnum_RoundTrip_Matches()
        {
            var value = TestEnum.WithDescription;
            var result = value.ToString().ToEnum<TestEnum>();
            Assert.AreEqual(value, result);
        }

        [TestMethod]
        [TestCategory("TimeSpanExtensions")]
        public void TimeSpanExtensions_ToReadableTime_CorrectOutput()
        {
            var ts = new TimeSpan(1, 2, 3, 4, 500);
            var result = ts.ToReadableTime();
            StringAssert.Contains(result, "1d");
            StringAssert.Contains(result, "2h");
            StringAssert.Contains(result, "3m");
            StringAssert.Contains(result, "4s");
            StringAssert.Contains(result, "500ms");
        }

        [TestMethod]
        [TestCategory("ExponentialRetry")]
        public async Task ExponentialRetry_WorksAfterRetries()
        {
            int attempts = 0;
            await ExponentialRetry.ExecuteAsync(
                async (msg) =>
                {
                    attempts++;
                    return await Task.FromResult(attempts >= 3);
                }, 10, 100, CancellationToken.None);

            Assert.IsTrue(attempts >= 3);
        }

        [TestMethod]
        [TestCategory("ExponentialRetry")]
        public void ExponentialRetry_CalculateDelay_MatchesExpectedValues()
        {
            int min = 50;
            int max = 1000;
            int scale = 10;
            double exponent = 2.5;

            int[] expectedDelays = new[] { 53, 66, 96, 146, 217, 314, 439, 593, 780, 1000 };

            for (int attempt = 1; attempt <= 10; attempt++)
            {
                TimeSpan actual = ExponentialRetry.CalculateDelay(attempt, min, max, scale, exponent);
                int actualMs = (int)actual.TotalMilliseconds;
                int expectedMs = expectedDelays[attempt - 1];

                Console.WriteLine($"Attempt {attempt}: Actual = {actualMs} ms, Expected = {expectedMs} ms");

                // Allow ±1 ms tolerance due to rounding behavior
                Assert.IsTrue(Math.Abs(actualMs - expectedMs) <= 1,
                    $"Delay for attempt {attempt} was {actualMs} ms; expected {expectedMs} ms");
            }
        }

        [TestMethod]
        [TestCategory("StartStopRegistration")]
        public void StartStopRegistration_TimesCorrectly()
        {
            long elapsed = 0;
            using (new StartStopRegistration(null, ms => elapsed = ms))
            {
                Thread.Sleep(50);
            }
            Assert.IsTrue(elapsed >= 50);
        }

        [TestClass]
        public class AsyncAutoResetEventTests
        {
            [TestMethod, Timeout(1000)]
            public async Task WaitAsync_ReturnsImmediately_WhenPreviouslySignaled()
            {
                var reset = new AsyncAutoResetEvent();
                reset.Signal(); // Pre-signal

                var task = reset.WaitAsync();

                await task; // Ensure the task completes and observe exceptions here

                Assert.IsTrue(task.IsCompleted);
                Assert.IsNull(task.Exception);
                Assert.IsFalse(task.IsCanceled);
            }

            [TestMethod, Timeout(1000)]
            public async Task WaitAsync_Completes_WhenSignalIsCalled()
            {
                var reset = new AsyncAutoResetEvent();

                var waiting = Task.Run(async () =>
                {
                    await reset.WaitAsync();
                    return true;
                });

                await Task.Delay(100); // Ensure WaitAsync is blocking

                reset.Signal(); // Should unblock

                var result = await Task.WhenAny(waiting, Task.Delay(1000));
                Assert.AreEqual(waiting, result);
                Assert.IsTrue(waiting.Result);
            }

            [TestMethod, Timeout(1000)]
            public async Task WaitAsync_DoesNotReleaseMultipleWaiters_PerSignal()
            {
                var reset = new AsyncAutoResetEvent();

                var t1 = Task.Run(() => reset.WaitAsync());
                var t2 = Task.Run(() => reset.WaitAsync());

                reset.Signal(); // should release one

                var completed = await Task.WhenAny(t1, t2, Task.Delay(1000));
                Assert.AreNotEqual(Task.WhenAll(t1, t2), completed); // Only one should complete

                // Now release second
                reset.Signal();
                await Task.WhenAll(t1, t2); // Now both should be complete
                Assert.IsTrue(t1.IsCompleted);
                Assert.IsTrue(t2.IsCompleted);
            }

            [TestMethod, Timeout(1000)]
            public async Task Signal_StoresOnlyOne_WhenNoWaiters()
            {
                var reset = new AsyncAutoResetEvent();

                // Multiple signals - only one should be stored
                reset.Signal();
                reset.Signal();
                reset.Signal();

                // First wait should complete immediately
                var first = reset.WaitAsync();
                await first;
                Assert.IsTrue(first.IsCompleted, "First wait should complete immediately due to stored signal.");

                // Second wait should remain pending
                var second = reset.WaitAsync();
                var completed = await Task.WhenAny(second, Task.Delay(200));
                Assert.AreNotEqual(second, completed, "Second wait should still be pending, only one signal should be stored.");

                // Now signal again to unblock second
                reset.Signal();
                await second;
                Assert.IsTrue(second.IsCompleted, "Second wait should complete after new signal.");
            }

            [TestMethod, Timeout(1000)]
            public async Task WaitAsync_RespectsCancellationToken()
            {
                var reset = new AsyncAutoResetEvent();
                using (var cts = new CancellationTokenSource(100))
                {
                    var ex = await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
                    {
                        await reset.WaitAsync(cts.Token);
                    });
                    Assert.IsInstanceOfType(ex, typeof(OperationCanceledException)); // Sanity check
                }
            }

            [TestMethod, Timeout(1000)]
            public async Task ConcurrentSignaling_ReleasesAllWaiters_OneAtATime()
            {
                var reset = new AsyncAutoResetEvent();
                int released = 0;

                var tasks = Enumerable.Range(0, 3).Select(async _ =>
                {
                    await reset.WaitAsync();
                    Interlocked.Increment(ref released);
                }).ToList();

                await Task.Delay(100); // Ensure all tasks are queued

                reset.Signal(); // Releases one
                await Task.Delay(100);
                Assert.AreEqual(1, released);

                reset.Signal(); // Releases second
                await Task.Delay(100);
                Assert.AreEqual(2, released);

                reset.Signal(); // Releases third
                await Task.WhenAll(tasks);
                Assert.AreEqual(3, released);
            }

            [TestMethod, Timeout(1000)]
            public async Task CancelledWait_DoesNotBlockFutureSignals()
            {
                var reset = new AsyncAutoResetEvent();
                var cts = new CancellationTokenSource();

                var cancelledTask = Task.Run(async () =>
                {
                    try
                    {
                        await reset.WaitAsync(cts.Token);
                    }
                    catch (OperationCanceledException) { /* expected */ }
                });

                // Cancel the wait
                cts.Cancel();
                await cancelledTask;

                // Now verify signal still works for next real waiter
                var successTask = reset.WaitAsync();
                reset.Signal();
                await successTask;

                Assert.IsTrue(successTask.IsCompleted);
            }
        }
    }
}