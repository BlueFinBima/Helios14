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
using System.Diagnostics.Eventing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Data;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon.BMS
{
    class BMSFalconDataExporter : FalconDataExporter
    {
        private const int SLOW_BLINK_LENGTH_MS = 500;
        private const int FAST_BLINK_LENGTH_MS = 200;
        private const int PHASE_LENGTH = 1;
        private const int MAX_SECONDS = 60 * 60 * 24;

        private SharedMemory _sharedMemory = null;
        private SharedMemory _sharedMemory2 = null;

        private RadarContact[] _contacts = new RadarContact[40];

        private FlightData _lastFlightData;
        private FlightData2 _lastFlightData2;

        private DateTime _outerMarkerLastTick;
        private bool _outerMarkerOnState;

        private DateTime _middleMarkerLastTick;
        private bool _middleMarkerOnState;

        private DateTime _probeheatLastTick;
        private bool _probeheatOnState;

        private DateTime _auxsrchLastTick;
        private bool _auxsrchOnState;

        private DateTime _launchLastTick;
        private bool _launchOnState;

        private DateTime _primodeLastTick;
        private bool _primodeOnState;

        private DateTime _unkLastTick;
        private bool _unkOnState;

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

            AddValue("HSI", "Outer marker indicator", "Outer marker indicator on HSI", "True if lit", BindingValueUnits.Boolean);
            AddValue("HSI", "Middle marker indicator", "Middle marker indicator on HSI", "True if lit", BindingValueUnits.Boolean);

            AddValue("HSI", "nav mode", "Nav mode currently selected for the HSI/eHSI", "", BindingValueUnits.Numeric);
            AddValue("Tacan", "ufc tacan band", "Tacan band set with the UFC.", "1 = X, 2 = Y", BindingValueUnits.Numeric);
            AddValue("Tacan", "aux tacan band", "Tacan band set with the AUX COM panel.", "1 = X, 2 = Y", BindingValueUnits.Numeric);
            AddValue("Tacan", "ufc tacan mode", "Tacan mode set with the UFC.", "1 = TR, 2 = AA", BindingValueUnits.Numeric);
            AddValue("Tacan", "aux tacan mode", "Tacan mode set with the AUX COM panel.", "1 = TR, 2 = AA", BindingValueUnits.Numeric);

            AddValue("AVTR", "avtr indicator", "Indicates whether the acmi is recording", "True if lit", BindingValueUnits.Boolean);

            //BMS 4.33 addition
            AddValue("Threat Warning Prime", "systest indicator", "Threat warning prime systest indicator", "True if lit", BindingValueUnits.Boolean);
            AddValue("Engine", "nozzle 2 position", "Current engine nozzle2.", "Percent open (0-100)", BindingValueUnits.Numeric);
            AddValue("Engine", "rpm2", "Current engine rpm2.", "Percent (0-103)", BindingValueUnits.Numeric);
            AddValue("Engine", "ftit2", "Current forward turbine inlet temp2", "Degrees C", BindingValueUnits.Numeric);
            AddValue("Engine", "oil pressure 2", "Current oil pressure 2 in the engine.", "Percent (0-100)", BindingValueUnits.Numeric);
            AddValue("CMDS", "CMDS Mode", "Current CMDS mode", "(0 off, 1 stby, 2 Man, 3 Semi, 4 Auto, 5 BYP)", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup channel", "Current Backup UHF channel", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency", "Current Backup UHF frequency", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency digit 1", "Current Backup UHF frequency digit 1", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency digit 2", "Current Backup UHF frequency digit 2", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency digit 3", "Current Backup UHF frequency digit 3", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency digit 4", "Current Backup UHF frequency digit 4", "", BindingValueUnits.Numeric);
            AddValue("UHF", "Backup frequency digit 5,6", "Current Backup UHF frequency digit 5,6", "", BindingValueUnits.Numeric);
            AddValue("Altitude", "Cabin Altitude", "Current cabin altitude", "", BindingValueUnits.Numeric);
            AddValue("Hydraulic", "Pressure A", "Current hydraulic pressure a", "", BindingValueUnits.PoundsPerSquareInch);
            AddValue("Hydraulic", "Pressure B", "Current hydraulic pressure b", "", BindingValueUnits.PoundsPerSquareInch);
            AddValue("Time", "Time", "Current tine in seconds", "(max 60 * 60 * 24)", BindingValueUnits.Seconds);
            AddValue("Engine", "fuel flow 2", "Current fuel flow to the engine 2.", "", BindingValueUnits.PoundsPerHour);

            //AltBits
            AddValue("Altimeter", "altimeter calibration type", "", "True if hg otherwise hpa.", BindingValueUnits.Boolean);
            AddValue("Altimeter", "altimeter pneu flag", "", "True if visible", BindingValueUnits.Boolean);

            //PowerBits
            AddValue("POWER", "bus power battery", "at least the battery bus is powered", "True if powered", BindingValueUnits.Boolean);
            AddValue("POWER", "bus power emergency", "at least the emergency bus is powered", "True if powered", BindingValueUnits.Boolean);
            AddValue("POWER", "bus power essential", "at least the essential bus is powered", "True if powered", BindingValueUnits.Boolean);
            AddValue("POWER", "bus power non essential", "at least the non-essential bus is powered", "True if powered", BindingValueUnits.Boolean);
            AddValue("POWER", "main generator", "main generator is online", "True if online", BindingValueUnits.Boolean);
            AddValue("POWER", "standby generator", "standby generator is online", "True if online", BindingValueUnits.Boolean);
            AddValue("POWER", "Jetfuel starter", "JFS is running, can be used for magswitch", "True if running", BindingValueUnits.Boolean);

            //Fuel
            AddValue("Fuel", "fwd fuel", "Amount of fuel in the fwd tanks", "", BindingValueUnits.Pounds);
            AddValue("Fuel", "aft fuel", "Amount of fuel in the aft tanks", "", BindingValueUnits.Pounds);
            AddValue("Fuel", "total fuel", "Amount of total fuel", "", BindingValueUnits.Pounds);

            //DED
            AddValue("DED", "DED Line 1", "Data entry display line 1", "", BindingValueUnits.Text);
            AddValue("DED", "DED Line 2", "Data entry display line 2", "", BindingValueUnits.Text);
            AddValue("DED", "DED Line 3", "Data entry display line 3", "", BindingValueUnits.Text);
            AddValue("DED", "DED Line 4", "Data entry display line 4", "", BindingValueUnits.Text);
            AddValue("DED", "DED Line 5", "Data entry display line 5", "", BindingValueUnits.Text);


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

                SetValue("Fuel", "fwd fuel", new BindingValue(_lastFlightData.fwd));
                SetValue("Fuel", "aft fuel", new BindingValue(_lastFlightData.aft));
                SetValue("Fuel", "total fuel", new BindingValue(_lastFlightData.total));

                SetValue("Tacan", "ufc tacan chan", new BindingValue(_lastFlightData.UFCTChan));
                SetValue("Tacan", "aux tacan chan", new BindingValue(_lastFlightData.AUXTChan));

                ProcessContacts(_lastFlightData);
                
                //DED
                SetValue("DED", "DED Line 1", new BindingValue(DecodeUserInterfaceText(_lastFlightData.DED,0,26)));
                SetValue("DED", "DED Line 2", new BindingValue(DecodeUserInterfaceText(_lastFlightData.DED, 26, 26)));
                SetValue("DED", "DED Line 3", new BindingValue(DecodeUserInterfaceText(_lastFlightData.DED, 26*2, 26)));
                SetValue("DED", "DED Line 4", new BindingValue(DecodeUserInterfaceText(_lastFlightData.DED, 26*3, 26)));
                SetValue("DED", "DED Line 5", new BindingValue(DecodeUserInterfaceText(_lastFlightData.DED, 26*4, 26)));

            }
            if(_sharedMemory2 != null & _sharedMemory2.IsDataAvailable)
            {
                _lastFlightData2 = (FlightData2)_sharedMemory2.MarshalTo(typeof(FlightData2));
                SetValue("Altimeter", "indicated altitude", new BindingValue(Math.Abs(_lastFlightData2.aauz)));
                SetValue("HSI", "nav mode", new BindingValue((int)_lastFlightData2.navMode));
                SetValue("Tacan", "ufc tacan band", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.UFC].HasFlag(TacanBits.band) ? 1 : 2));
                SetValue("Tacan", "ufc tacan mode", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.UFC].HasFlag(TacanBits.mode) ? 2 : 1));
                SetValue("Tacan", "aux tacan band", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.AUX].HasFlag(TacanBits.band) ? 1 : 2));
                SetValue("Tacan", "aux tacan mode", new BindingValue(_lastFlightData2.tacanInfo[(int)TacanSources.AUX].HasFlag(TacanBits.mode) ? 2 : 1));

                //BMS 4.33 addition
                SetValue("Engine", "nozzle 2 position", new BindingValue(_lastFlightData2.nozzlePos2 * 100));
                SetValue("Engine", "rpm2", new BindingValue(_lastFlightData2.rpm2));
                SetValue("Engine", "ftit2", new BindingValue(_lastFlightData2.ftit2 * 100));
                SetValue("Engine", "oil pressure 2", new BindingValue(_lastFlightData2.oilPressure2));
                SetValue("Engine", "fuel flow 2", new BindingValue(_lastFlightData2.fuelFlow2));
                SetValue("Altimeter", "barimetric pressure", new BindingValue(_lastFlightData2.AltCalReading));

                ProcessAltBits(_lastFlightData2.altBits);
                ProcessPowerBits(_lastFlightData2.powerBits);

                SetValue("CMDS", "CMDS Mode", new BindingValue((int)_lastFlightData2.cmdsMode));
                SetValue("UHF", "Backup channel", new BindingValue(_lastFlightData2.BupUhfPreset));
                SetValue("UHF", "Backup frequency", new BindingValue(_lastFlightData2.BupUhfFreq));
                SetValue("UHF", "Backup frequency digit 1", new BindingValue(_lastFlightData2.BupUhfFreq / 100000 % 10));
                SetValue("UHF", "Backup frequency digit 2", new BindingValue(_lastFlightData2.BupUhfFreq / 10000 % 10));
                SetValue("UHF", "Backup frequency digit 3", new BindingValue(_lastFlightData2.BupUhfFreq / 1000 % 10));
                SetValue("UHF", "Backup frequency digit 4", new BindingValue(_lastFlightData2.BupUhfFreq / 100 % 10));
                SetValue("UHF", "Backup frequency digit 5,6", new BindingValue(_lastFlightData2.BupUhfFreq % 100));
                SetValue("Altitude", "Cabin Altitude", new BindingValue(_lastFlightData2.cabinAlt));
                SetValue("Hydraulic", "Pressure A", new BindingValue(_lastFlightData2.hydPressureA));
                SetValue("Hydraulic", "Pressure B", new BindingValue(_lastFlightData2.hydPressureB));
                SetValue("Time", "Time", new BindingValue(_lastFlightData2.currentTime));

                ProcessHsiBits(_lastFlightData.hsiBits, _lastFlightData.desiredCourse, _lastFlightData.bearingToBeacon, _lastFlightData2.blinkBits, _lastFlightData2.currentTime);
                ProcessLightBits(_lastFlightData.lightBits);
                ProcessLightBits2(_lastFlightData.lightBits2, _lastFlightData2.blinkBits, _lastFlightData2.currentTime);
                ProcessLightBits3(_lastFlightData.lightBits3);
            }
        }

        internal string DecodeUserInterfaceText(byte[] buffer,int offset,int length)
        {
            var sanitized = new StringBuilder();
            ASCIIEncoding ascii = new ASCIIEncoding();

            // Decode the bytes
            String decoded = ascii.GetString(buffer, offset, length);

            foreach (var value in decoded)
            {
                var thisChar = value;
                if (value == 0x01) thisChar = '\u2195';
                if (value == 0x02) thisChar = '*';
                if (value == 0x5E) thisChar = '\u00B0'; //degree symbol
                sanitized.Append(thisChar);

            }

            return sanitized.ToString();
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

        protected void ProcessLightBits2(BMSLightBits2 bits, BlinkBits blinkBits, int time)
        {
            bool rwrPower = bits.HasFlag(BMSLightBits2.AuxPwr);

            SetValue("Threat Warning Prime", "handoff indicator", new BindingValue(bits.HasFlag(BMSLightBits2.HandOff)));
            //SetValue("Threat Warning Prime", "launch indicator", new BindingValue(bits.HasFlag(BMSLightBits2.Launch)));
            //SetValue("Threat Warning Prime", "prioirty mode indicator", new BindingValue(bits.HasFlag(BMSLightBits2.PriMode)));
            SetValue("Threat Warning Prime", "open mode indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AuxPwr) && !bits.HasFlag(BMSLightBits2.PriMode)));
            SetValue("Threat Warning Prime", "naval indicator", new BindingValue(bits.HasFlag(BMSLightBits2.Naval)));
            //SetValue("Threat Warning Prime", "unknown mode indicator", new BindingValue(bits.HasFlag(BMSLightBits2.Unk)));
            SetValue("Threat Warning Prime", "target step indicator", new BindingValue(bits.HasFlag(BMSLightBits2.TgtSep)));
            //SetValue("Aux Threat Warning", "search indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AuxSrch)));
            SetValue("Aux Threat Warning", "activity indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AuxAct)));
            SetValue("Aux Threat Warning", "low altitude indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AuxLow)));
            SetValue("Aux Threat Warning", "power indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AuxPwr)));

            SetValue("CMDS", "Go", new BindingValue(bits.HasFlag(BMSLightBits2.Go)));
            SetValue("CMDS", "NoGo", new BindingValue(bits.HasFlag(BMSLightBits2.NoGo)));
            SetValue("CMDS", "Degr", new BindingValue(bits.HasFlag(BMSLightBits2.Degr)));
            SetValue("CMDS", "Rdy", new BindingValue(bits.HasFlag(BMSLightBits2.Rdy)));
            SetValue("CMDS", "ChaffLo", new BindingValue(bits.HasFlag(BMSLightBits2.ChaffLo)));
            SetValue("CMDS", "FlareLo", new BindingValue(bits.HasFlag(BMSLightBits2.FlareLo)));

            SetValue("ECM", "power indicator", new BindingValue(bits.HasFlag(BMSLightBits2.EcmPwr)));
            SetValue("ECM", "fail indicator", new BindingValue(bits.HasFlag(BMSLightBits2.EcmFail)));
            SetValue("Caution", "forward fuel low indicator", new BindingValue(bits.HasFlag(BMSLightBits2.FwdFuelLow)));
            SetValue("Caution", "aft fuel low indicator", new BindingValue(bits.HasFlag(BMSLightBits2.AftFuelLow)));
            SetValue("EPU", "on indicator", new BindingValue(bits.HasFlag(BMSLightBits2.EPUOn)));
            SetValue("JFS", "run indicator", new BindingValue(bits.HasFlag(BMSLightBits2.JFSOn)));
            SetValue("Caution", "second engine compressor indicator", new BindingValue(bits.HasFlag(BMSLightBits2.SEC)));
            SetValue("Caution", "oxygen low indicator", new BindingValue(bits.HasFlag(BMSLightBits2.OXY_LOW)));
            //SetValue("Caution", "probe heat indicator", new BindingValue(bits.HasFlag(BMSLightBits2.PROBEHEAT)));
            SetValue("Caution", "seat arm indicator", new BindingValue(bits.HasFlag(BMSLightBits2.SEAT_ARM)));
            SetValue("Caution", "backup fuel control indicator", new BindingValue(bits.HasFlag(BMSLightBits2.BUC)));
            SetValue("Caution", "fuel oil hot indicator", new BindingValue(bits.HasFlag(BMSLightBits2.FUEL_OIL_HOT)));
            SetValue("Caution", "anti skid indicator", new BindingValue(bits.HasFlag(BMSLightBits2.ANTI_SKID)));
            SetValue("Misc", "tfs engaged indicator", new BindingValue(bits.HasFlag(BMSLightBits2.TFR_ENGAGED)));

            SetValue("Gear Handle", "handle indicator", new BindingValue(bits.HasFlag(BMSLightBits2.GEARHANDLE)));  // TODO This should be under device Landing Gear

            SetValue("Right Eyebrow", "engine indicator", new BindingValue(bits.HasFlag(BMSLightBits2.ENGINE)));

            UpdateBlinkingLightState(bits.HasFlag(BMSLightBits2.PROBEHEAT), blinkBits.HasFlag(BlinkBits.PROBEHEAT), ref _probeheatLastTick, ref _probeheatOnState);
            SetValue("Caution", "probe heat indicator", new BindingValue(_probeheatOnState));

            UpdateBlinkingLightState(bits.HasFlag(BMSLightBits2.AuxSrch), blinkBits.HasFlag(BlinkBits.AuxSrch), ref _auxsrchLastTick, ref _auxsrchOnState);
            SetValue("Aux Threat Warning", "search indicator", new BindingValue(_auxsrchOnState));

            UpdateBlinkingLightState(bits.HasFlag(BMSLightBits2.Launch), blinkBits.HasFlag(BlinkBits.Launch), ref _launchLastTick, ref _launchOnState);
            SetValue("Threat Warning Prime", "launch indicator", new BindingValue(_launchOnState));

            UpdateBlinkingLightState(bits.HasFlag(BMSLightBits2.PriMode), blinkBits.HasFlag(BlinkBits.PriMode), ref _primodeLastTick, ref _primodeOnState);
            SetValue("Threat Warning Prime", "prioirty mode indicator", new BindingValue(_primodeOnState));

            UpdateBlinkingLightState(bits.HasFlag(BMSLightBits2.Unk), blinkBits.HasFlag(BlinkBits.Unk), ref _unkLastTick, ref _unkOnState);
            SetValue("Threat Warning Prime", "unknown mode indicator", new BindingValue(_unkOnState));
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

            SetValue("Threat Warning Prime", "systest indicator", new BindingValue(bits.HasFlag(BMSLightBits3.SysTest)));
        }

        protected void ProcessHsiBits(HsiBits bits, float desiredCourse, float bearingToBeacon, BlinkBits blinkBits, int time)
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
            SetValue("AVTR", "avtr indicator", new BindingValue(bits.HasFlag(HsiBits.AVTR)));

            UpdateBlinkingLightState(bits.HasFlag(HsiBits.OuterMarker), blinkBits.HasFlag(BlinkBits.OuterMarker), ref _outerMarkerLastTick, ref _outerMarkerOnState);
            SetValue("HSI", "Outer marker indicator", new BindingValue(_outerMarkerOnState));

            UpdateBlinkingLightState(bits.HasFlag(HsiBits.MiddleMarker), blinkBits.HasFlag(BlinkBits.MiddleMarker), ref _middleMarkerLastTick, ref _middleMarkerOnState);
            SetValue("HSI", "Middle marker indicator", new BindingValue(_middleMarkerOnState));
        }

        protected void ProcessAltBits(AltBits bits)
        {
            SetValue("Altimeter", "altimeter calibration type", new BindingValue(bits.HasFlag(AltBits.CalType)));
            SetValue("Altimeter", "altimeter pneu flag", new BindingValue(bits.HasFlag(AltBits.PneuFlag)));
        }

        protected void ProcessPowerBits(PowerBits bits)
        {
            SetValue("POWER", "bus power battery", new BindingValue(bits.HasFlag(PowerBits.BusPowerBattery)));
            SetValue("POWER", "bus power emergency", new BindingValue(bits.HasFlag(PowerBits.BusPowerEmergency)));
            SetValue("POWER", "bus power essential", new BindingValue(bits.HasFlag(PowerBits.BusPowerEssential)));
            SetValue("POWER", "bus power non essential", new BindingValue(bits.HasFlag(PowerBits.BusPowerNonEssential)));
            SetValue("POWER", "main generator", new BindingValue(bits.HasFlag(PowerBits.MainGenerator)));
            SetValue("POWER", "standby generator", new BindingValue(bits.HasFlag(PowerBits.StandbyGenerator)));
            SetValue("POWER", "Jetfuel starter", new BindingValue(bits.HasFlag(PowerBits.JetFuelStarter)));
        }

        //protected void ProcessBlinkBits(BlinkBits bits)
        //{
        //    SetValue("Blink", "Outer marker", new BindingValue(bits.HasFlag(BlinkBits.OuterMarker)));
        //    SetValue("Blink", "Middle marker", new BindingValue(bits.HasFlag(BlinkBits.MiddleMarker)));
        //    SetValue("Blink", "probeheat", new BindingValue(bits.HasFlag(BlinkBits.PROBEHEAT)));
        //    SetValue("Blink", "aux search", new BindingValue(bits.HasFlag(BlinkBits.AuxSrch)));
        //    SetValue("Blink", "launch", new BindingValue(bits.HasFlag(BlinkBits.Launch)));
        //    SetValue("Blink", "primary mode", new BindingValue(bits.HasFlag(BlinkBits.PriMode)));
        //    SetValue("Blink", "unknown", new BindingValue(bits.HasFlag(BlinkBits.Unk)));
        //    SetValue("Blink", "elec fault", new BindingValue(bits.HasFlag(BlinkBits.Elec_Fault)));
        //    SetValue("Blink", "oxy brow", new BindingValue(bits.HasFlag(BlinkBits.OXY_BROW)));
        //    SetValue("Blink", "epu on", new BindingValue(bits.HasFlag(BlinkBits.EPUOn)));
        //    SetValue("Blink", "JFS on_slow", new BindingValue(bits.HasFlag(BlinkBits.JFSOn_Slow)));
        //    SetValue("Blink", "JFS on_fast", new BindingValue(bits.HasFlag(BlinkBits.JFSOn_Fast)));
        //}

        
        protected void UpdateBlinkingLightState(bool on, bool blinking, ref DateTime lastTick, ref bool onState)
        {
            if (blinking)
            {
                DateTime current = DateTime.Now;
                TimeSpan span = current - lastTick;

                if (span.Milliseconds > SLOW_BLINK_LENGTH_MS)
                {
                    onState = !onState;
                    lastTick = current;
                }
            }
            else
            {
                onState = on;
            }
        }

        ////https://stackoverflow.com/questions/1878907/the-smallest-difference-between-2-angles
        //private int negMod(int a, int n)
        //{
        //    return (a % n + n) % n;
        //}

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
