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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Text;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;
    
    [HeliosControl("Helios.Base.TextDisplay", "Text Display", "Text Displays", typeof(TextDisplayRenderer))]
    public class TextDisplay : HeliosVisual
    {
        private bool _useParseDicationary = false;
        private string _textValue = "";
        private string _rawValue = "";
        private string _textValueTest = "O";
        private string _onImage = "{Helios}/Images/Indicators/anunciator.png";
        private bool _useBackground = true;    // displaying the background or not
        private Color _onTextColor = Color.FromArgb(0xff, 0x40, 0xb3, 0x29);
        private Color _backgroundColor = Color.FromArgb(0xff, 0, 0, 0);
        private TextFormat _textFormat = new TextFormat();
        private Dictionary<string, string> _parserDictionary = new Dictionary<string, string>(); // the list of input -> output string modifications
        private HeliosValue _value;

        public TextDisplay()
            : base("TextDisplay", new System.Windows.Size(100, 50))
        {
            _textFormat.VerticalAlignment = TextVerticalAlignment.Center;
            _textFormat.HorizontalAlignment = TextHorizontalAlignment.Left;
            _textFormat.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(TextFormat_PropertyChanged);
            // _textFormat.FontFamily = FontManager.Instance.GetFontFamilyByName("SF Digital Readout");
            _value = new HeliosValue(this, new BindingValue(false), "", "TextDisplay", "Value of this Text Display", "String Value Unit.", BindingValueUnits.Text);
            _value.Execute += new HeliosActionHandler(On_Execute);
            Values.Add(_value);
            Actions.Add(_value);
        }

        #region Properties

        public string TextValue
        {
            get
            {
                return _textValue;
            }
            set
            {
               if (_useParseDicationary)
                {
                    if (!_rawValue.Equals(value))
                    {
                        _rawValue = value;
                        // parse the value
                        string parsedValue = value;
                        foreach (KeyValuePair<string, string> entry in _parserDictionary)
                        {
                            parsedValue = parsedValue.Replace(entry.Key, entry.Value);
                        }
                        string oldValue = _textValue;
                        _textValue = parsedValue;
                        _value.SetValue(new BindingValue(_textValue), BypassTriggers);
                        OnPropertyChanged("TextValue", oldValue, parsedValue, false);
                        OnDisplayUpdate();
                    }
                }
                else
                {
                    if (!value.Equals(_textValue))
                    {
                        string oldValue = _textValue;
                        _textValue = value;
                        _value.SetValue(new BindingValue(_textValue), BypassTriggers);
                        OnPropertyChanged("TextValue", oldValue, value, false);
                        OnDisplayUpdate();
                    }
                }
            }
        }

        public bool UseParseDictionary
        {
            get
            {
                return _useParseDicationary;
            }
            set
            {
                if (!_useParseDicationary.Equals(value))
                {
                    bool oldValue = _useParseDicationary;
                    _useParseDicationary = value;
                    OnPropertyChanged("UseParseDictionary", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public bool UseBackground {
            get
            {
                return _useBackground;
            }
            set
            {
                if (!_useBackground.Equals(value))
                {
                    bool oldValue = _useBackground;
                    _useBackground = value;
                    OnPropertyChanged("UseBackground", oldValue, value, true);
                    OnDisplayUpdate();
                }
            }
        }

        public string ParserDictionary
        {
            get
            { /// convert the dictionary to a string
                var stringBuilder = new StringBuilder();
                bool first = true;
                foreach (KeyValuePair<string, string> pair in _parserDictionary)
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        stringBuilder.Append(";");
                    }

                    stringBuilder.AppendFormat("{0}={1}", pair.Key, pair.Value);
                }
                return stringBuilder.ToString();
            }
            set /// convert the string to a dictionary
            {
                if (!value.Equals("")) {
                    Dictionary<string, string> oldValue = _parserDictionary;
                    _parserDictionary = value.TrimEnd(';').Split(';').ToDictionary(item => item.Split('=')[0], item => item.Split('=')[1]);
                    OnPropertyChanged("ParserDictionary", oldValue, value, false);
                    OnDisplayUpdate();
                }
            }
        }

        public string TextTestValue
        {
            get
            {
                return _textValueTest;
            }
            set
            {
                string oldValue = _textValueTest;
                _textValueTest = value;
                OnPropertyChanged("TextTestValue", oldValue, value, false);
                OnDisplayUpdate();
            }
        }

        public string OnImage
        {
            get
            {
                return _onImage;
            }
            set
            {
                if ((_onImage == null && value != null)
                    || (_onImage != null && !_onImage.Equals(value)))
                {
                    string oldValue = _onImage;
                    _onImage = value;
                    OnPropertyChanged("OnImage", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public Color BackgroundColor
        {
            get
            {
                return _backgroundColor;
            }
            set
            {
                if (!_backgroundColor.Equals(value))
                {
                    Color oldValue = _backgroundColor;
                    _backgroundColor = value;
                    OnPropertyChanged("BackgroundColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public Color OnTextColor
        {
            get
            {
                return _onTextColor;
            }
            set
            {
                if (!_onTextColor.Equals(value))
                {
                    Color oldValue = _onTextColor;
                    _onTextColor = value;
                    OnPropertyChanged("OnTextColor", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public TextFormat TextFormat
        {
            get
            {
                return _textFormat;
            }
            set
            {
                TextFormat oldValue = _textFormat;
                _textFormat = value;
                OnPropertyChanged("TextFormat", oldValue, value, true);
                Refresh();
            }
        }

        public double FontSize
        {
            get
            {
                return _textFormat.FontSize;
            }
            set {
                double oldValue = _textFormat.FontSize;
                _textFormat.FontSize = value;
                OnPropertyChanged("FontSize", oldValue, value, true);
                Refresh();
            }
        }

        #endregion

        void TextFormat_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            OnPropertyChanged("TextFormat", (PropertyNotificationEventArgs)e);
            OnDisplayUpdate();
        }

        void ToggleAction_Execute(object action, HeliosActionEventArgs e)
        {
            //BeginTriggerBypass(e.BypassCascadingTriggers);
            //On = !On;
            //EndTriggerBypass(e.BypassCascadingTriggers);
        }

        void On_Execute(object action, HeliosActionEventArgs e)
        {
            BeginTriggerBypass(e.BypassCascadingTriggers);
            TextValue = e.Value.StringValue;
            EndTriggerBypass(e.BypassCascadingTriggers);
        }

        protected override void PostUpdateRectangle(Rect previous, Rect current)
        {
            if (previous.Height == 0)
                return;
            double scale = current.Height / previous.Height;
            TextFormat.FontSize = Clamp(scale*TextFormat.FontSize, 1, 100);
        }

        public override void ScaleChildren(double scaleX, double scaleY)
        {
            double scale = scaleX > scaleY ? scaleX : scaleY;
        }

        public override void Reset()
        {
            BeginTriggerBypass(true);
            TextValue = "";
            EndTriggerBypass(true);
        }

        public override void MouseDown(Point location)
        {
            if (DesignMode)
            {
                TextValue = _textValueTest;
            }
        }

        public override void MouseDrag(Point location)
        {
            // No-Op
        }

        public override void MouseUp(Point location)
        {
            // No-Op
        }

        public override void WriteXml(XmlWriter writer)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            // writer.WriteElementString("OnImage", OnImage);
            writer.WriteStartElement("Font");
            _textFormat.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteElementString("OnTextColor", colorConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, OnTextColor));
            writer.WriteElementString("BackgroundColor", colorConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, BackgroundColor));
            writer.WriteElementString("TextTest", _textValueTest);
            writer.WriteElementString("ParserDictionary", ParserDictionary);
            writer.WriteElementString("UseBackground", boolConverter.ConvertToInvariantString(UseBackground));
            writer.WriteElementString("UseParserDictionary", boolConverter.ConvertToInvariantString(UseParseDictionary));
            base.WriteXml(writer);
        }

        public override void ReadXml(XmlReader reader)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            // OnImage = reader.ReadElementString("OnImage");
            reader.ReadStartElement("Font");
            _textFormat.ReadXml(reader);

            // save this size, because the automatic scaling will keep increasing it when we read the size of our rectangle
            // and we get called back on PostUpdateRectangle
            double fontSizeFromProfile = _textFormat.FontSize;

            reader.ReadEndElement();
            OnTextColor = (Color)colorConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, reader.ReadElementString("OnTextColor"));
            BackgroundColor = (Color)colorConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, reader.ReadElementString("BackgroundColor"));
            TextTestValue = reader.ReadElementString("TextTest");
            ParserDictionary = reader.ReadElementString("ParserDictionary");
            UseBackground = (bool)boolConverter.ConvertFromInvariantString(reader.ReadElementString("UseBackground"));
            UseParseDictionary = (bool)boolConverter.ConvertFromInvariantString(reader.ReadElementString("UseParserDictionary"));
            base.ReadXml(reader);

            // now the auto scaling has messed up our font size, so we restore it
            _textFormat.FontSize = fontSizeFromProfile;
        }

        private double Clamp(double value, double min, double max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }
    }
}
