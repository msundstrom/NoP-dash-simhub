using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
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


        [TestMethod]
        public void Test_AverageBrakeDiscWear()
        {
            List<LapUpdate.Wheels> testData = new List<LapUpdate.Wheels>();
            testData.Add(new LapUpdate.Wheels(29.5, 1, 1, 1));
            testData.Add(new LapUpdate.Wheels(28.3, 1, 1, 1)); // 1.2
            testData.Add(new LapUpdate.Wheels(26.8, 1, 1, 1)); // 1.5
            testData.Add(new LapUpdate.Wheels(24.1, 1, 1, 1)); // 2.7
            testData.Add(new LapUpdate.Wheels(21.3, 1, 1, 1)); // 2.8
            testData.Add(new LapUpdate.Wheels(20.5, 1, 1, 1)); // 0.8


            StintUpdate testStint = new StintUpdate();
            testStint.brakeDiscWear = testData;

            LapUpdate.Wheels average = testStint.averageBrakeDiscWear();

            Assert.AreEqual(average.FL, 1.8, 0.001);
        }
    }
}
