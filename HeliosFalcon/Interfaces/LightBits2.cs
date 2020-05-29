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
    enum LightBits2 : uint
    {
        // Threat Warning Prime
        HandOff = 0x1,
        Launch = 0x2,
        PriMode = 0x4,
        Naval = 0x8,
        Unk = 0x10,
        TgtSep = 0x20,

        // EWS
        Go = 0x40,		// On and operating normally
        NoGo = 0x80,     // On but malfunction present
        Degr = 0x100,    // Status message: AUTO DEGR
        Rdy = 0x200,    // Status message: DISPENSE RDY
        ChaffLo = 0x400,    // Bingo chaff quantity reached
        FlareLo = 0x800,    // Bingo flare quantity reached

        // Aux Threat Warning
        AuxSrch = 0x1000,
        AuxAct = 0x2000,
        AuxLow = 0x4000,
        AuxPwr = 0x8000,

        // ECM
        EcmPwr = 0x10000,
        EcmFail = 0x20000,

        // Caution Lights
        FwdFuelLow = 0x40000,
        AftFuelLow = 0x80000,

        EPUOn = 0x100000,  // EPU panel; run light
        JFSOn = 0x200000,  // Eng Jet Start panel; run light

        // Caution panel
        SEC = 0x400000,
        OXY_LOW = 0x800000,
        PROBEHEAT = 0x1000000,
        SEAT_ARM = 0x2000000,
        BUC = 0x4000000,
        FUEL_OIL_HOT = 0x8000000,
        ANTI_SKID = 0x10000000,

        TFR_ENGAGED = 0x20000000,  // MISC panel; upper half of split face TFR lamp
        GEARHANDLE = 0x40000000,  // Lamp in gear handle lights on fault or gear in motion
        ENGINE = 0x80000000,  // Lower half of right eyebrow ENG FIRE/ENGINE lamp
    };
}
