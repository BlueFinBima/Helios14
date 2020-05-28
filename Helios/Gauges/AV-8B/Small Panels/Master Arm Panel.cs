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

    [HeliosControl("Helios.AV8B.MasterArmPanel", "Master Arm Panel", "AV-8B Gauges", typeof(AV8BDeviceRenderer))]
    class MasterArmPanel: AV8BDevice
    {
        private string _interfaceDeviceName = "Master Modes";
        private string _font = "MS 33558";
        private string _imageLocation = "{AV-8B}/Images/";

        public MasterArmPanel()
            : base("Master Arm Panel", new Size(196, 881))
        {
            AddIndicator("Nav", 14, 380, new Size(68, 37), "Nav Indicator");
            AddButton("Nav Button", 14, 380, new Size(68, 37), true, true, "NAV Button");
            AddIndicator("VSTOL", 107, 380, new Size(68, 37), "VSTOL Indicator");
            AddButton("VSTOL Button", 107, 380, new Size(68, 37), true, true, "VSTOL Button");
            AddIndicator("AG", 99, 198, new Size(68, 37), "A/G Indicator");
            AddButton("AG Button", 99, 198, new Size(68, 37), true, true, "A/G Button");
            _interfaceDeviceName = "Stores Management";
            AddTwoWayToggle("Master Arm", 71, 747, new Size(74, 81), "Master Arm Switch");
            AddButton("Flare Salvo Button", 69, 518, new Size(86, 82), "Launch Flare Salvo");
        }
        public override string BezelImage
        {
            get { return _imageLocation + "WQHD/Panel/Master Arm.png"; }
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
                    image: _imageLocation + "WQHD/Button/" + name + " Normal.png",
                    pushedImage: _imageLocation + "WQHD/Button/" + name + " Pushed.png",
                    buttonText: "",
                    interfaceDeviceName: _interfaceDeviceName,
                    interfaceElementName: interfaceElementName,
                    fromCenter: false
                    );
            if (altImage)
            {
                button.Image = _imageLocation + "_transparent.png";
                button.PushedImage = _imageLocation + "_transparent.png";
            }
            button.Name = "Master Arm Panel_" + name;
        }
       private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.Two,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _imageLocation + "WQHD/Switch/" + name + " Up.png",
                positionTwoImage: _imageLocation + "WQHD/Switch/" + name + " Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                clickType: LinearClickType.Swipe,
                fromCenter: false
                );
            toggle.Name = "Master Arm Panel_" + name;
        }

        private void AddIndicator(string name, double x, double y, Size size, string interfaceElementName) { AddIndicator(name, x, y, size, false, interfaceElementName); }
        private void AddIndicator(string name, double x, double y, Size size, bool _vertical, string interfaceElementName)
        {
            Indicator indicator = AddIndicator(
                name: name,
                posn: new Point(x, y),
                size: size,
                onImage: _imageLocation + "WQHD/Indicator/" + name + " On.png",
                offImage: _imageLocation + "WQHD/Indicator/" + name + " Off.png",
                onTextColor: System.Windows.Media.Color.FromArgb(0x00, 0xff, 0xff, 0xff),
                offTextColor: System.Windows.Media.Color.FromArgb(0x00, 0x00, 0x00, 0x00),
                font: _font,
                vertical: _vertical,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
                indicator.Text = "";
                indicator.Name = "Master Arm Panel_" + name;
        }

        //public override bool HitTest(Point location)
        //{
        //    //if (_scaledScreenRect.Contains(location))
        //    //{
        //    //    return false;
        //    //}

        //    return true;
        //}

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
