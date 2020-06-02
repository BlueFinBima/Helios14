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
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Threading;

    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using GadrocsWorkshop.Helios.Interfaces.Profile;

    public class HeliosProfile : NotificationObject
    {
        private bool _invalidVersion = false;
        private bool _dirty = false;
        private bool _layoutChecked = false;
        private bool _validLayout = false;
        private bool _designMode = false;
        private bool _started = false;
        private string _name = "Untitled";
        private string _path = "";
        DateTime _loadTime;

        Dispatcher _dispatcher = null;

        private MonitorCollection _monitors = new MonitorCollection();
        private HeliosInterfaceCollection _interfaces = new HeliosInterfaceCollection();

         public HeliosProfile() : this(true)
        {
        }

        public HeliosProfile(bool autoAddInterfaces)
        {
            _monitors.CollectionChanged += new NotifyCollectionChangedEventHandler(Monitors_CollectionChanged);
            _interfaces.CollectionChanged += new NotifyCollectionChangedEventHandler(Interfaces_CollectionChanged);

            int i = 1;
            foreach (Monitor display in ConfigManager.DisplayManager.Displays)
            {
                Monitor monitor = new Monitor(display);
                monitor.Name = "Monitor " + i++;
                _monitors.Add(monitor);
            }

            if (autoAddInterfaces)
            {
                foreach (HeliosInterfaceDescriptor descriptor in ConfigManager.ModuleManager.InterfaceDescriptors)
                {
                    foreach(HeliosInterface newInterface in descriptor.GetAutoAddInstances(this))
                    {
                        Interfaces.Add(newInterface);
                    }
                }
            }

            LoadTime = DateTime.MinValue;
        }

        public event EventHandler ControlCenterShown;
        public event EventHandler ControlCenterHidden;
        public event EventHandler ProfileStarted;
        public event EventHandler ProfileStopped;
        public event EventHandler ProfileTick;

        #region Properties

        public Dispatcher Dispatcher
        {
            get
            {
                return _dispatcher;
            }
            set
            {
                if ((_dispatcher == null && value != null)
                    || (_dispatcher != null && !_dispatcher.Equals(value)))
                {
                    Dispatcher oldValue = _dispatcher;
                    _dispatcher = value;
                    OnPropertyChanged("Dispatcher", oldValue, value, false);
                }
            }
        }

        public bool DesignMode
        {
            get
            {
                return _designMode;
            }
            set
            {
                if (!_designMode.Equals(value))
                {
                    bool oldValue = _designMode;
                    _designMode = value;
                    OnPropertyChanged("DesignMode", oldValue, value, false);
                }
            }
        }


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
                    string oldValue = _name;
                    _name = value;
                    OnPropertyChanged("Name", oldValue, value, true);
                }
            }
        }

        public DateTime LoadTime
        {
            get
            {
                return _loadTime;
            }
            set
            {
                _loadTime = value;
            }
        }

        public bool IsInvalidVersion
        {
            get
            {
                return _invalidVersion;
            }
            set
            {
                if (!_invalidVersion.Equals(value))
                {
                    bool oldValue = _invalidVersion;
                    _invalidVersion = value;
                    OnPropertyChanged("IsInvalidVersion", oldValue, value, false);
                }
            }
        }

        public HeliosInterfaceCollection Interfaces
        {
            get
            {
                return _interfaces;
            }
        }

        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                if ((_path == null && value != null)
                    || (_path != null && !_path.Equals(value)))
                {
                    string oldPath = _path;
                    _path = value;
                    OnPropertyChanged("Path", oldPath, _path, false);
                }
            }
        }

        public bool IsDirty
        {
            get
            {
                return _dirty;
            }
            set
            {
                if (!_dirty.Equals(value))
                {
                    _dirty = value;
                    OnPropertyChanged("IsDirty", !_dirty, _dirty, false);
                }
            }
        }

        public MonitorCollection Monitors
        {
            get
            {
                return _monitors;
            }
        }

        /// <summary>
        /// Returns true if all monitors defined in this profile match system defined monitors.
        /// </summary>
        public bool IsValidMonitorLayout
        {
            get
            {
                if (!_layoutChecked)
                {
                    _validLayout = true;
                    foreach (Monitor display in Monitors)
                    {
                        if (!IsMonitorValid(display))
                        {
                            _validLayout = false;
                            break;
                        }
                    }
                }
                return _validLayout;
            }
        }

        public bool IsStarted
        {
            get
            {
                return _started;
            }
            private set
            {
                if (!_started.Equals(value))
                {
                    bool oldValue = _started;
                    _started = value;
                    OnPropertyChanged("IsStarted", oldValue, value, false);
                }
            }
        }

        #endregion

        #region Methods

        private bool IsMonitorValid(Monitor display)
        {
            foreach (Monitor systemDisplay in ConfigManager.DisplayManager.Displays)
            {
                if (systemDisplay.Top == display.Top &&
                    systemDisplay.Left == display.Left &&
                    systemDisplay.Width == display.Width &&
                    systemDisplay.Height == display.Height)
                {
                    return true;
                }
            }
            return false;
        }

        public void ShowControlCenter()
        {
            EventHandler handler = ControlCenterShown;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void HideControlCenter()
        {
            EventHandler handler = ControlCenterHidden;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Start()
        {
            if (!IsStarted)
            {
                ConfigManager.LogManager.LogInfo("Profile starting. (Name=\"" + Name + "\")");
                OnProfileStarted();
                IsStarted = true;
                foreach (ProfileInterface heliosInterface in Interfaces)
                {
                    if (heliosInterface.Name == "Profile")
                    {
                        heliosInterface.Start();
                        break;
                    }
                }
                ConfigManager.LogManager.LogInfo("Profile started. (Name=\"" + Name + "\")");
            }
        }

        protected virtual void OnProfileStarted()
        {
            EventHandler handler = ProfileStarted;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Reset()
        {
            ConfigManager.LogManager.LogInfo("Profile resetting. (Name=\"" + Name + "\")");
            foreach (Monitor monitor in Monitors)
            {
                monitor.Reset();
            }

            foreach (HeliosInterface heliosInterface in Interfaces)
            {
                heliosInterface.Reset();
            }
            ConfigManager.LogManager.LogInfo("Profile reset completed. (Name=\"" + Name + "\")");
        }

        public void Stop()
        {
            if (IsStarted)
            {
                ConfigManager.LogManager.LogInfo("Profile stopping. (Name=\"" + Name + "\")");
                OnProfileStopped();
                IsStarted = false;
                foreach (ProfileInterface heliosInterface in Interfaces)
                {
                    if (heliosInterface.Name == "Profile")
                    {
                        heliosInterface.Stop();
                        break;
                    }
                }

                ConfigManager.LogManager.LogInfo("Profile stopped. (Name=\"" + Name + "\")");
            }
        }

        protected virtual void OnProfileStopped()
        {
            EventHandler handler = ProfileStopped;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        public void Tick()
        {
            OnProfileTick();
        }

        protected virtual void OnProfileTick()
        {
            EventHandler handler = ProfileTick;
            if (handler != null)
            {
                handler.Invoke(this, EventArgs.Empty);
            }
        }

        private void Interfaces_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                    foreach (HeliosInterface heliosInterface in e.NewItems)
                    {
                        heliosInterface.Profile = this;
                        heliosInterface.ReconnectBindings();
                        heliosInterface.PropertyChanged += new PropertyChangedEventHandler(Child_PropertyChanged);
                    }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                    foreach (HeliosInterface heliosInterface in e.OldItems)
                    {
                        heliosInterface.Profile = null;
                        heliosInterface.DisconnectBindings();
                        heliosInterface.PropertyChanged -= new PropertyChangedEventHandler(Child_PropertyChanged);
                    }
            }
        }

        void Monitors_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                    foreach (Monitor monitor in e.NewItems)
                    {
                        monitor.Profile = this;
                        monitor.PropertyChanged += new PropertyChangedEventHandler(Child_PropertyChanged);
                    }
            }

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove ||
                e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (Monitor monitor in e.OldItems)
                {
                    monitor.Profile = null;
                    monitor.PropertyChanged -= new PropertyChangedEventHandler(Child_PropertyChanged);
                }
            }

            _layoutChecked = false;
        }

        void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            HeliosObject child = sender as HeliosObject;
            PropertyNotificationEventArgs args = e as PropertyNotificationEventArgs;
            if (child != null && args != null)
            {
                OnPropertyChanged(child.Name, args);
            }
        }

        #endregion
    }
}
