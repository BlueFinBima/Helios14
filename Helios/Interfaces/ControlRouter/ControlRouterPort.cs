// Copyright 2020 Ammo Goettsch
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using GadrocsWorkshop.Helios.Controls.Capabilities;

namespace GadrocsWorkshop.Helios.Interfaces.ControlRouter
{
    public class ControlRouterPort : NotificationObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// the persisted name of this port in case some ports are deleted later, this will
        /// keep bindings working
        /// </summary>
        [XmlAttribute("Name")]
        public string Name { get; set; }

        // control interfaces we know how to use, set to non-null if bound to a control that supports them
        private IRotaryControl _boundRotary;

        internal HeliosActionCollection Actions { get; } = new HeliosActionCollection();
        internal HeliosTriggerCollection Triggers { get; } = new HeliosTriggerCollection();
        internal HeliosValueCollection Values { get; } = new HeliosValueCollection();

        // currently bound control info
        private HeliosValue _controlName;

        /// <summary>
        /// backing field for property PulsesPerRevolution, contains
        /// the number of pulses that the input control generates per revolution
        /// </summary>
        private double _pulsesPerRevolution = 72d;

        /// <summary>
        /// backing field for property ValuePerRevolution, contains
        /// the value that would be sent by the input control if it was positioned at an angle of 360 degrees
        /// </summary>
        private double _valuePerRevolution = 1d;

        /// <summary>
        /// backing field for property ValueAtZeroDegrees, contains
        /// the value that would be sent by the input control if it was positioned at an angle of 0 degrees
        /// </summary>
        private double _valueAtZeroDegrees;

        // back reference to parent
        private ControlRouter _parent;

        // our target control's angle at time of binding
        private double _initialAngle;

        // our input value at time of binding
        private BindingValue _initialInputValue;
        
        // the angle to which we set the bound control on the last update
        private double _lastAngle;

        // just for debugging purposes, track how many pulses we have delivered
        private double _pulsesSinceBinding;

        /// <summary>
        /// if the bound control moves more than this many degrees without us doing it,
        /// we assume some other control has moved it and re-bind to the control so we don't
        /// suddenly snap it to our input value
        /// </summary>
        private const double CONTROL_ANGLE_TOLERANCE = 0.1d;

        // deserialization constructor
        public ControlRouterPort()
        {
            // no code
        }

        /// <summary>
        /// constructor for newly created port
        /// </summary>
        /// <param name="name"></param>
        public ControlRouterPort(string name) : this()
        {
            Name = name;
        }

        internal void Initialize(ControlRouter parent)
        {
            _parent = parent;

            // conversion from pulses to values
            HeliosAction changeValueByPulses = new HeliosAction(parent, Name, "Value from Pulses", "change",
                "Increment of decrement the value by a number of pulses", "+/- pulses", BindingValueUnits.Numeric);
            changeValueByPulses.Execute += _changeValueByPulses_Execute;
            Actions.Add(changeValueByPulses);

            // direct angle mapping
            HeliosAction changeAngle = new HeliosAction(parent, Name, "Relative Angle", "change",
                "Control the angle of bound rotary control relative to where it was when it was bound",
                "Value of Control", BindingValueUnits.Numeric);
            changeAngle.Execute += _changeAngle_Execute;
            Actions.Add(changeAngle);

            // status display: name of bound control
            _controlName = new HeliosValue(parent, BindingValue.Empty, Name, "Bound Control",
                "The name of the control receiving the output from this port", "Control name from Profile",
                BindingValueUnits.Text);
            Values.Add(_controlName);
            Triggers.Add(_controlName);
        }

        private void _changeValueByPulses_Execute(object action, HeliosActionEventArgs e)
        {
            ClaimControlIfAvailable(e);
            Logger.Debug("received change by pulses of {Pulses}", e.Value.DoubleValue);
            if (_boundRotary != null)
            {
                _pulsesSinceBinding += e.Value.DoubleValue;
                _boundRotary.ControlAngle += e.Value.DoubleValue * 360d / _pulsesPerRevolution;
                Logger.Debug("after {Pulses} pulses, set control to {Angle}", _pulsesSinceBinding, _boundRotary.ControlAngle);
            }
        }

        private void _changeAngle_Execute(object action, HeliosActionEventArgs e)
        {
            ClaimControlIfAvailable(e);
            Logger.Debug("received relative angle {Degrees}", e.Value.DoubleValue);
            if (_boundRotary != null)
            {
                // if the control has moved from where we last set it, then re-bind
                // don't just track deltas in this control because then error accumulates
                double currentAngle = _boundRotary.ControlAngle;
                if (Math.Abs(_lastAngle - currentAngle) > CONTROL_ANGLE_TOLERANCE)
                {
                    // some other control has moved it, rebind at this position
                    Logger.Debug("another control has moved our bound control, we will now process relative angle changes to this new starting position {Degrees}", currentAngle);
                    _initialAngle = currentAngle;
                    _lastAngle = _initialAngle;
                    _initialInputValue = e.Value;
                    return;
                }

                // division by zero guard and unreasonable scale guard
                double scale = ValuePerRevolution;
                if (Math.Abs(scale) < 0.00001)
                {
                    scale = 0.00001;
                }

                // calculate absolute position to avoid accumulating error
                _boundRotary.ControlAngle = _initialAngle +
                                            (e.Value.DoubleValue -
                                             (ValueAtZeroDegrees + _initialInputValue.DoubleValue)) * 360.0 / scale;

                // read back what it actually did
                _lastAngle = _boundRotary.ControlAngle;
            }
        }

        private void ClaimControlIfAvailable(HeliosActionEventArgs e)
        {
            if (_parent.TryClaimControl(out HeliosVisual visual))
            {
                _boundRotary = visual as IRotaryControl;
                _controlName.SetValue(new BindingValue(visual.Name), false);
                _initialAngle = _boundRotary?.ControlAngle ?? 0d;
                _lastAngle = _initialAngle;
                _initialInputValue = e.Value;
                _pulsesSinceBinding = 0d;
            }
        }

        /// <summary>
        /// the number of pulses that the input control generates per revolution
        /// </summary>
        [DefaultValue(72d)]
        [XmlElement("PulsesPerRevolution")]
        public double PulsesPerRevolution
        {
            get => _pulsesPerRevolution;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_pulsesPerRevolution == value)
                {
                    return;
                }

                double oldValue = _pulsesPerRevolution;
                _pulsesPerRevolution = value;
                OnPropertyChanged("PulsesPerRevolution", oldValue, value, true);
            }
        }

        /// <summary>
        /// the value that would be sent by the input control if it was positioned at an angle of 0 degrees
        /// </summary>
        [DefaultValue(0d)]
        [XmlElement("ValueAtZeroDegrees")]
        public double ValueAtZeroDegrees
        {
            get => _valueAtZeroDegrees;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_valueAtZeroDegrees == value)
                {
                    return;
                }

                double oldValue = _valueAtZeroDegrees;
                _valueAtZeroDegrees = value;
                OnPropertyChanged("ValueAtZeroDegrees", oldValue, value, true);
            }
        }

        /// <summary>
        /// the value that would be sent by the input control if it was positioned at an angle of 360 degrees
        /// </summary>
        [DefaultValue(1d)]
        [XmlElement("ValuePerRevolution")]
        public double ValuePerRevolution
        {
            get => _valuePerRevolution;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator we really do want to process every change
                if (_valuePerRevolution == value)
                {
                    return;
                }

                double oldValue = _valuePerRevolution;
                _valuePerRevolution = value;
                OnPropertyChanged("ValuePerRevolution", oldValue, value, true);
            }
        }
    }
}