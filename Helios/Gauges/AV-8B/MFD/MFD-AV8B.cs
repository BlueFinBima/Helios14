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
/// This has been deprecated in favour of the autobinding Left and Right MFCD's which uses a higher res background image
/// </summary>

namespace GadrocsWorkshop.Helios.Controls
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Globalization;
    using System.Windows;

    [HeliosControl("AV8B.MPCD", "MPCD", "_Hidden Parts", typeof(MFDRenderer))]
    class MPCD_AV8B : MFD
    {
        private static readonly Rect SCREEN_RECT = new Rect(44,85, 345, 317);
        private Rect _scaledScreenRect = SCREEN_RECT;

        public MPCD_AV8B()
            : base("MPCD", new Size(435, 475))
        {
            AddButton("OSB1", 1, 334, true);
            AddButton("OSB2", 1, 285, true);
            AddButton("OSB3", 1, 237, true);
            AddButton("OSB4", 1, 189, true);
            AddButton("OSB5", 1, 141, true);

            AddButton("OSB6", 96, 51, false);
            AddButton("OSB7", 150, 51, false);
            AddButton("OSB8", 204, 51, false);
            AddButton("OSB9", 256, 51, false);
            AddButton("OSB10", 310, 51, false);

            AddButton("OSB11", 405, 141, true);
            AddButton("OSB12", 405, 189, true);
            AddButton("OSB13", 405, 237, true);
            AddButton("OSB14", 405, 285, true);
            AddButton("OSB15", 405, 334, true);

            AddButton("OSB16", 310, 420, false);
            AddButton("OSB17", 256, 420, false);
            AddButton("OSB18", 204, 420, false);
            AddButton("OSB19", 150, 420, false);
            AddButton("OSB20", 96, 420, false);

            AddRocker("Day / Night", "MFD Rocker", "L", 36, 26);
            AddRocker("Symbols", "MFD Rocker", "R", 344, 26);
            AddRocker("Gain", "MFD Rocker", "V", 3, 390);
            AddRocker("Contrast", "MFD Rocker", "V", 406, 390);

            Helios.Controls.Potentiometer _knob = new Helios.Controls.Potentiometer();
            AddPot("Brightness Knob", new Point(200,4), new Size(40,40));
}
        #region Properties

        public override string BezelImage
        {
            get { return "{AV-8B}/Images/MPCD bezel.png"; }
        }

        #endregion

        protected override void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            if (args.PropertyName.Equals("Width") || args.PropertyName.Equals("Height"))
            {
                double scaleX = Width / NativeSize.Width;
                double scaleY = Height / NativeSize.Height;
                _scaledScreenRect.Scale(scaleX, scaleY);
            }
            base.OnPropertyChanged(args);
        }
        private void AddPot(string name, Point posn, Size size)
        {
            Helios.Controls.Potentiometer _knob = new Helios.Controls.Potentiometer();
            _knob.Name = name;
            _knob.KnobImage = "{AV-8B}/Images/Common Knob.png";
            _knob.InitialRotation = 219;
            _knob.RotationTravel = 291;
            _knob.MinValue = 0;
            _knob.MaxValue = 1;
            _knob.InitialValue = 0;
            _knob.StepValue = 0.1;
            _knob.Top = posn.Y;
            _knob.Left = posn.X;
            _knob.Width = size.Width;
            _knob.Height = size.Height;

            Children.Add(_knob);
            foreach (IBindingTrigger trigger in _knob.Triggers)
            {
                AddTrigger(trigger, name);
            }
            AddAction(_knob.Actions["set.value"], name);
        }
        private void AddButton(string name, double x, double y, bool horizontal)
        {
            Helios.Controls.PushButton button = new Helios.Controls.PushButton();
            button.Top = y;
            button.Left = x;
            button.Width = 30;
            button.Height = 30;
            //button.TextPushOffset = new System.Windows.Media.TranslateTransform(1,1);
            //button.Image = "{FA-18C}/Images/MFD Button 1 Up.png";
            //button.PushedImage = "{FA-18C}/Images/MFD Button 1 Dn.png";
            if (!horizontal)
            {
                button.Image = "{AV-8B}/Images/MFD Button 1 UpV.png";
                button.PushedImage = "{AV-8B}/Images/MFD Button 1 DnV.png";
                //button.Rotation = HeliosVisualRotation.CCW;
            }
            else
            {
                button.Image = "{AV-8B}/Images/MFD Button 1 UpH.png";
                button.PushedImage = "{AV-8B}/Images/MFD Button 1 DnH.png";
            }

            button.Name = name;

            Children.Add(button);

            AddTrigger(button.Triggers["pushed"], name);
            AddTrigger(button.Triggers["released"], name);

            AddAction(button.Actions["push"], name);
            AddAction(button.Actions["release"], name);
            AddAction(button.Actions["set.physical state"], name);
        }

        private new void AddTrigger(IBindingTrigger trigger, string device)
        {
            trigger.Device = device;
            Triggers.Add(trigger);
        }

        private new void AddAction(IBindingAction action, string device)
        {
            action.Device = device;
            Actions.Add(action);
        }

        private void AddRocker(string name, string imagePrefix, string imageOrientation, double x, double y)
        {
            Helios.Controls.ThreeWayToggleSwitch rocker = new Helios.Controls.ThreeWayToggleSwitch();
            rocker.Name = name;
            rocker.SwitchType = Helios.Controls.ThreeWayToggleSwitchType.MomOnMom;
            rocker.ClickType = Helios.Controls.LinearClickType.Touch;
            rocker.PositionTwoImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Mid.png";

            rocker.Top = y;
            rocker.Left = x;
            switch (imageOrientation) {
                case ("V"):
                    //rocker.Orientation = Helios.Controls.ToggleSwitchOrientation.Horizontal;
                    rocker.PositionOneImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Up.png";
                    rocker.PositionThreeImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Dn.png";
                    rocker.Height = 56;
                    rocker.Width = 27;
                    break;
                case ("L"):
                        rocker.PositionOneImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Up.png";
                        rocker.PositionThreeImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Dn.png";
                    rocker.Width = 54;
                    rocker.Height = 45;
                    break;
                case ("R"):
                    rocker.PositionOneImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Up.png";
                    rocker.PositionThreeImage = "{AV-8B}/Images/" + imagePrefix + " " + imageOrientation + " Dn.png";
                    rocker.Width = 54;
                    rocker.Height = 45;
                    break;
            default:
                    break;
            }

        Children.Add(rocker);
            foreach (IBindingTrigger trigger in rocker.Triggers)
            {
                AddTrigger(trigger, name);
            }

            AddAction(rocker.Actions["set.position"], name);
        }

        public override bool HitTest(Point location)
        {
            if (_scaledScreenRect.Contains(location))
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
