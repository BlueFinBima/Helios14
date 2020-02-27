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

namespace GadrocsWorkshop.Helios.Interfaces.Falcon
{
    using GadrocsWorkshop.Helios.Units;
    using System.Collections.Generic;

    abstract class FalconDataExporter
    {
        private FalconInterface _falconInterface;
        private Dictionary<string, HeliosValue> _values = new Dictionary<string, HeliosValue>();

        protected FalconDataExporter(FalconInterface falconInterface)
        {
            _falconInterface = falconInterface;

            AddValue("Altimeter", "altitidue", "Current altitude of the aircraft.", "Altitude in feet.", BindingValueUnits.Feet);
            AddValue("Altimeter", "indicated altitude", "Inidicated barometric altitude. (Depends on calibration)", "Altitiude in feet.", BindingValueUnits.Feet);
            AddValue("Altimeter", "barimetric pressure", "Calibrated barimetric pressure.", "", BindingValueUnits.InchesOfMercury);
            AddValue("ADI", "pitch", "Pitch of the aircraft", "", BindingValueUnits.Radians);
            AddValue("ADI", "roll", "Roll of the aircraft", "", BindingValueUnits.Radians);
            AddValue("ADI", "ils horizontal", "Position of horizontal ils bar.", "(-1 full left, 1 full right)", BindingValueUnits.Numeric);
            AddValue("ADI", "ils vertical", "Position of vertical ils bar.", "(-1 highest, 1 lowest)", BindingValueUnits.Numeric);
            AddValue("HSI", "bearing to beacon", "Compass heading in degrees to the currently selected beacon.", "", BindingValueUnits.Degrees);
            AddValue("HSI", "desired course", "Currently selected desired course in degrees.", "", BindingValueUnits.Degrees);
            AddValue("HSI", "current heading", "Current heading of the aircraft.", "", BindingValueUnits.Degrees);
            AddValue("HSI", "distance to beacon", "Distance to the currently selected beacon.", "", BindingValueUnits.NauticalMiles);
            AddValue("HSI", "desired heading", "Currently selected desired heading.", "", BindingValueUnits.Degrees);
            AddValue("HSI", "course deviation", "Current location of course deviation bar.", "(-1 full left to 1 full right)", BindingValueUnits.Numeric);
            AddValue("VVI", "vertical velocity", "Current vertical velocity of the aircraft.", "", BindingValueUnits.FeetPerSecond);
            AddValue("AOA", "angle of attack", "Current angle of attack of the aircraft.", "", BindingValueUnits.Degrees);
            AddValue("IAS", "mach", "Current mach speed of the aircraft.", "", BindingValueUnits.Numeric);
            AddValue("IAS", "indicated air speed", "Current indicated air speed in knots.", "", BindingValueUnits.Knots);
            AddValue("IAS", "true air speed", "Current true air speed in feet per second.", "", BindingValueUnits.FeetPerSecond);
            AddValue("General", "Gs", "Current g-force load", "", BindingValueUnits.Numeric);
            AddValue("Engine", "nozzle position", "Current afterburner nozzel position.", "Percent open (0-100)", BindingValueUnits.Numeric);
            AddValue("Fuel", "internal fuel", "Amount of fuel in the internal tanks.", "", BindingValueUnits.Pounds);
            AddValue("Fuel", "external fuel", "Amount of fuel in the external tanks.", "", BindingValueUnits.Pounds);
            AddValue("Engine", "fuel flow", "Current fuel flow to the engine.", "", BindingValueUnits.PoundsPerHour);
            AddValue("Engine", "rpm", "Current RPM of the engine.", "Percent (0-103)", BindingValueUnits.RPMPercent);
            AddValue("Engine", "ftit", "Forward turbine intake temperature", "", BindingValueUnits.Celsius);
            AddValue("Landging Gear", "position", "Landing gear current position.", "True for down, false for up", BindingValueUnits.Boolean);
            AddValue("General", "speed brake position", "Speed brake position", "0(Fully Closed) to 1(Fully Open)", BindingValueUnits.Degrees);
            AddValue("General", "speed brake indicator", "Speed brake open indicator.", "True if the speed brake is open at all.", BindingValueUnits.Boolean);
            AddValue("EPU", "fuel", "Remaining EPU fuel.", "Percent (0-100)", BindingValueUnits.Numeric);
            AddValue("Engine", "oil pressure", "Current oil pressure in the engine.", "Percent (0-100)", BindingValueUnits.Numeric);
            AddValue("CMDS", "chaff remaining", "Number chaff charges remaining.", "", BindingValueUnits.Numeric);
            AddValue("CMDS", "flares remaining", "Number of flares remaining.", "", BindingValueUnits.Numeric);
            AddValue("Trim", "roll trim", "Amount of roll trim currently set.", "(-0.5 to 0.5)", BindingValueUnits.Numeric);
            AddValue("Trim", "pitch trim", "Number of flares remaining.", "(-0.5 to 0.5)", BindingValueUnits.Numeric);
            AddValue("Trim", "yaw trim", "Number of flares remaining.", "(-0.5 to 0.5)", BindingValueUnits.Numeric);

            AddValue("Tacan", "ufc tacan chan", "Tacan channel set with the UFC.", "", BindingValueUnits.Numeric);
            AddValue("Tacan", "aux tacan chan", "Tacan channel set with the AUX COM panel.", "", BindingValueUnits.Numeric);

            // HSI Bits
            AddValue("HSI", "to flag", "HSI to flag indicating we are heading to the beacon.", "True if displayed and aircraft is heading towards beacon.", BindingValueUnits.Boolean);
            AddValue("HSI", "from flag", "HSI from flag indicating we are heading away from the beacon.", "True if displayed and aircraft is moving away from the beacon.", BindingValueUnits.Boolean);
            AddValue("HSI", "ils warning flag", "HSI ils warning flag indicating if course steering data is available.", "True if displayed and data is not accurate or available.", BindingValueUnits.Boolean);
            AddValue("HSI", "dme flag", "HSI dem flag indicating distance to beacon is not available.", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("HSI", "off flag", "HSI off flag indicating hsi is not recieving data.", "True if displayed and HSI data is not available.", BindingValueUnits.Boolean);
            AddValue("HSI", "init flag", "HSI init flag", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("ADI", "off flag", "ADI off flag indicating ADI is powered off or not recieving data.", "True if displayed and ADI is off or not recieving data.", BindingValueUnits.Boolean);
            AddValue("ADI", "aux flag", "ADI aux flag", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("ADI", "gs flag", "ADI gs flag", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("ADI", "loc flag", "ADI loc flag", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("Backup ADI", "off flag", "Backup ADI off flag", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("VVI", "off flag", "VVI Off flag indicating VVI is turned off or not receiving data.", "True if displayed.", BindingValueUnits.Boolean);
            AddValue("AOA", "off flag", "AOA Off flag indicating AOA is turned off or not receiving data.", "True if displayed.", BindingValueUnits.Boolean);

            // Lightbits
            AddValue("Left Eyebrow", "master caution indicator", "", "True if master caution light is turned on.", BindingValueUnits.Boolean);
            AddValue("Left Eyebrow", "tf-fail indicator", "", "True if TF switch in MAN TF positoin.", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "engine fire indicator", "", "True if engine fire is detected.", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "hydraulic/oil indicator", "", "True if hydraulic pressure is to high.", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "canopy indicator", "", "True if canopy is not closed or is damaged", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "takeoff landing config indicator", "", "True if configuration is inncorrect for takeoff or landing.", BindingValueUnits.Boolean);
            AddValue("Caution", "stores config indicator", "Caution panel stores config indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("AOA Indexer", "above indicator", "AOA Indexer above indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("AOA Indexer", "on indicator", "AOA Indexer on indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("AOA Indexer", "below indicator", "AOA Indexer below indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Refuel Indexer", "ready indicator", "Refuel Indexer ready indciator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Refuel Indexer", "air/nws indicator", "Refuel Indexer Air/NWS indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Refuel Indexer", "disconnect indicator", "Refuel Indexer disconnect indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "flight control system indicator", "Caution panel flight control indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "leading edge flaps indicator", "Caution panel leading edge flaps indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "engine fault indticator", "Caution panel engine fault indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "overheat indicator", "Caution panel overheat indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "low fuel indicator", "Caution panel low fuel indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "avionics indicator", "Caution panel avionics indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "radar altimeter indicator", "Caution panel radar altimeter indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "iff indicator", "Caution iff indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "ecm indicator", "Caution ecm indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "hook indicator", "Caution panel hook indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "nws fail indicator", "Caution panel nose wheel steering fail indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "cabin pressure indicator", "Caution panel cabin pressure indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Misc", "tfs stanby indicator", "Misc panel Terrain Following(TFS) standby indicator.", "True if lit.", BindingValueUnits.Boolean);

            // Lightbits2
            AddValue("Threat Warning Prime", "handoff indicator", "Threat warning prime handoff dot indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "launch indicator", "Threat warning prime launch indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "prioirty mode indicator", "Threat warning prime priority mode indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "open mode indicator", "Threat warning prime open mode indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "naval indicator", "Threat warning prime naval indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "unknown mode indicator", "Threat warning prime unkown mode indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Threat Warning Prime", "target step indicator", "Threat warning prime target step indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Aux Threat Warning", "search indicator", "Aux threat warning search indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Aux Threat Warning", "activity indicator", "Aux threat warning activity indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Aux Threat Warning", "low altitude indicator", "Aux threat warning low altitude indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Aux Threat Warning", "power indicator", "Aux threat warning system power indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("CMDS", "Go", "CMDS is on and operating normally.", "True if CMDS is on and operating", BindingValueUnits.Boolean);
            AddValue("CMDS", "NoGo", "CMDS is on but a malfunction is present.", "True if CMDS is on but malfunctioning", BindingValueUnits.Boolean);
            AddValue("CMDS", "Degr", "Status message AUTO DEGR should be displayed.", "True if AUTO DEGR should be displayed", BindingValueUnits.Boolean);
            AddValue("CMDS", "Rdy", "Status message DISPENSE RDY should be displayed.", "True if DISPENSE RDY should be displayed", BindingValueUnits.Boolean);
            AddValue("CMDS", "ChaffLo", "Indicates bingo chaff quantity is reached.", "True if bingo quantity reached", BindingValueUnits.Boolean);
            AddValue("CMDS", "FlareLo", "Inidcates bingo flare quantity is reached.", "True if bingo quantity reached", BindingValueUnits.Boolean);
            AddValue("ECM", "power indicator", "ECM power indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("ECM", "fail indicator", "ECM failure indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "forward fuel low indicator", "Caution panel forward fuel low indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "aft fuel low indicator", "Caution panel aft fuel low indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("EPU", "on indicator", "EPU on indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("JFS", "run indicator", "JFS run indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "second engine compressor indicator", "Caution panel second engine compressor indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "oxygen low indicator", "Caution panel oxygen low indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "probe heat indicator", "Caution panel probe heat indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "seat arm indicator", "Caution panel seat not armed indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "backup fuel control indicator", "Caution panel backup fuel control indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "fuel oil hot indicator", "Caution panel oil hot indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "anti skid indicator", "Caution panel anti skid indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Misc", "tfs engaged indicator", "Misc panel Terrain Following(TFS) engaged indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Landing Gear", "handle indicator", "Landing gear handle indicator light.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Right Eyebrow", "engine indicator", "Right eyebrow engine indicator.", "True if lit.", BindingValueUnits.Boolean);

            // Lightbits3
            AddValue("Electronic", "flcs pmg indicator", "Electronic panel flcs pmg indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "main gen indicator", "Electronic panel main generator indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "standby generator indicator", "Electronic panel standby generator indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "epu gen indicator", "Electronic panel epu gen indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "epu pmg indicator", "Electronic panel epu pmg indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "to flcs indicator", "Electronic panel to flcs indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "flcs rly indicator", "Electronic panel flcs rly indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Electronic", "bat fail indicator", "Electronic panel battery fail indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("EPU", "hydrazine indicator", "EPU hydrazine indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("EPU", "air indicator", "EPU air indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "electric bus fail indicator", "Caution panel electric bus fail indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Caution", "lef fault indicator", "Caution panel leading edge fault indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("General", "power off", "Flag indicating if the cockpit has any power.", "True if the cockpit does not have any power.", BindingValueUnits.Boolean);
            AddValue("Landing Gear", "nose gear indicator", "Landing gear panel nose gear indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Landing Gear", "left gear indicator", "Landing gear panel left gear indicator.", "True if lit.", BindingValueUnits.Boolean);
            AddValue("Landing Gear", "right gear indicator", "Landing gear panel right gear indicator.", "True if lit.", BindingValueUnits.Boolean);
        }

        #region Properties

        protected FalconInterface FalconInterface
        {
            get { return _falconInterface; }
        }

        internal abstract RadarContact[] RadarContacts { get; }

        #endregion

        internal void RemoveExportData(FalconInterface falcon)
        {
            foreach (HeliosValue value in _values.Values)
            {
                falcon.Triggers.Remove(value);
                falcon.Values.Remove(value);
            }
            _values.Clear();
        }

        internal abstract void InitData();
        internal abstract void PollData();
        internal abstract void CloseData();

        public static float AngleDelta(float Ang1, float Ang2)
        {
            Ang1 = Ang1 % 360;
            Ang2 = Ang2 % 360;
            if (Ang1 == Ang2)
            {
                return 0.0f; //No angle to compute
            }
            else
            {
                float fDif = (Ang2 - Ang1);//There is an angle to compute
                if (fDif >= 180.0f)
                {
                    fDif = fDif - 180.0f; //correct the half
                    fDif = 180.0f - fDif; //invert the half
                    fDif = 0 - fDif; //reverse direction
                    return fDif;
                }
                else
                {
                    if (fDif <= -180.0f)
                    {
                        fDif = fDif + 180.0f; //correct the half
                        fDif = 180.0f + fDif;
                        return fDif;
                    }
                }
                return fDif;
            }
        }

        protected HeliosValue AddValue(string device, string name, string description, string valueDescription, BindingValueUnit unit)
        {
            HeliosValue value = new HeliosValue(FalconInterface, BindingValue.Empty, device, name, description, valueDescription, unit);
            FalconInterface.Triggers.Add(value);
            FalconInterface.Values.Add(value);
            _values.Add(device + "." + name, value);
            return value;
        }

        protected void SetValue(string device, string name, BindingValue value)
        {
            string key = device + "." + name;
            if (_values.ContainsKey(key))
            {
                _values[key].SetValue(value, false);
            }
        }

        public BindingValue GetValue(string device, string name)
        {
            string key = device + "." + name;
            if (_values.ContainsKey(key))
            {
                return _values[key].Value;
            }
            return BindingValue.Empty;
        }
    }
}
