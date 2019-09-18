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

    [HeliosControl("Helios.Base.RotaryEncoder", "Encoder - Knob 1", "Rotary Encoders", typeof(RotaryRenderer))]
    public class RotaryEncoder : Rotary
    {
        private double _stepValue = 0.1d;
        private double _initialRotation = 0d;
        private double _rotationStep = 5d;

        private HeliosTrigger _incrementTrigger;
        private HeliosTrigger _decrementTrigger;

        public RotaryEncoder()
            : base("Rotary Encoder", new Size(100, 100))
        {
            KnobImage = "{Helios}/Images/Knobs/knob1.png";

            _incrementTrigger = new HeliosTrigger(this, "", "encoder", "incremented", "Triggered when encoder is incremented.", "Current encoder value", BindingValueUnits.Numeric);
            Triggers.Add(_incrementTrigger);
            _decrementTrigger = new HeliosTrigger(this, "", "encoder", "decremented", "Triggered when encoder is decremented.", "Current encoder value", BindingValueUnits.Numeric);
            Triggers.Add(_decrementTrigger);
        }

        #region Properties

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

        public double RotationStep
        {
            get
            {
                return _rotationStep;
            }
            set
            {
                if (!_rotationStep.Equals(value))
                {
                    double oldValue = _rotationStep;
                    _rotationStep = value;
                    OnPropertyChanged("RotationStep", oldValue, value, true);
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
                    KnobRotation = _initialRotation;
                }
            }
        }

        #endregion

        protected override void Pulse(bool increment)
        {
            if (increment)
            {
                KnobRotation += _rotationStep;
                if (!BypassTriggers)
                {
                    _incrementTrigger.FireTrigger(new BindingValue(StepValue));
                }
            }
            else
            {
                KnobRotation -= _rotationStep;
                if (!BypassTriggers)
                {
                    _decrementTrigger.FireTrigger(new BindingValue(-StepValue));
                }
            }

            OnDisplayUpdate();
        }


        public override void Reset()
        {
            BeginTriggerBypass(true);
            KnobRotation = InitialRotation;
            EndTriggerBypass(true);
        }


        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("KnobImage", KnobImage);
            writer.WriteElementString("RotationStep", RotationStep.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("StepValue", StepValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("InitialRotation", InitialRotation.ToString(CultureInfo.InvariantCulture));
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
            RotationStep = double.Parse(reader.ReadElementString("RotationStep"), CultureInfo.InvariantCulture);
            StepValue = double.Parse(reader.ReadElementString("StepValue"), CultureInfo.InvariantCulture);
            InitialRotation = double.Parse(reader.ReadElementString("InitialRotation"), CultureInfo.InvariantCulture);

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

            try
            {
                bool mw;
                bool.TryParse(reader.ReadElementString("MouseWheel"), out mw);
                MouseWheelAction = mw;
            } catch (Exception ex) {
                MouseWheelAction = true;
            }

            Reset();
        }
    }
}
