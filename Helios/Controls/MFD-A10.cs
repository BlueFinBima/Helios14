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

namespace GadrocsWorkshop.Helios.Controls
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Globalization;
    using System.Windows;

    [HeliosControl("A10.MFD", "MFD", "A-10", typeof(MFDRenderer))]
    class MFD_A10 : MFD
    {
        private static readonly Rect SCREEN_RECT = new Rect(67, 67, 341, 343);
        private Rect _scaledScreenRect = SCREEN_RECT;

        public MFD_A10()
            : base("MFD", new Size(475, 475))
        {
            AddButton("OSB1", 117, 15);
            AddButton("OSB2", 167, 15);
            AddButton("OSB3", 218, 15);
            AddButton("OSB4", 270, 15);
            AddButton("OSB5", 320, 15);

            AddButton("OSB6", 428, 114);
            AddButton("OSB7", 428, 166);
            AddButton("OSB8", 428, 216);
            AddButton("OSB9", 428, 266);
            AddButton("OSB10", 428, 315);

            AddButton("OSB11", 322, 428);
            AddButton("OSB12", 271, 428);
            AddButton("OSB13", 220, 428);
            AddButton("OSB14", 169, 428);
            AddButton("OSB15", 118, 428);

            AddButton("OSB16", 13, 316);
            AddButton("OSB17", 13, 267);
            AddButton("OSB18", 13, 217);
            AddButton("OSB19", 13, 166);
            AddButton("OSB20", 13, 114);

            AddRocker("Adjust Display", "adj", 15, 40, false);
            AddRocker("Backlight Brightness", "dsp", 429, 40, false);
            AddRocker("Video Contrast", "con", 15, 370, false);
            AddRocker("Display Brightness", "brt", 429, 370, false);
            AddRocker("Symbol Brightness", "sym", 374, 429, true);

            Helios.Controls.RotarySwitch knob = new Helios.Controls.RotarySwitch();
            knob.Name = "Mode Knob";
            knob.KnobImage = "{Helios}/Images/A-10/mfd-knob.png";
            knob.DrawLabels = false;
            knob.DrawLines = false;
            knob.Positions.Clear();
            knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(knob, 0, "Day", 60d));
            knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(knob, 1, "Night", 90d));
            knob.Positions.Add(new Helios.Controls.RotarySwitchPosition(knob, 2, "Off", 120d));
            knob.CurrentPosition = 1;
            knob.Top = 436;
            knob.Left = 38;
            knob.Width = 36;
            knob.Height = 36;

            Children.Add(knob);
            foreach (IBindingTrigger trigger in knob.Triggers)
            {
                AddTrigger(trigger, "Mode Knob");
            }
            AddAction(knob.Actions["set.position"], "Mode Knob");
        }

        #region Properties

        public override string BezelImage
        {
            get { return "{Helios}/Images/A-10/mfd-bezel.png"; }
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

        private void AddButton(string name, double x, double y)
        {
            Helios.Controls.PushButton button = new Helios.Controls.PushButton();
            button.Top = y;
            button.Left = x;
            button.Width = 36;
            button.Height = 36;

            button.Image = "{Helios}/Images/A-10/mfd-out.png";
            button.PushedImage = "{Helios}/Images/A-10/mfd-in.png";

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

        private void AddRocker(string name, string imagePrefix, double x, double y, bool horizontal)
        {
            Helios.Controls.ThreeWayToggleSwitch rocker = new Helios.Controls.ThreeWayToggleSwitch();
            rocker.Name = name;
            rocker.SwitchType = Helios.Controls.ThreeWayToggleSwitchType.MomOnMom;
            rocker.ClickType = Helios.Controls.LinearClickType.Touch;
            rocker.PositionTwoImage = "{Helios}/Images/A-10/" + imagePrefix + "-norm.png";

            rocker.Top = y;
            rocker.Left = x;
            if (horizontal)
            {
                rocker.Orientation = Helios.Controls.ToggleSwitchOrientation.Horizontal;
                rocker.PositionOneImage = "{Helios}/Images/A-10/" + imagePrefix + "-left.png";
                rocker.PositionThreeImage = "{Helios}/Images/A-10/" + imagePrefix + "-right.png";
                rocker.Width = 56;
                rocker.Height = 32;
            }
            else
            {
                rocker.Orientation = Helios.Controls.ToggleSwitchOrientation.Vertical;
                rocker.PositionOneImage = "{Helios}/Images/A-10/" + imagePrefix + "-up.png";
                rocker.PositionThreeImage = "{Helios}/Images/A-10/" + imagePrefix + "-down.png";
                rocker.Width = 32;
                rocker.Height = 56;
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
