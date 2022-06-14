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
        public List<LapUpdate.Wheels> brakePadWear = new List<LapUpdate.Wheels>();
        public LapUpdate.Wheels lastBrakePadReading = new LapUpdate.Wheels();
        public LapUpdate.Wheels lastTyreWearReading = new LapUpdate.Wheels();
        public List<LapUpdate.Wheels> brakeDiscWear = new List<LapUpdate.Wheels>();
        public int stintOutlap;

        private double padCriticalValue = 12.5;

        public StintUpdate(GameData data)
        {
            lapTimes = new List<TimeSpan>();
            stintOutlap = data.NewData.CurrentLap;
        }

        public StintUpdate()
        {
            lapTimes = new List<TimeSpan>();
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

        public void updatePhysicsProps(Physics physics)
        {
            brakeDiscWear.Add(new LapUpdate.Wheels(
                physics.discLife[0],
                physics.discLife[1],
                physics.discLife[2],
                physics.discLife[3]
            ));

            if (brakeDiscWear.Count > 10)
            {
                brakeDiscWear.RemoveAt(0);
            }

            brakePadWear.Add(new LapUpdate.Wheels(
                physics.padLife[0],
                physics.padLife[1],
                physics.padLife[2],
                physics.padLife[3]
            ));

            if (brakePadWear.Count > 10)
            {
                brakePadWear.RemoveAt(0);
            }

            lastBrakePadReading = new LapUpdate.Wheels(
                physics.padLife[0],
                physics.padLife[1],
                physics.padLife[2],
                physics.padLife[3]
            );

            lastTyreWearReading = new LapUpdate.Wheels(
                physics.TyreWear[0],
                physics.TyreWear[1],
                physics.TyreWear[2],
                physics.TyreWear[3]
            );
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

            double padPredicatedLifeFL = (lastBrakePadReading.FL - padCriticalValue) / averagePadWear.FL * averageLapTimeMs();
            double padPredicatedLifeFR = (lastBrakePadReading.FR - padCriticalValue) / averagePadWear.FR * averageLapTimeMs();
            double padPredicatedLifeRL = (lastBrakePadReading.RL - padCriticalValue) / averagePadWear.RL * averageLapTimeMs();
            double padPredicatedLifeRR = (lastBrakePadReading.RR - padCriticalValue) / averagePadWear.RR * averageLapTimeMs();
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeFL, padPredicatedLifeFL);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeFR, padPredicatedLifeFR);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeRL, padPredicatedLifeRL);
            pluginManager.updateProp(Properties.Stint.brakePadPredictedLifeRR, padPredicatedLifeRR);

            pluginManager.updateProp(Properties.Stint.tyreWearFL, lastTyreWearReading.FL);
            pluginManager.updateProp(Properties.Stint.tyreWearFR, lastTyreWearReading.FR);
            pluginManager.updateProp(Properties.Stint.tyreWearRL, lastTyreWearReading.RL);
            pluginManager.updateProp(Properties.Stint.tyreWearRR, lastTyreWearReading.RR);
        }
    }
}
