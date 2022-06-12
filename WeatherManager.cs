using System;

namespace User.NearlyOnPace {
    class WeatherManager {
        public float currentWeatherChangeTimestamp = 0;
        public float in10minWeatherChangeTimestamp = 0;
        public float in30minWeatherChangeTimestamp = 0;

        private ACC_RAIN_INTENSITY lastCurrentRainIntensity = ACC_RAIN_INTENSITY.ACC_NO_RAIN;
        private ACC_RAIN_INTENSITY lastRainIntensityIn10Min = ACC_RAIN_INTENSITY.ACC_NO_RAIN;
        private ACC_RAIN_INTENSITY lastRainIntensityIn30Min = ACC_RAIN_INTENSITY.ACC_NO_RAIN;

        public bool updateTimestamps(Graphics currentGraphics) {
            bool didUpdate = false;
            if (currentGraphics.rainIntensity != lastCurrentRainIntensity) {
                lastCurrentRainIntensity = currentGraphics.rainIntensity;
                currentWeatherChangeTimestamp = currentGraphics.Clock;
                didUpdate = true;
            }

            if (currentGraphics.rainIntensityIn10min != lastRainIntensityIn10Min) {
                lastRainIntensityIn10Min = currentGraphics.rainIntensityIn10min;
                in10minWeatherChangeTimestamp = currentGraphics.Clock;
                didUpdate = true;
            }

            if (currentGraphics.rainIntensityIn30min != lastRainIntensityIn30Min) {
                lastRainIntensityIn30Min = currentGraphics.rainIntensityIn30min;
                in30minWeatherChangeTimestamp = currentGraphics.Clock;
                didUpdate = true;
            }

            return didUpdate;
        }
    }
}
