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
/// This is the revised version of the Up Front Controller which is larger and uses text displays to avoid cutouts for the exported viewport.
/// It has a slightly different name because the old version is retained to help with backward compatability
/// </summary>
/// 
namespace GadrocsWorkshop.Helios.Gauges.AV8B
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using System;
    using System.Windows.Media;
    using System.Windows;
    using System.Windows.Threading;


    [HeliosControl("Helios.AV8B.Radio", "Radio & ACNIP", "AV-8B Gauges", typeof(AV8BDeviceRenderer))]
    class Radio: AV8BDevice
    {
        // these sections are the dead space in the image.
        private static readonly Rect SCREEN_RECT_0 = new Rect(166, 0, 368, 171);
        private Rect _scaledScreenRect0 = SCREEN_RECT_0;
        private static readonly Rect SCREEN_RECT_1 = new Rect(0, 276, 302, 250);
        private Rect _scaledScreenRect1 = SCREEN_RECT_1;
        private static readonly Rect SCREEN_RECT_2 = new Rect(0, 541, 83, 357);
        private Rect _scaledScreenRect2 = SCREEN_RECT_2;
        private static readonly Rect SCREEN_RECT_3 = new Rect(637, 196, 83, 702);
        private Rect _scaledScreenRect3 = SCREEN_RECT_3;
        private String _font = "Hornet IFEI Mono";
        private string _interfaceDeviceName = "V/UHF Radio";
        private string _imageLocation = "{AV-8B}/Images/";

        public Radio()
            : base("Radio/ACNIP", new Size(727, 902))
        {
            // 00000000*2100=99:2101=123.457:
            AddTextDisplay("Channel Number", 200, 20, new Size(81, 80), "Channel Number", 72, "99", TextHorizontalAlignment.Left);
            AddTextDisplay("Frequency Display", 281, 27, new Size(216, 51), "Frequency", 48, "888.888", TextHorizontalAlignment.Left);
            AddEncoder("Radio volume", new Point(95,39), new Size(50, 50), "Volume Knob", "WQHD/Knob/Radio Squlech Knob.png");
            AddEncoder("Channel Knob", new Point(547,37), new Size(70, 70), "Chan/Freq Knob", "WQHD/Knob/Radio Channel Select.png");
            AddButton("A Mode Button", 260, 186, new Size(68, 68), "Ancillary Mode Pointer A mode", "WQHD/Button/Radio A Up.png");
            AddButton("P Mode Button", 358, 186, new Size(68, 68), "Ancillary Mode Switch P mode", "WQHD/Button/Radio P Up.png");
            AddButton("Load Switch", 628, 105, new Size(68, 68), "LOAD/OFST Switch", "WQHD/Button/Radio Load Up.png");
            RotarySwitch _opKnob = AddKnob("Operational Mode", new Point(119, 163), new Size(90, 90), "Operational Mode Switch");
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 0, "Zero", 315d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 1, "Off", 0d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 2, "Test", 45d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 3, "TR+G", 90d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 4, "TR", 135d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 5, "ADF", 180d));
            _opKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_opKnob, 6, "Chng PRST", 225d));

            RotarySwitch _freqKnob = AddKnob("Frequency Mode", new Point(491, 163), new Size(90, 90), "Frequency Mode Switch");
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 0, "AJ/M", 315d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 1, "AJ", 0d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 2, "MAR", 45d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 3, "PRST", 90d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 4, "MAN", 135d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 5, "342", 180d));
            _freqKnob.Positions.Add(new Helios.Controls.RotarySwitchPosition(_freqKnob, 6, "121", 225d));


            _interfaceDeviceName = "ACNIP";
            // 00000000*2102=MODE:2103=PLN:2104=CODE:2105=00:2106=MODE:2107=CY:2108=CODE:2109=03: 
            AddTextDisplay("1 Mode Label", 138, 381, new Size(68, 25), "ACNIP 1 Mode Label", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("1 Mode display", 138, 406, new Size(68, 25), "ACNIP 1 Mode", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("1 Code Label", 138, 431, new Size(68, 25), "ACNIP 1 Code Label", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("1 Code display", 138, 456, new Size(68, 25), "ACNIP 1 Code", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("2 Mode Label", 208, 381, new Size(68, 25), "ACNIP 2 Mode Label", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("2 Mode display", 208, 406, new Size(68, 25), "ACNIP 2 Mode", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("2 Code Label", 208, 431, new Size(68, 25), "ACNIP 2 Code Label", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTextDisplay("2 Code display", 208, 456, new Size(68, 25), "ACNIP 2 Code", 24, "MMMM", TextHorizontalAlignment.Center);
            AddTwoWayToggle("Mode Switch", 323, 391, new Size(69, 107), "ACNIP Mode Switch","WQHD/Switch/Orange ");
            AddTwoWayToggle("KY-1 Cipher Switch", 435, 372, new Size(64, 146), "KY-1 Cipher Type Selector Switch", "WQHD/Switch/Domed ");
            AddTwoWayToggle("KY-2 Cipher Switch", 546, 372, new Size(64, 146), "KY-2 Cipher Type Selector Switch", "WQHD/Switch/Domed ");
            AddTwoWayToggle("KY-1 Code/Mode Switch", 108,570, new Size(69, 107), "KY-1 Code/Mode Switch", "WQHD/Switch/Orange ");
            AddTwoWayToggle("KY-2 Code/Mode Switch", 218, 570, new Size(69, 107), "KY-2 Code/Mode Switch", "WQHD/Switch/Orange ");
            AddTwoWayToggle("ACNIP Radio Selector Switch", 323, 553, new Size(64, 146), "ACNIP Radio Selector Switch", "WQHD/Switch/Domed ");
            AddTwoWayToggle("KY-58 Codes Clear Switch", 434, 553, new Size(64, 146), "KY-58 Codes Clear Switch", "WQHD/Switch/Domed ");
            AddThreeWayToggle("KY-58 Load Switch", 546, 553, new Size(64, 146), "KY-58 Remote Codes Load Switch", "WQHD/Switch/Domed ");
            AddTwoWayToggle("IFF Operational Mode Switch", 434, 733, new Size(64, 146), "IFF Operational Mode Switch", "WQHD/Switch/Domed ");
            AddThreeWayToggle("IFF Crypto Switch", 546, 733, new Size(64, 146), "IFF Crypto Mode Switch", "WQHD/Switch/Domed ");
            _interfaceDeviceName = "Intercomm";
            AddThreeWayToggle("Mic Switch", 321, 750, new Size(69, 107), "Mic Operational Mode Switch", "WQHD/Switch/Orange ");
            AddPot("Ground Volume", new Point(154, 719), new Size(120, 120), "Ground Volume Knob", "WQHD/Knob/Outer.png");
            AddPot("Aux Volume", new Point(174, 739), new Size(80, 80), "Aux Volume Knob", "WQHD/Knob/Inner.png",270,270);
        }

        public override string BezelImage
        {
            get { return _imageLocation + "WQHD/Panel/Right Radio.png"; }
        }

        private RotarySwitch AddKnob(string name, Point posn, Size size, string interfaceElementName)
        {
            Helios.Controls.RotarySwitch _knob = new Helios.Controls.RotarySwitch();
            _knob.Name = "Radio/ACNIP_" + name;
            _knob.KnobImage = _imageLocation + "WQHD/Knob/Radio Selector Knob.png";
            _knob.DrawLabels = false;
            _knob.DrawLines = false;
            _knob.Positions.Clear();
            _knob.CurrentPosition = 1;
            _knob.Top = posn.Y;
            _knob.Left = posn.X;
            _knob.Width = size.Width;
            _knob.Height = size.Height;

            AddRotarySwitchBindings(name, posn, size, _knob, _interfaceDeviceName, interfaceElementName);
            return _knob;
        }

        private void AddPot(string name, Point posn, Size size, string interfaceElementName) => AddPot(name, posn, size, interfaceElementName, "Common Knob.png");
        private void AddPot(string name, Point posn, Size size, string interfaceElementName, string imageName) => AddPot(name, posn, size, interfaceElementName, imageName, 219,270);
        private void AddPot(string name, Point posn, Size size, string interfaceElementName, string imageName, double initialrotation, double rotationtravel)
        {

            AddPot(name: name,
                posn: posn,
                size: size,
                knobImage: _imageLocation + imageName,
                initialRotation: initialrotation,
                rotationTravel: rotationtravel,
                minValue: 0,
                maxValue: 1,
                initialValue: 0,
                stepValue: 0.1,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false);
        }

        private void AddEncoder(string name, Point posn, Size size, string interfaceElementName) => AddEncoder(name, posn, size, interfaceElementName, "AV8BNA_Rotary5.png");
        private void AddEncoder(string name, Point posn, Size size, string interfaceElementName, string imageName)
        {
            AddEncoder(
                name: name,
                size: size,
                posn: posn,
                knobImage: _imageLocation + imageName,
                stepValue: 0.005,
                rotationStep: 10,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
        }

        private void AddTextDisplay(string name, double x, double y, Size size,
            string interfaceElementName, double baseFontsize, string testDisp, TextHorizontalAlignment hTextAlign)
        {
            AddTextDisplay(name, x, y, size,
            interfaceElementName, baseFontsize, testDisp, hTextAlign, "");
        }
        private void AddTextDisplay(string name, double x, double y, Size size,
            string interfaceElementName, double baseFontsize, string testDisp, TextHorizontalAlignment hTextAlign, string ufcDictionary)
        {
            TextDisplay display = AddTextDisplay(
                name: name,
                posn: new Point(x, y),
                size: size,
                font: _font,
                baseFontsize: baseFontsize,
                horizontalAlignment: hTextAlign,
                verticalAligment: TextVerticalAlignment.Center,
                testTextDisplay: testDisp,
                textColor: Color.FromArgb(0xff, 0x30, 0x30, 0x30),
                backgroundColor: Color.FromArgb(0x00, 0xbb, 0xbb, 0xbb),
                useBackground: true,
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                textDisplayDictionary: ufcDictionary
                );
        }

        private void AddButton(string name, double x, double y, Size size, string interfaceElementName) => AddButton(name, x, y, size, interfaceElementName, _imageLocation + "");
        private void AddButton(string name, double x, double y, Size size, string interfaceElementName, string imageStem)
        {
            Point pos = new Point(x, y);
            AddButton(
                name: name,
                posn: pos,
                size: size,
                image: _imageLocation + imageStem,
                pushedImage: _imageLocation + "_transparent.png",
                buttonText: "",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
        }

        private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName) =>
            AddTwoWayToggle(name, x, y, size, interfaceElementName, "Toggles/round-");
        private void AddTwoWayToggle(string name, double x, double y, Size size, string interfaceElementName, string imageStem)
        {
            ToggleSwitch toggle = AddToggleSwitch(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ToggleSwitchPosition.One,
                defaultType: ToggleSwitchType.OnOn,
                positionOneImage: _imageLocation + imageStem + "up.png",
                positionTwoImage: _imageLocation + imageStem + "down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                clickType: LinearClickType.Swipe,
                fromCenter: false
                );
        }

        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName) =>
            AddThreeWayToggle(name, x, y, size, interfaceElementName, "Toggles/round-");
        private void AddThreeWayToggle(string name, double x, double y, Size size, string interfaceElementName, string imageStem)
        {
            ThreeWayToggleSwitch toggle = AddThreeWayToggle(
                name: name,
                posn: new Point(x, y),
                size: size,
                defaultPosition: ThreeWayToggleSwitchPosition.Two,
                defaultType: ThreeWayToggleSwitchType.OnOnOn,
                positionOneImage: _imageLocation + imageStem + "Up.png",
                positionTwoImage: _imageLocation + imageStem + "Normal.png",
                positionThreeImage: _imageLocation + imageStem + "Down.png",
                interfaceDeviceName: _interfaceDeviceName,
                interfaceElementName: interfaceElementName,
                fromCenter: false
                );
        }


        public override bool HitTest(Point location)
        {
            if (_scaledScreenRect0.Contains(location) || _scaledScreenRect1.Contains(location) || _scaledScreenRect2.Contains(location) || _scaledScreenRect3.Contains(location))
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