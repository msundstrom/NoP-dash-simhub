using GameReaderCommon;
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

        public TimeSpan? averageLaptime()
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

        public void update(GameData data)
        {
            lapTimes.Add(data.NewData.LastLapTime);
        }
    }
}
