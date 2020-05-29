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
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.OpenFalcon
{
    class OpenFalconDataExporter : FalconDataExporter
    {
        private SharedMemory _sharedMemory = null;

        private RadarContact[] _contacts = new RadarContact[40];

        private FlightData _lastFlightData;

        public OpenFalconDataExporter(FalconInterface falconInterface)
            : base(falconInterface)
        {
            AddValue("Caution", "Caution panel engine 2 fire indicator", "", "True if lit.", BindingValueUnits.Boolean);
            AddValue("General", "lock cue", "Lock light cue. (Non-F16)", "True if lit.", BindingValueUnits.Boolean);
            AddValue("General", "shoot cue", "Shoot light cue. (Non-F16)", "True if lit.", BindingValueUnits.Boolean);
        }

        internal override void InitData()
        {
            _sharedMemory = new SharedMemory("FalconSharedMemoryArea");
            _sharedMemory.Open();
        }

        internal override void PollData()
        {
            if (_sharedMemory != null && _sharedMemory.IsDataAvailable)
            {
                _lastFlightData = (FlightData)_sharedMemory.MarshalTo(typeof(FlightData));

                float altitidue = _lastFlightData.z;
                if (_lastFlightData.z < 0)
                {
                    altitidue = 99999.99f - _lastFlightData.z;
                }
                SetValue("Altimeter", "altitidue", new BindingValue(altitidue));
                SetValue("Altimeter", "barimetric pressure", new BindingValue(29.92));
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
                SetValue("Engine", "nozzle position", new BindingValue(NOZ(_lastFlightData.rpm, _lastFlightData.z, _lastFlightData.fuelFlow)));
                SetValue("Fuel", "internal fuel", new BindingValue(_lastFlightData.internalFuel));
                SetValue("Fuel", "external fuel", new BindingValue(_lastFlightData.externalFuel));
                SetValue("Engine", "fuel flow", new BindingValue(_lastFlightData.fuelFlow));
                SetValue("Engine", "rpm", new BindingValue(_lastFlightData.rpm));
                SetValue("Engine", "ftit", new BindingValue(Ftit(_lastFlightData.ftit, _lastFlightData.rpm)));
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

                SetValue("Altimeter", "indicated altitude", new BindingValue(altitidue));
            }
        }

        protected void ProcessLightBits(LightBits bits)
        {
            SetValue("Left Eyebrow", "master caution indicator", new BindingValue(bits.HasFlag(LightBits.MasterCaution)));
            SetValue("Left Eyebrow", "tf-fail indicator", new BindingValue(bits.HasFlag(LightBits.TF)));
            SetValue("Right Eyebrow", "engine fire indicator", new BindingValue(bits.HasFlag(LightBits.ENG_FIRE)));
            SetValue("Right Eyebrow", "hydraulic/oil indicator", new BindingValue(bits.HasFlag(LightBits.HYD) || bits.HasFlag(LightBits.OIL)));
            SetValue("Right Eyebrow", "canopy indicator", new BindingValue(bits.HasFlag(LightBits.CAN)));
            SetValue("Right Eyebrow", "takeoff landing config indicator", new BindingValue(bits.HasFlag(LightBits.T_L_CFG)));
            SetValue("Caution", "stores config indicator", new BindingValue(bits.HasFlag(LightBits.CONFIG)));
            SetValue("AOA Indexer", "above indicator", new BindingValue(bits.HasFlag(LightBits.AOAAbove)));
            SetValue("AOA Indexer", "on indicator", new BindingValue(bits.HasFlag(LightBits.AOAOn)));
            SetValue("AOA Indexer", "below indicator", new BindingValue(bits.HasFlag(LightBits.AOABelow)));
            SetValue("Refuel Indexer", "ready indicator", new BindingValue(bits.HasFlag(LightBits.RefuelRDY)));
            SetValue("Refuel Indexer", "air/nws indicator", new BindingValue(bits.HasFlag(LightBits.RefuelAR)));
            SetValue("Refuel Indexer", "disconnect indicator", new BindingValue(bits.HasFlag(LightBits.RefuelDSC)));
            SetValue("Caution", "flight control system indicator", new BindingValue(bits.HasFlag(LightBits.FltControlSys)));
            SetValue("Caution", "leading edge flaps indicator", new BindingValue(bits.HasFlag(LightBits.LEFlaps)));
            SetValue("Caution", "engine fault indticator", new BindingValue(bits.HasFlag(LightBits.EngineFault)));
            SetValue("Caution", "overheat indicator", new BindingValue(bits.HasFlag(LightBits.Overheat)));
            SetValue("Caution", "low fuel indicator", new BindingValue(bits.HasFlag(LightBits.FuelLow)));
            SetValue("Caution", "avionics indicator", new BindingValue(bits.HasFlag(LightBits.Avionics)));
            SetValue("Caution", "radar altimeter indicator", new BindingValue(bits.HasFlag(LightBits.RadarAlt)));
            SetValue("Caution", "iff indicator", new BindingValue(bits.HasFlag(LightBits.IFF)));
            SetValue("Caution", "ecm indicator", new BindingValue(bits.HasFlag(LightBits.ECM)));
            SetValue("Caution", "hook indicator", new BindingValue(bits.HasFlag(LightBits.Hook)));
            SetValue("Caution", "nws fail indicator", new BindingValue(bits.HasFlag(LightBits.NWSFail)));
            SetValue("Caution", "cabin pressure indicator", new BindingValue(bits.HasFlag(LightBits.CabinPress)));
            SetValue("Misc", "tfs stanby indicator", new BindingValue(bits.HasFlag(LightBits.TFR_STBY)));
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
            SetValue("Gear Handle", "handle indicator", new BindingValue(bits.HasFlag(LightBits2.GEARHANDLE))); // TODO This should be under device Landing Gear
            SetValue("Right Eyebrow", "engine indicator", new BindingValue(bits.HasFlag(LightBits2.ENGINE)));
        }

        protected void ProcessLightBits3(LightBits3 bits)
        {
            SetValue("Electronic", "flcs pmg indicator", new BindingValue(bits.HasFlag(LightBits3.FlcsPmg)));
            SetValue("Electronic", "main gen indicator", new BindingValue(bits.HasFlag(LightBits3.MainGen)));
            SetValue("Electronic", "standby generator indicator", new BindingValue(bits.HasFlag(LightBits3.StbyGen)));
            SetValue("Electronic", "epu gen indicator", new BindingValue(bits.HasFlag(LightBits3.EpuGen)));
            SetValue("Electronic", "epu pmg indicator", new BindingValue(bits.HasFlag(LightBits3.EpuPmg)));
            SetValue("Electronic", "to flcs indicator", new BindingValue(bits.HasFlag(LightBits3.ToFlcs)));
            SetValue("Electronic", "flcs rly indicator", new BindingValue(bits.HasFlag(LightBits3.FlcsRly)));
            SetValue("Electronic", "bat fail indicator", new BindingValue(bits.HasFlag(LightBits3.BatFail)));
            SetValue("EPU", "hydrazine indicator", new BindingValue(bits.HasFlag(LightBits3.Hydrazine)));
            SetValue("EPU", "air indicator", new BindingValue(bits.HasFlag(LightBits3.Air)));
            SetValue("Caution", "electric bus fail indicator", new BindingValue(bits.HasFlag(LightBits3.Elec_Fault)));
            SetValue("Caution", "lef fault indicator", new BindingValue(bits.HasFlag(LightBits3.Lef_Fault)));
            SetValue("General", "power off", new BindingValue(bits.HasFlag(LightBits3.Power_Off)));
            SetValue("Caution", "engine 2 fire indicator", new BindingValue(bits.HasFlag(LightBits3.Eng2_Fire)));
            SetValue("General", "lock cue", new BindingValue(bits.HasFlag(LightBits3.Lock)));
            SetValue("General", "shoot cue", new BindingValue(bits.HasFlag(LightBits3.Shoot)));
            SetValue("Landing Gear", "nose gear indicator", new BindingValue(bits.HasFlag(LightBits3.NoseGearDown)));
            SetValue("Landing Gear", "left gear indicator", new BindingValue(bits.HasFlag(LightBits3.LeftGearDown)));
            SetValue("Landing Gear", "right gear indicator", new BindingValue(bits.HasFlag(LightBits3.RightGearDown)));
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

        private float NOZ(float RPM, float Alt, float FF)
        {
            float NewNoz = 0;
            float AC_Alt = Math.Abs(Alt);

            if (RPM < 58)
            {
                NewNoz = 0;
            }
            else if (RPM >= 58 && RPM < 62)
            {
                NewNoz = (RPM - 58) * 25;
            }
            else if (RPM >= 62 && RPM < 68)
            {
                NewNoz = 100;
            }
            else if (RPM >= 68 && RPM <= 70)
            {
                //Reduce the NOZ to around 92%
                NewNoz = (100 - (RPM - 68) * 4);
            }
            else if (RPM > 70 && RPM < 83)
            {
                NewNoz = 92 - (RPM - 70) * 7.1F;
            }
            else if (RPM >= 83)
            {
                //Get MilFuelFlow
                int Mil = MilFF(AC_Alt);
                int AB = AbFF(AC_Alt);
                int Spread = AB - Mil;

                if (FF < Mil)
                {
                    NewNoz = 0;
                }
                else
                {
                    if (FF > AB)
                    {
                        NewNoz = 100;
                    }
                    else
                    {
                        NewNoz = ((FF - Mil) / Spread) * 100;
                    }
                }
            }

            return NewNoz;
        }

        private int MilFF(float Alt)
        {
            int MilFF = 0;

            if (Alt >= 0 && Alt < 5000)
            {
                MilFF = 13000;
            }
            else if (Alt >= 5000 && Alt < 10000)
            {
                MilFF = 12700;
            }
            else if (Alt >= 10000 && Alt < 15000)
            {
                MilFF = 12000;
            }
            else if (Alt >= 15000 && Alt < 20000)
            {
                MilFF = 10500;
            }
            else if (Alt >= 20000 && Alt < 25000)
            {
                MilFF = 8600;
            }
            else if (Alt >= 25000 && Alt < 30000)
            {
                MilFF = 6800;
            }
            else if (Alt >= 30000 && Alt < 35000)
            {
                MilFF = 5400;
            }
            else if (Alt >= 35000 && Alt < 40000)
            {
                MilFF = 4400;
            }
            else if (Alt >= 40000 && Alt < 45000)
            {

            }
            else if (Alt >= 45000 && Alt < 50000)
            {

            }
            else if (Alt >= 50000 && Alt < 55000)
            {

            }

            return MilFF;
        }

        private int AbFF(float Alt)
        {
            int AbFF = 0;

            if (Alt >= 0 && Alt < 5000)
            {
                AbFF = 45000;
            }
            else if (Alt >= 5000 && Alt < 10000)
            {
                AbFF = 55000;
            }
            else if (Alt >= 10000 && Alt < 15000)
            {
                AbFF = 45000;
            }
            else if (Alt >= 15000 && Alt < 20000)
            {
                AbFF = 35000;
            }
            else if (Alt >= 20000 && Alt < 25000)
            {
                AbFF = 29000;
            }
            else if (Alt >= 25000 && Alt < 30000)
            {
                AbFF = 24000;
            }
            else if (Alt >= 30000 && Alt < 35000)
            {
                AbFF = 16000;
            }
            else if (Alt >= 35000 && Alt < 40000)
            {
                AbFF = 10000;
            }
            else if (Alt >= 40000 && Alt < 45000)
            {

            }
            else if (Alt >= 45000 && Alt < 50000)
            {

            }
            else if (Alt >= 50000 && Alt < 55000)
            {

            }

            return AbFF;
        }

        public static float Ftit(float FTIT, float RPM)
        {
            float NewFtit = 0;
            int DampingValue = 5;

            if (RPM > 25 && RPM <= 65)
            {
                NewFtit = (RPM - 5) * 10;
            }
            else if (RPM > 60 && RPM < 65)
            {
                NewFtit = RPM * 10;
            }
            else if (RPM > 65 && RPM <= 70)
            {
                NewFtit = (RPM - (RPM - 65) * 5) * 10;
            }
            else if (RPM > 70)
            {
                NewFtit = (RPM - 20) * 10;
            }
            else
            {
                NewFtit = 200;
            }

            //Dampen the movement.
            if (FTIT - NewFtit < -10)
            {
                NewFtit = FTIT + DampingValue;
            }
            else if (FTIT - NewFtit > 10)
            {
                NewFtit = FTIT - DampingValue;
            }

            if (NewFtit > 770)
            {
                NewFtit = 770;
            }

            return NewFtit;
        }
    }
}
