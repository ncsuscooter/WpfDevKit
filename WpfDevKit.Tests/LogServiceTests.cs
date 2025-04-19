using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using WpfDevKit.Logging;

namespace WpfDevKit.Tests.Logging
{
    internal class TestLogMessage : ILogMessage
    {
        public long Index => 0;
        public DateTime Timestamp => DateTime.UtcNow;
        public string MachineName => "TestMachine";
        public string UserName => "TestUser";
        public string ApplicationName => "TestApp";
        public Version ApplicationVersion => new Version(1, 0);
        public string ClassName => "TestClass";
        public string MethodName => "TestMethod";
        public int ThreadId => 1;
        public TLogCategory Category { get; set; }
        public string Message { get; set; }
        public string Attributes { get; set; }
        public int? ExceptionLevel { get; set; }
        public string ExceptionStackTrace { get; set; }
    }

    [TestClass]
    public class LogMetricsTests
    {
        [TestMethod]
        public void IncrementTotal_AddsOneEachTime()
        {
            var metrics = new LogMetrics();
            metrics.IncrementTotal();
            metrics.IncrementTotal();
            Assert.AreEqual(2, metrics.Total);
        }

        [TestMethod]
        public void IncrementQueued_AddsCorrectly()
        {
            var metrics = new LogMetrics();
            metrics.IncrementQueued();
            metrics.IncrementQueued();
            metrics.IncrementQueued();
            Assert.AreEqual(3, metrics.Queued);
        }

        [TestMethod]
        public void IncrementLost_AddsCorrectly()
        {
            var metrics = new LogMetrics();
            metrics.IncrementLost();
            Assert.AreEqual(1, metrics.Lost);
        }

        [TestMethod]
        public void IncrementNull_AddsCorrectly()
        {
            var metrics = new LogMetrics();
            metrics.IncrementNull();
            metrics.IncrementNull();
            Assert.AreEqual(2, metrics.Null);
        }

        [TestMethod]
        public void IncrementElapsed_AddsMilliseconds()
        {
            var metrics = new LogMetrics();
            metrics.IncrementElapsed(100);
            metrics.IncrementElapsed(200);
            Assert.AreEqual(TimeSpan.FromMilliseconds(300), metrics.Elapsed);
        }

        [TestMethod]
        public void IncrementCategory_CountsCorrectly()
        {
            var metrics = new LogMetrics();
            metrics.IncrementCategory(TLogCategory.Debug);
            metrics.IncrementCategory(TLogCategory.Debug);
            metrics.IncrementCategory(TLogCategory.Error);
            var dict = metrics.CategoryCounts;
            Assert.AreEqual(2, dict[TLogCategory.Debug]);
            Assert.AreEqual(1, dict[TLogCategory.Error]);
        }

        [TestMethod]
        public void StartStop_TracksCategoryAndTime()
        {
            var metrics = new LogMetrics();
            var message = new TestLogMessage { Category = TLogCategory.Warning };

            using (metrics.StartStop(message))
                Thread.Sleep(10);

            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.CategoryCounts[TLogCategory.Warning]);
            Assert.IsTrue(metrics.Elapsed.TotalMilliseconds >= 10);
        }

        [TestMethod]
        public void StartStop_WithNullMessage_TracksNull()
        {
            var metrics = new LogMetrics();

            using (metrics.StartStop(null))
                Thread.Sleep(5);

            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.Null);
            Assert.IsTrue(metrics.Elapsed.TotalMilliseconds >= 5);
        }
    }

    [TestClass]
    public class LogQueueTests
    {
        [TestMethod]
        public void TryWrite_AddsMessageToQueue()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var msg = new TestLogMessage { Message = "Test" };

            var result = queue.TryWrite(msg);

            Assert.IsTrue(result);
            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.Queued);
        }

        [TestMethod]
        public void TryRead_GetsMessageFromQueue()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var msg = new TestLogMessage { Message = "Test Read" };
            queue.TryWrite(msg);

            var success = queue.TryRead(out var readMsg);

            Assert.IsTrue(success);
            Assert.AreEqual("Test Read", readMsg.Message);
        }

        [TestMethod]
        public void TryRead_ReturnsFalse_WhenEmpty()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            var result = queue.TryRead(out var read);

            Assert.IsFalse(result);
            Assert.IsNull(read);
        }

        [TestMethod]
        public void Queue_Respects_FIFO_Order()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            queue.TryWrite(new TestLogMessage { Message = "First" });
            queue.TryWrite(new TestLogMessage { Message = "Second" });

            queue.TryRead(out var msg1);
            queue.TryRead(out var msg2);

            Assert.AreEqual("First", msg1.Message);
            Assert.AreEqual("Second", msg2.Message);
        }

        [TestMethod]
        public void TryWrite_NullMessage_Throws()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            var result = queue.TryWrite(null);

            Assert.IsFalse(result);
            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.Null);
            Assert.AreEqual(0, metrics.Queued);
        }

        [TestMethod]
        public async Task LogQueue_MultiThreaded_ReadWrite_Stability()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            int writeCount = 0, readCount = 0;
            object writeLock = new object(), readLock = new object();

            // Writer task: simulate high-volume writes
            Task[] writers = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    var msg = new TestLogMessage { Message = Guid.NewGuid().ToString() };
                    if (queue.TryWrite(msg))
                    {
                        lock (writeLock) writeCount++;
                    }
                }
            })).ToArray();

            // Reader task: continuously consume
            Task[] readers = Enumerable.Range(0, 3).Select(_ => Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested)
                {
                    if (queue.TryRead(out var _))
                    {
                        lock (readLock) readCount++;
                    }
                }
            })).ToArray();

            await Task.WhenAll(writers.Concat(readers));

            Console.WriteLine($"Total writes: {writeCount}");
            Console.WriteLine($"Total reads: {readCount}");

            // The queue may contain unconsumed items at shutdown
            Assert.IsTrue(writeCount > 0);
            Assert.IsTrue(readCount > 0);
            Assert.IsTrue(writeCount >= readCount); // Some may still be in the queue
            Assert.IsTrue(metrics.Queued > 0);
            Assert.IsTrue(metrics.Total > 0);
            Assert.AreEqual(metrics.Total, metrics.Queued + metrics.Null + metrics.Lost);
        }

        [TestMethod]
        public void TryWrite_ReturnsFalse_WhenQueueIsFull()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            var msg = new TestLogMessage { Message = "test" };
            var count = 0;

            // Fill the queue to its 8196-item limit
            for (; count < 8196; count++)
            {
                var success = queue.TryWrite(msg);
                if (!success) break;
            }

            Console.WriteLine($"Items written before full: {count}");
            Assert.AreEqual(8196, count);

            // At this point, the queue is full. TryWrite should now return false
            var result = queue.TryWrite(msg);
            Assert.IsFalse(result);
            Assert.AreEqual(8196, metrics.Queued);
            Assert.AreEqual(8197, metrics.Total);
            Assert.AreEqual(1, metrics.Lost);
        }

        [TestMethod]
        public async Task TryWrite_Unblocks_WhenConsumersReadFromFullQueue()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var message = new TestLogMessage { Message = "block test" };

            // Fill the queue to capacity
            for (int i = 0; i < 8196; i++)
                Assert.IsTrue(queue.TryWrite(message));

            // Start a consumer to dequeue one message after a short delay
            var consumer = Task.Run(async () =>
            {
                await Task.Delay(100); // Give producer time to hit full
                var success = queue.TryRead(out var _);
                Console.WriteLine($"Consumer read success: {success}");
                return success;
            });

            // Retry writes while the queue is full — expect first to fail, then succeed after consumer frees space
            bool writeSucceeded = false;
            for (int i = 0; i < 50; i++)
            {
                writeSucceeded = queue.TryWrite(message);
                if (writeSucceeded)
                    break;
                await Task.Delay(10);
            }

            Console.WriteLine($"Write succeeded after unblocking: {writeSucceeded}");
            Assert.IsTrue(await consumer);  // ensure read worked
            Assert.IsTrue(writeSucceeded);  // ensure producer eventually wrote
            Assert.IsTrue(metrics.Total > 8196); // some retry attempts
            Assert.IsTrue(metrics.Lost > 0);     // initial failures
            Assert.AreEqual(metrics.Queued + metrics.Lost, metrics.Total);
        }

        [TestMethod]
        public void TryRead_ReturnsFalse_WhenQueueDrained()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var message = new TestLogMessage { Message = "drain test" };

            queue.TryWrite(message);
            Assert.IsTrue(queue.TryRead(out var read));
            Assert.AreEqual("drain test", read.Message);

            // Now the queue should be empty
            var success = queue.TryRead(out var emptyRead);
            Assert.IsFalse(success);
            Assert.IsNull(emptyRead);
        }

        [TestMethod]
        public void TryWriteTryRead_Interleaved_MatchesCount()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var message = new TestLogMessage { Message = "interleave" };
            int written = 0, read = 0;

            for (int i = 0; i < 1000; i++)
            {
                if (queue.TryWrite(message))
                    written++;

                if (queue.TryRead(out var _))
                    read++;
            }

            Console.WriteLine($"Written: {written}, Read: {read}");
            Assert.AreEqual(written, read); // Should balance closely in loop
        }

        [TestMethod]
        public void TryWriteTryRead_PreservesOrder()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            for (int i = 0; i < 10; i++)
                queue.TryWrite(new TestLogMessage { Message = $"msg{i}" });

            for (int i = 0; i < 10; i++)
            {
                queue.TryRead(out var msg);
                Assert.AreEqual($"msg{i}", msg.Message);
            }
        }

    }
    
    [TestClass]
    public class LogServiceTests
    {
        [TestMethod]
        public void LogService_LogsTextMessage_ToQueue()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var service = new LogService(queue);

            service.Log(TLogCategory.Info, "Test Message", "attr=test", typeof(LogServiceTests));

            Assert.AreEqual(1, metrics.Queued);
            Assert.IsTrue(queue.TryRead(out var log));
            Assert.AreEqual("Test Message", log.Message);
            Console.WriteLine($"Message: '{log.Message}' | Category: {log.Category}");
        }

        [TestMethod]
        public void LogService_LogsException_WithInnerExceptions()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var service = new LogService(queue);

            var inner = new Exception("Inner exception");
            var outer = new InvalidOperationException("Outer exception", inner);

            service.Log(TLogCategory.Error, outer, typeof(LogServiceTests));

            Assert.AreEqual(2, metrics.Queued);
            Assert.IsTrue(queue.TryRead(out var first));
            Assert.AreEqual("Outer exception", first.Message);
            Assert.AreEqual(1, first.ExceptionLevel);

            Assert.IsTrue(queue.TryRead(out var second));
            Assert.AreEqual("Inner exception", second.Message);
            Assert.AreEqual(2, second.ExceptionLevel);
        }

        [TestMethod]
        public void LogService_LogsException_WithAttributes()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var service = new LogService(queue);

            var ex = new ArgumentNullException("param");
            ex.Data["Context"] = "UnitTest";

            service.Log(TLogCategory.Error, ex, typeof(LogServiceTests));

            Assert.AreEqual(1, metrics.Queued);
            Assert.IsTrue(queue.TryRead(out var log));
            Assert.AreEqual("param", log.Message);
            Assert.IsTrue(log.Attributes.Contains("Context='UnitTest'"));
        }

        [TestMethod]
        public void LogService_DoesNotThrow_OnNullMessage()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var service = new LogService(queue);

            service.Log(TLogCategory.Warning, (string)null);

            Assert.AreEqual(1, metrics.Queued);
            Assert.IsTrue(queue.TryRead(out var log));
            Assert.AreEqual(null, log.Message);
        }

        [TestMethod]
        public void LogService_DoesNotThrow_OnNullException()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var service = new LogService(queue);

            service.Log(TLogCategory.Error, (Exception)null);

            Assert.AreEqual(0, metrics.Queued); // Should not enqueue anything
        }
    }

    internal class TestLogProvider : ILogProvider
    {
        public List<ILogMessage> Logs { get; } = new List<ILogMessage>();
        public TLogCategory EnabledCategories { get; set; }
        public TLogCategory DisabledCategories { get; }
        public Task LogAsync(ILogMessage message)
        {
            Logs.Add(message);
            return Task.CompletedTask;
        }
        public void Write(string message, TLogCategory category, string attributes = null) => LogAsync(new TestLogMessage
        {
            Message = message,
            Category = category,
            Attributes = attributes
        });
        public void Write(Exception ex, TLogCategory category = TLogCategory.Error) => LogAsync(new TestLogMessage
        {
            Message = ex.Message,
            Category = category,
            ExceptionLevel = 1,
            ExceptionStackTrace = ex.StackTrace
        });
    }

    [TestClass]
    public class LogProviderCollectionTests
    {
        private LogProviderCollection CreateCollection(out LogService service)
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            service = new LogService(queue);
            return new LogProviderCollection(service);
        }

        [TestMethod]
        public void TryAddProvider_AddsSuccessfully()
        {
            var collection = CreateCollection(out var _);
            var provider = new TestLogProvider();

            var result = collection.TryAddProvider(provider, "default");

            Assert.IsTrue(result);
            Assert.AreEqual(1, collection.GetProviderInfos().Count());
        }

        [TestMethod]
        public void TryAddProvider_PreventsDuplicates()
        {
            var collection = CreateCollection(out var _);
            var provider = new TestLogProvider();

            Assert.IsTrue(collection.TryAddProvider(provider, "sameKey"));
            Assert.IsFalse(collection.TryAddProvider(provider, "sameKey")); // duplicate key & type
        }

        [TestMethod]
        public void TryRemoveProvider_RemovesSuccessfully()
        {
            var collection = CreateCollection(out var _);
            var provider = new TestLogProvider();

            collection.TryAddProvider(provider, "toRemove");
            var result = collection.TryRemoveProvider(provider, "toRemove");

            Assert.IsTrue(result);
            Assert.AreEqual(0, collection.GetProviderInfos().Count());
        }

        [TestMethod]
        public void TryRemoveProvider_FailsForNonexistent()
        {
            var collection = CreateCollection(out var _);
            var provider = new TestLogProvider();

            var result = collection.TryRemoveProvider(provider, "missing");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void GetProviderInfos_ContainsExpectedDescriptorData()
        {
            var collection = CreateCollection(out var _);
            var provider = new TestLogProvider();

            collection.TryAddProvider(provider, "info");

            var descriptor = collection.GetProviderInfos().FirstOrDefault();

            Assert.IsNotNull(descriptor);
            Assert.AreEqual("info", descriptor.Key);
            Assert.AreEqual(provider.GetType(), descriptor.ProviderType);
            Assert.IsNotNull(descriptor.Metrics);
        }

        [TestMethod]
        public void TryAddProvider_NullProvider_ReturnsFalse()
        {
            var collection = CreateCollection(out var _);

            var result = collection.TryAddProvider(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TryRemoveProvider_NullProvider_ReturnsFalse()
        {
            var collection = CreateCollection(out var _);

            var result = collection.TryRemoveProvider(null);

            Assert.IsFalse(result);
        }
    }


    //[TestClass]
    //public class MemoryLogProviderTests
    //{
    //    private TestableMemoryLogProvider CreateProvider(int capacity = 10)
    //    {
    //        var options = new MemoryLogProviderOptions
    //        {
    //            Capacity = capacity,
    //            FillFactor = 100
    //        };
    //        return new TestableMemoryLogProvider(options);
    //    }

    //    [TestMethod]
    //    public async Task LogAsync_AddsMessage_AndUpdatesMetrics()
    //    {
    //        var provider = CreateProvider();
    //        var message = new TestLogMessage { Message = "Hello World", Category = TLogCategory.Info };

    //        await provider.LogAsync(message);

    //        var logs = provider.Snapshot;
    //        Assert.AreEqual(1, logs.Count, "Message count mismatch.");
    //        Assert.AreEqual("Hello World", logs[0].Message);

    //        var metrics = provider.Metrics;
    //        Assert.AreEqual(1, metrics.Total);
    //        Assert.AreEqual(1, metrics.CategoryCounts[TLogCategory.Info]);
    //    }

    //    [TestMethod]
    //    public async Task LogAsync_ExceedsCapacity_DropsOldest()
    //    {
    //        var provider = CreateProvider(capacity: 3);

    //        await provider.LogAsync(new TestLogMessage { Message = "1", Category = TLogCategory.Debug });
    //        await provider.LogAsync(new TestLogMessage { Message = "2", Category = TLogCategory.Debug });
    //        await provider.LogAsync(new TestLogMessage { Message = "3", Category = TLogCategory.Debug });
    //        await provider.LogAsync(new TestLogMessage { Message = "4", Category = TLogCategory.Debug });

    //        var logs = provider.Snapshot;
    //        Assert.AreEqual(3, logs.Count);
    //        Assert.AreEqual("2", logs[0].Message);
    //        Assert.AreEqual("3", logs[1].Message);
    //        Assert.AreEqual("4", logs[2].Message);

    //        var metrics = provider.Metrics;
    //        Assert.AreEqual(4, metrics.Total);
    //        Assert.AreEqual(4, metrics.CategoryCounts[TLogCategory.Debug]);
    //    }

    //    [TestMethod]
    //    public async Task LogAsync_SupportsDifferentCategories()
    //    {
    //        var provider = CreateProvider();

    //        await provider.LogAsync(new TestLogMessage { Message = "Trace", Category = TLogCategory.Trace });
    //        await provider.LogAsync(new TestLogMessage { Message = "Warn", Category = TLogCategory.Warning });

    //        var metrics = provider.Metrics;
    //        Assert.AreEqual(2, metrics.Total);
    //        Assert.AreEqual(1, metrics.CategoryCounts[TLogCategory.Trace]);
    //        Assert.AreEqual(1, metrics.CategoryCounts[TLogCategory.Warning]);
    //    }

    //    [TestMethod]
    //    public async Task LogAsync_MetricsCategoryMapIsStable()
    //    {
    //        var provider = CreateProvider();

    //        foreach (var cat in Enum.GetValues(typeof(TLogCategory)).Cast<TLogCategory>())
    //        {
    //            if (cat == TLogCategory.None) continue;

    //            await provider.LogAsync(new TestLogMessage { Message = $"Test: {cat}", Category = cat });
    //        }

    //        var metrics = provider.Metrics;
    //        foreach (var cat in Enum.GetValues(typeof(TLogCategory)).Cast<TLogCategory>())
    //        {
    //            if (cat == TLogCategory.None) continue;
    //            Assert.AreEqual(1, metrics.CategoryCounts[cat], $"Category {cat} expected 1 log.");
    //        }
    //    }

    //    [TestMethod]
    //    public async Task FillFactor_75Percent_TrimsThenAdds_NewestPreserved()
    //    {
    //        var provider = new TestableMemoryLogProvider(new MemoryLogProviderOptions
    //        {
    //            Capacity = 4,
    //            FillFactor = 75
    //        });

    //        for (int i = 1; i <= 5; i++)
    //            await provider.LogAsync(new TestLogMessage { Message = $"msg {i}", Category = TLogCategory.Info });

    //        var logs = provider.Snapshot;
    //        Assert.AreEqual(4, logs.Count); // Capacity is preserved
    //        CollectionAssert.AreEqual(new[] { "msg 2", "msg 3", "msg 4", "msg 5" }, logs.Select(x => x.Message).ToArray());
    //    }

    //    [TestMethod]
    //    public async Task FillFactor_100Percent_StillDropsOne()
    //    {
    //        var provider = new TestableMemoryLogProvider(new MemoryLogProviderOptions
    //        {
    //            Capacity = 3,
    //            FillFactor = 100 // Would calculate to 0, but fallback ensures we drop at least 1
    //        });

    //        for (int i = 1; i <= 4; i++)
    //            await provider.LogAsync(new TestLogMessage { Message = $"msg {i}", Category = TLogCategory.Info });

    //        var logs = provider.Snapshot;
    //        Assert.AreEqual(3, logs.Count);
    //        CollectionAssert.AreEqual(new[] { "msg 2", "msg 3", "msg 4" }, logs.Select(x => x.Message).ToArray());
    //    }

    //}



    internal class TestableMemoryLogProvider : MemoryLogProvider
    {
        private static readonly FieldInfo itemsField = typeof(MemoryLogProvider)
            .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);

        public TestableMemoryLogProvider(MemoryLogProviderOptions options)
            : base(options)
        {
            if (itemsField == null)
                throw new InvalidOperationException("Failed to reflect internal 'items' list from MemoryLogProvider.");
        }

        /// <summary>
        /// Returns a thread-safe snapshot of the internal log buffer using reflection.
        /// </summary>
        public IReadOnlyList<ILogMessage> Snapshot
        {
            get
            {
                var list = (List<ILogMessage>)itemsField.GetValue(this);
                lock (list)
                {
                    return list.ToList();
                }
            }
        }
    }
}
