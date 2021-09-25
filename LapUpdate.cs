using ExtensionMethods;
using GameReaderCommon;
using SimHub.Plugins;
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

        public void updateSimhubProps(GameData data)
        {
            tyrePressures.Add(
                new LapUpdate.Wheels(
                    data.NewData.TyrePressureFrontLeft,
                    data.NewData.TyrePressureFrontRight,
                    data.NewData.TyrePressureRearLeft,
                    data.NewData.TyrePressureRearRight
                )
            );

            tyreTemperatures.Add(
                new LapUpdate.Wheels(
                    data.NewData.TyreTemperatureFrontLeft,
                    data.NewData.TyreTemperatureFrontRight,
                    data.NewData.TyreTemperatureRearLeft,
                    data.NewData.TyreTemperatureRearRight
                )
            );
        }

        public void writeSimhubProps(PluginManager pluginManager)
        {
            LapUpdate.Wheels averagePsi = averagePressures();
            pluginManager.updateProp(Properties.PSI.lastLapAverageFL, averagePsi.FL);
            pluginManager.updateProp(Properties.PSI.lastLapAverageFR, averagePsi.FR);
            pluginManager.updateProp(Properties.PSI.lastLapAverageRL, averagePsi.RL);
            pluginManager.updateProp(Properties.PSI.lastLapAverageRR, averagePsi.RR);

            LapUpdate.Wheels maxPsi = maxPressures();
            pluginManager.updateProp(Properties.PSI.lastLapMaxFL, maxPsi.FL);
            pluginManager.updateProp(Properties.PSI.lastLapMaxFR, maxPsi.FR);
            pluginManager.updateProp(Properties.PSI.lastLapMaxRL, maxPsi.RL);
            pluginManager.updateProp(Properties.PSI.lastLapMaxRR, maxPsi.RR);

            LapUpdate.Wheels averageTemp = averageTemps();
            pluginManager.updateProp(Properties.PSI.lastLapAverageFL, averageTemp.FL);
            pluginManager.updateProp(Properties.PSI.lastLapAverageFR, averageTemp.FR);
            pluginManager.updateProp(Properties.PSI.lastLapAverageRL, averageTemp.RL);
            pluginManager.updateProp(Properties.PSI.lastLapAverageRR, averageTemp.RR);

            LapUpdate.Wheels maxTemp = maxTemps();
            pluginManager.updateProp(Properties.PSI.lastLapMaxFL, maxTemp.FL);
            pluginManager.updateProp(Properties.PSI.lastLapMaxFR, maxTemp.FR);
            pluginManager.updateProp(Properties.PSI.lastLapMaxRL, maxTemp.RL);
            pluginManager.updateProp(Properties.PSI.lastLapMaxRR, maxTemp.RR);
        }
    }
}