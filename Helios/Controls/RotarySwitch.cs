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
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;

    [HeliosControl("Helios.Base.RotarySwitch", "Rotary - Knob 2", "Rotary Switches", typeof(RotarySwitchRenderer))]
    public class RotarySwitch : HeliosVisual
    {
        private const double SWIPE_SENSITIVY_BASE = 45d;

        private bool _mouseDown = false;
        private Point _mouseDownLocation;

        private ClickType _clickType = ClickType.Swipe;
        private bool _mouseWheelAction = true;
        private CalibrationPointCollectionDouble _swipeCalibration;
        private double _swipeThreshold = 45d;
        private double _swipeSensitivity = 0d;

        private RotarySwitchPositionCollection _positions = new RotarySwitchPositionCollection();
        private string _knobImage = "{Helios}/Images/Knobs/knob2.png";
        private int _currentPosition;
        private int _defaultPosition;
        private double _rotation;

        private bool _drawLines = true;
        private double _lineThickness = 2d;
        private Color _lineColor = Colors.White;
        private double _lineLength = 0.9d;

        private bool _drawLabels = true;
        private double _labelDistance = 1d;
        private double _maxLabelWidth = 40d;
        private double _maxLabelHeight = 0d;
        private Color _labelColor = Colors.White;
        private TextFormat _labelFormat = new TextFormat();

        private HeliosValue _positionValue;
        private HeliosValue _positionNameValue;

        private bool _isContinuous = false;

        public RotarySwitch()
            : base("Rotary Switch", new Size(100, 100))
        {
            _swipeCalibration = new CalibrationPointCollectionDouble(-1d, 2d, 1d, 0.5d);
            _swipeCalibration.Add(new CalibrationPointDouble(0.0d, 1d));

            _labelFormat.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(LabelFormat_PropertyChanged);

            _positionValue = new HeliosValue(this, new BindingValue(1), "", "position", "Current position of the switch.", "", BindingValueUnits.Numeric);
            _positionValue.Execute += new HeliosActionHandler(SetPositionAction_Execute);
            Values.Add(_positionValue);
            Actions.Add(_positionValue);
            Triggers.Add(_positionValue);

            _positionNameValue = new HeliosValue(this, new BindingValue("0"), "", "position name", "Name of the current position of the switch.", "", BindingValueUnits.Text);
            Values.Add(_positionNameValue);
            Triggers.Add(_positionNameValue);

            _positions.CollectionChanged += new NotifyCollectionChangedEventHandler(Positions_CollectionChanged);
            _positions.PositionChanged += new EventHandler<RotarySwitchPositionChangeArgs>(PositionChanged);
            _positions.Add(new RotarySwitchPosition(this, 1, "0", 0d));
            _positions.Add(new RotarySwitchPosition(this, 2, "1", 90d));
            _currentPosition = 1;
            _defaultPosition = 1;
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

        public bool DrawLabels
        {
            get
            {
                return _drawLabels;
            }
            set
            {
                if (!_drawLabels.Equals(value))
                {
                    bool oldValue = _drawLabels;
                    _drawLabels = value;
                    OnPropertyChanged("DrawLabels", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double LabelDistance
        {
            get
            {
                return _labelDistance;
            }
            set
            {
                if (!_labelDistance.Equals(value))
                {
                    double oldValue = _labelDistance;
                    _labelDistance = value;
                    OnPropertyChanged("LabelDistance", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double MaxLabelHeight
        {
            get
            {
                return _maxLabelHeight;
            }
            set
            {
                if (!_maxLabelHeight.Equals(value))
                {
                    double oldValue = _maxLabelHeight;
                    _maxLabelHeight = value;
                    OnPropertyChanged("MaxLabelHeight", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double MaxLabelWidth
        {
            get
            {
                return _maxLabelWidth;
            }
            set
            {
                if (!_maxLabelWidth.Equals(value))
                {
                    double oldValue = _maxLabelWidth;
                    _maxLabelWidth = value;
                    OnPropertyChanged("MaxLabelWidth", oldValue, value, true);
                    Refresh();
                }
            }
        }


        public bool DrawLines
        {
            get
            {
                return _drawLines;
            }
            set
            {
                if (!_drawLines.Equals(value))
                {
                    bool oldValue = _drawLines;
                    _drawLines = value;
                    OnPropertyChanged("DrawLines", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public bool IsContinuous
        {
            get
            {
                return _isContinuous;
            }
            set
            {
                if (!_isContinuous.Equals(value))
                {
                    bool oldValue = _isContinuous;
                    _isContinuous = value;
                    OnPropertyChanged("IsContinuous", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public Color LabelColor
        {
            get
            {
                return _labelColor;
            }
            set
            {
                if (!_labelColor.Equals(value))
                {
                    Color oldValue = _labelColor;
                    _labelColor = value;
                    OnPropertyChanged("LabelColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public TextFormat LabelFormat
        {
            get
            {
                return _labelFormat;
            }
        }

        public double LineThickness
        {
            get
            {
                return _lineThickness;
            }
            set
            {
                if (!_lineThickness.Equals(value))
                {
                    double oldValue = _lineThickness;
                    _lineThickness = value;
                    OnPropertyChanged("LineThickness", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public Color LineColor
        {
            get
            {
                return _lineColor;
            }
            set
            {
                if (!_lineColor.Equals(value))
                {
                    Color oldValue = _lineColor;
                    _lineColor = value;
                    OnPropertyChanged("LineColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public double LineLength
        {
            get
            {
                return _lineLength;
            }
            set
            {
                if (!_lineLength.Equals(value))
                {
                    double oldValue = _lineLength;
                    _lineLength = value;
                    OnPropertyChanged("LineLength", oldValue, value, true);
                    Refresh();
                }
            }
        }


        public RotarySwitchPositionCollection Positions
        {
            get { return _positions; }
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

        public int CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                if (!_currentPosition.Equals(value) && value > 0 && value <= Positions.Count)
                {
                    int oldValue = _currentPosition;
                    double oldRotatoin = _rotation;

                    _currentPosition = value;
                    _rotation = Positions[value-1].Rotation;

                    _positionValue.SetValue(new BindingValue((double)_currentPosition), BypassTriggers);
                    _positionNameValue.SetValue(new BindingValue(Positions[_currentPosition-1].Name), BypassTriggers);

                    if (!BypassTriggers)
                    {
                        if (oldValue > 0 && oldValue < Positions.Count)
                        {
                            Positions[oldValue-1].ExitTrigger.FireTrigger(BindingValue.Empty);
                        }
                        Positions[_currentPosition-1].EnterTriggger.FireTrigger(BindingValue.Empty);
                    }

                    OnPropertyChanged("CurrentPosition", oldValue, value, false);
                    OnPropertyChanged("Rotation", oldRotatoin, _rotation, false);
                    OnDisplayUpdate();
                }
            }
        }

        public int DefaultPosition
        {
            get
            {
                return _defaultPosition;
            }
            set
            {
                if (!_defaultPosition.Equals(value) && value > 0 && value <= Positions.Count)
                {
                    int oldValue = _defaultPosition;
                    _defaultPosition = value;
                    OnPropertyChanged("DefaultPosition", oldValue, value, true);
                }
            }
        }

        public double KnobRotation
        {
            get { return _rotation; }
        }
        
        #endregion

        void LabelFormat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            PropertyNotificationEventArgs origArgs = e as PropertyNotificationEventArgs;
            if (origArgs != null)
            {
                OnPropertyChanged("LabelFormat", origArgs);
            }
            Refresh();
        }

        void Positions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (RotarySwitchPosition position in e.OldItems)
                {
                    Triggers.Remove(position.EnterTriggger);
                    Triggers.Remove(position.ExitTrigger);
                }

                if (Positions.Count == 0)
                {
                    _currentPosition = 0;
                }
                else if (_currentPosition > Positions.Count)
                {
                    _currentPosition = Positions.Count;
                }
            }

            if (e.NewItems != null)
            {
                foreach (RotarySwitchPosition position in e.NewItems)
                {
                    Triggers.Add(position.EnterTriggger);
                    Triggers.Add(position.ExitTrigger);
                }
            }

            // Need to do it twice to prevent duplicates...  this is
            // just an easy way to do it instead of reordering everything in the loops above.
            int i = 1000;
            foreach (RotarySwitchPosition position in Positions)
            {
                position.Index = i++;
            }

            i = 1;
            foreach (RotarySwitchPosition position in Positions)
            {
                position.Index = i++;
            }
            UpdateValueHelp();
        }

        void PositionChanged(object sender, RotarySwitchPositionChangeArgs e)
        {
            PropertyNotificationEventArgs args = new PropertyNotificationEventArgs(e.Position, e.PropertyName, e.OldValue, e.NewValue, true);
            if (e.PropertyName.Equals("Rotation"))
            {
                _rotation = Positions[CurrentPosition - 1].Rotation;
            }
            OnPropertyChanged("Positions", args);
            UpdateValueHelp();
            Refresh();
        }

        private void UpdateValueHelp()
        {
            StringBuilder sb = new StringBuilder("");
            sb.Append(" (");
            for (int i = 0; i < Positions.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(",");
                }
                RotarySwitchPosition position = Positions[i];
                sb.Append(i + 1);
                if (position.Name != null && position.Name.Length > 0)
                {
                    sb.Append("=");
                    sb.Append(position.Name);
                }
            }
            sb.Append(")");
            _positionValue.ValueDescription = sb.ToString();
        }

        private Vector VectorFromCenter(Point devicePosition)
        {
            return devicePosition - new Point(DisplayRectangle.Width / 2, DisplayRectangle.Height / 2);
        }

        private double GetAngle(Point startPoint, Point endPoint)
        {
            return Vector.AngleBetween(VectorFromCenter(startPoint), VectorFromCenter(endPoint));
        }

        public override void MouseWheel(int delta)
        {
            if (_mouseWheelAction)
            {
                if(delta > 0)
                {
                    if (_currentPosition <= Positions.Count)
                    {
                        CurrentPosition = _currentPosition + 1;
                    }
                }
                else
                {
                    if (_currentPosition > 1)
                    {
                        CurrentPosition = _currentPosition - 1;
                    }
                }
            }
        }

        public override void MouseDown(Point location)
        {
            if(NonClickableZones != null)
            {
                foreach (NonClickableZone zone in NonClickableZones)
                {
                    if (zone.AllPositions && zone.isClickInZone(location))
                    {
                        zone.ChildVisual.MouseDown(new System.Windows.Point(location.X - (zone.ChildVisual.Left - this.Left), location.Y - (zone.ChildVisual.Top - this.Top)));
                        return; //we get out to let the ChildVisual using the click
                    }
                }
            }
            if (_clickType == ClickType.Touch)
            {
                bool increment = (location.X > Width / 2d);

                if (increment)
                {
                    if (_currentPosition < Positions.Count)
                    {
                        CurrentPosition = _currentPosition + 1;
                    }
                    else if (_isContinuous == true)
                    {
                        CurrentPosition = 1;
                    }
                }
                else
                {
                    if (_currentPosition > 1)
                    {
                        CurrentPosition = _currentPosition - 1;
                    }
                    else if (_isContinuous == true)
                    {
                        CurrentPosition = Positions.Count;
                    }
                }
            }
            else if (_clickType == ClickType.Swipe)
            {
                _mouseDown = true;
                _mouseDownLocation = location;
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
                    if (increment)
                    {
                        if (_currentPosition <= Positions.Count)
                        {
                            CurrentPosition = _currentPosition + 1;
                        }
                    }
                    else
                    {
                        if (_currentPosition > 1)
                        {
                            CurrentPosition = _currentPosition - 1;
                        }
                    }
                    _mouseDownLocation = location;
                }
            }
        }

        public override void MouseUp(Point location)
        {
            if (NonClickableZones != null)
            {
                foreach (NonClickableZone zone in NonClickableZones)
                {
                    if (zone.AllPositions && zone.isClickInZone(location))
                    {
                        zone.ChildVisual.MouseUp(new System.Windows.Point(location.X - (zone.ChildVisual.Left - this.Left), location.Y - (zone.ChildVisual.Top - this.Top)));
                        return; //we get out to let the ChildVisual using the click
                    }
                }
            }
            if (_mouseDown)
            {
                _mouseDown = false;
            }
        }

        #region Actions

        void SetPositionAction_Execute(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);
            if (int.TryParse(e.Value.StringValue, out int index))
            {
                if (index >= 0 && index < Positions.Count)
                {
                    CurrentPosition = index;
                }
            }
            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        #endregion

        public override void Reset()
        {
            BeginTriggerBypass(true);
            CurrentPosition = DefaultPosition;
            EndTriggerBypass(true);
        }

        public override void WriteXml(XmlWriter writer)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            base.WriteXml(writer);
            writer.WriteElementString("KnobImage", KnobImage);
            writer.WriteStartElement("Positions");
            foreach (RotarySwitchPosition position in Positions)
            {
                writer.WriteStartElement("Position");
                writer.WriteAttributeString("Name", position.Name);
                writer.WriteAttributeString("Rotation", position.Rotation.ToString(CultureInfo.InvariantCulture));
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
            writer.WriteElementString("DefaultPosition", DefaultPosition.ToString(CultureInfo.InvariantCulture));
            if (DrawLines)
            {
                writer.WriteStartElement("Lines");
                writer.WriteElementString("Thickness", LineThickness.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Length", LineLength.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Color", colorConverter.ConvertToInvariantString(LineColor));
                writer.WriteEndElement();
            }
            if (DrawLabels)
            {
                writer.WriteStartElement("Labels");
                writer.WriteElementString("Color", colorConverter.ConvertToInvariantString(LabelColor));
                writer.WriteElementString("MaxWidth", MaxLabelWidth.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("MaxHeight", MaxLabelHeight.ToString(CultureInfo.InvariantCulture));
                writer.WriteElementString("Distance", LabelDistance.ToString(CultureInfo.InvariantCulture));
                LabelFormat.WriteXml(writer);
                writer.WriteEndElement();
            }
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
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            base.ReadXml(reader);
            KnobImage = reader.ReadElementString("KnobImage");
            if (!reader.IsEmptyElement)
            {
                Positions.Clear();
                reader.ReadStartElement("Positions");
                int i = 1;
                while (reader.NodeType != XmlNodeType.EndElement)
                {
                    Positions.Add(new RotarySwitchPosition(this, i++, reader.GetAttribute("Name"), Double.Parse(reader.GetAttribute("Rotation"), CultureInfo.InvariantCulture)));
                    reader.Read();
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }
            DefaultPosition = int.Parse(reader.ReadElementString("DefaultPosition"), CultureInfo.InvariantCulture);

            if (reader.Name.Equals("Lines"))
            {
                DrawLines = true;
                reader.ReadStartElement("Lines");
                LineThickness = double.Parse(reader.ReadElementString("Thickness"), CultureInfo.InvariantCulture);
                LineLength = double.Parse(reader.ReadElementString("Length"), CultureInfo.InvariantCulture);
                LineColor = (Color)colorConverter.ConvertFromInvariantString(reader.ReadElementString("Color"));
                reader.ReadEndElement();
            }
            else
            {
                DrawLines = false;
            }

            if (reader.Name.Equals("Labels"))
            {
                DrawLabels = true;
                reader.ReadStartElement("Labels");
                LabelColor = (Color)colorConverter.ConvertFromInvariantString(reader.ReadElementString("Color"));
                MaxLabelWidth = double.Parse(reader.ReadElementString("MaxWidth"), CultureInfo.InvariantCulture);
                MaxLabelHeight = double.Parse(reader.ReadElementString("MaxHeight"), CultureInfo.InvariantCulture);
                LabelDistance = double.Parse(reader.ReadElementString("Distance"), CultureInfo.InvariantCulture);
                LabelFormat.ReadXml(reader);
                reader.ReadEndElement();
            }
            else
            {
                DrawLabels = false;
            }

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
            }
            catch 
            {
                MouseWheelAction = true;
            }

            BeginTriggerBypass(true);
            CurrentPosition = DefaultPosition;
            EndTriggerBypass(true);
        }
    }
}