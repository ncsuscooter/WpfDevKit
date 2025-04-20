using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfDevKit.DependencyInjection;
using WpfDevKit.Hosting;
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
    public class LogQueueTests1
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
    public class LogQueueTests2
    {
        [TestMethod]
        public void TryWrite_AddsMessage_AndTracksMetrics()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var msg = new TestLogMessage { Message = "Test" };

            var result = queue.TryWrite(msg);

            Assert.IsTrue(result);
            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.Queued);
            Assert.AreEqual(0, metrics.Lost);
            Assert.AreEqual(0, metrics.Null);
            Assert.AreEqual(1, metrics.CategoryCounts[msg.Category]);
        }

        [TestMethod]
        public void TryWrite_NullMessage_TracksNullAndRejects()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);

            var result = queue.TryWrite(null);

            Assert.IsFalse(result);
            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(0, metrics.Queued);
            Assert.AreEqual(1, metrics.Lost);
            Assert.AreEqual(1, metrics.Null);
        }

        [TestMethod]
        public void TryWrite_WhenQueueFull_TracksLost()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var msg = new TestLogMessage { Message = "Full" };

            for (int i = 0; i < 8196; i++)
                Assert.IsTrue(queue.TryWrite(msg));

            Assert.IsFalse(queue.TryWrite(msg));  // 8197th write fails
            Assert.AreEqual(8197, metrics.Total);
            Assert.AreEqual(8196, metrics.Queued);
            Assert.AreEqual(1, metrics.Lost);
            Assert.AreEqual(0, metrics.Null);
        }

        [TestMethod]
        public void TryRead_MetricsNotAffected()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            queue.TryWrite(new TestLogMessage { Message = "ReadTest" });

            queue.TryRead(out var result);

            Assert.IsNotNull(result);
            Assert.AreEqual(1, metrics.Total);
            Assert.AreEqual(1, metrics.Queued);
            Assert.AreEqual(0, metrics.Lost);
            Assert.AreEqual(0, metrics.Null);
        }

        [TestMethod]
        public void TryWriteTryRead_TracksEachCategory()
        {
            var metrics = new LogMetrics();
            var queue = new LogQueue(metrics);
            var categories = new[] { TLogCategory.Debug, TLogCategory.Error, TLogCategory.Info };

            foreach (var cat in categories)
                queue.TryWrite(new TestLogMessage { Message = "x", Category = cat });

            foreach (var cat in categories)
                Assert.AreEqual(1, metrics.CategoryCounts[cat]);
        }

        [TestMethod]
        public async Task LogMetrics_StartStop_AccumulatesElapsedTime()
        {
            var metrics = new LogMetrics();

            using (metrics.StartStop(new TestLogMessage()))
                await Task.Delay(50);

            using (metrics.StartStop(new TestLogMessage()))
                await Task.Delay(100);

            var totalMs = metrics.Elapsed.TotalMilliseconds;
            Console.WriteLine($"Total Elapsed = {totalMs:N0}ms - Expected: >=150ms");

            Assert.IsTrue(totalMs >= 145); // Accept small timing variation
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
            StringAssert.Contains(log.Message, "Value cannot be null");
            StringAssert.Contains(log.Message, "param");
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

    //internal class TestLogProvider : ILogProvider
    //{
    //    public Task LogAsync(ILogMessage message) => Task.CompletedTask;
    //}
    
    //internal class TestLogProviderOptions : ILogProviderOptions
    //{
    //    public TLogCategory EnabledCategories { get; set; }
    //    public TLogCategory DisabledCategories { get; }
    //}

    //[TestClass]
    //public class LogProviderDescriptorCollectionTests
    //{
    //    [TestMethod]
    //    public void Add_ThenGetDescriptorByGeneric_ReturnsSameDescriptor()
    //    {
    //        var collection = new LogProviderDescriptorCollection();
    //        var descriptor = new LogProviderDescriptor(new TestLogProvider(), new TestLogProviderOptions());

    //        collection.Add<TestLogProvider>(descriptor);
    //        var result = collection.GetDescriptor<TestLogProvider>();

    //        Assert.AreSame(descriptor, result);
    //    }

    //    [TestMethod]
    //    public void GetDescriptor_ByUnknownType_ReturnsNull()
    //    {
    //        var collection = new LogProviderDescriptorCollection();

    //        var result = collection.GetDescriptor(typeof(ConsoleLogProvider));

    //        Assert.IsNull(result);
    //    }

    //    [TestMethod]
    //    public void GetDescriptors_ReturnsAllAdded()
    //    {
    //        var collection = new LogProviderDescriptorCollection();

    //        collection.Add<TestLogProvider>(new LogProviderDescriptor(new TestLogProvider(), new TestLogProviderOptions()));
    //        collection.Add<ConsoleLogProvider>(new LogProviderDescriptor(new ConsoleLogProvider(new Options<ConsoleLogProviderOptions>(new ConsoleLogProviderOptions())), new ConsoleLogProviderOptions()));

    //        var all = collection.GetDescriptors();
    //        Assert.AreEqual(2, all.Count);
    //    }

    //    [TestMethod]
    //    public void Add_ReplacesExistingDescriptor()
    //    {
    //        var collection = new LogProviderDescriptorCollection();

    //        var old = new LogProviderDescriptor(new TestLogProvider(), new TestLogProviderOptions());
    //        var updated = new LogProviderDescriptor(new TestLogProvider(), new TestLogProviderOptions());

    //        collection.Add<TestLogProvider>(old);
    //        collection.Add<TestLogProvider>(updated);

    //        var result = collection.GetDescriptor<TestLogProvider>();
    //        Assert.AreSame(updated, result);
    //    }
    //}

    [TestClass]
    public class MemoryLogProviderTests
    {
        private MemoryLogProvider CreateProvider(int capacity = 10, int fill = 100)
        {
            return new MemoryLogProvider(new Options<MemoryLogProviderOptions>(new MemoryLogProviderOptions()
            {
                Capacity = capacity,
                FillFactor = fill
            }));
        }

        private static IReadOnlyList<ILogMessage> GetLogs(MemoryLogProvider provider)
        {
            var itemsField = typeof(MemoryLogProvider).GetField("items", BindingFlags.NonPublic | BindingFlags.Instance) ??
                throw new InvalidOperationException("Failed to reflect internal 'items' list from MemoryLogProvider.");
            var list = (List<ILogMessage>)itemsField.GetValue(provider);
            lock (list)
                return list.ToList();
        }

        [TestMethod]
        public async Task LogAsync_AppendsMessages_InOrder()
        {
            var provider = CreateProvider();
            await provider.LogAsync(new TestLogMessage { Message = "First" });
            await provider.LogAsync(new TestLogMessage { Message = "Second" });

            var logs = GetLogs(provider);
            Assert.AreEqual(2, logs.Count);
            Assert.AreEqual("First", logs[0].Message);
            Assert.AreEqual("Second", logs[1].Message);
        }

        [TestMethod]
        public async Task LogAsync_TrimsCorrectly_WhenOverCapacity()
        {
            var provider = CreateProvider(capacity: 3);
            await provider.LogAsync(new TestLogMessage { Message = "1" });
            await provider.LogAsync(new TestLogMessage { Message = "2" });
            await provider.LogAsync(new TestLogMessage { Message = "3" });
            await provider.LogAsync(new TestLogMessage { Message = "4" });

            var logs = GetLogs(provider);
            Assert.AreEqual(3, logs.Count);
            CollectionAssert.AreEqual(new[] { "2", "3", "4" }, logs.Select(x => x.Message).ToArray());
        }

        [TestMethod]
        public async Task LogAsync_FillFactor75_TrimsToPreserveRecent()
        {
            var provider = CreateProvider(capacity: 4, fill: 75);
            for (int i = 1; i <= 5; i++)
                await provider.LogAsync(new TestLogMessage { Message = $"msg {i}" });

            var logs = GetLogs(provider);
            Assert.AreEqual(4, logs.Count);
            CollectionAssert.AreEqual(new[] { "msg 2", "msg 3", "msg 4", "msg 5" }, logs.Select(x => x.Message).ToArray());
        }

        [TestMethod]
        public async Task LogAsync_FillFactor100_AlwaysTrimsAtLeastOne()
        {
            var provider = CreateProvider(capacity: 3, fill: 100);
            for (int i = 1; i <= 4; i++)
                await provider.LogAsync(new TestLogMessage { Message = $"msg {i}" });

            var logs = GetLogs(provider);
            Assert.AreEqual(3, logs.Count);
            CollectionAssert.AreEqual(new[] { "msg 2", "msg 3", "msg 4" }, logs.Select(x => x.Message).ToArray());
        }
    }

    internal class TestTraceListener : TraceListener
    {
        private readonly Action<string> onWrite;
        public TestTraceListener(Action<string> onWrite) => this.onWrite = onWrite;
        public override void Write(string message) { }
        public override void WriteLine(string message) => onWrite?.Invoke(message);
    }

    internal class FailingTextWriter : TextWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
        public override void WriteLine(string value) => throw new IOException("Simulated failure");
    }

    [TestClass]
    public class ConsoleLogProviderTests
    {
        private ConsoleLogProvider CreateProvider(out StringBuilder output)
        {
            output = new StringBuilder();
            var writer = new StringWriter(output);
            Console.SetOut(writer); // Redirect console output

            var options = new ConsoleLogProviderOptions
            {
                LogOutputFormat = msg => $"[{msg.Category}] {msg.Message}"
            };

            return new ConsoleLogProvider(new Options<ConsoleLogProviderOptions>(options));
        }

        [TestMethod]
        public async Task LogAsync_WritesFormattedMessage_ToConsole()
        {
            var msg = new TestLogMessage { Category = TLogCategory.Info, Message = "Hello" };
            var provider = CreateProvider(out var output);

            await provider.LogAsync(msg);

            string result = output.ToString();
            Assert.IsTrue(result.Contains("[Info] Hello"), $"Output: {result}");
        }

        [TestMethod]
        public async Task LogAsync_WritesToTrace_WhenConsoleUnavailable()
        {
            var msg = new TestLogMessage { Category = TLogCategory.Error, Message = "Trace fallback" };
            var calledTrace = false;

            var options = new ConsoleLogProviderOptions
            {
                LogOutputWriter = new FailingTextWriter()
            };

            var provider = new ConsoleLogProvider(new Options<ConsoleLogProviderOptions>(options));

            Trace.Listeners.Clear();
            Trace.Listeners.Add(new TestTraceListener(s => {
                calledTrace = s == "WpfDevKit.Tests.Logging.TestLogMessage";
            }));

            await provider.LogAsync(msg);

            Assert.IsTrue(calledTrace, "Expected fallback to Trace logging.");
        }

        [TestMethod]
        public async Task LogAsync_Respects_CustomFormattedOutput()
        {
            var writer = new StringWriter();
            Console.SetOut(writer);

            var provider = new ConsoleLogProvider(new Options<ConsoleLogProviderOptions>(new ConsoleLogProviderOptions
            {
                LogOutputFormat = msg => $"CUSTOM >> {msg.Message}",
                LogOutputWriter = writer
            }));

            await provider.LogAsync(new TestLogMessage { Message = "XUnit" });

            var output = writer.ToString();
            Assert.IsTrue(output.Contains("CUSTOM >> XUnit"));
        }
    }

    [TestClass]
    public class DatabaseLogProviderTests1
    {
        private DatabaseLogProviderOptions GetValidOptions() => new DatabaseLogProviderOptions
        {
            ConnectionString = "Server=.;Database=TestDb;Trusted_Connection=True;",
            TableName = "Logs"
        };

        [TestMethod]
        public async Task LogAsync_Ignores_WhenOptionsInvalid()
        {
            var options = new DatabaseLogProviderOptions(); // Missing ConnectionString and TableName
            var provider = new DatabaseLogProvider(new Options<DatabaseLogProviderOptions>(options));
            await provider.LogAsync(new TestLogMessage { Message = "Should not log" });

            Assert.IsTrue(true); // Should not throw
        }

        [TestMethod]
        public async Task LogAsync_Handles_Exception_Gracefully()
        {
            var options = GetValidOptions();
            options.ConnectionString = "invalid"; // Triggers failure

            var provider = new DatabaseLogProvider(new Options<DatabaseLogProviderOptions>(options));
            await provider.LogAsync(new TestLogMessage { Message = "Should fail silently" });

            Assert.IsTrue(true); // Should not throw
        }

        [TestMethod]
        public void Constructor_SetsOptions_Correctly()
        {
            var options = GetValidOptions();
            var provider = new DatabaseLogProvider(new Options<DatabaseLogProviderOptions>(options));
            Assert.IsNotNull(provider);
        }
    }

    [TestClass]
    public class DatabaseLogProviderTests2
    {
        private DatabaseLogProvider CreateProvider(Action<DatabaseLogProviderOptions> configure)
        {
            var options = new DatabaseLogProviderOptions
            {
                ConnectionString = "Server=.;Database=Logs;Trusted_Connection=True;",
                TableName = "LogEntries"
            };
            configure?.Invoke(options);
            return new DatabaseLogProvider(new Options<DatabaseLogProviderOptions>(options));
        }

        [TestMethod]
        public void GenerateCommandText_BuildsInsertWithCorrectColumns()
        {
            var provider = CreateProvider(opt =>
            {
                opt.AddElement(TLogElement.Message, x => x.Message, "Msg", isNullable: false);
                opt.AddElement(TLogElement.Category, x => x.Category.ToString(), "Cat", isNullable: false);
                opt.AddElement(TLogElement.Timestamp, x => x.Timestamp, "Time", isNullable: false);
            });

            var field = typeof(DatabaseLogProvider).GetField("commandText", BindingFlags.NonPublic | BindingFlags.Instance);
            var sql = field.GetValue(provider) as string;

            Console.WriteLine("Generated SQL: " + sql);

            Assert.IsTrue(sql.Contains("INSERT INTO [LogEntries]"), "SQL should target correct table");
            Assert.IsTrue(sql.Contains("[Msg]"), "Should include Msg");
            Assert.IsTrue(sql.Contains("[Cat]"), "Should include Cat");
            Assert.IsTrue(sql.Contains("@Msg"), "Should include parameter");
        }

        [TestMethod]
        public async Task LogAsync_WithNoProviderFactory_DoesNotThrow()
        {
            var provider = CreateProvider(opt =>
            {
                opt.AddElement(TLogElement.Message, x => x.Message, "Message", isNullable: false);
            });

            var msg = new TestLogMessage { Message = "Hello", Category = TLogCategory.Info };

            await provider.LogAsync(msg); // Should not throw even without a factory
            Assert.IsTrue(true); // Pass if no exception thrown
        }

        [TestMethod]
        public void AddElement_AddsAndOverwritesCorrectly()
        {
            var options = new DatabaseLogProviderOptions();
            var field = typeof(DatabaseLogProvider).GetField("elements", BindingFlags.NonPublic | BindingFlags.Instance);
            var elements = field.GetValue(options) as ConcurrentDictionary<TLogElement, DatabaseLogColumn>;

            options.AddElement(TLogElement.ThreadId, x => x.ThreadId, "Thread", isNullable: true);
            Assert.AreEqual("Thread", elements[TLogElement.ThreadId].ColumnName);

            // Overwrite same key
            options.AddElement(TLogElement.ThreadId, x => x.ThreadId, "Thread_2", isNullable: true);
            Assert.AreEqual("Thread_2",elements[TLogElement.ThreadId].ColumnName);
        }

        [TestMethod]
        public void CommandText_BuildsEmpty_WhenNoColumnsAdded()
        {
            var provider = CreateProvider(opt =>
            {
                //opt.Elements.Clear(); // Remove default column definitions
            });

            var field = typeof(DatabaseLogProvider).GetField("commandText", BindingFlags.NonPublic | BindingFlags.Instance);
            var sql = field.GetValue(provider) as string;

            Assert.IsTrue(sql.Contains("INSERT INTO [LogEntries] () VALUES ()"), "Empty INSERT expected");
        }

        [TestMethod]
        public void AddElement_WithMaxLength_IsCaptured()
        {
            var options = new DatabaseLogProviderOptions();
            var field = typeof(DatabaseLogProvider).GetField("elements", BindingFlags.NonPublic | BindingFlags.Instance);
            var elements = field.GetValue(options) as ConcurrentDictionary<TLogElement, DatabaseLogColumn>;

            options.AddElement(TLogElement.UserName, x => x.UserName, "User", isNullable: true, maxLength: 50);
            var col = elements[TLogElement.UserName];

            Assert.AreEqual("User", col.ColumnName);
            Assert.AreEqual(50, col.MaxLength);
        }
    }

    [TestClass]
    public class LogBackgroundServiceTests
    {
        [TestMethod]
        public async Task Host_LogsMessageToMemoryProvider()
        {
            var builder = HostBuilder.CreateHostBuilder();

            using (var host = builder.Build())
            {
                await host.StartAsync();

                // Log a message through the ILogService
                var logService = host.Services.GetService<ILogService>();
                logService.Log(TLogCategory.Info, "Integration test message");

                // Allow background service time to process
                await Task.Delay(200);

                // Use reflection to inspect logged messages
                var memoryLogProvider = host.Services.GetService<MemoryLogProvider>();
                var logList = memoryLogProvider.GetLogs();

                lock (logList)
                {
                    Assert.AreEqual(1, logList.Count);
                    Assert.AreEqual("Integration test message", logList.First().Message);
                    Console.WriteLine($"Logged message = '{logList.First().Message}'");
                }

                await host.StopAsync();
            }
        }
    }
}
