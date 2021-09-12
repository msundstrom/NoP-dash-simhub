using Microsoft.VisualStudio.TestTools.UnitTesting;
using User.NearlyOnPace;

namespace StintUnitTests
{
    [TestClass]
    public class StintTests
    {
        [TestMethod]
        public void Test_IsDisregardingOutlap()
        {
            StintUpdate testStint = new StintUpdate();
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 2));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1));

            Assert.AreEqual(testStint.averageLapTimeMs(), 1000);
        }

        [TestMethod]
        public void Test_FormattingOfAverageLapTime()
        {
            StintUpdate testStint = new StintUpdate();
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1, 43, 482));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1, 42, 589));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1, 43, 109));
            testStint.lapTimes.Add(new System.TimeSpan(0, 0, 1, 42, 943));

            Assert.AreEqual("01:42.880", testStint.formattedAverageLapTime());
        }
    }
}
