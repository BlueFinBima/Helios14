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


    [HeliosControl("Helios.Base.KneeBoard", "KneeBoard", "Panel Decorations", typeof(KneeBoardRenderer))]
    public class KneeBoard : HeliosVisual
    {
        private string _imageFile = "";
        private ImageAlignmentPlus _alignment = ImageAlignmentPlus.Uniform;
        private Color _borderColor = Colors.Black;
        private KneeBoardPositionCollection _positions = new KneeBoardPositionCollection();

        public int _currentPosition;
        private int _defaultPosition;
        private HeliosValue _positionValue;
  


        public KneeBoard()
            : base("Kneeboard_Carousel", new Size(100, 100))
        {
            IsSnapTarget = false;

            _positionValue = new HeliosValue(this, new BindingValue(0), "", "position", "Current position of the KneeBoard.", "", BindingValueUnits.Numeric);
             _positionValue.Execute += new HeliosActionHandler(SetPositionAction_Execute);
            Values.Add(_positionValue);
            Actions.Add(_positionValue);
            Triggers.Add(_positionValue);

          
            _positions.CollectionChanged += new NotifyCollectionChangedEventHandler(Positions_CollectionChanged);
            _positions.PositionChanged += new EventHandler<KneeBoardPositionChangeArgs>(PositionChanged);
            _positions.Add(new KneeBoardPosition(this, 1, "{Helios}/Images/KneeBoards/default_kneeboard_image.png"));
            _positions.Add(new KneeBoardPosition(this, 2, "{Helios}/Images/KneeBoards/default_kneeboard_image.png"));
            _currentPosition = 0;
            _defaultPosition = 0;


        }

        #region Properties

        public string Image
        {
            get
            {
                return _imageFile;
            }
            set
            {
                if ((_imageFile == null && value != null)
                    || (_imageFile != null && !_imageFile.Equals(value)))
                {
                    string oldValue = _imageFile;
                    _imageFile = _positions[0].Name; // value

                    ImageSource image = ConfigManager.ImageManager.LoadImage(_positions[0].Name); //_imageFile

                    if (image != null)
                    {
                        Width = image.Width;
                        Height = image.Height;
                    }
                    OnPropertyChanged("Name", oldValue, value, true);
                    Refresh();
                }
            }
        }

        public ImageAlignmentPlus Alignment
        {
            get
            {
                return _alignment;
            }
            set
            {
                if (!_alignment.Equals(value))
                {
                    ImageAlignmentPlus oldValue = _alignment;
                    _alignment = value;
                    OnPropertyChanged("Alignment", oldValue, value, true);
                    Refresh();
                }
            }
        }

   

        public KneeBoardPositionCollection Positions
        {
            get { return _positions; }
        }

      

        public int CurrentPosition
        {
            get
            {
                return _currentPosition;
            }
            set
            {
                if (!_currentPosition.Equals(value) && value >= 0 && value < Positions.Count)
                {
                    int oldValue = _currentPosition;
                    

                    _currentPosition = value;
                   

                    _positionValue.SetValue(new BindingValue((double)_currentPosition), BypassTriggers);



                    OnPropertyChanged("CurrentPosition", oldValue, value, false);
                   
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
                if (!_defaultPosition.Equals(value) && value > -1 && value <= Positions.Count)
                {
                    int oldValue = _defaultPosition;
                    _defaultPosition = value;
                    OnPropertyChanged("DefaultPosition", oldValue, value, true);
                    ///Refresh();
                }
            }
        }
       
        void PositionChanged(object sender, KneeBoardPositionChangeArgs e)
        {
            PropertyNotificationEventArgs args = new PropertyNotificationEventArgs(e.Position, e.PropertyName, e.OldValue, e.NewValue, true);

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
                KneeBoardPosition position = Positions[i];
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

        void Positions_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
              
                if (Positions.Count == 0)
                {
                    _currentPosition = 0;
                }
                else if (_currentPosition > Positions.Count)
                {
                    _currentPosition = Positions.Count;
                }
            }
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

        public override void ReadXml(XmlReader reader)
        {
          

            if (!reader.IsEmptyElement)
            {
                Positions.Clear();
                reader.ReadStartElement("Positions");
                int i = 1;
                while (reader.NodeType != XmlNodeType.EndElement)
                {
					Positions.Add(new KneeBoardPosition(this, i++, reader.GetAttribute("ImageFilename")));
                    reader.Read();
                }
                reader.ReadEndElement();
            }
            else
            {
                reader.Read();
            }
           
         
            if (reader.Name.Equals("Alignment"))
            {
                Alignment = (ImageAlignmentPlus)Enum.Parse(typeof(ImageAlignmentPlus), reader.ReadElementString("Alignment"));
            }
        
            // Load base after image so size is properly persisted.
            base.ReadXml(reader);

			BeginTriggerBypass(true);
			CurrentPosition = DefaultPosition; // 
			EndTriggerBypass(true);
		}

        public override void WriteXml(XmlWriter writer)
        {
          

            writer.WriteStartElement("Positions");
            foreach (KneeBoardPosition position in Positions)
            {
                writer.WriteStartElement("Position");
                writer.WriteAttributeString("ImageFilename", position.Name);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteElementString("Alignment", Alignment.ToString());
          
            // Save base after image so size is properly persisted.
            base.WriteXml(writer);
        }
    }
}
