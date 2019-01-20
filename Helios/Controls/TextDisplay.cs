﻿//  Copyright 2014 Craig Courtney
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
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;
    
    [HeliosControl("Helios.Base.TextDisplay", "Text Display", "Text Displays", typeof(TextDisplayRenderer))]
    public class TextDisplay : HeliosVisual
    {
        private string _textValue = "No Text";

        private string _onImage = "{Helios}/Images/Indicators/caution-indicator-on.png";
        
        private Color _onTextColor = Color.FromRgb(179, 162, 41);
        private TextFormat _textFormat = new TextFormat();

        private HeliosValue _value;

        public TextDisplay()
            : base("TextDisplay", new System.Windows.Size(100, 50))
        {
            _textFormat.VerticalAlignment = TextVerticalAlignment.Center;
            _textFormat.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(TextFormat_PropertyChanged);

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
                if (!_textValue.Equals(value))
                {
                    string oldValue = _textValue;
                    _textValue = value;
                    _value.SetValue(new BindingValue(_textValue), BypassTriggers);
                    OnPropertyChanged("TextValue", oldValue, value, false);
                    OnDisplayUpdate();
                }
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

        public override void ScaleChildren(double scaleX, double scaleY)
        {
            double scale = scaleX > scaleY ? scaleX : scaleY;
            TextFormat.FontSize *= scale;
        }

        public override void Reset()
        {
            BeginTriggerBypass(true);
            TextValue = "Reset";
            EndTriggerBypass(true);
        }

        public override void MouseDown(Point location)
        {
            if (DesignMode)
            {
                TextValue = "100";
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

            writer.WriteElementString("OnImage", OnImage);
            writer.WriteStartElement("Font");
            _textFormat.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteElementString("OnTextColor", colorConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, OnTextColor));
            base.WriteXml(writer);
        }

        public override void ReadXml(XmlReader reader)
        {
            TypeConverter colorConverter = TypeDescriptor.GetConverter(typeof(Color));

            OnImage = reader.ReadElementString("OnImage");
            reader.ReadStartElement("Font");
            _textFormat.ReadXml(reader);
            reader.ReadEndElement();
            OnTextColor = (Color)colorConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, reader.ReadElementString("OnTextColor"));
            base.ReadXml(reader);
        }
    }
}