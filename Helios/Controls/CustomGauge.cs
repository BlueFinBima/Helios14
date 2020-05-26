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

    [HeliosControl("Helios.Base.CustomGauge", "Custom Gauge", "Custom Controls", typeof(CustomGaugeRenderer))]
    public class CustomGauge : CustomNeedle
    {
        private double _value = 0.0d;

        private double _Needle_Scale = 1d;
        private double _needle_height = 1d;
        private double _needle_PivotX = 0.5d;
        private double _needle_PivotY = 0.5d;
        private double _needle_PosX = 0d;
        private double _needle_PosY = 0d;
        private double _initialValue = 0.0d;
        private double _stepValue = 0.1d;
        private double _minValue = 0d;
        private double _maxValue = 1d;
        private string _bgplateImage = "{Helios}/Gauges/KA-50/RadarAltimeter/radar_alt_faceplate.xaml";
        private double _initialRotation = 0d;
        private double _rotationTravel = 360d;

        private HeliosValue _potValue;

        public CustomGauge()
            : base("CustomGauge", new Size(100, 100))
        {
            KnobImage = "{Helios}/Gauges/KA-50/RadarAltimeter/radar_alt_needle.xaml";
            _potValue = new HeliosValue(this, new BindingValue(0d), "", "value", "Current value of the CustomGauge.", "", BindingValueUnits.Numeric);
            _potValue.Execute += new HeliosActionHandler(SetValue_Execute);
            Values.Add(_potValue);
            Actions.Add(_potValue);
            //Triggers.Add(_potValue);
        }

        #region Properties


        public string BGPlateImage
        {
            get
            {
                return _bgplateImage;
            }
            set
            {
                if ((_bgplateImage == null && value != null)
                    || (_bgplateImage != null && !_bgplateImage.Equals(value)))
                {
                    string oldValue = _bgplateImage;
                    _bgplateImage = value;
                    OnPropertyChanged("BGPlateImage", oldValue, value, true);
                    Refresh();
                }
            }
        }

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


        public double Needle_Scale
        {
            get
            {
                return _Needle_Scale;
            }
            set
            {
                if (!_Needle_Scale.Equals(value))
                {
                    double oldValue = _Needle_Scale;
                    _Needle_Scale = value;
                    OnPropertyChanged("Needle_Scale", oldValue, value, true);
                    Refresh();
                }
            }
        }


        public double Needle_Height
        {
            get
            {
                return _needle_height;
            }
            set
            {
                if (!_needle_height.Equals(value))
                {
                    double oldValue = _needle_height;
                    _needle_height = value;
                    OnPropertyChanged("Needle_Height", oldValue, value, true);
                    Refresh();
                }
            }
        }



        public double Needle_PivotX
        {
            get
            {
                return _needle_PivotX;
            }
            set
            {
                if (!_needle_PivotX.Equals(value))
                {
                    double oldValue = _needle_PivotX;
                    _needle_PivotX = value;
                    OnPropertyChanged("Needle_PivotX", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double Needle_PivotY
        {
            get
            {
                return _needle_PivotY;
            }
            set
            {
                if (!_needle_PivotY.Equals(value))
                {
                    double oldValue = _needle_PivotY;
                    _needle_PivotY = value;
                    OnPropertyChanged("Needle_PivotY", oldValue, value, true);
                    Refresh();
                }
            }
        }


        public double Needle_PosX
        {
            get
            {
                return _needle_PosX;
            }
            set
            {
                if (!_needle_PosX.Equals(value))
                {
                    double oldValue = _needle_PosX;
                    _needle_PosX = value;
                    OnPropertyChanged("Needle_PosX", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double Needle_PosY
        {
            get
            {
                return _needle_PosY;
            }
            set
            {
                if (!_needle_PosY.Equals(value))
                {
                    double oldValue = _needle_PosY;
                    _needle_PosY = value;
                    OnPropertyChanged("Needle_PosY", oldValue, value, true);
                    Refresh();
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

        public override void Reset()
        {
            BeginTriggerBypass(true);
            Value = InitialValue;
            EndTriggerBypass(true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            writer.WriteElementString("BGPlateImage", BGPlateImage);
            writer.WriteElementString("KnobImage", KnobImage);
            writer.WriteElementString("Needle_Scale", Needle_Scale.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Needle_PosX", Needle_PosX.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Needle_PosY", Needle_PosY.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Needle_PivotX", Needle_PivotX.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("Needle_PivotY", Needle_PivotY.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("InitialValue", InitialValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("StepValue", StepValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("MaxValue", MaxValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("MinValue", MinValue.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("InitialRotation", InitialRotation.ToString(CultureInfo.InvariantCulture));
            writer.WriteElementString("RotationTravel", RotationTravel.ToString(CultureInfo.InvariantCulture));
 
        }

        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);
            BGPlateImage = reader.ReadElementString("BGPlateImage");
            KnobImage = reader.ReadElementString("KnobImage");
            Needle_Scale = double.Parse(reader.ReadElementString("Needle_Scale"), CultureInfo.InvariantCulture);
            Needle_PosX = double.Parse(reader.ReadElementString("Needle_PosX"), CultureInfo.InvariantCulture);
            Needle_PosY = double.Parse(reader.ReadElementString("Needle_PosY"), CultureInfo.InvariantCulture);
            Needle_PivotX = double.Parse(reader.ReadElementString("Needle_PivotX"), CultureInfo.InvariantCulture);
            Needle_PivotY = double.Parse(reader.ReadElementString("Needle_PivotY"), CultureInfo.InvariantCulture);
            InitialValue = double.Parse(reader.ReadElementString("InitialValue"), CultureInfo.InvariantCulture);
            StepValue = double.Parse(reader.ReadElementString("StepValue"), CultureInfo.InvariantCulture);
            MaxValue = double.Parse(reader.ReadElementString("MaxValue"), CultureInfo.InvariantCulture);
            MinValue = double.Parse(reader.ReadElementString("MinValue"), CultureInfo.InvariantCulture);
            InitialRotation = double.Parse(reader.ReadElementString("InitialRotation"), CultureInfo.InvariantCulture);
            RotationTravel = double.Parse(reader.ReadElementString("RotationTravel"), CultureInfo.InvariantCulture);
     
            BeginTriggerBypass(true);
            Value = InitialValue;
            SetRotation();
            EndTriggerBypass(true);
        }
    }
}
