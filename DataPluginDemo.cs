using GameReaderCommon;
using SimHub.Plugins;
using System;
using ExtensionMethods;
using static User.NearlyOnPace.SharedMemoryReader;

namespace ExtensionMethods
{
    public static class MyExtensions
    {
        public static void updateProp(this PluginManager manager, string propName, int propValue)
        {
            manager.SetPropertyValue(propName, typeof(User.NearlyOnPace.NearlyOnPace), propValue);
        }
        public static void updateProp(this PluginManager manager, string propName, double propValue)
        {
            manager.SetPropertyValue(propName, typeof(User.NearlyOnPace.NearlyOnPace), propValue);
        }

        public static void updateProp(this PluginManager manager, string propName, string propValue)
        {
            manager.SetPropertyValue(propName, typeof(User.NearlyOnPace.NearlyOnPace), propValue);
        }

        public static void addProp(this PluginManager manager, string propName, int defaultValue)
        {
            manager.AddProperty(propName, typeof(User.NearlyOnPace.NearlyOnPace), defaultValue);
        }
        public static void addProp(this PluginManager manager, string propName, double defaultValue)
        {
            manager.AddProperty(propName, typeof(User.NearlyOnPace.NearlyOnPace), defaultValue);
        }
        public static void addProp(this PluginManager manager, string propName, string defaultValue)
        {
            manager.AddProperty(propName, typeof(User.NearlyOnPace.NearlyOnPace), defaultValue);
        }
    }
}

namespace User.NearlyOnPace
{
    [PluginDescription("Nearly on Pace")]
    [PluginAuthor("msundstrom")]
    [PluginName("NearlyOnPace")]
    public class NearlyOnPace : IPlugin, IDataPlugin, IWPFSettings
    {
        

        private LapUpdate currentLapUpdate;
        private StintUpdate currentStintUpdate;

        /// <summary>
        /// Instance of the current plugin manager
        /// </summary>
        public PluginManager PluginManager { get; set; }

        private SharedMemoryReader memoryReader;

        

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
            if (!data.GameRunning)
            {
                return;
            }

            Graphics? currentGraphics = memoryReader.readGraphics();

            if (currentGraphics == null)
            {
                return;
            }

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
            updateConstants(pluginManager, (Graphics)currentGraphics);
            updateAggregates(data);
        }

        private void outlapUpdate(PluginManager pluginManager, GameData data)
        {
            currentStintUpdate = new StintUpdate(data);
            pluginManager.SetPropertyValue(Properties.Stint.lastOutlap, this.GetType(), currentStintUpdate.stintOutlap);
        }

        private void newLapUpdate(PluginManager pluginManager, GameData data)
        {
            currentLapUpdate.updateSimhubProps(pluginManager);
            
            currentLapUpdate = new LapUpdate(data.NewData.CurrentLap);

            currentStintUpdate.update(data);

            if (data.NewData.CurrentLap > currentStintUpdate.stintOutlap)
            {
                currentStintUpdate.updateSimhubProps(pluginManager);
            }
        }

        private void updateConstants(PluginManager pluginManager, Graphics currentGraphics)
        {
            pluginManager.updateProp(Properties.Misc.currentTyreSet, currentGraphics.currentTyreSet);
            pluginManager.updateProp(Properties.Weather.rainIntensity, (int)currentGraphics.rainIntensity);
            pluginManager.updateProp(Properties.Weather.rainIntensityIn10min, (int)currentGraphics.rainIntensityIn10min);
            pluginManager.updateProp(Properties.Weather.rainIntensityIn30min, (int)currentGraphics.rainIntensityIn30min);
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
            memoryReader = new SharedMemoryReader();

            registerProps(pluginManager);
        }

        private void registerProps(PluginManager pluginManager)
        {

            pluginManager.addProp(Properties.Misc.currentTyreSet, -1);

            pluginManager.addProp(Properties.PSI.lastLapAverageFL, -1);
            pluginManager.addProp(Properties.PSI.lastLapAverageFR, -1);
            pluginManager.addProp(Properties.PSI.lastLapAverageRL, -1);
            pluginManager.addProp(Properties.PSI.lastLapAverageRR, -1);

            pluginManager.addProp(Properties.PSI.lastLapMaxFL, -1);
            pluginManager.addProp(Properties.PSI.lastLapMaxFR, -1);
            pluginManager.addProp(Properties.PSI.lastLapMaxRL, -1);
            pluginManager.addProp(Properties.PSI.lastLapMaxRR, -1);

            pluginManager.addProp(Properties.Temp.lastLapAverageFL, -1);
            pluginManager.addProp(Properties.Temp.lastLapAverageFR, -1);
            pluginManager.addProp(Properties.Temp.lastLapAverageRL, -1);
            pluginManager.addProp(Properties.Temp.lastLapAverageRR, -1);

            pluginManager.addProp(Properties.Temp.lastLapMaxFL, -1);
            pluginManager.addProp(Properties.Temp.lastLapMaxFR, -1);
            pluginManager.addProp(Properties.Temp.lastLapMaxRL, -1);
            pluginManager.addProp(Properties.Temp.lastLapMaxRR, -1);

            pluginManager.addProp(Properties.Weather.rainIntensity, -1);
            pluginManager.addProp(Properties.Weather.rainIntensityIn10min, -1);
            pluginManager.addProp(Properties.Weather.rainIntensityIn30min, -1);

            pluginManager.addProp(Properties.Stint.stintAverageLapTime, "-");
            pluginManager.addProp(Properties.Stint.stintAverageLapTimeMs, -1);
            pluginManager.addProp(Properties.Stint.lastOutlap, -1);
        }
    }
}