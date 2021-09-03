using GameReaderCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Extensions
{   public static TimeSpan Average(this IEnumerable<TimeSpan> timeSpans)
    {
        IEnumerable<long> ticksPerTimeSpan = timeSpans.Select(t => t.Ticks);
        double averageTicks = ticksPerTimeSpan.Average();
        long averageTicksLong = Convert.ToInt64(averageTicks);

        TimeSpan averageTimeSpan = TimeSpan.FromTicks(averageTicksLong);

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
        public int stintOutlap;


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

        public TimeSpan averageLaptime()
        {
            return Extensions.Average(lapTimes);
        }

        public String formattedAverageLapTime()
        {
            if (lapTimes.Count <= 1)
            {
                return "-";
            }

            //exclude outlap
            List<TimeSpan> stintLaps = lapTimes.GetRange(1, lapTimes.Count - 2);
            TimeSpan averageTime = Extensions.Average(stintLaps);
            return averageTime.Minutes + ":" + averageTime.Seconds + "." + averageTime.Milliseconds;
        }

        public void update(GameData data)
        {
            lapTimes.Add(data.NewData.LastLapTime);
        }
    }
}
