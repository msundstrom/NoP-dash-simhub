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

        private MemoryMappedFile graphicsMMF;
        private AC_MEMORY_STATUS memoryStatus = AC_MEMORY_STATUS.DISCONNECTED;

        private LapUpdate currentLapUpdate;
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
                if (!ConnectToSharedMemory())
                {
                    return;
                }
            }

            Graphics currentGraphics = readGraphics();

            // new lap
            if (data.OldData.CurrentLap < data.NewData.CurrentLap)
            {
                newLapUpdate(pluginManager, data);
            }

            // new stint
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
            currentStintUpdate = new StintUpdate(data);
            pluginManager.SetPropertyValue(Properties.Stint.lastOutlap, this.GetType(), currentStintUpdate.stintOutlap);
        }

        private void newLapUpdate(PluginManager pluginManager, GameData data)
        {
            LapUpdate.Wheels averagePsi = currentLapUpdate.averagePressures();
            pluginManager.SetPropertyValue(Properties.PSI.lastLapAverageFL, this.GetType(), averagePsi.FL);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapAverageFR, this.GetType(), averagePsi.FR);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapAverageRL, this.GetType(), averagePsi.RL);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapAverageRR, this.GetType(), averagePsi.RR);

            LapUpdate.Wheels maxPsi = currentLapUpdate.maxPressures();
            pluginManager.SetPropertyValue(Properties.PSI.lastLapMaxFL, this.GetType(), maxPsi.FL);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapMaxFR, this.GetType(), maxPsi.FR);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapMaxRL, this.GetType(), maxPsi.RL);
            pluginManager.SetPropertyValue(Properties.PSI.lastLapMaxRR, this.GetType(), maxPsi.RR);

            LapUpdate.Wheels averageTemp = currentLapUpdate.averageTemps();
            pluginManager.SetPropertyValue(Properties.Temp.lastLapAverageFL, this.GetType(), averageTemp.FL);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapAverageFR, this.GetType(), averageTemp.FR);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapAverageRL, this.GetType(), averageTemp.RL);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapAverageRR, this.GetType(), averageTemp.RR);

            LapUpdate.Wheels maxTemp = currentLapUpdate.maxTemps();
            pluginManager.SetPropertyValue(Properties.Temp.lastLapMaxFL, this.GetType(), maxTemp.FL);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapMaxFR, this.GetType(), maxTemp.FR);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapMaxRL, this.GetType(), maxTemp.RL);
            pluginManager.SetPropertyValue(Properties.Temp.lastLapMaxRR, this.GetType(), maxTemp.RR);

            currentLapUpdate = new LapUpdate(data.NewData.CurrentLap);

            currentStintUpdate.update(data);

            if (data.NewData.CurrentLap > currentStintUpdate.stintOutlap)
            {
                pluginManager.SetPropertyValue(Properties.Stint.stintAverageLapTime, this.GetType(), currentStintUpdate.formattedAverageLapTime());
                pluginManager.SetPropertyValue(Properties.Stint.stintAverageLapTimeMs, this.GetType(), currentStintUpdate.averageLapTimeMs());
            }
        }

        private void updateConstants(PluginManager pluginManager, Graphics currentGraphics)
        {
            pluginManager.SetPropertyValue(Properties.Misc.currentTyreSet, this.GetType(), currentGraphics.currentTyreSet);
            pluginManager.SetPropertyValue(Properties.Weather.rainIntensity, this.GetType(), currentGraphics.rainIntensity);
            pluginManager.SetPropertyValue(Properties.Weather.rainIntensityIn10min, this.GetType(), currentGraphics.rainIntensityIn10min);
            pluginManager.SetPropertyValue(Properties.Weather.rainIntensityIn30min, this.GetType(), currentGraphics.rainIntensityIn30min);
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
            pluginManager.AddProperty(Properties.Misc.currentTyreSet, this.GetType(), -1);

            pluginManager.AddProperty(Properties.PSI.lastLapAverageFL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapAverageFR, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapAverageRL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapAverageRR, this.GetType(), -1);

            pluginManager.AddProperty(Properties.PSI.lastLapMaxFL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapMaxFR, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapMaxRL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.PSI.lastLapMaxRR, this.GetType(), -1);

            pluginManager.AddProperty(Properties.Temp.lastLapAverageFL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapAverageFR, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapAverageRL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapAverageRR, this.GetType(), -1);

            pluginManager.AddProperty(Properties.Temp.lastLapMaxFL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapMaxFR, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapMaxRL, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Temp.lastLapMaxRR, this.GetType(), -1);

            pluginManager.AddProperty(Properties.Weather.rainIntensity, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Weather.rainIntensityIn10min, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Weather.rainIntensityIn30min, this.GetType(), -1);

            pluginManager.AddProperty(Properties.Stint.stintAverageLapTime, this.GetType(), "-");
            pluginManager.AddProperty(Properties.Stint.stintAverageLapTimeMs, this.GetType(), -1);
            pluginManager.AddProperty(Properties.Stint.lastOutlap, this.GetType(), -1);
        }
    }
}