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

/// <summary>
/// This is the revised version of the Stores Management Controller unit which is larger and uses improved graphics.
/// It has a slightly different name because the old version is retained to help with backward compatability
/// </summary>
/// 

namespace GadrocsWorkshop.Helios.Gauges.AV8B
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System.Windows;

    [HeliosControl("Helios.AV8B.SMC1", "Stores Management", "AV-8B Gauges", typeof(AV8BDeviceRenderer))]
    class SMC_1: AV8BDevice
    {
        private string _interfaceDeviceName = "Stores Management";
        private string _font = "MS 33558";

        public SMC_1()
            : base("SMC", new Size(1231, 470))
        {
            AddDisplay("Stores Mode", new smcModeDisplay(), new Point(81, 64), new Size(68, 41), "SMC mode (value)");
            AddDisplay("Fuze Mode", new fuzeDisplay(), new Point(218, 59), new Size(112, 50), "Fuze Mode");
            AddDisplay("Quantity", new SMC.TwoDigitDisplay(), new Point(407, 66), new Size(98, 48), "Stores quantity display");
            AddDisplay("Multiple", new SMC.OneDigitDisplay(), new Point(607,66), new Size(41, 48), "Stores multiple display");
            AddDisplay("Interval", new ThreeDigitDisplay(), new Point(746, 66), new Size(168, 48), "Stores interval display");

            AddButton("Station 1", 383, 393, new Size(50, 50), "Station 1 Button");
            AddButton("Station 2", 486, 393, new Size(50, 50), "Station 2 Button");
            AddButton("Station 3", 586, 393, new Size(50, 50), "Station 3 Button");
            AddButton("Station 4", 687, 393, new Size(50, 50), "Station 4 Button");
            AddButton("Station 5", 787, 392, new Size(50, 50), "Station 5 Button");
            AddButton("Station 6", 885, 392, new Size(50, 50), "Station 6 Button");
            AddButton("Station 7", 987, 392, new Size(50, 50), "Station 7 Button");

            AddIndicator("Station 1 Flag", 385, 330, new Size(47, 41), "Station 1 Selected Indicator");
            AddIndicator("Station 2 Flag", 488, 330, new Size(47, 41), "Station 2 Selected Indicator"); 
            AddIndicator("Station 3 Flag", 588, 330, new Size(47, 41), "Station 3 Selected Indicator");
            AddIndicator("Station 4 Flag", 687, 330, new Size(47, 41), "Station 4 Selected Indicator");
            AddIndicator("Station 5 Flag", 784, 330, new Size(47, 41), "Station 5 Selected Indicator");
            AddIndicator("Station 6 Flag", 886, 330, new Size(47, 41), "Station 6 Selected Indicator");
            AddIndicator("Station 7 Flag", 988, 330, new Size(47, 41), "Station 7 Selected Indicator");

            AddThreeWayToggle("Aiming Mode Switch", 56, 130, new Size(65, 153), "Armament Mode control");
            AddThreeWayToggle("Fuze Toggle Switch", 256, 130, new Size(65, 153), "Armament Fuzing control");
            AddThreeWayToggle("Quantity 10's", 366, 134, new Size(65, 153), "Armament Quantity Tens");
            AddThreeWayToggle("Quantity 1's", 472, 134, new Size(65, 153), "Armament Quantity Units");
            AddThreeWayToggle("Multiple Switch", 580, 132, new Size(65, 153), "Armament Multiple Release");
            AddThreeWayToggle("Interval 100's", 696, 140, new Size(65, 153), "Armament Release interval hundreds");
            AddThreeWayToggle("Interval 10's", 804, 140, new Size(65, 153), "Armament Release interval tens");
            AddThreeWayToggle("Interval 1's", 911, 140, new Size(65, 153), "Armament Release interval units");
            AddTwoWayToggle("IR Cool Switch", 1094, 294, new Size(67, 154), "IR Cool Switch");

            AddKnobSMC1("Fuzing Options", new Point(1025,76), new Size(110,110), "Manual Fuzing Release Control");
            AddKnobSMC2("Stores Jettison Switch", new Point(96, 278), new Size(186, 186), "Jettison Mode Selector");
            AddButton("Jettison Button", 131, 312, new Size(118, 118), true, true, "Jettison Stores");
        }
        private void AddDisplay(string name, BaseGauge _gauge, Point posn, Size displaySize, string interfaceElementName)
        {
            AddDisplay(
                name: name,
                gauge: _gauge,
                posn: posn,
                size: displaySize,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName
                );
            _gauge.Name = "SMC_" + name;
        }

        public override string BezelImage
        {
            get { return "{AV-8B}/Images/AV-8B SMC Faceplate.png"; }
        }
            
        private void AddKnobSMC1(string name, Point posn, Size size, string interfaceElementName)
        {
            Helios.Controls.RotarySwitch _knob = new Helios.Controls.RotarySwitch();
            _knob.Name = "SMC_" + name;
            _knob.KnobImage = "{AV-8B}/Images/SMC Selector Knob.png";
            _knob.DrawLabels = false;
            _knob.DrawLines = false;
            _knob.Positions.Clear();
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 0, "Norm", 315d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 1, "N/T", 0d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 2, "N", 45d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 3, "T", 90d));
            _knob.CurrentPosition = 1;
            _knob.Top = posn.Y;
            _knob.Left = posn.X;
            _knob.Width = size.Width;
            _knob.Height = size.Height;

            AddRotarySwitchBindings(name, posn, size, _knob, _interfaceDeviceName, interfaceElementName);
        }
        private void AddKnobSMC2(string name, Point posn, Size size, string interfaceElementName)
        {
            Helios.Controls.RotarySwitch _knob = new Helios.Controls.RotarySwitch();
            _knob.Name = "SMC_" + name;
            _knob.KnobImage = "{AV-8B}/Images/SMC Jettison Knob.png";
            _knob.DrawLabels = false;
            _knob.DrawLines = false;
            _knob.Positions.Clear();
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 0, "Station", 270d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 1, "Stores", 315d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 2, "Safe", 0d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 3, "Combat", 45d));
            _knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_knob, 4, "Fuel", 90d));
            _knob.CurrentPosition = 3;
            _knob.Top = posn.Y;
            _knob.Left = posn.X;
            _knob.Width = size.Width;
            _knob.Height = size.Height;

            AddRotarySwitchBindings(name, posn, size, _knob, _interfaceDeviceName, interfaceElementName);
        }


        private void AddButton(string name, double x, double y, string interfaceElementName) { AddButton(name, x, y, false, interfaceElementName); }
        private void AddButton(string name, double x, double y, Size size, string interfaceElementName) { AddButton(name, x, y, size, false, interfaceElementName); }
        private void AddButton(string name, double x, double y, bool horizontal, string interfaceElementName) { AddButton(name, x, y, new Size(40,40),false, interfaceElementName); }
        private void AddButton(string name, double x, double y, Size size, bool horizontal, string interfaceElementName) { AddButton(name, x, y, size, horizontal, false, interfaceElementName); }
        private void AddButton(string name, double x, double y, Size size, bool horizontal, bool altImage, string interfaceElementName)
        {
            Point pos = new Point(x, y);
            PushButton button = AddButton(
                    name: name,
                    posn: pos,
                    size: size,
                    image: "{Helios}/Images/Buttons/tactile-dark-round.png",
                    pushedImage: "{Helios}/Images/Buttons/tactile-dark-round-in.png",
                    buttonText: "",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: interfaceElementName,
                    fromCenter: false
                    );

                    if (altImage)
                    {
                        button.Image = "{AV-8B}/Images/SMC Jettison Button.png";
                        button.PushedImage = "{AV-8B}/Images/SMC Jettison Button.png";
                    }
            button.Name = "SMC_" + name;
        }
       private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.Two,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: "{AV-8B}/Images/SMC Cool Switch Up.png",
                positionTwoImage: "{AV-8B}/Images/SMC Cool Switch Dn.png",
                horizontal: false,
                clickType: LinearClickType.Swipe, 
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            toggle.Name = "SMC_" + name;
        }

        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName)
        {
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: ThreeWayToggleSwitchType.MomOnMom,
                positionOneImage: "{AV-8B}/Images/3 Way Toggle Square Up 1.png",
                positionTwoImage: "{AV-8B}/Images/3 Way Toggle Square Mid 1.png",
                positionThreeImage: "{AV-8B}/Images/3 Way Toggle Square Down 1.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            toggle.Name = "SMC_" + name;
        }

        private void AddIndicator(string name, double x, double y, Size size, string interfaceElementName) { AddIndicator(name, x, y, size, false, interfaceElementName); }
        private void AddIndicator(string name, double x, double y, Size size, bool _vertical, string interfaceElementName)
        {
            Indicator indicator = AddIndicator(
                name: name,
                posn: new Point(x, y),
                size: size,
                onImage: "{AV-8B}/Images/AV8BNA_SEL_On.png",
                offImage: "{AV-8B}/Images/AV8BNA_SEL_Off.png",
                onTextColor: System.Windows.Media.Color.FromArgb(0x00, 0xff, 0xff, 0xff),
                offTextColor: System.Windows.Media.Color.FromArgb(0x00, 0x00, 0x00, 0x00),
                font: _font,
                vertical: _vertical,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
                indicator.Text = "";
                indicator.Name = "SMC_" + name;
        }

        public override bool HitTest(Point location)
        {
            //if (_scaledScreenRect.Contains(location))
            //{
            //    return false;
            //}

            return true;
        }

        public override void MouseDown(Point location)
        {
            // No-Op
        }

        public override void MouseDrag(Point location)
        {
            // No-Op
        }

        public override void MouseUp(Point location)
        {
            // No-Op
        }
    }
}
