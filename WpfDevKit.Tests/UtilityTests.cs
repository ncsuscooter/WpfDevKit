using System;
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
    }
}
