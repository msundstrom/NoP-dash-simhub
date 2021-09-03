using System;
using System.Runtime.InteropServices;

namespace User.NearlyOnPace
{
    public enum AC_FLAG_TYPE
    {
        AC_NO_FLAG = 0,
        AC_BLUE_FLAG = 1,
        AC_YELLOW_FLAG = 2,
        AC_BLACK_FLAG = 3,
        AC_WHITE_FLAG = 4,
        AC_CHECKERED_FLAG = 5,
        AC_PENALTY_FLAG = 6
    }

    public enum AC_STATUS
    {
        AC_OFF = 0,
        AC_REPLAY = 1,
        AC_LIVE = 2,
        AC_PAUSE = 3
    };

    public enum AC_SESSION_TYPE
    {
        AC_UNKNOWN = -1,
        AC_PRACTICE = 0,
        AC_QUALIFY = 1,
        AC_RACE = 2,
        AC_HOTLAP = 3,
        AC_TIME_ATTACK = 4,
        AC_DRIFT = 5,
        AC_DRAG = 6
    };
    
    public enum ACC_TRACK_GRIP_STATUS
    {
        ACC_GREEN = 0,
        ACC_FAST = 1,
        ACC_OPTIMUM = 2,
        ACC_GREASY = 3,
        ACC_DAMP = 4,
        ACC_WET = 5,
        ACC_FLOODED = 6
    };

    public enum ACC_RAIN_INTENSITY
    {
        ACC_NO_RAIN = 0,
        ACC_DRIZZLE = 1,
        ACC_LIGHT_RAIN = 2,
        ACC_MEDIUM_RAIN = 3,
        ACC_HEAVY_RAIN = 4,
        ACC_THUNDERSTORM = 5
    };

    public enum PenaltyShortcut
    {
        None,
        DriveThrough_Cutting,
        StopAndGo_10_Cutting,
        StopAndGo_20_Cutting,
        StopAndGo_30_Cutting,
        Disqualified_Cutting,
        RemoveBestLaptime_Cutting,

        DriveThrough_PitSpeeding,
        StopAndGo_10_PitSpeeding,
        StopAndGo_20_PitSpeeding,
        StopAndGo_30_PitSpeeding,
        Disqualified_PitSpeeding,
        RemoveBestLaptime_PitSpeeding,

        Disqualified_IgnoredMandatoryPit,

        PostRaceTime,
        Disqualified_Trolling,
        Disqualified_PitEntry,
        Disqualified_PitExit,
        Disqualified_WrongWay,

        DriveThrough_IgnoredDriverStint,
        Disqualified_IgnoredDriverStint,

        Disqualified_ExceededDriverStintLimit,
    };

    public class GraphicsEventArgs : EventArgs
    {
        public GraphicsEventArgs(Graphics graphics)
        {
            this.Graphics = graphics;
        }

        public Graphics Graphics { get; private set; }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Unicode)]
    [Serializable]
    public struct Graphics
    {
        public int PacketId;
        public AC_STATUS Status;
        public AC_SESSION_TYPE Session;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String CurrentTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String LastTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String BestTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String Split;

        public int completedLaps;
        public int position;
        public int iCurrentTime;
        public int iLastTime;
        public int iBestTime;
        public float sessionTimeLeft;
        public float distanceTraveled;
        public int isInPit;
        public int currentSectorIndex;
        public int lastSectorTime;
        public int numberOfLaps;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public String TyreCompound;
        public float replayTimeMultiplier;
        public float normalizedCarPosition;

        public int ActiveCars;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 180)]
        public float[] CarCoordinates;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public int[] CarId;
        public int playerCarID;
        public float penaltyTime;
        public AC_FLAG_TYPE flag;
        public int penalty;
        public int idealLineOn;
        public int isInPitLane;
        public float surfaceGrip;
        public int mandatoryPitDone;
        public float windSpeed;
        public float windDirection;
        public int isSetupMenuVisible;
        public int mainDisplayIndex;
        public int secondaryDisplayIndex;
        public int TC;
        public int TCCut;
        public int EngineMap;
        public int ABS;
        public int fuelXLap;
        public int rainLights;
        public int flashingLights;
        public int lightsStage;
        public float exhaustTemperature;
        public int wiperLV;
        public int DriverStintTotalTimeLeft;
        public int DriverStintTimeLeft;
        public int rainTyres;
        public int sessionIndex;
        public float usedFuel;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String deltaLapTime;
        public int iDeltaLapTime;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public String estimatedLapTime;
        public int iEstimatedLapTime;
        public int isDeltaPositive;
        public int iSplit;
        public int isValidLap;
        public float fuelEstimatedLaps;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 33)]
        public String trackStatus;
        public int missingMandatoryPits;
        public float Clock;
        public int directionLightsLeft;
        public int directionLightsRight;
        public int GlobalYellow;
        public int GlobalYellow1;
        public int GlobalYellow2;
        public int GlobalYellow3;
        public int GlobalWhite;
        public int GlobalGreen;
        public int GlobalChequered;
        public int GlobalRed;
        public int mfdTyreSet;
        public float mfdFuelToAdd;
        public float mfdTyrePressureLF;
        public float mfdTyrePressureRF;
        public float mfdTyrePressureLR;
        public float mfdTyrePressureRR;
        public ACC_TRACK_GRIP_STATUS trackGripStatus;
        public ACC_RAIN_INTENSITY rainIntensity;
        public ACC_RAIN_INTENSITY rainIntensityIn10min;
        public ACC_RAIN_INTENSITY rainIntensityIn30min;
        public int currentTyreSet;
        public int strategyTyreSet;
    }
}
