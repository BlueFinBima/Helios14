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
    using System.Windows;

    [HeliosControl("F16.MFD", "MFD", "F-16", typeof(MFDRenderer))]
    class MFD_F16 : MFD
    {
        private static readonly Rect SCREEN_RECT = new Rect(67, 67, 341, 343);

        private Rect _scaledScreenRect = SCREEN_RECT;

        public MFD_F16()
            : base("MFD", new Size(475, 475))
        {
            AddButton("OSB1", 105, 16);
            AddButton("OSB2", 162, 16);
            AddButton("OSB3", 218, 16);
            AddButton("OSB4", 274, 16);
            AddButton("OSB5", 331, 16);

            AddButton("OSB6", 419, 105);
            AddButton("OSB7", 419, 162);
            AddButton("OSB8", 419, 218);
            AddButton("OSB9", 419, 275);
            AddButton("OSB10", 419, 332);

            AddButton("OSB15", 105, 419);
            AddButton("OSB14", 162, 419);
            AddButton("OSB13", 218, 419);
            AddButton("OSB12", 274, 419);
            AddButton("OSB11", 331, 419);

            AddButton("OSB20", 16, 105);
            AddButton("OSB19", 16, 162);
            AddButton("OSB18", 16, 218);
            AddButton("OSB17", 16, 275);
            AddButton("OSB16", 16, 332);

            AddRocker("Sensor Gain", "rocker", 16, 30, false);
            AddRocker("Symbology", "rocker", 419, 30, false);
            AddRocker("Brightness", "rocker", 16, 382, false);
            AddRocker("Contrast", "rocker", 419, 382, false);
        }

        #region Properties

        public override string BezelImage
        {
            get { return "{Helios}/Images/F-16/mfd_bezel.png"; }
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
            PushButton button = new PushButton();
            button.Top = y;
            button.Left = x;
            button.Width = 36;
            button.Height = 36;

            button.Image = "{Helios}/Images/F-16/mfd-out.png";
            button.PushedImage = "{Helios}/Images/F-16/mfd-in.png";

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
            ThreeWayToggleSwitch rocker = new ThreeWayToggleSwitch();
            rocker.Name = name;
            rocker.SwitchType = ThreeWayToggleSwitchType.MomOnMom;
            rocker.ClickType = LinearClickType.Touch;
            rocker.PositionTwoImage = "{Helios}/Images/F-16/" + imagePrefix + "-middle.png";

            rocker.Top = y;
            rocker.Left = x;
            if (horizontal)
            {
                rocker.Orientation = ToggleSwitchOrientation.Horizontal;
                rocker.PositionOneImage = "{Helios}/Images/F-16/" + imagePrefix + "-left.png";
                rocker.PositionThreeImage = "{Helios}/Images/F-16/" + imagePrefix + "-right.png";
                rocker.Width = 56;
                rocker.Height = 32;
            }
            else
            {
                rocker.Orientation = ToggleSwitchOrientation.Vertical;
                rocker.PositionOneImage = "{Helios}/Images/F-16/" + imagePrefix + "-up.png";
                rocker.PositionThreeImage = "{Helios}/Images/F-16/" + imagePrefix + "-down.png";
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
            return _scaledScreenRect.Contains(location);
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
