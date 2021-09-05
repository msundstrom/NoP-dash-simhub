using GameReaderCommon;
using SimHub.Plugins;
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace User.NearlyOnPace
{
    [PluginDescription("Nearly on Pace")]
    [PluginAuthor("msundstrom")]
    [PluginName("NearlyOnPace")]
    public class NearlyOnPace : IPlugin, IDataPlugin, IWPFSettings
    {
        enum AC_MEMORY_STATUS { DISCONNECTED, CONNECTING, CONNECTED }

        public struct Props
        {
#pragma warning disable CS0414
            public struct PSI
            {
                public static string lastLapMaxFL = "LastLapMaxPsiFL";
                public static string lastLapMaxFR = "LastLapMaxPsiFR";
                public static string lastLapMaxRL = "LastLapMaxPsiRL";
                public static string lastLapMaxRR = "LastLapMaxPsiRR";

                public static string lastLapAverageFL = "LastLapAveragePsiFL";
                public static string lastLapAverageFR = "LastLapAveragePsiFR";
                public static string lastLapAverageRL = "LastLapAveragePsiRL";
                public static string lastLapAverageRR = "LastLapAveragePsiRR";
            }
            public struct Temp
            {
                public static string lastLapMaxFL = "LastLapMaxTempFL";
                public static string lastLapMaxFR = "LastLapMaxTempFR";
                public static string lastLapMaxRL = "LastLapMaxTempRL";
                public static string lastLapMaxRR = "LastLapMaxTempRR";

                public static string lastLapAverageFL = "LastLapAverageTempFL";
                public static string lastLapAverageFR = "LastLapAverageTempFR";
                public static string lastLapAverageRL = "LastLapAverageTempRL";
                public static string lastLapAverageRR = "LastLapAverageTempRR";
            }

            public struct Weather
            {
                public static string rainIntensity = "RainIntensity";
                public static string rainIntensityIn10min = "RainIntensityIn10min";
                public static string rainIntensityIn30min = "RainIntensityIn30min";
            }

            public struct Stint
            {
                public static string stintAverageLapTime = "StintAverageLaptime";
                public static string stintAverageLapTimeMs = "StintAverageLaptimeMs";
            }

            public struct Misc
            {
                public static string lastOutlap = "LastOutlap";
                public static string currentTyreSet = "CurrentTyreSet";
            }
        }


        private MemoryMappedFile graphicsMMF;
        private AC_MEMORY_STATUS memoryStatus = AC_MEMORY_STATUS.DISCONNECTED;

        private LapUpdate currentLapUpdate;

        private StintUpdate lastStintUpdate;
        private StintUpdate currentStintUpdate;



        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        private bool ConnectToSharedMemory()
        {
            try
            {
                memoryStatus = AC_MEMORY_STATUS.CONNECTING;
                // Connect to shared memory
                graphicsMMF = MemoryMappedFile.OpenExisting("Local\\acpmf_graphics");

                memoryStatus = AC_MEMORY_STATUS.CONNECTED;
                return true;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        public Graphics readGraphics()
        {
            var size = Marshal.SizeOf(typeof(Graphics));
            using (var stream = graphicsMMF.CreateViewStream(0, size))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var bytes = reader.ReadBytes(size);
                    var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
                    var data = (Graphics)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Graphics));
                    handle.Free();
                    return data;
                }
            }
        }

        /// <summary>
        /// Called one time per game data update, contains all normalized game data, 
        /// raw data are intentionnally "hidden" under a generic object type (A plugin SHOULD NOT USE IT)
        /// 
        /// This method is on the critical path, it must execute as fast as possible and avoid throwing any error
        /// 
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <param name="data"></param>
        public void DataUpdate(PluginManager pluginManager, ref GameData data)
        {
            if (memoryStatus == AC_MEMORY_STATUS.DISCONNECTED || graphicsMMF == null || !data.GameRunning)
            {
                return;
            }

            Graphics currentGraphics = readGraphics();

            // new lap
            if (data.OldData.CurrentLap < data.NewData.CurrentLap)
            {
                newLapUpdate(pluginManager, data);
            }

            if (data.OldData.IsInPitLane == 1 && data.NewData.IsInPitLane == 0)
            {
                outlapUpdate(pluginManager, data);
            }

            // other, constant updates
            updateConstants(pluginManager, currentGraphics);
            updateAggregates(data);
        }

        private void outlapUpdate(PluginManager pluginManager, GameData data)
        {
            lastStintUpdate = currentStintUpdate;
            currentStintUpdate = new StintUpdate(data);
            pluginManager.SetPropertyValue(Props.Misc.lastOutlap, this.GetType(), currentStintUpdate.stintOutlap);
        }

        private void newLapUpdate(PluginManager pluginManager, GameData data)
        {
            LapUpdate.Wheels averagePsi = currentLapUpdate.averagePressures();
            pluginManager.SetPropertyValue(Props.PSI.lastLapAverageFL, this.GetType(), averagePsi.FL);
            pluginManager.SetPropertyValue(Props.PSI.lastLapAverageFR, this.GetType(), averagePsi.FR);
            pluginManager.SetPropertyValue(Props.PSI.lastLapAverageRL, this.GetType(), averagePsi.RL);
            pluginManager.SetPropertyValue(Props.PSI.lastLapAverageRR, this.GetType(), averagePsi.RR);

            LapUpdate.Wheels maxPsi = currentLapUpdate.maxPressures();
            pluginManager.SetPropertyValue(Props.PSI.lastLapMaxFL, this.GetType(), maxPsi.FL);
            pluginManager.SetPropertyValue(Props.PSI.lastLapMaxFR, this.GetType(), maxPsi.FR);
            pluginManager.SetPropertyValue(Props.PSI.lastLapMaxRL, this.GetType(), maxPsi.RL);
            pluginManager.SetPropertyValue(Props.PSI.lastLapMaxRR, this.GetType(), maxPsi.RR);

            LapUpdate.Wheels averageTemp = currentLapUpdate.averageTemps();
            pluginManager.SetPropertyValue(Props.Temp.lastLapAverageFL, this.GetType(), averageTemp.FL);
            pluginManager.SetPropertyValue(Props.Temp.lastLapAverageFR, this.GetType(), averageTemp.FR);
            pluginManager.SetPropertyValue(Props.Temp.lastLapAverageRL, this.GetType(), averageTemp.RL);
            pluginManager.SetPropertyValue(Props.Temp.lastLapAverageRR, this.GetType(), averageTemp.RR);

            LapUpdate.Wheels maxTemp = currentLapUpdate.maxTemps();
            pluginManager.SetPropertyValue(Props.Temp.lastLapMaxFL, this.GetType(), maxTemp.FL);
            pluginManager.SetPropertyValue(Props.Temp.lastLapMaxFR, this.GetType(), maxTemp.FR);
            pluginManager.SetPropertyValue(Props.Temp.lastLapMaxRL, this.GetType(), maxTemp.RL);
            pluginManager.SetPropertyValue(Props.Temp.lastLapMaxRR, this.GetType(), maxTemp.RR);

            currentLapUpdate = new LapUpdate(data.NewData.CurrentLap);

            currentStintUpdate.update(data);

            if (data.NewData.CurrentLap > currentStintUpdate.stintOutlap)
            {
                pluginManager.SetPropertyValue(Props.Stint.stintAverageLapTime, this.GetType(), currentStintUpdate.formattedAverageLapTime());
                pluginManager.SetPropertyValue(Props.Stint.stintAverageLapTimeMs, this.GetType(), currentStintUpdate.averageLapTimeMs());
            }
        }

        private void updateConstants(PluginManager pluginManager, Graphics currentGraphics)
        {
            pluginManager.SetPropertyValue(Props.Misc.currentTyreSet, this.GetType(), currentGraphics.currentTyreSet);
            pluginManager.SetPropertyValue(Props.Weather.rainIntensity, this.GetType(), currentGraphics.rainIntensity);
            pluginManager.SetPropertyValue(Props.Weather.rainIntensityIn10min, this.GetType(), currentGraphics.rainIntensityIn10min);
            pluginManager.SetPropertyValue(Props.Weather.rainIntensityIn30min, this.GetType(), currentGraphics.rainIntensityIn30min);
        }

        private void updateAggregates(GameData data)
        {
            if (currentLapUpdate != null)
            {
                // update aggregated data
                currentLapUpdate.tyrePressures.Add(
                    new LapUpdate.Wheels(
                        data.NewData.TyrePressureFrontLeft,
                        data.NewData.TyrePressureFrontRight,
                        data.NewData.TyrePressureRearLeft,
                        data.NewData.TyrePressureRearRight
                    )
                );

                currentLapUpdate.tyreTemperatures.Add(
                    new LapUpdate.Wheels(
                        data.NewData.TyreTemperatureFrontLeft,
                        data.NewData.TyreTemperatureFrontRight,
                        data.NewData.TyreTemperatureRearLeft,
                        data.NewData.TyreTemperatureRearRight
                    )
                );
            }
        }

        /// <summary>
        /// Called at plugin manager stop, close/dispose anything needed here ! 
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void End(PluginManager pluginManager)
        {
        }

        /// <summary>
        /// Returns the settings control, return null if no settings control is required
        /// </summary>
        /// <param name="pluginManager"></param>
        /// <returns></returns>
        public System.Windows.Controls.Control GetWPFSettingsControl(PluginManager pluginManager)
        {
            return null;
        }

        /// <summary>
        /// Called once after plugins startup
        /// Plugins are rebuilt at game change
        /// </summary>
        /// <param name="pluginManager"></param>
        public void Init(PluginManager pluginManager)
        {

            SimHub.Logging.Current.Info("Starting plugin");

            currentLapUpdate = new LapUpdate(1);
            currentStintUpdate = new StintUpdate();

            ConnectToSharedMemory();

            registerProps(pluginManager);
        }

        public void registerProps(PluginManager pluginManager)
        {
            pluginManager.AddProperty(Props.Misc.currentTyreSet, this.GetType(), -1);

            pluginManager.AddProperty(Props.PSI.lastLapAverageFL, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapAverageFR, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapAverageRL, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapAverageRR, this.GetType(), -1);

            pluginManager.AddProperty(Props.PSI.lastLapMaxFL, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapMaxFR, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapMaxRL, this.GetType(), -1);
            pluginManager.AddProperty(Props.PSI.lastLapMaxRR, this.GetType(), -1);

            pluginManager.AddProperty(Props.Temp.lastLapAverageFL, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapAverageFR, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapAverageRL, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapAverageRR, this.GetType(), -1);

            pluginManager.AddProperty(Props.Temp.lastLapMaxFL, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapMaxFR, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapMaxRL, this.GetType(), -1);
            pluginManager.AddProperty(Props.Temp.lastLapMaxRR, this.GetType(), -1);

            pluginManager.AddProperty(Props.Weather.rainIntensity, this.GetType(), -1);
            pluginManager.AddProperty(Props.Weather.rainIntensityIn10min, this.GetType(), -1);
            pluginManager.AddProperty(Props.Weather.rainIntensityIn30min, this.GetType(), -1);

            pluginManager.AddProperty(Props.Stint.stintAverageLapTime, this.GetType(), "-");
            pluginManager.AddProperty(Props.Stint.stintAverageLapTimeMs, this.GetType(), -1);
        }
    }
}