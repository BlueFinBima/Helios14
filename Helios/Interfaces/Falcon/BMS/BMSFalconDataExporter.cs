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

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.BMS
{
    using GadrocsWorkshop.Helios.Util;
    using System;

    class BMSFalconDataExporter : FalconDataExporter
    {
        private SharedMemory _sharedMemory = null;
        private SharedMemory _sharedMemory2 = null;

        private RadarContact[] _contacts = new RadarContact[40];

        private FlightData _lastFlightData;
        private FlightData2 _lastFlightData2;

        public BMSFalconDataExporter(FalconInterface falconInterface)
            : base(falconInterface)
        {
            AddValue("Right Eyebrow", "oxy low indicator", "OXY LOW indicator on right eyebrow", "True if lit", BindingValueUnits.Boolean);
            AddValue("Caution", "equip hot indicator", "Equip hot indicator on caution panel", "True if lit", BindingValueUnits.Boolean);
            AddValue("Test Panel", "FLCS channel lamps", "FLCS channel lamps on test panel (abcd)", "True if lit", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "flcs indicator", "FLCS Indicator", "True if lit", BindingValueUnits.Boolean);

            AddValue("Autopilot", "on indicator", "Indicates whether the autopilot is on.", "True if on", BindingValueUnits.Boolean);

            AddValue("General", "on ground", "Indicates weight on wheels.", "True if wheight is on wheels.", BindingValueUnits.Boolean);
            AddValue("Flight Control", "run light", "Run light on the flight control panel indicating bit is running.", "True if lit", BindingValueUnits.Boolean);
            AddValue("Flight Control", "fail light", "Fail light on the flight control panel indicating bit failure.", "True if lit", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "dbu on indicator", "DBU Warning light on the right eyebrow.", "True if lit", BindingValueUnits.Boolean);
            AddValue("General", "parking brake engaged", "Indicates if the parking brake is engaged.", "True if engaged", BindingValueUnits.Boolean);
            AddValue("Caution", "cadc indicator", "CADC indicator lamp on the caution panel.", "True if lit", BindingValueUnits.Boolean);
            AddValue("General", "speed barke", "Indicates if the speed brake is deployed.", "True if speed breake is in any other position than stowed.", BindingValueUnits.Boolean);

            AddValue("HSI", "nav mode", "Nav mode currently selected for the HSI/eHSI", "", BindingValueUnits.Numeric);
            AddValue("Tacan", "ufc tacan band", "Tacan band set with the UFC.", "1 = X, 2 = Y", BindingValueUnits.Numeric);
            AddValue("Tacan", "aux tacan band", "Tacan band set with the AUX COM panel.", "1 = X, 2 = Y", BindingValueUnits.Numeric);
            AddValue("Tacan", "ufc tacan mode", "Tacan mode set with the UFC.", "1 = TR, 2 = AA", BindingValueUnits.Numeric);
            AddValue("Tacan", "aux tacan mode", "Tacan mode set with the AUX COM panel.", "1 = TR, 2 = AA", BindingValueUnits.Numeric);
        }

        internal override void InitData()
        {
            _sharedMemory = new SharedMemory("FalconSharedMemoryArea");
            _sharedMemory.Open();

            _sharedMemory2 = new SharedMemory("FalconSharedMemoryArea2");
            _sharedMemory2.Open();
        }

        internal override void PollData()
        {
            if (_sharedMemory != null && _sharedMemory.IsDataAvailable)
            {
                _lastFlightData = (FlightData)_sharedMemory.MarshalTo(typeof(FlightData));

                SetValue("Altimeter", "altitidue", new BindingValue(Math.Abs(_lastFlightData.z)));
                SetValue("ADI", "pitch", new BindingValue(_lastFlightData.pitch));
                SetValue("ADI", "roll", new BindingValue(_lastFlightData.roll));
                SetValue("ADI", "ils horizontal", new BindingValue((_lastFlightData.AdiIlsHorPos / 2.5f) - 1f));
                SetValue("ADI", "ils vertical", new BindingValue((_lastFlightData.AdiIlsVerPos * 2f) - 1f));
                SetValue("HSI", "bearing to beacon", new BindingValue(_lastFlightData.bearingToBeacon));
                SetValue("HSI", "current heading", new BindingValue(_lastFlightData.currentHeading));
                SetValue("HSI", "desired course", new BindingValue(_lastFlightData.desiredCourse));
                SetValue("HSI", "desired heading", new BindingValue(_lastFlightData.desiredHeading));

                float deviation = _lastFlightData.courseDeviation % 180;
                SetValue("HSI", "course deviation", new BindingValue(deviation / _lastFlightData.deviationLimit));

                SetValue("HSI", "distance to beacon", new BindingValue(_lastFlightData.distanceToBeacon));
                SetValue("VVI", "vertical velocity", new BindingValue(_lastFlightData.zDot));
                SetValue("AOA", "angle of attack", new BindingValue(_lastFlightData.alpha));
                SetValue("IAS", "mach", new BindingValue(_lastFlightData.mach));
                SetValue("IAS", "indicated air speed", new BindingValue(_lastFlightData.kias));
                SetValue("IAS", "true air speed", new BindingValue(_lastFlightData.vt));

                SetValue("General", "Gs", new BindingValue(_lastFlightData.gs));
                SetValue("Engine", "nozzle position", new BindingValue(_lastFlightData.nozzlePos * 100));
                SetValue("Fuel", "internal fuel", new BindingValue(_lastFlightData.internalFuel));
                SetValue("Fuel", "external fuel", new BindingValue(_lastFlightData.externalFuel));
                SetValue("Engine", "fuel flow", new BindingValue(_lastFlightData.fuelFlow));
                SetValue("Engine", "rpm", new BindingValue(_lastFlightData.rpm));
                SetValue("Engine", "ftit", new BindingValue(_lastFlightData.ftit * 100));
                SetValue("Landging Gear", "position", new BindingValue(_lastFlightData.gearPos != 0d));
                SetValue("General", "speed brake position", new BindingValue(_lastFlightData.speedBrake));
                SetValue("General", "speed brake indicator", new BindingValue(_lastFlightData.speedBrake > 0d));
                SetValue("EPU", "fuel", new BindingValue(_lastFlightData.epuFuel));
                SetValue("Engine", "oil pressure", new BindingValue(_lastFlightData.oilPressure));

                SetValue("CMDS", "chaff remaining", new BindingValue(_lastFlightData.ChaffCount));
                SetValue("CMDS", "flares remaining", new BindingValue(_lastFlightData.FlareCount));

                SetValue("Trim", "roll trim", new BindingValue(_lastFlightData.TrimRoll));
                SetValue("Trim", "pitch trim", new BindingValue(_lastFlightData.TrimPitch));
                SetValue("Trim", "yaw trim", new BindingValue(_lastFlightData.TrimYaw));


                SetValue("Tacan", "ufc tacan chan", new BindingValue(_lastFlightData.UFCTChan));
                SetValue("Tacan", "aux tacan chan", new BindingValue(_lastFlightData.AUXTChan));

                ProcessHsiBits(_lastFlightData.hsiBits, _lastFlightData.desiredCourse, _lastFlightData.bearingToBeacon);
                ProcessLightBits(_lastFlightData.lightBits);
                ProcessLightBits2(_lastFlightData.lightBits2);
                ProcessLightBits3(_lastFlightData.lightBits3);

                ProcessContacts(_lastFlightData);
            }
            if(_sharedMemory2 != null & _sharedMemory2.IsDataAvailable)
            {
                _lastFlightData2 = (FlightData2)_sharedMemory2.MarshalTo(typeof(FlightData2));
                SetValue("Altimeter", "indicated altitude", new BindingValue(Math.Abs(_lastFlightData2.aauz)));
                SetValue("Altimeter", "barimetric pressure", new BindingValue(_lastFlightData2.AltCalReading));

                SetValue("HSI", "nav mode", new BindingValue(_lastFlightData2.navMode));
                SetValue("Tacan", "ufc tacan band", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.UFC].HasFlag(TacanBits.band) ? 1 : 2));
                SetValue("Tacan", "aux tacan band", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.AUX].HasFlag(TacanBits.mode) ? 2 : 1));
                SetValue("Tacan", "ufc tacan mode", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.UFC].HasFlag(TacanBits.band) ? 1 : 2));
                SetValue("Tacan", "aux tacan mode", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.AUX].HasFlag(TacanBits.mode) ? 2 : 1));
            }
        }

        protected void ProcessLightBits(BMSLightBits bits)
        {
            SetValue("Left Eyebrow", "master caution indicator", new BindingValue(bits.HasFlag(BMSLightBits.MasterCaution)));
            SetValue("Left Eyebrow", "tf-fail indicator", new BindingValue(bits.HasFlag(BMSLightBits.TF)));
            SetValue("Right Eyebrow", "oxy low indicator", new BindingValue(bits.HasFlag(BMSLightBits.OXY_BROW)));
            SetValue("Right Eyebrow", "engine fire indicator", new BindingValue(bits.HasFlag(BMSLightBits.ENG_FIRE)));
            SetValue("Right Eyebrow", "hydraulic/oil indicator", new BindingValue(bits.HasFlag(BMSLightBits.HYD)));
            SetValue("Right Eyebrow", "canopy indicator", new BindingValue(bits.HasFlag(BMSLightBits.CAN)));
            SetValue("Right Eyebrow", "takeoff landing config indicator", new BindingValue(bits.HasFlag(BMSLightBits.T_L_CFG)));
            SetValue("Right Eyebrow", "flcs indicator", new BindingValue(bits.HasFlag(BMSLightBits.FLCS)));
            SetValue("Caution", "stores config indicator", new BindingValue(bits.HasFlag(BMSLightBits.CONFIG)));
            SetValue("AOA Indexer", "above indicator", new BindingValue(bits.HasFlag(BMSLightBits.AOAAbove)));
            SetValue("AOA Indexer", "on indicator", new BindingValue(bits.HasFlag(BMSLightBits.AOAOn)));
            SetValue("AOA Indexer", "below indicator", new BindingValue(bits.HasFlag(BMSLightBits.AOABelow)));
            SetValue("Refuel Indexer", "ready indicator", new BindingValue(bits.HasFlag(BMSLightBits.RefuelRDY)));
            SetValue("Refuel Indexer", "air/nws indicator", new BindingValue(bits.HasFlag(BMSLightBits.RefuelAR)));
            SetValue("Refuel Indexer", "disconnect indicator", new BindingValue(bits.HasFlag(BMSLightBits.RefuelDSC)));
            SetValue("Caution", "flight control system indicator", new BindingValue(bits.HasFlag(BMSLightBits.FltControlSys)));
            SetValue("Caution", "leading edge flaps indicator", new BindingValue(bits.HasFlag(BMSLightBits.LEFlaps)));
            SetValue("Caution", "engine fault indticator", new BindingValue(bits.HasFlag(BMSLightBits.EngineFault)));
            SetValue("Caution", "equip hot indicator", new BindingValue(bits.HasFlag(BMSLightBits.EQUIP_HOT)));
            SetValue("Caution", "overheat indicator", new BindingValue(bits.HasFlag(BMSLightBits.Overheat)));
            SetValue("Caution", "low fuel indicator", new BindingValue(bits.HasFlag(BMSLightBits.FuelLow)));
            SetValue("Caution", "avionics indicator", new BindingValue(bits.HasFlag(BMSLightBits.Avionics)));
            SetValue("Caution", "radar altimeter indicator", new BindingValue(bits.HasFlag(BMSLightBits.RadarAlt)));
            SetValue("Caution", "iff indicator", new BindingValue(bits.HasFlag(BMSLightBits.IFF)));
            SetValue("Caution", "ecm indicator", new BindingValue(bits.HasFlag(BMSLightBits.ECM)));
            SetValue("Caution", "hook indicator", new BindingValue(bits.HasFlag(BMSLightBits.Hook)));
            SetValue("Caution", "nws fail indicator", new BindingValue(bits.HasFlag(BMSLightBits.NWSFail)));
            SetValue("Caution", "cabin pressure indicator", new BindingValue(bits.HasFlag(BMSLightBits.CabinPress)));
            SetValue("Autopilot", "on indicator", new BindingValue(bits.HasFlag(BMSLightBits.AutoPilotOn)));
            SetValue("Misc", "tfs stanby indicator", new BindingValue(bits.HasFlag(BMSLightBits.TFR_STBY)));
            SetValue("Test Panel", "FLCS channel lamps", new BindingValue(bits.HasFlag(BMSLightBits.Flcs_ABCD)));
        }

        protected void ProcessLightBits2(LightBits2 bits)
        {
            bool rwrPower = bits.HasFlag(LightBits2.AuxPwr);

            SetValue("Threat Warning Prime", "handoff indicator", new BindingValue(bits.HasFlag(LightBits2.HandOff)));
            SetValue("Threat Warning Prime", "launch indicator", new BindingValue(bits.HasFlag(LightBits2.Launch)));
            SetValue("Threat Warning Prime", "prioirty mode indicator", new BindingValue(bits.HasFlag(LightBits2.PriMode)));
            SetValue("Threat Warning Prime", "open mode indicator", new BindingValue(bits.HasFlag(LightBits2.AuxPwr) && !bits.HasFlag(LightBits2.PriMode)));
            SetValue("Threat Warning Prime", "naval indicator", new BindingValue(bits.HasFlag(LightBits2.Naval)));
            SetValue("Threat Warning Prime", "unknown mode indicator", new BindingValue(bits.HasFlag(LightBits2.Unk)));
            SetValue("Threat Warning Prime", "target step indicator", new BindingValue(bits.HasFlag(LightBits2.TgtSep)));
            SetValue("Aux Threat Warning", "search indicator", new BindingValue(bits.HasFlag(LightBits2.AuxSrch)));
            SetValue("Aux Threat Warning", "activity indicator", new BindingValue(bits.HasFlag(LightBits2.AuxAct)));
            SetValue("Aux Threat Warning", "low altitude indicator", new BindingValue(bits.HasFlag(LightBits2.AuxLow)));
            SetValue("Aux Threat Warning", "power indicator", new BindingValue(bits.HasFlag(LightBits2.AuxPwr)));

            SetValue("CMDS", "Go", new BindingValue(bits.HasFlag(LightBits2.Go)));
            SetValue("CMDS", "NoGo", new BindingValue(bits.HasFlag(LightBits2.NoGo)));
            SetValue("CMDS", "Degr", new BindingValue(bits.HasFlag(LightBits2.Degr)));
            SetValue("CMDS", "Rdy", new BindingValue(bits.HasFlag(LightBits2.Rdy)));
            SetValue("CMDS", "ChaffLo", new BindingValue(bits.HasFlag(LightBits2.ChaffLo)));
            SetValue("CMDS", "FlareLo", new BindingValue(bits.HasFlag(LightBits2.FlareLo)));

            SetValue("ECM", "power indicator", new BindingValue(bits.HasFlag(LightBits2.EcmPwr)));
            SetValue("ECM", "fail indicator", new BindingValue(bits.HasFlag(LightBits2.EcmFail)));
            SetValue("Caution", "forward fuel low indicator", new BindingValue(bits.HasFlag(LightBits2.FwdFuelLow)));
            SetValue("Caution", "aft fuel low indicator", new BindingValue(bits.HasFlag(LightBits2.AftFuelLow)));
            SetValue("EPU", "on indicator", new BindingValue(bits.HasFlag(LightBits2.EPUOn)));
            SetValue("JFS", "run indicator", new BindingValue(bits.HasFlag(LightBits2.JFSOn)));
            SetValue("Caution", "second engine compressor indicator", new BindingValue(bits.HasFlag(LightBits2.SEC)));
            SetValue("Caution", "oxygen low indicator", new BindingValue(bits.HasFlag(LightBits2.OXY_LOW)));
            SetValue("Caution", "probe heat indicator", new BindingValue(bits.HasFlag(LightBits2.PROBEHEAT)));
            SetValue("Caution", "seat arm indicator", new BindingValue(bits.HasFlag(LightBits2.SEAT_ARM)));
            SetValue("Caution", "backup fuel control indicator", new BindingValue(bits.HasFlag(LightBits2.BUC)));
            SetValue("Caution", "fuel oil hot indicator", new BindingValue(bits.HasFlag(LightBits2.FUEL_OIL_HOT)));
            SetValue("Caution", "anti skid indicator", new BindingValue(bits.HasFlag(LightBits2.ANTI_SKID)));
            SetValue("Misc", "tfs engaged indicator", new BindingValue(bits.HasFlag(LightBits2.TFR_ENGAGED)));
            SetValue("Gear Handle", "handle indicator", new BindingValue(bits.HasFlag(LightBits2.GEARHANDLE)));
            SetValue("Right Eyebrow", "engine indicator", new BindingValue(bits.HasFlag(LightBits2.ENGINE)));
        }

        protected void ProcessLightBits3(BMSLightBits3 bits)
        {
            SetValue("Electronic", "flcs pmg indicator", new BindingValue(bits.HasFlag(BMSLightBits3.FlcsPmg)));
            SetValue("Electronic", "main gen indicator", new BindingValue(bits.HasFlag(BMSLightBits3.MainGen)));
            SetValue("Electronic", "standby generator indicator", new BindingValue(bits.HasFlag(BMSLightBits3.StbyGen)));
            SetValue("Electronic", "epu gen indicator", new BindingValue(bits.HasFlag(BMSLightBits3.EpuGen)));
            SetValue("Electronic", "epu pmg indicator", new BindingValue(bits.HasFlag(BMSLightBits3.EpuPmg)));
            SetValue("Electronic", "to flcs indicator", new BindingValue(bits.HasFlag(BMSLightBits3.ToFlcs)));
            SetValue("Electronic", "flcs rly indicator", new BindingValue(bits.HasFlag(BMSLightBits3.FlcsRly)));
            SetValue("Electronic", "bat fail indicator", new BindingValue(bits.HasFlag(BMSLightBits3.BatFail)));
            SetValue("EPU", "hydrazine indicator", new BindingValue(bits.HasFlag(BMSLightBits3.Hydrazine)));
            SetValue("EPU", "air indicator", new BindingValue(bits.HasFlag(BMSLightBits3.Air)));
            SetValue("Caution", "electric bus fail indicator", new BindingValue(bits.HasFlag(BMSLightBits3.Elec_Fault)));
            SetValue("Caution", "lef fault indicator", new BindingValue(bits.HasFlag(BMSLightBits3.Lef_Fault)));

            SetValue("General", "on ground", new BindingValue(bits.HasFlag(BMSLightBits3.OnGround)));
            SetValue("Flight Control", "run light", new BindingValue(bits.HasFlag(BMSLightBits3.FlcsBitRun)));
            SetValue("Flight Control", "fail light", new BindingValue(bits.HasFlag(BMSLightBits3.FlcsBitFail)));
            SetValue("Right Eyebrow", "dbu on indicator", new BindingValue(bits.HasFlag(BMSLightBits3.DbuWarn)));
            SetValue("General", "parking brake engaged", new BindingValue(bits.HasFlag(BMSLightBits3.ParkBrakeOn)));
            SetValue("Caution", "cadc indicator", new BindingValue(bits.HasFlag(BMSLightBits3.cadc)));
            SetValue("General", "speed barke", new BindingValue(bits.HasFlag(BMSLightBits3.SpeedBrake)));

            SetValue("Landing Gear", "nose gear indicator", new BindingValue(bits.HasFlag(BMSLightBits3.NoseGearDown)));
            SetValue("Landing Gear", "left gear indicator", new BindingValue(bits.HasFlag(BMSLightBits3.LeftGearDown)));
            SetValue("Landing Gear", "right gear indicator", new BindingValue(bits.HasFlag(BMSLightBits3.RightGearDown)));
            SetValue("General", "power off", new BindingValue(bits.HasFlag(BMSLightBits3.Power_Off)));

        }

        protected void ProcessHsiBits(HsiBits bits, float desiredCourse, float bearingToBeacon)
        {
            SetValue("HSI", "to flag", new BindingValue(bits.HasFlag(HsiBits.ToTrue)));
            SetValue("HSI", "from flag", new BindingValue(bits.HasFlag(HsiBits.FromTrue)));

            SetValue("HSI", "ils warning flag", new BindingValue(bits.HasFlag(HsiBits.IlsWarning)));
            SetValue("HSI", "course warning flag", new BindingValue(bits.HasFlag(HsiBits.CourseWarning)));
            SetValue("HSI", "off flag", new BindingValue(bits.HasFlag(HsiBits.HSI_OFF)));
            SetValue("HSI", "init flag", new BindingValue(bits.HasFlag(HsiBits.Init)));
            SetValue("ADI", "off flag", new BindingValue(bits.HasFlag(HsiBits.ADI_OFF)));
            SetValue("ADI", "aux flag", new BindingValue(bits.HasFlag(HsiBits.ADI_AUX)));
            SetValue("ADI", "gs flag", new BindingValue(bits.HasFlag(HsiBits.ADI_GS)));
            SetValue("ADI", "loc flag", new BindingValue(bits.HasFlag(HsiBits.ADI_LOC)));
            SetValue("Backup ADI", "off flag", new BindingValue(bits.HasFlag(HsiBits.BUP_ADI_OFF)));
            SetValue("VVI", "off flag", new BindingValue(bits.HasFlag(HsiBits.VVI)));
            SetValue("AOA", "off flag", new BindingValue(bits.HasFlag(HsiBits.AOA)));
        }

        private float ClampValue(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        internal override void CloseData()
        {
            _sharedMemory.Close();
            _sharedMemory.Dispose();
            _sharedMemory = null;

            _sharedMemory2.Close();
            _sharedMemory2.Dispose();
            _sharedMemory2 = null;
        }

        override internal RadarContact[] RadarContacts
        {
            get
            {
                return _contacts;
            }
        }

        private void ProcessContacts(FlightData flightData)
        {
            for(int i = 0; i < flightData.RWRsymbol.Length; i++)
            {
                _contacts[i].Symbol = (RadarSymbols)flightData.RWRsymbol[i];
                _contacts[i].Selected = flightData.selected[i] > 0;
                _contacts[i].Bearing = flightData.bearing[i] * 57.3f;
                _contacts[i].RelativeBearing = (-flightData.currentHeading + _contacts[i].Bearing) % 360d;
                _contacts[i].Lethality = flightData.lethality[i];
                _contacts[i].MissileActivity = flightData.missileActivity[i] > 0;
                _contacts[i].MissileLaunch = flightData.missileLaunch[i] > 0;
                _contacts[i].NewDetection = flightData.newDetection[i] > 0;
                _contacts[i].Visible = i < flightData.RwrObjectCount;
            }
        }
    }
}
