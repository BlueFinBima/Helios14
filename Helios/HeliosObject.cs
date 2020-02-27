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
    using System.Windows.Threading;
    using System.Xml;

    public abstract class HeliosObject : NotificationObject
    {
        private string _name;
        private bool _designMode;
        private WeakReference _profile = new WeakReference(null);
        private Dispatcher _dispatcher;
        private int _bypassCount = 0;

        private HeliosActionCollection _actions = new HeliosActionCollection();
        private HeliosTriggerCollection _triggers = new HeliosTriggerCollection();
        private HeliosValueCollection _values = new HeliosValueCollection();

        private HeliosBindingCollection _inputs = new HeliosBindingCollection();
        private HeliosBindingCollection _outputs = new HeliosBindingCollection();
#if DEVELOPMENT_CONFIGURATION
        internal bool _tracing = false;
#endif

        protected HeliosObject(string name)
        {
            _name = name;
            _outputs.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Outputs_CollectionChanged);
        }

        #region Properties

        public Dispatcher Dispatcher
        {
            get { return _dispatcher; }
            set { _dispatcher = value; }
        }

        /// <summary>
        /// Returns the profile that this visual is a part of
        /// </summary>
        public HeliosProfile Profile
        {
            get { return _profile.Target as HeliosProfile; }
            set 
            {
                HeliosProfile oldProfile = _profile.Target as HeliosProfile;
                _profile = new WeakReference(value);
                OnProfileChanged(oldProfile);
            }
        }

        public abstract string TypeIdentifier
        {
            get;
        }

        /// <summary>
        /// Gets the flag to bypass trigger events.  When this
        /// is set to true no triggers should be fired.
        /// </summary>
        public bool BypassTriggers
        {
            get
            {
                return (_bypassCount > 0 || DesignMode);
            }
        }

        /// <summary>
        /// Returns the internal collection of Action descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosActionCollection Actions
        {
            get
            {
                return _actions;
            }
        }

        /// <summary>
        /// Returns the internal collection of Trigger descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosTriggerCollection Triggers
        {
            get
            {
                return _triggers;
            }
        }

        /// <summary>
        /// Returns the internal collection of Value descriptors used
        /// to respond to the ActionDescriptors.  Sub-classes should
        /// use this to populate their actions.
        /// </summary>
        public HeliosValueCollection Values
        {
            get
            {
                return _values;
            }
        }

        /// <summary>
        /// Collection of bindings which execute actions of this object.
        /// </summary>
        public HeliosBindingCollection InputBindings
        {
            get
            {
                return _inputs;
            }
        }

        /// <summary>
        /// Colleciton of bindings which this object triggers.
        /// </summary>
        public HeliosBindingCollection OutputBindings
        {
            get
            {
                return _outputs;
            }
        }

        /// <summary>
        /// Name of this profile object.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if ((_name == null && value != null)
                    || (_name != null && !_name.Equals(value)))
                {
                    string oldName = _name;
                    _name = value;
                    OnPropertyChanged("Name", oldName, value, true);
                }
            }
        }

        /// <summary>
        /// Gets or sets whether this control is currently being displayed in the editor.
        /// </summary>
        public virtual bool DesignMode
        {
            get
            {
                return _designMode || (Profile != null && Profile.DesignMode);
            }
            set
            {
                if (_designMode != value)
                {
                    _designMode = value;
                    OnPropertyChanged("DesignMode", !value, value, false);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Disconnects an object bindings.  This should be called on any object
        /// being removed from the profile.
        /// </summary>
        public virtual void DisconnectBindings()
        {
            foreach (HeliosBinding binding in OutputBindings)
            {
                binding.Action.Target.InputBindings.Remove(binding);
            }

            foreach (HeliosBinding binding in InputBindings)
            {
                binding.Trigger.Source.OutputBindings.Remove(binding);
            }
        }

        /// <summary>
        /// Reconnects any orphaned bindings of an object.  This should be called on any object
        /// added to the profile.
        /// </summary>
        public virtual void ReconnectBindings()
        {
            if (Profile != null)
            {
                foreach (HeliosBinding binding in OutputBindings)
                {
                    if (binding.Action.Target.Profile == Profile && !binding.Action.Target.InputBindings.Contains(binding))
                    {
                        binding.Action.Target.InputBindings.Add(binding);
                    }
                }

                foreach (HeliosBinding binding in InputBindings)
                {
                    if (binding.Trigger.Source.Profile == Profile && !binding.Trigger.Source.OutputBindings.Contains(binding))
                    {
                        binding.Trigger.Source.OutputBindings.Add(binding);
                    }
                }
            }
        }

        /// <summary>
        /// Notification method for profile changes.
        /// </summary>
        protected virtual void OnProfileChanged(HeliosProfile oldProfile)
        {

        }

        /// <summary>
        /// Method to indicate we are begining a unit of work which should not fire any triggers.
        /// </summary>
        public void BeginTriggerBypass(bool bypassTriggers)
        {
            if (bypassTriggers)
            {
                _bypassCount++;
            }
        }

        /// <summary>
        /// Method to indicate we are finished with a unit of work which should not fire any triggers.
        /// </summary>
        public void EndTriggerBypass(bool bypassTriggers)
        {
            if (bypassTriggers)
            {
                _bypassCount--;
            }
        }

        /// <summary>
        /// Resets this object to default state.
        /// </summary>
        public virtual void Reset()
        {
            // Default No-Op
        }

        public abstract void ReadXml(XmlReader reader);

        public abstract void WriteXml(XmlWriter writer);

        #endregion

        void Outputs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (HeliosBinding binding in e.NewItems)
                {
                    binding.Trigger.TriggerFired -= new HeliosTriggerHandler(binding.OnTriggerFired);
                    binding.Trigger.TriggerFired += new HeliosTriggerHandler(binding.OnTriggerFired);
                }
            }
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (HeliosBinding binding in e.OldItems)
                {
                    binding.Trigger.TriggerFired -= new HeliosTriggerHandler(binding.OnTriggerFired);
                }
            }
        }

    }
}
