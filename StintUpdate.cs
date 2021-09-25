using ExtensionMethods;
using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{   public static TimeSpan Average(this IEnumerable<TimeSpan> timeSpans)
    {
        IEnumerable<double> ticksPerTimeSpan = timeSpans.Select(t => t.TotalMilliseconds);
        double averageMs = ticksPerTimeSpan.Average();

        TimeSpan averageTimeSpan = TimeSpan.FromMilliseconds(averageMs);

        return averageTimeSpan;
    }
}


namespace User.NearlyOnPace
{
    public class StintUpdate
    {
        public List<TimeSpan> lapTimes = new List<TimeSpan>();
        public TimeSpan stintStartTime;
        public TimeSpan stintEndTime = new TimeSpan();
        public List<LapUpdate.Wheels> brakePadWear = new List<LapUpdate.Wheels>();
        public List<LapUpdate.Wheels> brakeDiscWear = new List<LapUpdate.Wheels>();
        public int stintOutlap;

        private double startingPadValue = 29.0;
        private double padCriticalValue = 10.0;
        private double startinDiscValue = 32.0;

        public StintUpdate(GameData data)
        {
            lapTimes = new List<TimeSpan>();
            stintStartTime = data.NewData.SessionTimeLeft;
            stintEndTime = new TimeSpan();
            stintOutlap = data.NewData.CurrentLap;
        }

        public StintUpdate()
        {
            lapTimes = new List<TimeSpan>();
            stintStartTime = new TimeSpan();
            stintEndTime = new TimeSpan();
            stintOutlap = 1;
        }

        private TimeSpan? averageLaptime()
        {
            if (lapTimes.Count < 1)
            {
                return null;
            }

            //exclude outlap
            List<TimeSpan> stintLaps = lapTimes.GetRange(1, lapTimes.Count - 1);
            TimeSpan averageTime = Extensions.Average(stintLaps);

            return averageTime;
        }

        public String formattedAverageLapTime()
        {
            if (averageLaptime() == null)
            {
                return "-";
            }

            TimeSpan averageTime = (TimeSpan)averageLaptime();
            return  averageTime.Minutes.ToString("D2") + 
                    ":" + averageTime.Seconds.ToString("D2") + 
                    "." + averageTime.Milliseconds.ToString("D3");
        }

        public int averageLapTimeMs()
        {
            if (averageLaptime() == null)
            {
                return -1;
            }

            TimeSpan averageTime = (TimeSpan)averageLaptime();

            return (int)Math.Floor(averageTime.TotalMilliseconds);
        }

        public LapUpdate.Wheels averageBrakeDiscWear()
        {
            LapUpdate.Wheels totalWear = new LapUpdate.Wheels();

            int snapshotCount = brakeDiscWear.Count() - 1;

            for (int i = 0; i < snapshotCount; i++)
            {
                totalWear.FL += brakeDiscWear[i].FL - brakeDiscWear[i + 1].FL;
                totalWear.FR += brakeDiscWear[i].FR - brakeDiscWear[i + 1].FR;
                totalWear.RL += brakeDiscWear[i].RL - brakeDiscWear[i + 1].RL;
                totalWear.RR += brakeDiscWear[i].RR - brakeDiscWear[i + 1].RR;
            }

            return new LapUpdate.Wheels(
                totalWear.FL / snapshotCount,
                totalWear.FR / snapshotCount,
                totalWear.RL / snapshotCount,
                totalWear.RR / snapshotCount
                );
        }

        public LapUpdate.Wheels averageBrakePadWear()
        {
            LapUpdate.Wheels totalWear = new LapUpdate.Wheels();

            int snapshotCount = brakePadWear.Count() - 1;

            for (int i = 0; i < snapshotCount; i++)
            {
                totalWear.FL += brakePadWear[i].FL - brakePadWear[i + 1].FL;
                totalWear.FR += brakePadWear[i].FR - brakePadWear[i + 1].FR;
                totalWear.RL += brakePadWear[i].RL - brakePadWear[i + 1].RL;
                totalWear.RR += brakePadWear[i].RR - brakePadWear[i + 1].RR;
            }

            return new LapUpdate.Wheels(
                totalWear.FL / snapshotCount,
                totalWear.FR / snapshotCount,
                totalWear.RL / snapshotCount,
                totalWear.RR / snapshotCount
                );
        }

        public void updateSimhubProps(GameData data)
        {
            lapTimes.Add(data.NewData.LastLapTime);
        }

        public void updatePhysicsProps(Physics graphics)
        {
            brakeDiscWear.Add(new LapUpdate.Wheels(
                graphics.discLife[0],
                graphics.discLife[1],
                graphics.discLife[2],
                graphics.discLife[3]
            ));

            if (brakeDiscWear.Count > 10)
            {
                brakeDiscWear.RemoveAt(0);
            }

            brakePadWear.Add(new LapUpdate.Wheels(
                graphics.padLife[0],
                graphics.padLife[1],
                graphics.padLife[2],
                graphics.padLife[3]
            ));

            if (brakePadWear.Count > 10)
            {
                brakePadWear.RemoveAt(0);
            }
        }

        public void writeSimhubProps(PluginManager pluginManager)
        {
            pluginManager.updateProp(Properties.Stint.stintAverageLapTime, formattedAverageLapTime());
            pluginManager.updateProp(Properties.Stint.stintAverageLapTimeMs, averageLapTimeMs());

            LapUpdate.Wheels averageDiscWear = averageBrakeDiscWear();
            pluginManager.updateProp(Properties.Stint.brakeDiscAverageWearFL, averageDiscWear.FL);
            pluginManager.updateProp(Properties.Stint.brakeDiscAverageWearFR, averageDiscWear.FR);
            pluginManager.updateProp(Properties.Stint.brakeDiscAverageWearRL, averageDiscWear.RL);
            pluginManager.updateProp(Properties.Stint.brakeDiscAverageWearRR, averageDiscWear.RR);

            LapUpdate.Wheels averagePadWear = averageBrakePadWear();
            pluginManager.updateProp(Properties.Stint.brakePadAverageWearFL, averagePadWear.FL);
            pluginManager.updateProp(Properties.Stint.brakePadAverageWearFR, averagePadWear.FR);
            pluginManager.updateProp(Properties.Stint.brakePadAverageWearRL, averagePadWear.RL);
            pluginManager.updateProp(Properties.Stint.brakePadAverageWearRR, averagePadWear.RR);

            pluginManager.updateProp(Properties.Stint.brakeWearLapCount, brakeDiscWear.Count);

            double padPredicatedLifeFL = (averagePadWear.FL - padCriticalValue) * averagePadWear.FL * averageLapTimeMs();
            double padPredicatedLifeFR = (averagePadWear.FR - padCriticalValue) * averagePadWear.FR * averageLapTimeMs();
            double padPredicatedLifeRL = (averagePadWear.RL - padCriticalValue) * averagePadWear.RL * averageLapTimeMs();
            double padPredicatedLifeRR = (averagePadWear.RR - padCriticalValue) * averagePadWear.RR * averageLapTimeMs();
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeFL, padPredicatedLifeFL);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeFR, padPredicatedLifeFR);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeRL, padPredicatedLifeRL);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeRR, padPredicatedLifeRR);

        }
    }
}
