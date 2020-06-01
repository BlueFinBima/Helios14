//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.AlliedForces
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct FlightData
    {
        public float x;            // Ownship North (Ft)
        public float y;            // Ownship East (Ft)
        public float z;            // Ownship Down (Ft)
        public float xDot;         // Ownship North Rate (ft/sec)
        public float yDot;         // Ownship East Rate (ft/sec)
        public float zDot;         // Ownship Down Rate (ft/sec)
        public float alpha;        // Ownship AOA (Degrees)
        public float beta;         // Ownship Beta (Degrees)
        public float gamma;        // Ownship Gamma (Radians)
        public float pitch;        // Ownship Pitch (Radians)
        public float roll;         // Ownship Pitch (Radians)
        public float yaw;          // Ownship Pitch (Radians)
        public float mach;         // Ownship Mach number
        public float kias;         // Ownship Indicated Airspeed (Knots)
        public float vt;           // Ownship True Airspeed (Ft/Sec)
        public float gs;           // Ownship Normal Gs
        public float windOffset;   // Wind delta to FPM (Radians)
        public float nozzlePos;    // Ownship engine nozzle percent open (0-100)
        public float internalFuel; // Ownship internal fuel (Lbs)
        public float externalFuel; // Ownship external fuel (Lbs)
        public float fuelFlow;     // Ownship fuel flow (Lbs/Hour)
        public float rpm;          // Ownship engine rpm (Percent 0-103)
        public float ftit;         // Ownship Forward Turbine Inlet Temp (Degrees C)
        public float gearPos;      // Ownship Gear position 0 = up, 1 = down;
        public float speedBrake;   // Ownship speed brake position 0 = closed, 1 = 60 Degrees open
        public float epuFuel;      // Ownship EPU fuel (Percent 0-100)
        public float oilPressure;  // Ownship Oil Pressure (Percent 0-100)
        public LightBits lightBits;    // Cockpit Indicator Lights, one bit per bulb. See enum

        // These are inputs. Use them carefully
        // NB: these do not work when TrackIR device is enabled
        public float headPitch;    // Head pitch offset from design eye (radians)
        public float headRoll;     // Head roll offset from design eye (radians)
        public float headYaw;      // Head yaw offset from design eye (radians)

        // new lights
        public LightBits2 lightBits2;   // Cockpit Indicator Lights, one bit per bulb. See enum
        public LightBits3 lightBits3;   // Cockpit Indicator Lights, one bit per bulb. See enum

        // chaff/flare
        public float ChaffCount;   // Number of Chaff left
        public float FlareCount;   // Number of Flare left

        // landing gear
        public float NoseGearPos;  // Position of the nose landinggear; caution: full down values defined in dat files
        public float LeftGearPos;  // Position of the left landinggear; caution: full down values defined in dat files
        public float RightGearPos; // Position of the right landinggear; caution: full down values defined in dat files

        // ADI values
        public float AdiIlsHorPos; // Position of horizontal ILS bar
        public float AdiIlsVerPos; // Position of vertical ILS bar

        // HSI states
        public int courseState;    // HSI_STA_CRS_STATE
        public int headingState;   // HSI_STA_HDG_STATE
        public int totalStates;    // HSI_STA_TOTAL_STATES; never set

        // HSI values
        public float courseDeviation;  // HSI_VAL_CRS_DEVIATION
        public float desiredCourse;    // HSI_VAL_DESIRED_CRS *
        public float distanceToBeacon;    // HSI_VAL_DISTANCE_TO_BEACON *
        public float bearingToBeacon;  // HSI_VAL_BEARING_TO_BEACON *
        public float currentHeading;      // HSI_VAL_CURRENT_HEADING *
        public float desiredHeading;   // HSI_VAL_DESIRED_HEADING *
        public float deviationLimit;      // HSI_VAL_DEV_LIMIT
        public float halfDeviationLimit;  // HSI_VAL_HALF_DEV_LIMIT
        public float localizerCourse;     // HSI_VAL_LOCALIZER_CRS
        public float airbaseX;            // HSI_VAL_AIRBASE_X
        public float airbaseY;            // HSI_VAL_AIRBASE_Y
        public float totalValues;         // HSI_VAL_TOTAL_VALUES; never set

        public float TrimPitch;  // Value of trim in pitch axis, -0.5 to +0.5
        public float TrimRoll;   // Value of trim in roll axis, -0.5 to +0.5
        public float TrimYaw;    // Value of trim in yaw axis, -0.5 to +0.5

        // HSI flags
        public HsiBits hsiBits;      // HSI flags

        //DED Lines
        public TextLines DED;
        public TextLines DEDInverse;

        //PFL Lines
        public TextLines PFL;
        public TextLines PFLInverse;

        //TacanChannel
        public int UFCTChan;
        public int AUXTChan;

        // RWR
        public int RwrObjectCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public int[] RWRsymbol;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public float[] bearing;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public uint[] missileActivity;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public uint[] missileLaunch;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public uint[] selected;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public float[] lethality;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public uint[] newDetection;

        //fuel values
        public float fwd;
        public float aft;
        public float total;

        public int VersionNum;
        public float headX;        // Head X offset from design eye (feet)
        public float headY;        // Head Y offset from design eye (feet)
        public float headZ;        // Head Z offset from design eye (feet)
        public int MainPower;
    }
}
