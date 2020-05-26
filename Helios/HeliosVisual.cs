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

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Media;
    using System.Xml;

    using GadrocsWorkshop.Helios.ComponentModel;


    /// <summary>
    /// HeliosVisual objects are helios objects which will be rendered on screen.
    /// </summary>
    public abstract class HeliosVisual : HeliosObject
    {
        private HeliosVisualRenderer _renderer;

        private WeakReference _parent = new WeakReference(null);
        private string _typeIdentifier;

        private HeliosVisualCollection _children;
        private bool _persistChildren = true;

        private Rect _rectangle;
        private Rect _adjustedRectangle;
        private Size _nativeSize;
        private HeliosVisualRotation _rotation = HeliosVisualRotation.None;

        private bool _locked = false;
        private bool _snapable = true;
        private bool _hidden = false;
        private bool _defaultHidden = false;

        private HeliosValue _hiddenValue;
        private NonClickableZone[] _nonClickableZones;


        /// <summary>
        /// Base constructor for a HeliosVisual.
        /// </summary>
        /// <param name="name">Default name for this object.</param>
        /// <param name="nativeSize">Native size that this control renderes at.</param>
        protected HeliosVisual(string name, Size nativeSize)
            : base(name)
        {
            _rectangle = new Rect(nativeSize);
            _nativeSize = nativeSize;
            UpdateRectangle();

            _children = new HeliosVisualCollection();

            HeliosAction toggleVisibleAction = new HeliosAction(this, "", "hidden", "toggle", "Toggles whether this control is displayed on screen.");
            toggleVisibleAction.Execute += new HeliosActionHandler(ToggleVisibleAction_Execute);
            Actions.Add(toggleVisibleAction);

            _hiddenValue = new HeliosValue(this, new BindingValue(false), "", "hidden", "Indicates if this control is hidden and off screen.", "True if this panel is not displayed.", BindingValueUnits.Boolean);
            _hiddenValue.Execute += new HeliosActionHandler(SetHiddenAction_Execute);
            Values.Add(_hiddenValue);
            Actions.Add(_hiddenValue);

            Children.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Children_CollectionChanged);

        }

        /// <summary>
        /// Event triggered when this panel should be re-displayed.
        /// </summary>
        public event EventHandler DisplayUpdate;

        public event EventHandler Resized;

        public event EventHandler Moved;

        public event EventHandler HiddenChanged;

        #region Properties

        public NonClickableZone[] NonClickableZones
        {
            get
            {
                return _nonClickableZones;
            }
            set
            {
                _nonClickableZones = value;
            }
        }

        public bool PersistChildren
        {
            get { return _persistChildren; }
            set { _persistChildren = value; }
        }

        public override string TypeIdentifier
        {
            get
            {
                if (_typeIdentifier == null)
                {
                    HeliosDescriptor descriptor = ConfigManager.ModuleManager.ControlDescriptors[this.GetType()];
                    _typeIdentifier = descriptor.TypeIdentifier;
                }
                return _typeIdentifier;
            }
        }

        public override bool DesignMode
        {
            get
            {
                return base.DesignMode || (Profile != null && Profile.DesignMode) || (Parent != null && Parent.DesignMode);
            }
            set
            {
                base.DesignMode = value;
            }
        }

        public Monitor Monitor
        {
            get
            {
                HeliosVisual visual = this;
                while (visual.Parent != null)
                {
                    visual = visual.Parent;
                }
                return visual as Monitor;
            }
        }

        /// <summary>
        /// Gets the this visuals parent visual
        /// </summary>
        public HeliosVisual Parent
        {
            get { return _parent.Target as HeliosVisual; }
            set { _parent = new WeakReference(value); }
        }

        /// <summary>
        /// Returns the collection of child visuals for this visual.
        /// </summary>
        public HeliosVisualCollection Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Returns true if this visual is hidden and not displayed on the screen.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                return _hidden;
            }
            set
            {
                if (!_hidden.Equals(value))
                {
                    _hidden = value;
                    _hiddenValue.SetValue(new BindingValue(_hidden), false);
                    OnPropertyChanged("IsHidden", !value, value, false);
                    OnHiddenChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this control is hidden load/reset of a profile.
        /// </summary>
        public bool IsDefaultHidden
        {
            get
            {
                return _defaultHidden;
            }
            set
            {
                if (!_defaultHidden.Equals(value))
                {
                    bool oldValue = _defaultHidden;
                    _defaultHidden = value;
                    OnPropertyChanged("IsDefaultHidden", oldValue, value, true);
                }
            }
        }

        public bool IsVisible
        {
            get
            {
                if (IsHidden)
                {
                    return false;
                }

                if (Parent != null)
                {
                    return Parent.IsVisible;
                }

                return true;
            }
        }

        /// <summary>
        /// Gets and sets the lock status for this visual.  When locked it cannot be selected, moved or edited.
        /// </summary>
        public bool IsLocked
        {
            get
            {
                return _locked;
            }
            set
            {
                if (!_locked.Equals(value))
                {
                    bool oldValue = _locked;
                    _locked = value;
                    OnPropertyChanged("IsLocked", oldValue, value, false);
                }
            }
        }

        /// <summary>
        /// Gets and sets the snap status for this visual.  When true other visuals will snap to this visual when they are moved or resized.
        /// </summary>
        public bool IsSnapTarget
        {
            get
            {
                return _snapable;
            }
            set
            {
                if (!_snapable.Equals(value))
                {
                    bool oldValue = _snapable;
                    _snapable = value;
                    OnPropertyChanged("IsSnapTarget", oldValue, value, true);
                }
            }
        }

        /// <summary>
        /// Returns the renderer for this visual
        /// </summary>
        public virtual HeliosVisualRenderer Renderer
        {
            get
            {
                if (_renderer == null)
                {
                    _renderer = ConfigManager.ModuleManager.CreaterRenderer(this);
                }
                return _renderer;
            }
        }

        /// <summary>
        /// Top edge of where this visual will be displayed.
        /// </summary>
        public double Top
        {
            get
            {
                return _rectangle.Top;
            }
            set
            {
                double newValue = Math.Truncate(value);
                if (!_rectangle.Top.Equals(newValue))
                {
                    double oldValue = _rectangle.Top;
                    _rectangle.Y = newValue;
                    OnPropertyChanged("Top", oldValue, newValue, true);
                    UpdateRectangle();
                    OnMoved();
                }
            }
        }

        /// <summary>
        /// Left edge of where this visual will be displayed
        /// </summary>
        public double Left
        {
            get
            {
                return _rectangle.Left;
            }
            set
            {
                double newValue = Math.Truncate(value);
                if (!_rectangle.Left.Equals(newValue))
                {
                    double oldValue = _rectangle.Left;
                    _rectangle.X = newValue;
                    OnPropertyChanged("Left", oldValue, newValue, true);
                    UpdateRectangle();
                    OnMoved();
                }
            }
        }

        /// <summary>
        /// Width of this visual
        /// </summary>
        public double Width
        {
            get
            {
                return _rectangle.Width;
            }
            set
            {
                double newValue = Math.Truncate(value);
                if (!_rectangle.Width.Equals(newValue))
                {
                    double oldValue = _rectangle.Width;
                    _rectangle.Width = newValue;
                    OnPropertyChanged("Width", oldValue, newValue, true);
                    UpdateRectangle();
                    Refresh();
                    OnResized();
                }
            }
        }

        /// <summary>
        /// Height of this visual
        /// </summary>
        public double Height
        {
            get
            {
                return _rectangle.Height;
            }
            set
            {
                double newValue = Math.Truncate(value);
                if (!_rectangle.Height.Equals(newValue))
                {
                    double oldValue = _rectangle.Height;
                    _rectangle.Height = newValue;
                    OnPropertyChanged("Height", oldValue, newValue, true);
                    UpdateRectangle();
                    Refresh();
                    OnResized();
                }
            }
        }

        /// <summary>
        /// Native size for this visual.
        /// </summary>
        public Size NativeSize
        {
            get
            {
                return _nativeSize;
            }
        }

        /// <summary>
        /// Rectangle of this visual as it's displayed
        /// </summary>
        public Rect DisplayRectangle
        {
            get
            {
                return _adjustedRectangle;
            }
            private set
            {
                if (!_adjustedRectangle.Equals(value))
                {
                    Rect oldValue = _adjustedRectangle;
                    _adjustedRectangle = value;
                    OnPropertyChanged("DisplayRectangle", oldValue, value, false);
                }
            }
        }

        /// <summary>
        /// Gets and sets the rotation of this visual
        /// </summary>
        public HeliosVisualRotation Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                if (!_rotation.Equals(value))
                {
                    HeliosVisualRotation oldValue = _rotation;
                    _rotation = value;
                    OnPropertyChanged("Rotation", oldValue, value, true);
                    UpdateRectangle();
                    Refresh();
                    OnResized();
                }
            }
        }

        /// <summary>
        /// returns true if this visual view can process more mouse wheel input right now
        /// otherwise the mouse wheel input will be delivered to a containing view
        /// </summary>
        public virtual bool CanConsumeMouseWheel => false;

        #endregion

        #region Actions

        /// <summary>
        /// Set Hidden action on control
        /// </summary>
        /// <param name="action"></param>
        /// <param name="e"></param>
        void SetHiddenAction_Execute(object action, HeliosActionEventArgs e)
        {
            IsHidden = e.Value.BoolValue;
        }

        /// <summary>
        /// Toggles this visual from being displayed and hidden.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="e"></param>
        void ToggleVisibleAction_Execute(object action, HeliosActionEventArgs e)
        {
            IsHidden = !IsHidden;
        }

        #endregion

        public override void Reset()
        {
            base.Reset();
            IsHidden = IsDefaultHidden;
        }

        /// <summary>
        /// Updates the display rectangle based on current rotation and co-ordinates
        /// </summary>
        private void UpdateRectangle()
        {
            Matrix m1 = new Matrix();
            Rect newRect = _rectangle;

            switch (Rotation)
            {
                case HeliosVisualRotation.CW:
                    m1.RotateAt(90, _rectangle.X, _rectangle.Y);
                    m1.Translate(_rectangle.Height, 0);
                    newRect.Transform(m1);
                    break;

                case HeliosVisualRotation.CCW:
                    m1.RotateAt(-90, _rectangle.X, _rectangle.Y);
                    m1.Translate(0, _rectangle.Width);
                    newRect.Transform(m1);
                    break;

				case HeliosVisualRotation.ROT180:
					m1.RotateAt(180, _rectangle.X + (_rectangle.Width/2), _rectangle.Y + (_rectangle.Height/2));
					m1.Translate(0, 0);
					newRect.Transform(m1);
					break;
			}
            PostUpdateRectangle(DisplayRectangle, newRect);
            DisplayRectangle = newRect;
        }

        /// 
        /// Method that can be implemented by sub classes
        /// 
        protected virtual void PostUpdateRectangle(Rect previous, Rect current) {

        }


        /// <summary>
        /// Method call used to linear scale this control and it's components.
        /// </summary>
        /// <param name="scaleX"></param>
        /// <param name="scaleY"></param>
        public virtual void ScaleChildren(double scaleX, double scaleY)
        {
            // No-Op by default
        }

        /// <summary>
        /// Forces renderer to reload any necessary resoruces.
        /// </summary>
        protected void Refresh()
        {
            if (Renderer != null)
            {
                Renderer.Refresh();
            }
            OnDisplayUpdate();
        }

        /// <summary>
        /// Fires the display update event.
        /// </summary>
        protected virtual void OnDisplayUpdate()
        {
            EventHandler handler = DisplayUpdate;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnMoved()
        {
            EventHandler handler = Moved;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnResized()
        {
            EventHandler handler = Resized;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        private void OnHiddenChanged()
        {
            EventHandler handler = HiddenChanged;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Method to override default configurations when this is used as an icon.
        /// </summary>
        public virtual void ConfigureIconInstance()
        {
        }

        /// <summary>
        /// Called to determine if a point inside this location should be handled.  True
        /// should be returned if this control is opaque at that location to prevent
        /// tunneling to visuals below it.
        /// </summary>
        /// <param name="location">Point inside visual boundaries.</param>
        /// <returns>True if this visual should handle the interaction.</returns>
        public virtual bool HitTest(Point location)
        {
            return true;
        }

        /// <summary>
        /// Called when a mouse wheel is rotated on this control.
        /// </summary>
        /// <param name="location">Current location of the mouse relative to this controls upper left corner.</param>
        public virtual void MouseWheel(int delta)
        {
        }

        /// <summary>
        /// Called when a mouse button is pressed on this control.
        /// </summary>
        /// <param name="location">Current location of the mouse relative to this controls upper left corner.</param>
        public abstract void MouseDown(Point location);

        /// <summary>
        /// Called when the mouse is dragged after being pressed on this control.
        /// </summary>
        /// <param name="location">Current location of the mouse relative to this controls upper left corner.</param>
        public abstract void MouseDrag(Point location);

        /// <summary>
        /// Called when the mouse button is released after being pressed on this control.
        /// </summary>
        /// <param name="location">Current location of the mouse relative to this controls upper left corner.</param>
        public abstract void MouseUp(Point location);

        void Children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if ((e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add) ||
                (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (HeliosVisual control in e.NewItems)
                {
                    control.Parent = this;
                    control.Profile = Profile;
                    control.PropertyChanged += new PropertyChangedEventHandler(Child_PropertyChanged);
                    control.ReconnectBindings();
                }
            }

            if ((e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove) ||
                (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace))
            {
                foreach (HeliosVisual control in e.OldItems)
                {
                    control.Parent = null;
                    control.Profile = Profile;
                    control.PropertyChanged -= new PropertyChangedEventHandler(Child_PropertyChanged);
                    control.DisconnectBindings();
                }
            }
        }

        public override void DisconnectBindings()
        {
            base.DisconnectBindings();
            foreach (HeliosVisual child in Children)
            {
                child.DisconnectBindings();
            }
        }

        public override void ReconnectBindings()
        {
            base.ReconnectBindings();
            foreach (HeliosVisual child in Children)
            {
                child.ReconnectBindings();
            }
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);
            foreach (HeliosVisual child in Children)
            {
                child.Profile = Profile;
            }
        }

        void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HeliosVisual control = sender as HeliosVisual;
            OnPropertyChanged("Controls." + control.Name, (PropertyNotificationEventArgs)e);
        }

        public override void WriteXml(XmlWriter writer)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            TypeConverter sizeConverter = TypeDescriptor.GetConverter(typeof(Size));
            TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));

            writer.WriteElementString("Location", pointConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, new Point(Left, Top)));
            writer.WriteElementString("Size", sizeConverter.ConvertToString(null, System.Globalization.CultureInfo.InvariantCulture, new Size(Width, Height)));
            if (Rotation != HeliosVisualRotation.None)
            {
                writer.WriteElementString("Rotation", Rotation.ToString());
            }

            writer.WriteElementString("Hidden", boolConverter.ConvertToInvariantString(IsDefaultHidden));
        }

        public override void ReadXml(XmlReader reader)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));
            TypeConverter sizeConverter = TypeDescriptor.GetConverter(typeof(Size));
            TypeConverter pointConverter = TypeDescriptor.GetConverter(typeof(Point));

            if (reader.Name.Equals("Location"))
            {
                Point location = (Point)pointConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, reader.ReadElementString("Location"));
                Left = location.X;
                Top = location.Y;

                Size size = (Size)sizeConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, reader.ReadElementString("Size"));
                Width = size.Width;
                Height = size.Height;
            }
            if (reader.Name.Equals("Rotation"))
            {
                Rotation = (HeliosVisualRotation)Enum.Parse(typeof(HeliosVisualRotation), reader.ReadElementString("Rotation"));
            }
            if (reader.Name.Equals("Hidden"))
            {
                IsDefaultHidden = (bool)boolConverter.ConvertFromInvariantString(reader.ReadElementString("Hidden"));
                IsHidden = IsDefaultHidden;
            }
        }

    }
}
