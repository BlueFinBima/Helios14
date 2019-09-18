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
    using System.Xml;

    [HeliosControl("Helios.Base.Potentiometer", "Potentiometer - Knob 1", "Potentiometers", typeof(RotaryRenderer))]
    public class Potentiometer : Rotary
    {
        private double _value = 0.0d;

        private double _initialValue = 0.0d;
        private double _stepValue = 0.1d;
        private double _minValue = 0d;
        private double _maxValue = 1d;

        private double _initialRotation = 225d;
        private double _rotationTravel = 270d;

        private HeliosValue _potValue;

        public Potentiometer()
            : base("Potentiometer", new Size(100, 100))
        {
            KnobImage = "{Helios}/Images/Knobs/knob1.png";

            _potValue = new HeliosValue(this, new BindingValue(0d), "", "value", "Current value of the potentiometer.", "", BindingValueUnits.Numeric);
            _potValue.Execute += new HeliosActionHandler(SetValue_Execute);
            Values.Add(_potValue);
            Actions.Add(_potValue);
            Triggers.Add(_potValue);
        }

        #region Properties

        public double InitialValue
        {
            get
            {
                return _initialValue;
            }
            set
            {
                if (!_initialValue.Equals(value))
                {
                    double oldValue = _initialValue;
                    _initialValue = value;
                    OnPropertyChanged("InitialValue", oldValue, value, true);
                }
            }
        }

        public double MinValue
        {
            get
            {
                return _minValue;
            }
            set
            {
                if (!_minValue.Equals(value))
                {
                    double oldValue = _minValue;
                    _minValue = value;
                    OnPropertyChanged("MinValue", oldValue, value, true);
                    SetRotation();
                }
            }
        }

        public double MaxValue
        {
            get
            {
                return _maxValue;
            }
            set
            {
                if (!_maxValue.Equals(value))
                {
                    double oldValue = _maxValue;
                    _maxValue = value;
                    OnPropertyChanged("MaxValue", oldValue, value, true);
                    SetRotation();
                }
            }
        }

        public double StepValue
        {
            get
            {
                return _stepValue;
            }
            set
            {
                if (!_stepValue.Equals(value))
                {
                    double oldValue = _stepValue;
                    _stepValue = value;
                    OnPropertyChanged("StepValue", oldValue, value, true);
                }
            }
        }

        public double Value
        {
            get
            {
                return _value;
            }
            set
            {
                if (!_value.Equals(value))
                {
                    double oldValue = _value;
                    _value = value;
                    _potValue.SetValue(new BindingValue(_value), BypassTriggers);
                    OnPropertyChanged("Value", oldValue, value, true);
                    SetRotation();
                }
            }
        }

        public double InitialRotation
        {
            get
            {
                return _initialRotation;
            }
            set
            {
                if (!_initialRotation.Equals(value))
                {
                    double oldValue = _initialRotation;
                    _initialRotation = value;
                    OnPropertyChanged("InitialRotation", oldValue, value, true);
                    SetRotation();
                }
            }
        }

        public double RotationTravel
        {
            get
            {
                return _rotationTravel;
            }
            set
            {
                if (!_rotationTravel.Equals(value))
                {
                    double oldValue = _rotationTravel;
                    _rotationTravel = value;
                    OnPropertyChanged("RotationTravel", oldValue, value, true);
                    SetRotation();
                }
            }
        }

        #endregion

        #region Actions

        void SetValue_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                BeginTriggerBypass(e.BypassCascadingTriggers);
                Value = e.Value.DoubleValue;
                EndTriggerBypass(e.BypassCascadingTriggers);
            }
            catch
            {
                // No-op if the parse fails we won't set the position.
            }
        }

        #endregion

        private void SetRotation()
        {
            KnobRotation = InitialRotation + (((Value - MinValue) / (MaxValue - MinValue)) * RotationTravel);
        }

        protected override void Pulse(bool increment)
        {
            double newValue = Value;

            if (increment)
            {
                newValue = Math.Min(Value + _stepValue, _maxValue);
            }
            else
            {
                newValue = Math.Max(Value - _stepValue, _minValue);
            }

            Value = newValue;
        }

        public override void Reset()
        {
            BeginTriggerBypass(true);
            Value = InitialValue;
            EndTriggerBypass(true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("KnobImage", KnobImage);
            writer.WriteElementString("InitialValue", InitialValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("StepValue", StepValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("MaxValue", MaxValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("MinValue", MinValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("InitialRotation", InitialRotation.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("RotationTravel", RotationTravel.ToString(CultureInfo.InvariantCulture));
            writer.WriteStartElement("ClickType");
            writer.WriteElementString("Type", ClickType.ToString());
            if (ClickType == Controls.ClickType.Swipe)
            {
                writer.WriteElementString("Sensitivity", SwipeSensitivity.ToString(CultureInfo.InvariantCulture));
            }
            writer.WriteEndElement();
            writer.WriteElementString("MouseWheel", MouseWheelAction.ToString(CultureInfo.InvariantCulture));
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            KnobImage = reader.ReadElementString("KnobImage");
            InitialValue = double.Parse(reader.ReadElementString("InitialValue"), CultureInfo.InvariantCulture);
            StepValue = double.Parse(reader.ReadElementString("StepValue"), CultureInfo.InvariantCulture);
            MaxValue = double.Parse(reader.ReadElementString("MaxValue"), CultureInfo.InvariantCulture);
            MinValue = double.Parse(reader.ReadElementString("MinValue"), CultureInfo.InvariantCulture);
            InitialRotation = double.Parse(reader.ReadElementString("InitialRotation"), CultureInfo.InvariantCulture);
            RotationTravel = double.Parse(reader.ReadElementString("RotationTravel"), CultureInfo.InvariantCulture);
            if (reader.Name.Equals("ClickType"))
            {
                reader.ReadStartElement("ClickType");
                ClickType = (ClickType)Enum.Parse(typeof(ClickType), reader.ReadElementString("Type"));
                if (ClickType == Controls.ClickType.Swipe)
                {
                    SwipeSensitivity = double.Parse(reader.ReadElementString("Sensitivity"), CultureInfo.InvariantCulture);
                }
                reader.ReadEndElement();
            }
            else
            {
                ClickType = Controls.ClickType.Swipe;
                SwipeSensitivity = 0d;
            }

            try { 
                bool mw;
                bool.TryParse(reader.ReadElementString("MouseWheel"), out mw);
                MouseWheelAction = mw;
            }
            catch (Exception ex)
            {
                MouseWheelAction = true;
            }

            BeginTriggerBypass(true);
            Value = InitialValue;
            SetRotation();
            EndTriggerBypass(true);
        }
    }
}
