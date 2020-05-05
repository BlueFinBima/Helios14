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

using System;
using System.Runtime.InteropServices;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.BMS
{
    enum TacanSources : int
    {
        UFC = 0,
        AUX = 1,
        NUMBER_OF_SOURCES = 2
    }

    [Flags]
    enum TacanBits : byte
    {
        band = 0x01,   // true in this bit position if band is X
        mode = 0x02    // true in this bit position if domain is air to air
    }

    [Flags]
    public enum AltBits : uint
    {
        CalType = 0x01, // true if calibration in inches of Mercury (Hg), false if in hectoPascal (hPa)
        PneuFlag = 0x02	// true if PNEU flag is visible
    }

    [Flags]
    public enum PowerBits : uint
    {
        BusPowerBattery = 0x01, // true if at least the battery bus is powered
        BusPowerEmergency = 0x02,   // true if at least the emergency bus is powered
        BusPowerEssential = 0x04,   // true if at least the essential bus is powered
        BusPowerNonEssential = 0x08,    // true if at least the non-essential bus is powered
        MainGenerator = 0x10,   // true if the main generator is online
        StandbyGenerator = 0x20,    // true if the standby generator is online
        JetFuelStarter = 0x40	// true if JFS is running, can be used for magswitch
    }

    [Flags]
    public enum BlinkBits : uint
    {
        // currently working
        OuterMarker = 0x01, // defined in HsiBits    - slow flashing for outer marker
        MiddleMarker = 0x02,    // defined in HsiBits    - fast flashing for middle marker
        PROBEHEAT = 0x04,   // defined in LightBits2 - probeheat system is tested
        AuxSrch = 0x08, // defined in LightBits2 - search function in NOT activated and a search radar is painting ownship
        Launch = 0x10,  // defined in LightBits2 - missile is fired at ownship
        PriMode = 0x20, // defined in LightBits2 - priority mode is enabled but more than 5 threat emitters are detected
        Unk = 0x40, // defined in LightBits2 - unknown is not active but EWS detects unknown radar

        // not working yet, defined for future use
        Elec_Fault = 0x80,  // defined in LightBits3 - non-resetting fault
        OXY_BROW = 0x100,   // defined in LightBits  - monitor fault during Obogs
        EPUOn = 0x200,  // defined in LightBits3 - abnormal EPU operation
        JFSOn_Slow = 0x400, // defined in LightBits3 - slow blinking: non-critical failure
        JFSOn_Fast = 0x800	// defined in LightBits3 - fast blinking: critical failure
    }

    public enum CmdsModes : int
    {
        CmdsOFF = 0,
        CmdsSTBY = 1,
        CmdsMAN = 2,
        CmdsSEMI = 3,
        CmdsAUTO = 4,
        CmdsBYP = 5
    }

    public enum NavModes : byte
    {
        ILS_TACAN = 0,
        TACAN = 1,
        NAV = 2,
        ILS_NAV = 3
    }

    public enum FlyStates : int
    {
        IN_UI = 0, // UI      - in the UI
        LOADING = 1, // UI>3D   - loading the sim data
        WAITING = 2, // UI>3D   - waiting for other players
        FLYING = 3, // 3D      - flying
        DEAD = 4, // 3D>Dead - dead, waiting to respawn
        UNKNOWN = 5 // ???
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FlightData2
    {
        public float nozzlePos2;   // Ownship engine nozzle2 percent open (0-100)
        public float rpm2;         // Ownship engine rpm2 (Percent 0-103)
        public float ftit2;        // Ownship Forward Turbine Inlet Temp2 (Degrees C)
        public float oilPressure2; // Ownship Oil Pressure2 (Percent 0-100)
        public NavModes navMode;  // current mode selected for HSI/eHSI (added in BMS4)
        public float aauz; // Ownship barometric altitude given by AAU (depends on calibration)
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)TacanSources.NUMBER_OF_SOURCES)]
        public TacanBits[] tacanInfo;      // Tacan band/mode settings for UFC and AUX COMM

        //VERSION 2/7
        public int AltCalReading;  // barometric altitude calibration (depends on CalType)
        public AltBits altBits;       // various altimeter bits, see AltBits enum for details
        public PowerBits powerBits;     // Ownship power bus / generator states, see PowerBits enum for details
        public BlinkBits blinkBits;     // Cockpit indicator lights blink status, see BlinkBits enum for details
                                        // NOTE: these bits indicate only *if* a lamp is blinking, in addition to the
                                        // existing on/off bits. It's up to the external program to implement the
                                        // *actual* blinking.
        public CmdsModes cmdsMode;       // Ownship CMDS mode state, see CmdsModes enum for details
        public int BupUhfPreset;   // BUP UHF channel preset

        // VERSION 3
        public int BupUhfFreq;     // BUP UHF channel frequency
        public float cabinAlt;     // Ownship cabin altitude
        public float hydPressureA; // Ownship Hydraulic Pressure A
        public float hydPressureB; // Ownship Hydraulic Pressure B
        public int currentTime;    // Current time in seconds (max 60 * 60 * 24)
        public short vehicleACD;   // Ownship ACD index number, i.e. which aircraft type are we flying.
        public int VersionNum;     // Version of FlightData2 mem area

        // VERSION 4
        public float fuelFlow2;    // Ownship fuel flow2 (Lbs/Hour)

        // VERSION 6
        public float vtolPos;      // Ownship VTOL exhaust angle
    }
}
