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

namespace GadrocsWorkshop.Helios.Interfaces.DCS.AV8B
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Interfaces.DCS.AV8B.Functions;
    using GadrocsWorkshop.Helios.Interfaces.DCS.Common;
    using GadrocsWorkshop.Helios.UDPInterface;
    using Microsoft.Win32;
    using System;

    [HeliosInterface("Helios.AV8B", "DCS AV-8B", typeof(AV8BInterfaceEditor), typeof(UniqueHeliosInterfaceFactory))]
    public class AV8BInterface : BaseUDPInterface
    {
        private string _dcsPath;

        private bool _phantomFix;
        private int _phantomLeft;
        private int _phantomTop;

        private long _nextCheck = 0;

        #region Devices
        //  The device list seems to be a moving target at this moment and 2.5.1 saw some changes which altered numbers. 
        //  Need to keep an eye on devices.lua
        private const string ELECTRIC = "1";
        private const string COMM1 = "2";
        private const string COMM2 = "3";
        private const string INTERCOM = "4";
        private const string TACAN = "5";
        private const string AWLS = "6";
        private const string RSC = "7";
        private const string ACNIP = "8";
        private const string DECS = "9";
        private const string ADC = "10";
        private const string MSC = "11";
        private const string NAV_INS = "12";
        private const string VREST = "13";
        private const string NAVFLIR = "14";
        private const string TGTPOD = "15";
        private const string DMT = "16";
        private const string RWR = "17";
        private const string RWRCONTROL = "18";
        private const string FLIGHTINSTRUMENTS = "19";
        private const string EDP = "20";
        private const string FQIS = "21";
        private const string HUDCONTROL = "22";
        private const string UFCCONTROL = "23";
        private const string ODUCONTROL = "24";
        private const string EHSD_MAP = "25";
        private const string MPCD_LEFT = "26";
        private const string MPCD_RIGHT = "27";
        private const string FLIGHTCONTROLS = "28";
        private const string SMC = "29";
        private const string EWS = "30";
        private const string CBTSYST = "31";
        private const string TVWPNS = "32";
        private const string LTEXT = "33";
        private const string LTINT = "34";
        private const string LTWCA = "35";
        private const string ECS = "36";
        private const string HYDRAULIC = "37";
        private const string NETWORKINTERFACE = "38";
        private const string ANVIS9 = "39";
        private const string AUTOSTART = "40";
        #endregion

        public AV8BInterface()
            : base("DCS AV8B")
        {
            DCSConfigurator config = new DCSConfigurator("DCSAV8B", DCSPath);
            config.ExportConfigPath = "Config\\Export";
            config.ExportFunctionsPath = "pack://application:,,,/Helios;component/Interfaces/DCS/AV-8B/ExportFunctions.lua";
            Port = config.Port;
            _phantomFix = config.PhantomFix;
            _phantomLeft = config.PhantomFixLeft;
            _phantomTop = config.PhantomFixTop;

            #region Left MCFD
            AddFunction(new PushButton(this, MPCD_LEFT, "3200", "3200", "Left MFCD", "OSB01", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3201", "3201", "Left MFCD", "OSB02", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3202", "3202", "Left MFCD", "OSB03", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3203", "3203", "Left MFCD", "OSB04", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3204", "3204", "Left MFCD", "OSB05", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3205", "3205", "Left MFCD", "OSB06", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3206", "3206", "Left MFCD", "OSB07", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3207", "3207", "Left MFCD", "OSB08", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3208", "3208", "Left MFCD", "OSB09", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3209", "3209", "Left MFCD", "OSB10", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3210", "3210", "Left MFCD", "OSB11", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3211", "3211", "Left MFCD", "OSB12", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3212", "3212", "Left MFCD", "OSB13", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3213", "3213", "Left MFCD", "OSB14", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3214", "3214", "Left MFCD", "OSB15", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3215", "3215", "Left MFCD", "OSB16", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3216", "3216", "Left MFCD", "OSB17", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3217", "3217", "Left MFCD", "OSB18", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3218", "3218", "Left MFCD", "OSB19", "1", "0", "%1d"));
            AddFunction(new PushButton(this, MPCD_LEFT, "3219", "3219", "Left MFCD", "OSB20", "1", "0", "%1d"));
            AddFunction(new Axis(this, MPCD_LEFT, "3194", "3194", 0.1d, 0d, 1d, "Left MFCD", "Off/Brightness Control"));
            #endregion

            #region Right MCFD
            AddFunction(new PushButton(this, MPCD_RIGHT, "3224", "3224", "Right MFCD", "OSB01"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3225", "3225", "Right MFCD", "OSB02"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3226", "3226", "Right MFCD", "OSB03"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3227", "3227", "Right MFCD", "OSB04"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3228", "3228", "Right MFCD", "OSB05"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3229", "3229", "Right MFCD", "OSB06"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3230", "3230", "Right MFCD", "OSB07"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3231", "3231", "Right MFCD", "OSB08"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3232", "3232", "Right MFCD", "OSB09"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3233", "3233", "Right MFCD", "OSB10"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3234", "3234", "Right MFCD", "OSB11"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3235", "3235", "Right MFCD", "OSB12"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3236", "3236", "Right MFCD", "OSB13"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3237", "3237", "Right MFCD", "OSB14"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3238", "3238", "Right MFCD", "OSB15"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3239", "3239", "Right MFCD", "OSB16"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3240", "3240", "Right MFCD", "OSB17"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3241", "3241", "Right MFCD", "OSB18"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3242", "3242", "Right MFCD", "OSB19"));
            AddFunction(new PushButton(this, MPCD_RIGHT, "3243", "3243", "Right MFCD", "OSB20"));
            AddFunction(new Axis(this, MPCD_RIGHT, "3195", "3195", 0.1d, 0d, 1d, "Right MFCD", "Off/Brightness Control"));

            #endregion

            #region UFC
            AddFunction(new PushButton(this, UFCCONTROL, "3302", "3302", "UFC", "1"));
            AddFunction(new PushButton(this, UFCCONTROL, "3303", "3303", "UFC", "2/N"));
            AddFunction(new PushButton(this, UFCCONTROL, "3304", "3304", "UFC", "3"));
            AddFunction(new PushButton(this, UFCCONTROL, "3306", "3306", "UFC", "4/W"));
            AddFunction(new PushButton(this, UFCCONTROL, "3307", "3307", "UFC", "5"));
            AddFunction(new PushButton(this, UFCCONTROL, "3308", "3308", "UFC", "6/E"));
            AddFunction(new PushButton(this, UFCCONTROL, "3310", "3310", "UFC", "7"));
            AddFunction(new PushButton(this, UFCCONTROL, "3311", "3311", "UFC", "8/S"));
            AddFunction(new PushButton(this, UFCCONTROL, "3312", "3312", "UFC", "9"));
            AddFunction(new PushButton(this, UFCCONTROL, "3315", "3315", "UFC", "0"));
            AddFunction(new PushButton(this, UFCCONTROL, "3316", "3316", "UFC", "."));
            AddFunction(new PushButton(this, UFCCONTROL, "3313", "3313", "UFC", "-"));
            AddFunction(new PushButton(this, UFCCONTROL, "3314", "3314", "UFC", "Enter"));
            AddFunction(new PushButton(this, UFCCONTROL, "3305", "3305", "UFC", "Clear"));
            AddFunction(new PushButton(this, UFCCONTROL, "3294", "3294", "UFC", "Timer"));
            AddFunction(new PushButton(this, UFCCONTROL, "3324", "3324", "UFC", "Altitude"));
            AddFunction(new PushButton(this, UFCCONTROL, "3318", "3318", "UFC", "IFF"));
            AddFunction(new PushButton(this, UFCCONTROL, "3319", "3319", "UFC", "TACAN"));
            AddFunction(new PushButton(this, UFCCONTROL, "3320", "3320", "UFC", "All Weather Landing"));
            AddFunction(new PushButton(this, UFCCONTROL, "3317", "3317", "UFC", "On/Off"));
            AddFunction(new PushButton(this, UFCCONTROL, "3325", "3325", "UFC", "Emmission Control"));
            AddFunction(new PushButton(this, UFCCONTROL, "3296", "3296", "UFC", "Target of Opportunity"));
            AddFunction(new PushButton(this, UFCCONTROL, "3322", "3322", "UFC", "Waypoint Overfly"));
            AddFunction(new PushButton(this, UFCCONTROL, "3321", "3321", "UFC", "Weapons"));
            AddFunction(new PushButton(this, UFCCONTROL, "3323", "3323", "UFC", "RADAR Beacon"));
            AddFunction(new PushButton(this, UFCCONTROL, "3297", "3297", "UFC", "I/P"));
            AddFunction(new PushButton(this, UFCCONTROL, "3309", "3309", "UFC", "Save"));
            AddFunction(new PushButton(this, LTWCA, "3198", "3198", "Caution/Warning", "Master Caution Button"));
            AddFunction(new FlagValue(this, "196", "Caution/Warning", "Master Caution", "Master Caution indicator"));
            AddFunction(new PushButton(this, LTWCA, "3199", "3199", "Caution/Warning", "Master Warning Button"));
            AddFunction(new FlagValue(this, "197", "Caution/Warning", "Master Warning", "Master warning indicator"));
            AddFunction(new Axis(this, UFCCONTROL, "3295", "3295", 0.1d, 0d, 1d, "UFC", "Display Brightness"));
            AddFunction(new Axis(this, UFCCONTROL, "3298", "3298", 0.1d, 0d, 1d, "UFC", "Comm 1 Volume"));
            AddFunction(new Axis(this, UFCCONTROL, "3299", "3299", 0.1d, 0d, 1d, "UFC", "Comm 2 Volume"));
            AddFunction(new Axis(this, UFCCONTROL, "3300", "3300", 0.1d, 0d, 1d, "UFC", "Comm 1 Channel selector"));
            AddFunction(new Axis(this, UFCCONTROL, "3301", "3301", 0.1d, 0d, 1d, "UFC", "Comm 2 Channel selector"));
            //       public IndicatorPushButton(BaseUDPInterface sourceInterface, string deviceId, string buttonId, string argId, string device, string name)
            //       public FlagValue(BaseUDPInterface sourceInterface, string id, string device, string name, string description)
            //       public PushButton(BaseUDPInterface sourceInterface, string deviceId, string buttonId, string argId, string device, string name)
            AddFunction(Switch.CreateThreeWaySwitch(this, HUDCONTROL, "3288", "3288", "0", "Norm", "0.5", "Reject 1", "1", "Reject 2", "HUD Control", "Declutter switch", "%1d"));
            AddFunction(new Axis(this, HUDCONTROL, "3289", "3289", 0.1d, 0d, 1d, "HUD Control", "Off/Brightness control"));
            AddFunction(Switch.CreateThreeWaySwitch(this, HUDCONTROL, "3290", "3290", "0", "Day", "0.5", "Auto", "1", "Night", "HUD Control", "Display Mode switch", "%1d"));
            AddFunction(new Axis(this, HUDCONTROL, "3291", "3291", 0.1d, 0d, 1d, "HUD Control", "Video Brightness"));
            AddFunction(new Axis(this, HUDCONTROL, "3292", "3292", 0.1d, 0d, 1d, "HUD Control", "Video Contrast"));
            AddFunction(Switch.CreateToggleSwitch(this, HUDCONTROL, "3293", "3293", "0", "RDR", "1", "Baro", "HUD Control", "Altimeter Mode Switch", "%1d"));
            #endregion

            #region Master Modes
            AddFunction(new PushButton(this, MSC, "3282", "3282", "Master Modes", "NAV Button"));
            AddFunction(new FlagValue(this, "283", "Master Modes", "Nav Indicator", "Nav Master Mode Indicator"));
            AddFunction(new PushButton(this, MSC, "3284", "3284", "Master Modes", "VSTOL Button"));
            AddFunction(new FlagValue(this, "285", "Master Modes", "VSTOL Indicator", "VSTOL Master Mode Indicator"));
            AddFunction(new PushButton(this, MSC, "3280", "3280", "Master Modes", "A/G Button"));
            AddFunction(new FlagValue(this, "281", "Master Modes", "A/G Indicator", "A/G Master Mode Indicator"));
            #endregion

            #region ODU
            AddFunction(new PushButton(this, ODUCONTROL, "3250", "3250", "ODU", "1"));
            AddFunction(new PushButton(this, ODUCONTROL, "3251", "3251", "ODU", "2"));
            AddFunction(new PushButton(this, ODUCONTROL, "3252", "3252", "ODU", "3"));
            AddFunction(new PushButton(this, ODUCONTROL, "3248", "3248", "ODU", "4"));
            AddFunction(new PushButton(this, ODUCONTROL, "3249", "3249", "ODU", "5"));
            #endregion

            #region Threat Indicators
            AddFunction(new FlagValue(this, "276", "Threat Indicators", "SAM", "SAM launch detected"));
            AddFunction(new FlagValue(this, "277", "Threat Indicators", "CW", "CW Ground Tracking (Continuous Wave) radar is locked on aircraft"));
            AddFunction(new FlagValue(this, "278", "Threat Indicators", "AI", "Air Intercept radar is locked on aircraft. Flashes if launch detected"));
            AddFunction(new FlagValue(this, "279", "Threat Indicators", "AAA", "AAA gun radar is locked on aircraft"));
            #endregion

            #region Caution Indicators
            AddFunction(new FlagValue(this, "326", "Caution Indicators", "L Fuel Indicator", ""));
            AddFunction(new FlagValue(this, "327", "Caution Indicators", "R fuel Indicator", ""));
            AddFunction(new FlagValue(this, "328", "Caution Indicators", "15 Sec Indicator", "JPT above normal lift rating"));
            AddFunction(new FlagValue(this, "329", "Caution Indicators", "MFS", "Manual Fuel System on"));
            AddFunction(new FlagValue(this, "330", "Caution Indicators", "Bingo", "Bingo Warning Light"));
            AddFunction(new FlagValue(this, "331", "Caution Indicators", "H2O", "Less than 15 secs water left"));
            #endregion

            #region Warning Indicators
            AddFunction(new FlagValue(this, "334", "Warning Indicators", "Fire", "Fire in engine"));
            AddFunction(new FlagValue(this, "335", "Warning Indicators", "LAW", "Low alt warning"));
            AddFunction(new FlagValue(this, "336", "Warning Indicators", "Flaps Warning Light", "Flap System Failure"));
            AddFunction(new FlagValue(this, "337", "Warning Indicators", "L Tank", "Left tank over pressure"));
            AddFunction(new FlagValue(this, "338", "Warning Indicators", "R Tank", "Right Tank Over pressure"));
            AddFunction(new FlagValue(this, "339", "Warning Indicators", "HYD", "Both hydraulics failed"));
            AddFunction(new FlagValue(this, "340", "Warning Indicators", "Gear", "Gear Warning Light"));
            AddFunction(new FlagValue(this, "341", "Warning Indicators", "OT", "Over temp JPT"));
            AddFunction(new FlagValue(this, "342", "Warning Indicators", "JPTL", "JPTL Control Fail"));
            AddFunction(new FlagValue(this, "343", "Warning Indicators", "EFC", "All Digital engine controls failed"));
            AddFunction(new FlagValue(this, "344", "Warning Indicators", "GEN", "AC Generator offline"));
            #endregion

            #region Refueling Indicators
            AddFunction(new FlagValue(this, "750", "Refueling Indicators", "Canopy Left", "Refueling Left Tank"));
            AddFunction(new FlagValue(this, "751", "Refueling Indicators", "Canopy Ready", "Refueling Ready light"));
            AddFunction(new FlagValue(this, "752", "Refueling Indicators", "Canopy Right", "Refuelng Right Light"));
            #endregion

            #region Fuel Quantity Indicator System
            AddFunction(new RotaryEncoder(this, FQIS, "3380", "-1", 1d, "Fuel Quantity", "Bingo Fuel Set Knob"));
            AddFunction(new Switch(this, FQIS, "3379", new SwitchPosition[] { new SwitchPosition("-0.99", "BIT", "3379"), new SwitchPosition("-0.66", "FEED", "3379"), new SwitchPosition("-0.33", "TOTAL", "3379"), new SwitchPosition("0.0", "INT", "3379"), new SwitchPosition("0.33", "WING", "3379"), new SwitchPosition("0.66", "INBD", "3379"), new SwitchPosition("0.99", "OUTBD", "3379") }, "Fuel Quantity", "Fuel Totaliser Selector", "%0.1f"));
            AddFunction(new Digits4Display(this, FQIS, "2011", "FQIS", "Left Tank display", "Fuel left tank quantity"));
            AddFunction(new Digits4Display(this, FQIS, "2012", "FQIS", "Right Tank display", "Fuel right tank quantity"));
            AddFunction(new Digits4Display(this, FQIS, "2013", "FQIS", "Bingo value display", "Fuel Bingo amount"));
            AddFunction(new FuelTotalDisplay(this));
            #endregion

            #region ECM
            AddFunction(new Switch(this, RWRCONTROL, "3273", new SwitchPosition[] { new SwitchPosition("0.0", "Off", "3273"), new SwitchPosition("0.3", "posn 1", "3273"), new SwitchPosition("0.4", "posn 2", "3273"), new SwitchPosition("0.5", "posn 3", "3273"), new SwitchPosition("0.6", "posn 4", "3273"), new SwitchPosition("0.7", "posn 5", "3273"), new SwitchPosition("0.8", "posn 6", "3273"), new SwitchPosition("0.9", "posn 7", "3273"), new SwitchPosition("1.0", "posn 8", "3273") }, "RWR / ECM", "Off/Volume", "%0.1f"));
            AddFunction(new Switch(this, EWS, "3274", new SwitchPosition[] { new SwitchPosition("0.0", "OFF", "3274"),new SwitchPosition("0.25", "AUTO", "3274"), new SwitchPosition("0.50", "UP", "3274"), new SwitchPosition("0.75", "Down", "3274"), new SwitchPosition("1.00", "RWR", "3274") }, "RWR / ECM", "Decoy Dispenser Control", "%0.1f"));
            AddFunction(new Switch(this, EWS, "3275", new SwitchPosition[] { new SwitchPosition("0.0", "OFF", "3275"),new SwitchPosition("0.25", "STBY", "3275"), new SwitchPosition("0.50", "BIT", "3275"), new SwitchPosition("0.75", "RPT", "3275"), new SwitchPosition("1.00", "RPT", "3275")}, "RWR / ECM", "Jammer Control", "%0.1f"));
            #endregion

            #region Advisory indicators
            AddFunction(new FlagValue(this, "560", "Advisory Indicators", "OXY", "OBOGS malfunction"));
            AddFunction(new FlagValue(this, "561", "Advisory Indicators", "WSHLD", "Windshield hot"));
            AddFunction(new FlagValue(this, "562", "Advisory Indicators", "HYD 1", "HYD 1 pressure ≤ 1400 psi"));
            AddFunction(new FlagValue(this, "563", "Advisory Indicators", "HYD 2", "HYD 2 pressure ≤ 1400 psi"));
            AddFunction(new FlagValue(this, "564", "Advisory Indicators", "L PUMP", "Left fuel boost pump pressure low"));
            AddFunction(new FlagValue(this, "565", "Advisory Indicators", "R PUMP", "Right fuel boost pump pressure low"));
            AddFunction(new FlagValue(this, "566", "Advisory Indicators", "L TRANS", "Low air pressure to the left feeder tank"));
            AddFunction(new FlagValue(this, "567", "Advisory Indicators", "R TRANS", "Low air pressure to the right feeder tank"));
            AddFunction(new FlagValue(this, "568", "Advisory Indicators", "FLAPS 1", "Flaps 1 channel failed"));
            AddFunction(new FlagValue(this, "569", "Advisory Indicators", "FLAPS 2", "Flaps 2 channel failed"));
            AddFunction(new FlagValue(this, "570", "Advisory Indicators", "AUT FLP", "Auto flap mode or ADC failed"));
            AddFunction(new FlagValue(this, "571", "Advisory Indicators", "PROP", "Fuel proportioner off or failed"));
            AddFunction(new FlagValue(this, "572", "Advisory Indicators", "LIDS", "LIDS not in correct position"));
            AddFunction(new FlagValue(this, "573", "Advisory Indicators", "OIL", "Oil pressure low"));
            AddFunction(new FlagValue(this, "574", "Advisory Indicators", "APU GEN", "APU selected and emergency generator failed"));
            AddFunction(new FlagValue(this, "575", "Advisory Indicators", "Blank 1", "Unused"));
            AddFunction(new FlagValue(this, "576", "Advisory Indicators", "GPS", "GPS not valid"));
            AddFunction(new FlagValue(this, "577", "Advisory Indicators", "DEP RES", "Departure resistance reduced"));
            AddFunction(new FlagValue(this, "578", "Advisory Indicators", "DC", "Main transformer-rectifier failed"));
            AddFunction(new FlagValue(this, "579", "Advisory Indicators", "STBY TRU", "Standby TRU inoperative or off line"));
            AddFunction(new FlagValue(this, "580", "Advisory Indicators", "CS COOL", "Cockpit avionics cooling fan failed"));
            AddFunction(new FlagValue(this, "581", "Advisory Indicators", "LOAD", "Fuel asymmetry over VL limit"));
            AddFunction(new FlagValue(this, "582", "Advisory Indicators", "CANOPY", "Canopy not closed and locked"));
            AddFunction(new FlagValue(this, "583", "Advisory Indicators", "INS", "INS aligning or failed"));
            AddFunction(new FlagValue(this, "584", "Advisory Indicators", "SKID", "Anti-Skid System Malfunction"));
            AddFunction(new FlagValue(this, "585", "Advisory Indicators", "EFC", "DECU 1 or DECU 2 has failed"));
            AddFunction(new FlagValue(this, "586", "Advisory Indicators", "NWS", "Nosewheel Steering malfunction"));
            AddFunction(new FlagValue(this, "587", "Advisory Indicators", "AFC", "AFC malfunction or AFC deselected"));
            AddFunction(new FlagValue(this, "588", "Advisory Indicators", "C*AUT", "Computed delivery mode (AUTO and CCIP) not available"));
            AddFunction(new FlagValue(this, "589", "Advisory Indicators", "H2O SEL", "Over 250 knots and water switch not OFF"));
            AddFunction(new FlagValue(this, "590", "Advisory Indicators", "APU", "APU operating"));
            AddFunction(new FlagValue(this, "591", "Advisory Indicators", "PITCH", "Pitch stab aug off or failed"));
            AddFunction(new FlagValue(this, "592", "Advisory Indicators", "IFF", "Mode 4 off, not zeroized or not responding"));
            AddFunction(new FlagValue(this, "593", "Advisory Indicators", "SPD BRK", "Gear up and speed brake extended. Gear down and speed brake not 250"));
            AddFunction(new FlagValue(this, "594", "Advisory Indicators", "DROOP", "Ailerons dropped"));
            AddFunction(new FlagValue(this, "595", "Advisory Indicators", "ROLL", "Roll stab aug off or failed"));
            AddFunction(new FlagValue(this, "596", "Advisory Indicators", "AFT BAY", "Aft avionics bay ECS failed"));
            AddFunction(new FlagValue(this, "597", "Advisory Indicators", "AV BIT", "Refuelng Right Light"));
            AddFunction(new FlagValue(this, "598", "Advisory Indicators", "Blank 2", "Unused"));
            AddFunction(new FlagValue(this, "599", "Advisory Indicators", "YAW", "Yaw stab aug off or failed"));
            AddFunction(new FlagValue(this, "600", "Advisory Indicators", "CW NOGO", "Jammer Failure. Cannot jam CW radars"));
            AddFunction(new FlagValue(this, "601", "Advisory Indicators", "P JAM", "Jammer Pod Active: Jamming Pulse-Doppler radar signals"));
            AddFunction(new FlagValue(this, "602", "Advisory Indicators", "JMR HOT", "Jammer pod overtemp"));
            AddFunction(new FlagValue(this, "603", "Advisory Indicators", "ENG EXC", "Engine overspeed, overtemperature or over g was detected"));
            AddFunction(new FlagValue(this, "604", "Advisory Indicators", "P NOGO", "Jammer Failure. Cannot jam Pulse-Doppler radars"));
            AddFunction(new FlagValue(this, "605", "Advisory Indicators", "CW JAM", "Jammer Pod Active: Jamming CW radar signals"));
            AddFunction(new FlagValue(this, "606", "Advisory Indicators", "REPLY", "IFF responding to Mode 4 interrogation"));
            #endregion

            #region Landing Gear
            AddFunction(new FlagValue(this, "446", "Landing Gear", "Gear - Lever", "Red Gear Lever"));
            AddFunction(new FlagValue(this, "462", "Landing Gear", "Nose Wrn", "Nose Gear Warning"));
            AddFunction(new FlagValue(this, "463", "Landing Gear", "Nose", "Nose Gear Ready"));
            AddFunction(new FlagValue(this, "464", "Landing Gear", "Left Wrn", "Left Wheel Warning"));
            AddFunction(new FlagValue(this, "465", "Landing Gear", "Left", "Left Wheel Ready"));
            AddFunction(new FlagValue(this, "466", "Landing Gear", "Right Wrn", "Right Wheel Warning"));
            AddFunction(new FlagValue(this, "467", "Landing Gear", "Right", "Right Wheel Ready"));
            //AddFunction(new FlagValue(this, "468", "Landing Gear", "Main Wrn", "Main Gear Warning"));
            //AddFunction(new FlagValue(this, "469", "Landing Gear", "Main", "Main Gear Ready"));
            AddFunction(new FlagValue(this, "469", "Landing Gear", "Main Wrn", "Main Gear Warning"));   // I think the codes are switched for this and might get corrected in future by Bazbam
            AddFunction(new FlagValue(this, "468", "Landing Gear", "Main", "Main Gear Ready"));         // I think the codes are switched for this and might get corrected in future by Bazbam
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3461", "3461", "1", "Gear up", "0", "Gear down", "Landing Gear", "lever", "%1d"));
            #endregion

            #region Left Hand Advisory Indicators
            AddFunction(new FlagValue(this, "451", "LH Flaps & Water", "SEL indicator", "Combat thrust limiter selected"));
            AddFunction(new FlagValue(this, "452", "LH Flaps & Water", "CMBT indicator", "Combat thrust activated. Flashes after 2 ½ minutes"));
            AddFunction(new FlagValue(this, "453", "LH Flaps & Water", "STO indicator", "Flap switch in STOL"));
            AddFunction(Switch.CreateThreeWaySwitch(this, DECS, "3449", "3449", "1", "TO", "0.5", "Off", "0", "VSTOL", "LH Flaps & Water", "H2O Mode Switch", "%1d"));
            AddFunction(new PushButton(this, DECS, "3450", "3450", "LH Flaps & Water", "Combat Thrust Button"));
            AddFunction(Switch.CreateThreeWaySwitch(this, VREST, "3454", "3454", "1", "VSTOL", "0.5", "Auto", "0", "Cruise", "LH Flaps & Water", "Flaps Mode Switch", "%1d"));
            AddFunction(new Digits2Display(this, SMC, "2014", "LH Flaps & Water", "Flaps position", "Position of the flaps in degrees"));
            AddFunction(new PushButton(this, VREST, "3460", "3460", "LH Flaps & Water", "Flaps BIT"));
            AddFunction(Switch.CreateThreeWaySwitch(this, VREST, "3457", "3457", "0", "Off", "0.5", "On", "1", "Reset", "LH Flaps & Water", "Flaps Power Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, FLIGHTCONTROLS, "3459", "3459", "1", "NWS", "0.5", "On", "0", "Test", "LH Flaps & Water", "Anti-Skid Switch", "%1d"));
            #endregion

            #region Centre Console
            AddFunction(new Axis(this, NAV_INS, "3364", "3464", 0.1d, 0d, 1d, "Centre Console", "NAV Course Setting"));
            AddFunction(new Axis(this, ADC, "3653", "3653", 0.1d, 0d, 1d, "Centre Console", "Barometric Pressure Calibration"));
            //elements["PTN_351"] = default_button_lever(_("Backup ADI Cage/Pitch Adjust Knob"), devices.FLIGHTINSTRUMENTS, inst_commands.ADI_Cage, inst_commands.Knob_ADI, 350, 351)

            //-- Misc Switch Panel
            AddFunction(Switch.CreateToggleSwitch(this, NAVFLIR, "3422", "3422", "0", "Off", "1", "Auto", "Centre Console", "Video Recorder System Mode Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, NAVFLIR, "3423", "3423", "0", "HUD", "1", "MPCD", "Centre Console", "Video Recorder System Display Selector Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DMT, "3424", "3424", "1", "DMT", "0", "Off", "Centre Console", "DMT Toggle On/Off", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, MSC, "3425", "3425", "1", "DP Prim", "0.5", "Auto", "0", "Alter", "Centre Console", "Dual Processor Mode Selector Switch", "%.1f"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTINSTRUMENTS, "3426", "3426", "1", "PRB H T", "0", "Auto", "Centre Console", "Probe Heat Mode Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, MSC, "3427", "3427", "1", "Mission Computer OVerride", "0.5", "Auto", "0", "Off", "Centre Console", "Mission Computer Mode Switch", "%.1f"));
            AddFunction(Switch.CreateToggleSwitch(this, NAVFLIR, "3429", "3429", "1", "FLIR", "0", "Off", "Centre Console", "FLIR Power Switch", "%1d"));
            AddFunction(new Switch(this, NAV_INS, "3421", new SwitchPosition[] { new SwitchPosition("0.0", "Off", "3421"), new SwitchPosition("0.1", "Sea", "3421"), new SwitchPosition("0.2", "INS GND", "3421"), new SwitchPosition("0.3", "Nav", "3421"), new SwitchPosition("0.4", "IFA", "3421"), new SwitchPosition("0.5", "Gyro", "3421"), new SwitchPosition("0.6", "GB", "3421"), new SwitchPosition("0.7", "Test", "3421") }, "Centre Console", "INS Mode Switch", "%.1f"));

            #endregion

            #region Throttle Quadrant
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3481", "3481", "1", "On", "0", "Off", "Throttle Quadrant", "JPTL Switch", "%1d"));
            AddFunction(new Switch(this, FLIGHTCONTROLS, "3483", new SwitchPosition[] { new SwitchPosition("0.0", "Left", "3483"), new SwitchPosition("0.5", "Centre", "3483"), new SwitchPosition("1.0", "Right", "3483") }, "Throttle Quadrant", "Rudder Trim Switch", "%.1f"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3482", "3482", "1", "On", "0", "Off", "Throttle Quadrant", "EMS Button", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3484", "3484", "1", "On", "0", "Off", "Throttle Quadrant", "Manual Fuel Switch", "%1d"));
            AddFunction(new Axis(this, FLIGHTCONTROLS, "3485", "3485", 0.1d, 0d, 1d, "Throttle Quadrant", "Throttle Lever Friction Knob"));
            AddFunction(new Axis(this, FLIGHTCONTROLS, "3486", "3486", 0.1d, 0d, 1d, "Throttle Quadrant", "Nozzle Lever Friction Knob"));
            AddFunction(new Axis(this, DECS, "3490", "3490", 0.1d, 0d, 1d, "Throttle Quadrant", "Throttle Cutoff Lever"));
            AddFunction(new Axis(this, FLIGHTCONTROLS, "3489", "3489", 0.1d, 0d, 1d, "Throttle Quadrant", "Parking Brake Lever"));
            AddFunction(new Axis(this, VREST, "3487", "3487", 0.1d, 0d, 1d, "Throttle Quadrant", "Nozzle Control Lever"));
            AddFunction(new Axis(this, VREST, "3488", "3488", 0.1d, 0d, 1d, "Throttle Quadrant", "STO Stop Lever"));

            #endregion


            #region Left Hand Left Hand Switches Fuel External Lights SAAHS
            ////         --Trim Panel
            AddFunction(Switch.CreateThreeWaySwitch(this, FLIGHTCONTROLS, "3471", "3471", "1", "Test", "0.5", "On", "0", "Off", "SAAHS", "RPS/YAW Trim Switch", "%.1f"));
            AddFunction(Switch.CreateThreeWaySwitch(this, FLIGHTCONTROLS, "3472", "3472", "1", "Approach", "0.5", "Hover", "0", "Off", "SAAHS", "Trim Mode Switch", "%.1f"));
            //AddFunction(Switch.CreateThreeWaySwitch(this, FLIGHTCONTROLS, "3483", "3483", "1", "Left", "0.5", "Centre", "0", "Right", "SAAHS", "Rudder trim switch", "%1d"));

            CalibrationPointCollectionDouble trimScale = new CalibrationPointCollectionDouble(-1.0d, -10d, 1.0d, 10d);
            AddFunction(new ScaledNetworkValue(this, "473", trimScale, "SAAHS", "Aileron trim", "Position in degrees","", BindingValueUnits.Degrees));  // values at -1 to 1
            AddFunction(new ScaledNetworkValue(this, "474", trimScale, "SAAHS", "Rudder trim", "Position in degrees","", BindingValueUnits.Degrees));  // values at -1 to 1
            //         --SAAHS Panel
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3476", "3476", "1", "Hold", "0", "Off", "SAAHS", "Altitude hold switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, FLIGHTCONTROLS, "3477", "3477", "1", "On", "0.5", "Off", "0", "Reset", "SAAHS", "AFC Switch", "%.1f"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3475", "3475", "1", "On", "0", "Off", "SAAHS", "Q Feel switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3478", "3478", "1", "On", "0", "Off", "SAAHS", "SAS Yaw Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3479", "3479", "1", "On", "0", "Off", "SAAHS", "SAS Roll Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3480", "3480", "1", "On", "0", "Off", "SAAHS", "SAS Pitch Switch", "%1d"));
            //         --Fuel Panel
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3504", "3504", "1", "On", "0", "Off", "Fuel", "Fuel Proportioner", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3508", "3508", "1", "Dump", "0", "Off", "Fuel", "Fuel Dump L Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3509", "3509", "1", "Dump", "0", "Off", "Fuel", "Fuel Dump R Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, DECS, "3505", "3505", "1", "Norm", "0.5", "Off", "0", "DC Oper", "Fuel", "Fuel Pump L Switch", "%.1f"));
            AddFunction(Switch.CreateThreeWaySwitch(this, DECS, "3506", "3506", "1", "Norm", "0.5", "Off", "0", "DC Oper", "Fuel", "Fuel Pump R Switch", "%.1f"));
            AddFunction(Switch.CreateThreeWaySwitch(this, DECS, "3507", "3507", "1", "Press", "0.5", "Out", "0", "In", "Fuel", "Air Refueling Probe Switch", "%.1f"));

            //          -- External Lights Panel
            AddFunction(Switch.CreateThreeWaySwitch(this, LTEXT, "3472", "3472", "1", "Bright", "0.5", "Dim", "0", "Off", "External Lights", "Landing/Taxi Lights Switch", "%.1f"));
            AddFunction(Switch.CreateThreeWaySwitch(this, LTEXT, "3503", "3503", "1", "Bright", "0.5", "Dim", "0", "Off", "External Lights", "External Lights Mode Switch", "%.1f"));
            AddFunction(new Axis(this, LTEXT, "3510", "3510", 0.1d, 0d, 1d, "External Lights", "Formation Lights brightness"));
            AddFunction(Switch.CreateThreeWaySwitch(this, LTEXT, "3511", "3511", "1", "Bright", "0.5", "Dim", "0", "Off", "External Lights", "Position Lights Switch", "%.1f"));
            AddFunction(Switch.CreateToggleSwitch(this, LTEXT, "3512", "3512", "1", "On", "0", "Off", "External Lights", "Anti-Collision Lights Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, LTEXT, "3513", "3513", "1", "On", "0", "Off", "External Lights", "External Auxiliary Lights Switch", "%1d"));

            //          -- Pilot Service Panel
            AddFunction(Switch.CreateToggleSwitch(this, ECS, "3514", "3514", "1", "On", "0", "Off", "Pilot Service", "Oxygen Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3515", "3515", "1", "Dump", "0", "Off", "Pilot Service", "H2O Dump Switch", "%1d"));
            // need proper labels
            AddFunction(Switch.CreateToggleSwitch(this, VREST, "3516", "3516", "1", "On", "0", "Off", "Pilot Service", "LIDS Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3517", "3517", "1", "On", "0", "Off", "Pilot Service", "ENG RPM Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3518", "3518", "1", "On", "0", "Off", "Pilot Service", "EFC Switch", "%1d"));

            #endregion

            #region Stores Management Controller
            AddFunction(Switch.CreateToggleSwitch(this, SMC, "3287", "3287", "1", "Armed", "0", "Safe", "Stores Management", "Master Arm Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3396", "3396", "1", "Up", "0", "Off", "-1", "Dn", "Stores Management", "Armament Mode control", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3397", "3397", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Fuzing control", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3398", "3398", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Quantity Tens", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3399", "3399", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Quantity Units", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3400", "3400", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Multiple Release", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3401", "3401", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Release interval hundreds", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3402", "3402", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Release interval tens", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, SMC, "3403", "3403", "1", "Increase", "0", "Off", "-1", "decrease", "Stores Management", "Armament Release interval units", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, SMC, "3420", "3420", "1", "Cooling", "0", "Off", "Stores Management", "IR Cool Switch", "%1d"));
            AddFunction(new PushButton(this, SMC, "3407", "3407", "Stores Management", "Select station 1"));
            AddFunction(new FlagValue(this, "406", "Stores Management", "Station 1 Selected", "Station 1 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3409", "3409", "Stores Management", "Select station 2"));
            AddFunction(new FlagValue(this, "408", "Stores Management", "Station 2 Selected", "station 2 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3411", "3411", "Stores Management", "Select station 3"));
            AddFunction(new FlagValue(this, "410", "Stores Management", "Station 3 Selected", "station 3 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3413", "3413", "Stores Management", "Select station 4"));
            AddFunction(new FlagValue(this, "412", "Stores Management", "Station 4 Selected", "station 4 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3415", "3415", "Stores Management", "Select station 5"));
            AddFunction(new FlagValue(this, "414", "Stores Management", "Station 5 Selected", "station 5 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3417", "3417", "Stores Management", "Select station 6"));
            AddFunction(new FlagValue(this, "416", "Stores Management", "Station 6 Selected", "station 6 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3419", "3419", "Stores Management", "Select station 7"));
            AddFunction(new FlagValue(this, "418", "Stores Management", "Station 7 Selected", "station 7 selected indicator"));
            AddFunction(new PushButton(this, SMC, "3286", "3286", "Stores Management", "Launch Flare Salvo"));
            AddFunction(new Switch(this, SMC, "3404", new SwitchPosition[] { new SwitchPosition("-1.0", "STA", "3404"), new SwitchPosition("-0.5", "STOR", "3404"), new SwitchPosition("0.0", "SAFE", "3404"), new SwitchPosition("0.5", "CMBT", "3404"), new SwitchPosition("1.0", "FUEL", "3404") },  "Stores Management", "Jettison Mode Selector", "%0.1f"));
            AddFunction(new PushButton(this, SMC, "3405", "3405", "Stores Management", "Jettison Stores"));
            AddFunction(new Switch(this, SMC, "3395", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3395"), new SwitchPosition("0.33", "N/T", "3395"), new SwitchPosition("0.66", "N", "3395"), new SwitchPosition("1.0", "T", "3395")  }, "Stores Management", "Manual Fuzing Release Control", "%0.1f"));
            AddFunction(new Digits3Display(this, SMC, "2020", "Stores Management", "Stores interval display", "Interval value in metres"));
            AddFunction(new Digits2Display(this, SMC, "2022", "Stores Management", "Stores quantity display", "Quantity of stores"));
            AddFunction(new SMCMultipleDisplay(this));
            //AddFunction(new Text(this, "2018", "Stores Management", "SMC mode", "Stores management mode in text form"));
            #endregion

            #region Left Bulkhead
            // switch positions still need to be labeled
            AddFunction(Switch.CreateToggleSwitch(this, MSC, "3502", "3502", "1", "Pressed", "0", "Off", "Left Bulkhead", "Seat adjustment switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3519", "3519", "1", "Pressed", "0", "Off", "Left Bulkhead", "Fuel Shutoff Lever", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, DECS, "3520", "3520", "1", "On", "0", "Off", "Left Bulkhead", "DECS switch", "%1d"));

            #endregion

            #region V/UHF Radio
            // switch positions still need to be labeled
            AddFunction(new Axis(this, RSC, "3614", "3614", 0.03d, 0d, 1d, "V/UHF Radio", "Volume Knob"));
            AddFunction(new Axis(this, RSC, "3615", "3615", 0.03d, 0d, 1d, "V/UHF Radio", "Chan/Freq Knob"));
            AddFunction(new Switch(this, RSC, "3616", new SwitchPosition[] {new SwitchPosition("-0.20", "Norm", "3616"), new SwitchPosition("0.0", "Norm", "3616"), new SwitchPosition("0.0", "Norm", "3616"), new SwitchPosition("0.20", "Norm", "3616"), new SwitchPosition("0.40", "Norm", "3616"), new SwitchPosition("0.60", "Norm", "3616"), new SwitchPosition("0.80", "Norm", "3616"), new SwitchPosition("1.0", "Norm", "3616") }, "V / UHF Radio", "Operational Mode Switch", "%0.1f"));
            AddFunction(new PushButton(this, RSC, "3617", "3617", "V/UHF Radio", "Ancillary Mode Pointer A mode"));
            AddFunction(new PushButton(this, RSC, "3618", "3618", "V/UHF Radio", "Ancillary Mode Switch P mode"));
            AddFunction(new Switch(this, RSC, "3619", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3619"), new SwitchPosition("0.0", "Norm", "3619"), new SwitchPosition("0.15", "Norm", "3619"), new SwitchPosition("0.30", "Norm", "3619"), new SwitchPosition("0.45", "Norm", "3619"), new SwitchPosition("0.60", "Norm", "3619"), new SwitchPosition("0.75", "Norm", "3619"), new SwitchPosition("0.90", "Norm", "3619"), new SwitchPosition("1.00", "Norm", "3619") }, "V / UHF Radio", "Frequency Mode Switch", "%0.1f"));
            AddFunction(new PushButton(this, RSC, "3620", "3620", "V/UHF Radio", "LOAD/OFST Switch"));

            #endregion

            #region ACNIP
            // switch positions still need to be labeled
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3621", "3621", "1", "On", "0", "Off", "ACNIP", "ACNIP Mode Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3622", "3622", "1", "On", "0", "Off", "ACNIP", "KY-1 Cipher Type Selector Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3623", "3623", "1", "On", "0", "Off", "ACNIP", "KY-2 Cipher Type Selector Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3624", "3624", "1", "On", "0", "Off", "ACNIP", "KY-1 Code/Mode Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3625", "3625", "1", "On", "0", "Off", "ACNIP", "KY-2 Code/Mode Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3626", "3626", "1", "On", "0", "Off", "ACNIP", "ACNIP Radio Selector Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3627", "3627", "1", "On", "0", "Off", "ACNIP", "KY-58 Codes Clear Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, ACNIP, "3628", "3628", "1", "On", "0.5", "Off", "0", "Off", "ACNIP", "KY-58 Remote Codes Load Switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ACNIP, "3632", "3632", "1", "On", "0", "Off", "ACNIP", "IFF Operational Mode Switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, ACNIP, "3633", "3633", "1", "On", "0.5", "Off", "0", "Off", "ACNIP", "IFF Crypto Mode Switch", "%1d"));

            #endregion

            #region ICS IFF
            // switch positions still need to be labeled
            AddFunction(new Axis(this, INTERCOM, "3629", "3629", 0.03d, 0d, 1d, "Intercomm", "Aux Volume Knob"));
            AddFunction(new Axis(this, INTERCOM, "3630", "3630", 0.03d, 0d, 1d, "Intercomm", "Ground Volume Knob"));
            AddFunction(new Switch(this, RSC, "3631", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3631"), new SwitchPosition("0.5", "Norm", "3631"), new SwitchPosition("1.0", "Norm", "3631") }, "Intercomm", "Mic Operational Mode Switch", "%0.1f"));

            #endregion

            #region Interior Lights
            // switch positions still need to be labeled
            AddFunction(new PushButton(this, LTINT, "3634", "3634", "Interior Lights", "Compass Light/Test Lights")); //default_tumb_button(_("Compass Light/Test Lights") * * * Not sure if this is correct
            AddFunction(new Axis(this, LTINT, "3635", "3635", 0.03d, 0d, 1d, "Interior Lights", "Instruments Lights"));
            AddFunction(new Axis(this, LTINT, "3636", "3636", 0.03d, 0d, 1d, "Interior Lights", "Console Lights"));
            AddFunction(new Axis(this, LTINT, "3637", "3637", 0.03d, 0d, 1d, "Interior Lights", "Flood Lights"));
            AddFunction(new Axis(this, LTINT, "3638", "3638", 0.03d, 0d, 1d, "Interior Lights", "Annunciator Lights"));
            AddFunction(new Axis(this, LTINT, "3150", "3150", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Right Canopy Frame Top"));
            AddFunction(new Axis(this, LTINT, "3151", "3151", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Right Canopy Frame Bottom"));
            AddFunction(new Axis(this, LTINT, "3152", "3152", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Right Bulkhead Forward"));
            AddFunction(new Axis(this, LTINT, "3153", "3153", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Right Bulkhead Aft Front"));
            AddFunction(new Axis(this, LTINT, "3154", "3154", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Right Bulkhead Aft Back"));
            AddFunction(new Axis(this, LTINT, "3155", "3155", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Left Bulkhead Aft Back"));
            AddFunction(new Axis(this, LTINT, "3156", "3156", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Left Bulkhead Aft Front"));
            AddFunction(new Axis(this, LTINT, "3157", "3157", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Left Bulkhead Forward"));
            AddFunction(new Axis(this, LTINT, "3158", "3158", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Left Canopy Frame Bottom"));
            AddFunction(new Axis(this, LTINT, "3159", "3159", 0.03d, -1d, 1d, "Interior Lights", "Flood Lamp Left Canopy Frame Top"));

            #endregion

            #region Canopy Controls
            // switch positions still need to be labeled
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3801", "3801", "1", "On", "0", "Off", "Canopy Controls", "Canopy Handle Left", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3802", "3802", "1", "On", "0", "Off", "Canopy Controls", "Canopy Handle Right", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3803", "3803", "1", "On", "0", "Off", "Canopy Controls", "Canopy Locking Lever", "%1d"));

            AddFunction(Switch.CreateToggleSwitch(this, MSC, "3800", "3800", "1", "On", "0", "Off", "Canopy Controls", "Seat Ground Safety Lever", "%1d"));

            AddFunction(Switch.CreateToggleSwitch(this, FLIGHTCONTROLS, "3501", "3501", "1", "On", "0", "Off", "Canopy Controls", "MFS Emergency Lever", "%1d"));
            #endregion

            #region Stopwatch
            AddFunction(new PushButton(this, MSC, "4121", "4121", "Stop Watch", "Stopwatch Start/Stop"));
            AddFunction(new PushButton(this, MSC, "4122", "4122", "Stop Watch", "Stopwatch Lap/Reset"));
            #endregion

            #region ECS
            // switch positions still need to be labeled
            AddFunction(new Axis(this, ECS, "3639", "3639", 0.03d, 0d, 1d, "Environment Control", "Temperature Control Knob"));
            AddFunction(Switch.CreateToggleSwitch(this, ECS, "3640", "3640", "1", "On", "0", "Off", "Environment Control", "Fwd Equipment Bay ECS Switch", "%1d"));
            AddFunction(new Switch(this, ECS, "3641", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3641"), new SwitchPosition("0.5", "Norm", "3641"), new SwitchPosition("1.0", "Norm", "3641") }, "Environment Control", "Cabin Defog Switch", "%0.1f"));
            AddFunction(new Switch(this, ECS, "3642", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3642"), new SwitchPosition("0.5", "Norm", "3642"), new SwitchPosition("1.0", "Norm", "3642") }, "Environment Control", "Aft Equipment Bay ECS Switch", "%0.1f"));
            AddFunction(new Switch(this, ECS, "3643", new SwitchPosition[] { new SwitchPosition("0.0", "Norm", "3643"), new SwitchPosition("0.5", "Norm", "3643"), new SwitchPosition("1.0", "Norm", "3643") }, "Environment Control", "Cabin Pressure Switch", "%0.1f"));
            #endregion

            #region Engine Display Panel
            AddFunction(new ScaledNetworkValue(this, "271", 0.94d, "EDP", "Nozzle Position", "Current Nozzle position.", "Nozzle position in degrees", BindingValueUnits.Degrees));
            AddFunction(new PushButton(this, EDP, "3655", "3655", "EDP", "BIT"));
            AddFunction(new Digits2Display(this, EDP, "2006", "EDP", "H2O display", "Amount of H2O available"));
            AddFunction(new Digits4Display(this, EDP, "2002", "EDP", "RPM display", "Engine RPM percentage"));
            AddFunction(new Digits3Display(this, EDP, "2001", "EDP", "Duct pressure display", "Engine Duct pressure"));
            AddFunction(new Digits3Display(this, EDP, "2003", "EDP", "FF display", "Engine FF percentage"));
            AddFunction(new Digits3Display(this, EDP, "2004", "EDP", "JPT display", "Engine Jet pipe temperature"));
            AddFunction(new Digits2Display(this, EDP, "2005", "EDP", "Stabiliser display", "Amount of Stabiliser"));
            AddFunction(new FlagValue(this, "266", "EDP", "Stabilzer Arrow", "Up/Down Arrow for the stabilizer"));  // 266 returns -1 or 1  and 0 is a dash * * * Currently causing problems with the FlagValue!
            #endregion
 
            #region Electrical Panel
            //Battery
            CalibrationPointCollectionDouble batteryScale = new CalibrationPointCollectionDouble(0.0d, 0d, 0.30d, 30d);
            AddFunction(new ScaledNetworkValue(this, "608", batteryScale, "Electrical", "Battery Voltage", "Voltage of battery 15v to 30v", "", BindingValueUnits.Volts));
            AddFunction(Switch.CreateThreeWaySwitch(this, ELECTRIC, "3613", "3613", "1", "On", "0.5", "Off", "0", "Alert", "Electrical", "Battery switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, ELECTRIC, "3612", "3612", "1", "On", "0.5", "Off", "0", "Test", "Electrical", "Generator switch", "%1d"));
            AddFunction(Switch.CreateToggleSwitch(this, ELECTRIC, "3611", "3611", "1", "Start", "0", "Off", "Electrical", "Engine start switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, ELECTRIC, "3610", "3610", "1", "Reset", "0.5", "On", "0", "Off", "Electrical", "APU generator switch", "%1d"));
            AddFunction(Switch.CreateThreeWaySwitch(this, ELECTRIC, "3609", "3609", "1", "Main", "0.5", "Off", "0", "Standby", "Electrical", "DC Test switch", "%1d"));
            #endregion

            #region Brake / Hydraulic Pressures
            AddFunction(new Digits3Display(this, FLIGHTCONTROLS, "2015", "Brake/ Hydraulic", "Brake pressure display", "Brake pressure in psi"));
            AddFunction(new Digits3Display(this, FLIGHTCONTROLS, "2016", "Brake/ Hydraulic", "Hyd 1 pressure display", "Hydraulic system 1 pressure in psi"));
            AddFunction(new Digits3Display(this, FLIGHTCONTROLS, "2017", "Brake/ Hydraulic", "Hyd 2 pressure display", "Hydraulic system 2 pressure in psi"));
            // Brake accumulator pressure           
            CalibrationPointCollectionDouble accummulatorScale = new CalibrationPointCollectionDouble(0d, 0d, 0.36d, 3600d);
            AddFunction(new ScaledNetworkValue(this, "559", accummulatorScale, "Brake/ Hydraulic", "Brake accummulator", "Brake accummulator pressure in psi 600 to 3600 psi", "", BindingValueUnits.PoundsPerSquareInch));
            //Cabin Altitude Pressure 
            CalibrationPointCollectionDouble cabinScale = new CalibrationPointCollectionDouble(-0.003d,-300d,0.5000d, 50000d);
            cabinScale.Add(new CalibrationPointDouble(0d, 0d));
            AddFunction(new ScaledNetworkValue(this, "607", cabinScale, "Brake/ Hydraulic", "Cabin Altitude", "Cabin altitude pressue in feet 0 to +50000.", "", BindingValueUnits.Feet));

            #endregion


            #region Flight Instruments
            AddFunction(new ScaledNetworkValue(this, "363", 0.94d, "Flight Instruments", "Sideslip", "Sideslip Ball Indicator", "", BindingValueUnits.Degrees));

            AddFunction(new Altimeter(this,"Altimeter","2051","Altitude", "Barometric altitude above sea level of the aircraft.", "Value is adjusted per altimeter pressure setting.", "2059","Pressure", "Manually set barometric altitude.",""));
            AddFunction(new RotaryEncoder(this, ADC, "3653", "3653", 0.04d, "Altimeter", "Pressure"));
            //AddFunction(new Axis(this, ADC, "3653", "3653", 0.1d,0d,1d, "Altimeter", "Pressure adjust",true, "%1d"));

            CalibrationPointCollectionDouble vviScale = new CalibrationPointCollectionDouble(-0.6d, -6000d, 0.6d, 6000d);
            vviScale.Add(new CalibrationPointDouble(0d, 0d));
            AddFunction(new ScaledNetworkValue(this, "362", vviScale, "Flight Instruments", "VVI", "Vertical velocity indicator -6000 to +6000.", "", BindingValueUnits.FeetPerMinute));

            CalibrationPointCollectionDouble AoAScale = new CalibrationPointCollectionDouble(-0.05d, -5d, 0.20d, 20d);
            AoAScale.Add(new CalibrationPointDouble(0d, 0d));
            AddFunction(new ScaledNetworkValue(this, "361", AoAScale, "Flight Instruments", "Angle of Attack", "Current angle of attack of the aircraft.", "", BindingValueUnits.Degrees));
            AddFunction(new FlagValue(this, "360", "Flight Instruments", "AOA", "Off Flag"));

            CalibrationPointCollectionDouble airspeedScale = new CalibrationPointCollectionDouble(0.0d, 0.0d, 1.0d, 1000d);
            AddFunction(new ScaledNetworkValue(this, "346", airspeedScale, "Flight Instruments", "IAS Airspeed", "Current indicated air speed of the aircraft.", "", BindingValueUnits.Knots));
            AddFunction(new Axis(this, NAV_INS, "3364", "3364", 0.1d,0d,1d, "NAV course", "Course Setting",true, "%1d"));

            //AddFunction(new ScaledNetworkValue(this, "17", -90d, "ADI", "Pitch", "Current pitch displayed on the ADI.", "", BindingValueUnits.Degrees));
            //AddFunction(new ScaledNetworkValue(this, "18", 180d, "ADI", "Bank", "Current bank displayed on the ADI.", "", BindingValueUnits.Degrees));
            //AddFunction(new NetworkValue(this, "24", "ADI", "Slip Ball", "Current position of the slip ball relative to the center of the tube.", "(-1 to 1) -1 is full left and 1 is full right.", BindingValueUnits.Numeric));
            //AddFunction(new NetworkValue(this, "23", "ADI", "Turn Needle", "Position of the turn needle.", "(-1 to 1)", BindingValueUnits.Numeric));
            //AddFunction(new FlagValue(this, "25", "ADI", "Attitude Warning Flag", "Indicates that the ADI has lost electrical power or otherwise been disabled."));
            //AddFunction(new FlagValue(this, "19", "ADI", "Course Warning Flag", "Indicates thatn an operative ILS or TACAN signal is received."));
            //AddFunction(new FlagValue(this, "26", "ADI", "Glide Slope Warning Flag", "Indicates that the ADI is not recieving a ILS glide slope signal."));
            //AddFunction(new NetworkValue(this, "20", "ADI", "Bank Steering Bar Offset", "Location of bank steering bar relative to the middle of the ADI.", "(-1 to 1) -1 is full left and 1 is full right.", BindingValueUnits.Numeric));
            //AddFunction(new NetworkValue(this, "21", "ADI", "Pitch Steering Bar Offset", "Location of pitch steering bar relative to the middle of the ADI.", "(-1 to 1) 1 is full up and -1 is full down.", BindingValueUnits.Numeric));
            //AddFunction(new NetworkValue(this, "27", "ADI", "Glide Slope Indicator", "Location of the glide slope indicator relative to the middle of the slope deviation scale.", "(-1 to 1) 1 is full up and -1 is full down.", BindingValueUnits.Numeric));
            //AddFunction(new Axis(this, ADI, BUTTON_1, "22", 0.05d, -0.5d, 0.5d, "ADI", "Pitch Trim Knob"));

            AddFunction(new ScaledNetworkValue(this, "349", 90d, "Flight Instruments", "SAI Pitch", "Current pitch displayed on the SAI.", "", BindingValueUnits.Degrees));
            AddFunction(new ScaledNetworkValue(this, "348", -180d, "Flight Instruments", "SAI Bank", "Current bank displayed on the SAI.", "", BindingValueUnits.Degrees));
            AddFunction(new FlagValue(this, "347", "Flight Instruments", "SAI Warning Flag", "Displayed when SAI is caged or non-functional."));
            //AddFunction(new RotaryEncoder(this, FLIGHTCONTROLS, BUTTON_3, "66", 0.1d, "SAI", "Pitch Trim / Cage"));
            //AddFunction(new NetworkValue(this, "715", "SAI", "Pitch Adjust", "Current pitch adjustment setting", "0 to 1", BindingValueUnits.Numeric));
            AddFunction(new NetworkValue(this, "363", "Flight Instruments", "Slip Ball", "Current position of the slip ball relative to the center of the tube.", "(-1 to 1) -1 is full left and 1 is full right.", BindingValueUnits.Numeric));

            #endregion

        }

        private string DCSPath
        {
            get
            {
                if (_dcsPath == null)
                {
                    RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Eagle Dynamics\DCS AV-8B");
                    if (pathKey != null)
                    {
                        _dcsPath = (string)pathKey.GetValue("Path");
                        pathKey.Close();
                        ConfigManager.LogManager.LogDebug("DCS AV-8B Interface Editor - Found DCS Path (Path=\"" + _dcsPath + "\")");
                    }
                    else
                    {
                        _dcsPath = "";
                    }
                }
                return _dcsPath;
            }
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

            if (oldProfile != null)
            {
                oldProfile.ProfileTick -= Profile_Tick;
            }

            if (Profile != null)
            {
                Profile.ProfileTick += Profile_Tick;
            }
        }

        void Profile_Tick(object sender, EventArgs e)
        {
            if (_phantomFix && System.Environment.TickCount - _nextCheck >= 0)
            {
                System.Diagnostics.Process[] dcs = System.Diagnostics.Process.GetProcessesByName("DCS");
                if (dcs.Length == 1)
                {
                    IntPtr hWnd = dcs[0].MainWindowHandle;
                    NativeMethods.Rect dcsRect;
                    NativeMethods.GetWindowRect(hWnd, out dcsRect);

                    if (dcsRect.Width > 640 && (dcsRect.Left != _phantomLeft || dcsRect.Top != _phantomTop))
                    {
                        NativeMethods.MoveWindow(hWnd, _phantomLeft, _phantomTop, dcsRect.Width, dcsRect.Height, true);
                    }
                }
                _nextCheck = System.Environment.TickCount + 5000;
            }
        }
    }
}
