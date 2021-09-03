using System.Collections.Generic;
using System.Linq;

namespace User.NearlyOnPace
{
    public class LapUpdate
    {   
        public struct Wheels
        {
            public double FR;
            public double FL;
            public double RL;
            public double RR;

            public Wheels(double frontRight, double frontLeft, double rearLeft, double rearRight)
            {
                FR = frontRight;
                FL = frontLeft;
                RL = rearLeft;
                RR = rearRight;
            }
        }

        public int lap;

        public List<Wheels> tyrePressures = new List<Wheels>();
        public List<Wheels> tyreTemperatures = new List<Wheels>();

        public LapUpdate(int lap)
        {
            this.lap = lap;
        }

        public Wheels maxPressures()
        {
            double flMax = tyrePressures.Select(wheel => wheel.FL).ToArray().Max();
            double frMax = tyrePressures.Select(wheel => wheel.FR).ToArray().Max();
            double rlMax = tyrePressures.Select(wheel => wheel.RL).ToArray().Max();
            double rrMax = tyrePressures.Select(wheel => wheel.RR).ToArray().Max();

            return new Wheels(flMax, frMax, rlMax, rrMax);
        }

        public Wheels averagePressures()
        {
            double flAvg = tyrePressures.Select(wheel => wheel.FL).ToArray().Average();
            double frAvg = tyrePressures.Select(wheel => wheel.FR).ToArray().Average();
            double rlAvg = tyrePressures.Select(wheel => wheel.RL).ToArray().Average();
            double rrAvg = tyrePressures.Select(wheel => wheel.RR).ToArray().Average();

            return new Wheels(flAvg, frAvg, rlAvg, rrAvg);
        }

        public Wheels maxTemps()
        {
            double flMax = tyreTemperatures.Select(wheel => wheel.FL).ToArray().Max();
            double frMax = tyreTemperatures.Select(wheel => wheel.FR).ToArray().Max();
            double rlMax = tyreTemperatures.Select(wheel => wheel.RL).ToArray().Max();
            double rrMax = tyreTemperatures.Select(wheel => wheel.RR).ToArray().Max();

            return new Wheels(flMax, frMax, rlMax, rrMax);
        }

        public Wheels averageTemps()
        {
            double flAvg = tyreTemperatures.Select(wheel => wheel.FL).ToArray().Average();
            double frAvg = tyreTemperatures.Select(wheel => wheel.FR).ToArray().Average();
            double rlAvg = tyreTemperatures.Select(wheel => wheel.RL).ToArray().Average();
            double rrAvg = tyreTemperatures.Select(wheel => wheel.RR).ToArray().Average();

            return new Wheels(flAvg, frAvg, rlAvg, rrAvg);
        }
    }
}