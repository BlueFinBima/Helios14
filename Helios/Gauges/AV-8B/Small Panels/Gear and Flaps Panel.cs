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

namespace GadrocsWorkshop.Helios.Gauges.AV8B
{
    using GadrocsWorkshop.Helios.Gauges.AV8B;
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows;

    [HeliosControl("Helios.AV8B.GearPanel", "Flaps & Landing Gear Panel", "AV-8B Gauges", typeof(AV8BDeviceRenderer))]
    class GearPanel: AV8BDevice
    {
        private string _interfaceDeviceName = "Landing Gear";
        private string _imageLocation = "{AV-8B}/Images/";

        // these sections are the dead space in the image.
        private static readonly Rect SCREEN_RECT_0 = new Rect(0, 0, 209, 839);
        private Rect _scaledScreenRect0 = SCREEN_RECT_0;
        private static readonly Rect SCREEN_RECT_1 = new Rect(612, 386, 141, 453);
        private Rect _scaledScreenRect1 = SCREEN_RECT_1;
        private static readonly Rect SCREEN_RECT_2 = new Rect(449, 0, 151, 507);
        private Rect _scaledScreenRect2 = SCREEN_RECT_2;



        public GearPanel()
            : base("Flaps and Gear Panel", new Size(755, 839))
        {
            AddButton("Emergency Jettison Button", 253, 90, new Size(198, 206), "Emergency Jettison Button");
            AddTwoWayToggle("Landing Gear Lever", 451, 447, new Size(157, 392), "lever", "Landing Gear", "Gear - Lever");
            AddButtonBasic("Gear Lock Override", 364, 645, new Size(43, 43), "Gear Down Lock Override Button");
            _interfaceDeviceName = "LH Flaps & Water";
            AddThreeWayToggle("Flaps Mode", 620, 70, new Size(89, 114), "Flaps Mode Switch","Flaps Switch");
            AddThreeWayToggleHorizontal("Flaps Power", 599, 274, new Size(89, 137), "Flaps Power Switch","Horizontal Switch");
            AddThreeWayToggle("Anti Skid", 237, 479, new Size(74, 114), ThreeWayToggleSwitchType.MomOnOn, "Anti-Skid Switch","Orange");
            AddButtonBasic("Flaps BIT", 222, 649, new Size(69, 69), "Flaps BIT");


            TwoDigitDisplay _flapDisplay = new TwoDigitDisplay();
            _flapDisplay.Components.RemoveAt(0);  // remove the inter-digit background
            AddDisplay("Flaps Position", _flapDisplay, new Point(532, 76), new Size(86, 68), "Flaps position");
        }
        public override string BezelImage
        {
            get { return _imageLocation + "WQHD/Panel/Flaps and Landing Gear.png"; }
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
            _gauge.Name = "Flaps and Gear Panel_" + name;
        }

        private void AddButton(string name, double x, double y, string interfaceElementName) => AddButton(name, x, y, false, interfaceElementName); 
        private void AddButton(string name, double x, double y, Size size, string interfaceElementName) => AddButton(name, x, y, size, false, interfaceElementName);
        private void AddButton(string name, double x, double y, bool horizontal, string interfaceElementName) => AddButton(name, x, y, new Size(40,40),false, interfaceElementName); 
        private void AddButton(string name, double x, double y, Size size, bool horizontal, string interfaceElementName) => AddButton(name, x, y, size, horizontal, false, interfaceElementName); 
        private void AddButton(string name, double x, double y, Size size, bool horizontal, bool altImage, string interfaceElementName)
        {
            Point pos = new Point(x, y);
            PushButton button = AddButton(
                    name: name,
                    posn: pos,
                    size: size,
                    image: _imageLocation + "_transparent.png",
                    pushedImage: _imageLocation + "WQHD/Button/" + name + " Pushed.png",
                    buttonText: "",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: interfaceElementName,
                    fromCenter: false
                    );
            button.Name = "Flaps and Gear Panel_" + name;
        }
        private void AddButtonBasic(string name, double x, double y, Size size, string interfaceElementName)
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
            button.Name = "Flaps and Gear Panel_" + name;
        }

        private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName, string imageName, string indicator = "")
        {
            string _up;
            string _down;
            if(indicator != "")
            {
                _up = _imageLocation + "WQHD/Switch/" + imageName + " Up";
                _down = _imageLocation + "WQHD/Switch/" + imageName + " Down";
            }
            else
            {
                _up = _imageLocation + "WQHD/Switch/" + imageName + " Up Off.png";
                _down = _imageLocation + "WQHD/Switch/" + imageName + " Down Off.png";
            }
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.Two,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _up,
                positionTwoImage: _down,
                horizontal: false,
                clickType: LinearClickType.Swipe,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                interfaceIndicatorElementName: indicator,
                fromCenter: false
                );
            toggle.Name = "Flaps and Gear Panel_" + name;
        }

        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName) => AddThreeWayToggle(name, x, y, size, ThreeWayToggleSwitchType.OnOnOn, interfaceElementName, name);
        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName, string imageName) => AddThreeWayToggle(name, x, y, size, ThreeWayToggleSwitchType.OnOnOn, interfaceElementName, imageName);
        private void AddThreeWayToggle(string name, double x, double y, Size size, ThreeWayToggleSwitchType switchType, string interfaceElementName, string imageName)
        {
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: switchType,
                positionOneImage: _imageLocation + "WQHD/Switch/" + imageName + " Up.png",
                positionTwoImage: _imageLocation + "WQHD/Switch/" + imageName + " Normal.png",
                positionThreeImage: _imageLocation + "WQHD/Switch/" + imageName + " Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
            toggle.Name = "Flaps and Gear Panel_" + name;
        }
        private void AddThreeWayToggleHorizontal(string name, double x, double y, Size size, string interfaceElementName) => AddThreeWayToggleHorizontal(name, x, y, size, interfaceElementName, name);
        private void AddThreeWayToggleHorizontal(string name, double x, double y, Size size, string interfaceElementName, string imageName)
        {
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: ThreeWayToggleSwitchType.OnOnMom,
                positionOneImage: _imageLocation + "WQHD/Switch/" + imageName + " Right.png",
                positionTwoImage: _imageLocation + "WQHD/Switch/" + imageName + " Normal.png",
                positionThreeImage: _imageLocation + "WQHD/Switch/" + imageName + " Left.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                horizontal: true,
                fromCenter: false
                );
            toggle.Name = "Flaps and Gear Panel_" + name;
        }

        public override bool HitTest(Point location)
        {
            if (_scaledScreenRect0.Contains(location) || _scaledScreenRect1.Contains(location) || _scaledScreenRect2.Contains(location))
            {
                return false;
            }

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
