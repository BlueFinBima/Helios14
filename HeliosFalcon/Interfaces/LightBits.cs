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

namespace GadrocsWorkshop.Helios.Interfaces.Falcon
{
    [Flags]
    enum LightBits : uint
    {
        MasterCaution = 0x1,  // Left eyebrow

        // Brow Lights
        TF = 0x2,   // Left eyebrow
        OBS = 0x4,   // Not used
        ALT = 0x8,   // Caution light; not used
        WOW = 0x10,  // True if weight is on wheels: this is not a lamp bit!
        ENG_FIRE = 0x20,  // Right eyebrow; upper half of split face lamp
        CONFIG = 0x40,  // Stores config, caution panel
        HYD = 0x80,  // Right eyebrow; see also OIL (this lamp is not split face)
        OIL = 0x100, // Right eyebrow; see also HYD (this lamp is not split face)
        DUAL = 0x200, // Right eyebrow; block 25, 30/32 and older 40/42
        CAN = 0x400, // Right eyebrow
        T_L_CFG = 0x800, // Right eyebrow

        // AOA Indexers
        AOAAbove = 0x1000,
        AOAOn = 0x2000,
        AOABelow = 0x4000,

        // Refuel/NWS
        RefuelRDY = 0x8000,
        RefuelAR = 0x10000,
        RefuelDSC = 0x20000,

        // Caution Lights
        FltControlSys = 0x40000,
        LEFlaps = 0x80000,
        EngineFault = 0x100000,
        Overheat = 0x200000,
        FuelLow = 0x400000,
        Avionics = 0x800000,
        RadarAlt = 0x1000000,
        IFF = 0x2000000,
        ECM = 0x4000000,
        Hook = 0x8000000,
        NWSFail = 0x10000000,
        CabinPress = 0x20000000,

        AutoPilotOn = 0x40000000,  // TRUE if is AP on.  NB: This is not a lamp bit!
        TFR_STBY = 0x80000000,  // MISC panel; lower half of split face TFR lamp
    };
}
