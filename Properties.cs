using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.NearlyOnPace
{

    class Properties
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
            public static string trackCondition = "TrackCondition";
            public static string rainIntensity = "RainIntensity";
            public static string rainIntensityIn10min = "RainIntensityIn10min";
            public static string rainIntensityIn30min = "RainIntensityIn30min";
        }

        public struct Stint
        {
            public static string stintAverageLapTime = "StintAverageLaptime";
            public static string stintAverageLapTimeMs = "StintAverageLaptimeMs";
            public static string lastOutlap = "LastOutlap";

            public static string brakePadAverageWearFL = "BrakePadAverageWearFL";
            public static string brakePadAverageWearFR = "BrakePadAverageWearFR";
            public static string brakePadAverageWearRL = "BrakePadAverageWearRL";
            public static string brakePadAverageWearRR = "BrakePadAverageWearRR";

            public static string brakeDiscAverageWearFL = "BrakeDiscAverageWearFL";
            public static string brakeDiscAverageWearFR = "BrakeDiscAverageWearFR";
            public static string brakeDiscAverageWearRL = "BrakeDiscAverageWearRL";
            public static string brakeDiscAverageWearRR = "BrakeDiscAverageWearRR";

            public static string brakeWearLapCount = "BrakeWearLapCount";
            public static string brakePadPredictedLifeFL = "BrakePadPredicatedLifeFL";
            public static string brakePadPredictedLifeFR = "BrakePadPredicatedLifeFR";
            public static string brakePadPredictedLifeRL = "BrakePadPredicatedLifeRL";
            public static string brakePadPredictedLifeRR = "BrakePadPredicatedLifeRR";
        }

        public struct Misc
        {
            public static string currentTyreSet = "CurrentTyreSet";
            public static string frontBrakePad = "FrontBrakePad";
            public static string rearBrakePad = "RearBrakePad";
        }
    }
}
