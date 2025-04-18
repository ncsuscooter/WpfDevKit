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
    //[TestClass]
    //public class LogServiceTests
    //{
    //    [TestMethod]
    //    public void LogService_LogsMessage_ToProvider()
    //    {
    //        var provider = new TestProvider();
    //        var service = new LogService(new[] { provider });

    //        service.Log("Hello World", TLogCategory.Info, "Test");

    //        Assert.AreEqual(1, provider.Received.Count);
    //        Console.WriteLine($"Message = '{provider.Received[0].Message}' - Expected: 'Hello World'");
    //    }

    //    [TestMethod]
    //    public void LogService_Handles_NoProviders()
    //    {
    //        var service = new LogService(new List<ILogProvider>());

    //        service.Log("No provider", TLogCategory.Warning, "Test");

    //        Assert.IsTrue(true); // Should not throw
    //    }

    //    [TestMethod]
    //    public void LogService_Honors_DisabledCategories()
    //    {
    //        var provider = new TestProvider
    //        {
    //            EnabledCategories = TLogCategory.All,
    //            // Simulate ignoring Debug
    //            // Internally check (Enabled & msg.Category != 0) && (Disabled & msg.Category == 0)
    //        };

    //        provider.GetType().GetProperty("DisabledCategories").SetValue(provider, TLogCategory.Debug);

    //        var service = new LogService(new[] { provider });
    //        service.Log("Debug msg", TLogCategory.Debug, "Test");

    //        Assert.AreEqual(0, provider.Received.Count);
    //    }

    //    [TestMethod]
    //    public void LogService_Logs_Exception()
    //    {
    //        var provider = new TestProvider();
    //        var service = new LogService(new[] { provider });

    //        var ex = new InvalidOperationException("Broken");
    //        service.LogError(ex, typeof(LogServiceTests));

    //        Assert.AreEqual(1, provider.Received.Count);
    //        Console.WriteLine($"Exception = '{provider.Received[0].Message}' - Expected to contain 'Broken'");
    //    }

    //    [TestMethod]
    //    public void LogService_Logs_ToMultipleProviders()
    //    {
    //        var p1 = new TestProvider();
    //        var p2 = new TestProvider();
    //        var service = new LogService(new[] { p1, p2 });

    //        service.Log("Multi", TLogCategory.Info, "Test");

    //        Assert.AreEqual(1, p1.Received.Count);
    //        Assert.AreEqual(1, p2.Received.Count);
    //    }
    //}
    
    //[TestClass]
    //public class LogQueueTests
    //{
    //    [TestMethod]
    //    public void TryWrite_AddsMessageToQueue()
    //    {
    //        var queue = new LogQueue();
    //        var msg = new TestLogMessage { Message = "Test" };

    //        var result = queue.TryWrite(msg);

    //        Assert.IsTrue(result);
    //    }

    //    [TestMethod]
    //    public void TryRead_GetsMessageFromQueue()
    //    {
    //        var queue = new LogQueue();
    //        var msg = new TestLogMessage { Message = "Test Read" };
    //        queue.TryWrite(msg);

    //        var success = queue.TryRead(out var readMsg);

    //        Assert.IsTrue(success);
    //        Assert.AreEqual("Test Read", readMsg.Message);
    //    }

    //    [TestMethod]
    //    public void TryRead_ReturnsFalse_WhenEmpty()
    //    {
    //        var queue = new LogQueue();

    //        var result = queue.TryRead(out var read);

    //        Assert.IsFalse(result);
    //        Assert.IsNull(read);
    //    }

    //    [TestMethod]
    //    public void Queue_Respects_FIFO_Order()
    //    {
    //        var queue = new LogQueue();

    //        queue.TryWrite(new TestLogMessage { Message = "First" });
    //        queue.TryWrite(new TestLogMessage { Message = "Second" });

    //        queue.TryRead(out var msg1);
    //        queue.TryRead(out var msg2);

    //        Assert.AreEqual("First", msg1.Message);
    //        Assert.AreEqual("Second", msg2.Message);
    //    }

    //    [TestMethod]
    //    public void TryWrite_NullMessage_Throws()
    //    {
    //        var queue = new LogQueue();

    //        var result = queue.TryWrite(null);

    //        Assert.IsFalse(result);
    //    }

    //    [TestMethod]
    //    public async Task LogQueue_MultiThreaded_ReadWrite_Stability()
    //    {
    //        var queue = new LogQueue();
    //        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    //        int writeCount = 0, readCount = 0;
    //        object writeLock = new object(), readLock = new object();

    //        // Writer task: simulate high-volume writes
    //        Task[] writers = Enumerable.Range(0, 5).Select(_ => Task.Run(() =>
    //        {
    //            while (!cts.Token.IsCancellationRequested)
    //            {
    //                var msg = new TestLogMessage { Message = Guid.NewGuid().ToString() };
    //                if (queue.TryWrite(msg))
    //                {
    //                    lock (writeLock) writeCount++;
    //                }
    //            }
    //        })).ToArray();

    //        // Reader task: continuously consume
    //        Task[] readers = Enumerable.Range(0, 3).Select(_ => Task.Run(() =>
    //        {
    //            while (!cts.Token.IsCancellationRequested)
    //            {
    //                if (queue.TryRead(out var _))
    //                {
    //                    lock (readLock) readCount++;
    //                }
    //            }
    //        })).ToArray();

    //        await Task.WhenAll(writers.Concat(readers));

    //        Console.WriteLine($"Total writes: {writeCount}");
    //        Console.WriteLine($"Total reads: {readCount}");

    //        // The queue may contain unconsumed items at shutdown
    //        Assert.IsTrue(writeCount > 0);
    //        Assert.IsTrue(readCount > 0);
    //        Assert.IsTrue(writeCount >= readCount); // Some may still be in the queue
    //    }

    //    [TestMethod]
    //    public void TryWrite_ReturnsFalse_WhenQueueIsFull()
    //    {
    //        var queue = new LogQueue();

    //        var msg = new TestLogMessage { Message = "test" };
    //        var count = 0;

    //        // Fill the queue to its 8196-item limit
    //        for (; count < 8196; count++)
    //        {
    //            var success = queue.TryWrite(msg);
    //            if (!success) break;
    //        }

    //        Console.WriteLine($"Items written before full: {count}");
    //        Assert.AreEqual(8196, count);

    //        // At this point, the queue is full. TryWrite should now return false
    //        var result = queue.TryWrite(msg);
    //        Assert.IsFalse(result);
    //    }

    //    [TestMethod]
    //    public async Task TryWrite_Unblocks_WhenConsumersReadFromFullQueue()
    //    {
    //        var queue = new LogQueue();
    //        var message = new TestLogMessage { Message = "block test" };

    //        // Fill the queue to capacity
    //        for (int i = 0; i < 8196; i++)
    //            Assert.IsTrue(queue.TryWrite(message));

    //        // Start a consumer to dequeue one message after a short delay
    //        var consumer = Task.Run(async () =>
    //        {
    //            await Task.Delay(100); // Give producer time to hit full
    //            var success = queue.TryRead(out var _);
    //            Console.WriteLine($"Consumer read success: {success}");
    //            return success;
    //        });

    //        // Retry writes while the queue is full — expect first to fail, then succeed after consumer frees space
    //        bool writeSucceeded = false;
    //        for (int i = 0; i < 50; i++)
    //        {
    //            writeSucceeded = queue.TryWrite(message);
    //            if (writeSucceeded)
    //                break;
    //            await Task.Delay(10);
    //        }

    //        Console.WriteLine($"Write succeeded after unblocking: {writeSucceeded}");
    //        Assert.IsTrue(await consumer);  // ensure read worked
    //        Assert.IsTrue(writeSucceeded);  // ensure producer eventually wrote
    //    }

    //    [TestMethod]
    //    public void TryRead_ReturnsFalse_WhenQueueDrained()
    //    {
    //        var queue = new LogQueue();
    //        var message = new TestLogMessage { Message = "drain test" };

    //        queue.TryWrite(message);
    //        Assert.IsTrue(queue.TryRead(out var read));
    //        Assert.AreEqual("drain test", read.Message);

    //        // Now the queue should be empty
    //        var success = queue.TryRead(out var emptyRead);
    //        Assert.IsFalse(success);
    //        Assert.IsNull(emptyRead);
    //    }

    //    [TestMethod]
    //    public void TryWriteTryRead_Interleaved_MatchesCount()
    //    {
    //        var queue = new LogQueue();
    //        var message = new TestLogMessage { Message = "interleave" };
    //        int written = 0, read = 0;

    //        for (int i = 0; i < 1000; i++)
    //        {
    //            if (queue.TryWrite(message))
    //                written++;

    //            if (queue.TryRead(out var _))
    //                read++;
    //        }

    //        Console.WriteLine($"Written: {written}, Read: {read}");
    //        Assert.AreEqual(written, read); // Should balance closely in loop
    //    }

    //    [TestMethod]
    //    public void TryWriteTryRead_PreservesOrder()
    //    {
    //        var queue = new LogQueue();

    //        for (int i = 0; i < 10; i++)
    //            queue.TryWrite(new TestLogMessage { Message = $"msg{i}" });

    //        for (int i = 0; i < 10; i++)
    //        {
    //            queue.TryRead(out var msg);
    //            Assert.AreEqual($"msg{i}", msg.Message);
    //        }
    //    }

    //}
    
    //[TestClass]
    //public class LogProviderTests
    //{
    //    [TestMethod]
    //    public void LogAsync_AddsMessage()
    //    {
    //        var provider = new TestLogProvider();
    //        var message = new TestLogMessage { Message = "Async message", Category = TLogCategory.Debug };

    //        provider.LogAsync(message).Wait();

    //        Assert.AreEqual(1, provider.Logs.Count);
    //        Assert.AreEqual("Async message", provider.Logs[0].Message);
    //        Assert.AreEqual(TLogCategory.Debug, provider.Logs[0].Category);
    //    }

    //    [TestMethod]
    //    public void Write_MessageObject_AddsMessage()
    //    {
    //        var provider = new TestLogProvider();
    //        var message = new TestLogMessage { Message = "Direct Write", Category = TLogCategory.Info };

    //        provider.Write(message);

    //        Assert.AreEqual(1, provider.Logs.Count);
    //        Assert.AreEqual("Direct Write", provider.Logs[0].Message);
    //        Assert.AreEqual(TLogCategory.Info, provider.Logs[0].Category);
    //    }

    //    [TestMethod]
    //    public void Write_String_AddsMessageWithCategory()
    //    {
    //        var provider = new TestLogProvider();

    //        provider.Write("Raw message", TLogCategory.Debug, "key=value");

    //        Assert.AreEqual(1, provider.Logs.Count);
    //        var log = provider.Logs[0];

    //        Assert.AreEqual("Raw message", log.Message);
    //        Assert.AreEqual(TLogCategory.Debug, log.Category);
    //        Assert.AreEqual("key=value", log.Attributes);
    //    }

    //    [TestMethod]
    //    public void Write_Exception_CapturesStackTraceAndLevel()
    //    {
    //        var provider = new TestLogProvider();

    //        try
    //        {
    //            throw new ArgumentOutOfRangeException("param", "Something went wrong");
    //        }
    //        catch (Exception ex)
    //        {
    //            provider.Write(ex);
    //        }

    //        Assert.AreEqual(1, provider.Logs.Count);
    //        var log = provider.Logs[0];

    //        StringAssert.StartsWith(log.Message, "Something went wrong");
    //        StringAssert.Contains(log.Message, "param");
    //        Assert.AreEqual(1, log.ExceptionLevel);
    //        Assert.IsNotNull(log.ExceptionStackTrace);
    //        Assert.AreEqual(TLogCategory.Error, log.Category);
    //    }

    //    [TestMethod]
    //    public void Write_Exception_WithCustomCategory()
    //    {
    //        var provider = new TestLogProvider();

    //        try
    //        {
    //            throw new Exception("Unhandled exception");
    //        }
    //        catch (Exception ex)
    //        {
    //            provider.Write(ex, TLogCategory.Fatal);
    //        }

    //        Assert.AreEqual(1, provider.Logs.Count);
    //        Assert.AreEqual(TLogCategory.Fatal, provider.Logs[0].Category);
    //    }
    //}

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

    //[TestClass]
    //public class LogMetricsTests
    //{
    //    private ILogMetrics<TestLogProvider> metrics;

    //    [TestInitialize]
    //    public void Setup()
    //    {
    //        metrics = new LogMetricsFactory().Create<TestLogProvider>();
    //    }

    //    [TestMethod]
    //    public void IncrementCounters_WorkAsExpected()
    //    {
    //        metrics.IncrementTotal();
    //        metrics.IncrementQueued();
    //        metrics.IncrementLost();
    //        metrics.IncrementNull();

    //        Assert.AreEqual(1, metrics.Total);
    //        Assert.AreEqual(1, metrics.Queued);
    //        Assert.AreEqual(1, metrics.Lost);
    //        Assert.AreEqual(1, metrics.Null);
    //    }

    //    [TestMethod]
    //    public void IncrementUnhandled_AddsException()
    //    {
    //        var ex = new InvalidOperationException("Oops!");
    //        metrics.IncrementUnhandled(ex);

    //        Assert.AreEqual(1, metrics.Unhandled);
    //        Assert.AreEqual(1, metrics.UnhandledExceptions.Count);
    //        Assert.AreSame(ex, metrics.UnhandledExceptions.First());
    //    }

    //    [TestMethod]
    //    public void IncrementCategory_TracksMultipleCategories()
    //    {
    //        metrics.IncrementCategory(TLogCategory.Debug);
    //        metrics.IncrementCategory(TLogCategory.Debug);
    //        metrics.IncrementCategory(TLogCategory.Error);

    //        Assert.AreEqual(2, metrics.CategoryCounts[TLogCategory.Debug]);
    //        Assert.AreEqual(1, metrics.CategoryCounts[TLogCategory.Error]);
    //    }

    //    [TestMethod]
    //    public void StartStop_TracksElapsedTime()
    //    {
    //        var fakeMessage = new TestLogMessage();
    //        var stopwatch = Stopwatch.StartNew();

    //        using (metrics.StartStop(fakeMessage))
    //        {
    //            Thread.Sleep(50);
    //        }

    //        stopwatch.Stop();
    //        Assert.IsTrue(metrics.Elapsed.TotalMilliseconds >= 50, $"Elapsed = {metrics.Elapsed.TotalMilliseconds}");
    //    }
    //}

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
        public void Write(ILogMessage message) => LogAsync(message);
        public void Write(string message, TLogCategory category, string attributes = null) => Write(new TestLogMessage
        {
            Message = message,
            Category = category,
            Attributes = attributes
        });
        public void Write(Exception ex, TLogCategory category = TLogCategory.Error) => Write(new TestLogMessage
        {
            Message = ex.Message,
            Category = category,
            ExceptionLevel = 1,
            ExceptionStackTrace = ex.StackTrace
        });
    }
    internal class TestableMemoryLogProvider : MemoryLogProvider
    {
        private static readonly FieldInfo itemsField = typeof(MemoryLogProvider)
            .GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);

        public TestableMemoryLogProvider(MemoryLogProviderOptions options)
            : base(new LogMetrics<MemoryLogProvider>(), options)
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
