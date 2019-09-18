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
    using System;
    using System.Windows;

    public abstract class Rotary : HeliosVisual
    {
        private string _knobImage;
        private double _rotation;

        private double _repeatDelay = 750d;
        private double _repeatRate = 200d;
        private int _lastRepeat = int.MinValue;
        private int _lastPulse = int.MinValue;
        private bool _repeating = false;
        private bool _increment = false;

        private const double SWIPE_SENSITIVY_BASE = 45d;

        private bool _mouseDown = false;
        private Point _mouseDownLocation;

        private ClickType _clickType = ClickType.Swipe;
        private bool _mouseWheelAction = true;
        private CalibrationPointCollectionDouble _swipeCalibration;
        private double _swipeThreshold = 45d;
        private double _swipeSensitivity = 0d;

        protected Rotary(string name, Size defaultSize)
            : base(name, defaultSize)
        {
            _swipeCalibration = new CalibrationPointCollectionDouble(-1d, 2d, 1d, 0.5d);
            _swipeCalibration.Add(new CalibrationPointDouble(0.0d, 1d));
        }

        #region Properties

        public ClickType ClickType
        {
            get
            {
                return _clickType;
            }
            set
            {
                if (!_clickType.Equals(value))
                {
                    ClickType oldValue = _clickType;
                    _clickType = value;
                    OnPropertyChanged("ClickType", oldValue, value, true);
                }
            }
        }

        public bool MouseWheelAction
        {
            get
            {
                return _mouseWheelAction;
            }
            set
            {
                if (!_clickType.Equals(value))
                {
                    bool oldValue = _mouseWheelAction;
                    _mouseWheelAction = value;
                    OnPropertyChanged("MouseWheelAction", oldValue, value, true);
                }
            }
        }

        public double SwipeSensitivity
        {
            get
            {
                return _swipeSensitivity;
            }
            set
            {
                if (!_swipeSensitivity.Equals(value))
                {
                    double oldValue = _swipeSensitivity;
                    _swipeSensitivity = value;
                    _swipeThreshold = SWIPE_SENSITIVY_BASE * _swipeCalibration.Interpolate(_swipeSensitivity);
                    OnPropertyChanged("SwipeSensitivity", oldValue, value, true);
                }
            }
        }

        public string KnobImage
        {
            get
            {
                return _knobImage;
            }
            set
            {
                if ((_knobImage == null && value != null)
                    || (_knobImage != null && !_knobImage.Equals(value)))
                {
                    string oldValue = _knobImage;
                    _knobImage = value;
                    OnPropertyChanged("KnobImage", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double KnobRotation
        {
            get
            {
                return _rotation;
            }
            protected set
            {
                if (!_rotation.Equals(value))
                {
                    double oldValue = _rotation;
                    _rotation = value % 360d;
                    OnPropertyChanged("KnobRotation", oldValue, value, false);
                    OnDisplayUpdate();
                }
            }
        }

        public double RepeatDelay
        {
            get
            {
                return _repeatDelay;
            }
            set
            {
                if (!_repeatDelay.Equals(value))
                {
                    double oldValue = _repeatDelay;
                    _repeatDelay = value;
                    OnPropertyChanged("RepeatDelay", oldValue, value, true);
                }
            }
        }

        public double RepeatRate
        {
            get
            {
                return _repeatRate;
            }
            set
            {
                if (!_repeatRate.Equals(value))
                {
                    double oldValue = _repeatRate;
                    _repeatRate = value;
                    OnPropertyChanged("RepeatRate", oldValue, value, true);
                }
            }
        }

        #endregion

        protected abstract void Pulse(bool increment);

        private Vector VectorFromCenter(Point devicePosition)
        {
            return devicePosition - new Point(DisplayRectangle.Width / 2, DisplayRectangle.Height / 2);
        }

        private double GetAngle(Point startPoint, Point endPoint)
        {
            return Vector.AngleBetween(VectorFromCenter(startPoint), VectorFromCenter(endPoint));
        }

        public override void MouseDown(Point location)
        {
            if (_clickType == ClickType.Touch)
            {
                _increment = (location.X > Width / 2d);
                Pulse(_increment);
                _repeating = false;
                _repeatRate = 200d;
                _lastRepeat = Environment.TickCount & Int32.MaxValue;

                if (Parent != null && Parent.Profile != null)
                {
                    Parent.Profile.ProfileTick += new EventHandler(Profile_ProfileTick);
                }
            }
            else if (_clickType == ClickType.Swipe)
            {
                _mouseDown = true;
                _mouseDownLocation = location;
            }
        }

        public override void MouseWheel(int delta)
        {
            if (_mouseWheelAction)
            {
                Pulse(delta > 0);
            }
        }

        void Profile_ProfileTick(object sender, EventArgs e)
        {
            int currentTick = Environment.TickCount & Int32.MaxValue;

            if (_repeating && (currentTick < _lastPulse || (currentTick - _lastPulse > _repeatRate)))
            {
                Pulse(_increment);
                _lastPulse = currentTick;
            }

            if (currentTick < _lastRepeat || (currentTick - _lastRepeat > _repeatDelay))
            {
                if (_repeating && _repeatRate > 33)
                {
                    _repeatRate = _repeatRate / 2;
                    if (_repeatRate < 33) _repeatRate = 33;
                }
                Pulse(_increment);
                _lastPulse = currentTick;
                _lastRepeat = currentTick;
                _repeating = true;
            }
        }

        public override void MouseDrag(Point location)
        {
            if (_mouseDown && _clickType == ClickType.Swipe)
            {
                double newAngle = GetAngle(_mouseDownLocation, location);

                if (Math.Abs(newAngle) > _swipeThreshold)
                {
                    bool increment = (newAngle > 0);
                    Pulse(increment);
                    _mouseDownLocation = location;
                }
            }
        }

        public override void MouseUp(Point location)
        {
            _mouseDown = false;
            if (Parent != null && Parent.Profile != null)
            {
                Parent.Profile.ProfileTick -= new EventHandler(Profile_ProfileTick);
            }
        }
    }
}
