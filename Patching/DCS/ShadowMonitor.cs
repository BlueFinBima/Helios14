// Copyright 2020 Helios Contributors
// 
// Helios is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Helios is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ShadowMonitorEventArgs : EventArgs
    {
        #region Private

        public ShadowMonitor Data { get; }

        #endregion

        public ShadowMonitorEventArgs(ShadowMonitor shadow)
        {
            Data = shadow;
        }
    }

    public class RawMonitorEventArgs : EventArgs
    {
        #region Private

        public Monitor Raw { get; }

        #endregion

        public RawMonitorEventArgs(Monitor monitor)
        {
            Raw = monitor;
        }
    }

    /// <summary>
    /// This class represents a Helios shadow being observed.
    /// It is the model class.
    /// </summary>
    public class ShadowMonitor : ShadowVisual
    {
        #region Delegates

        public event EventHandler<KeyChangeEventArgs> KeyChanged;

        #endregion

        #region Nested

        public class KeyChangeEventArgs : EventArgs
        {
            public KeyChangeEventArgs(string oldKey, string newKey)
            {
                OldKey = oldKey;
                NewKey = newKey;
            }

            public string OldKey { get; }
            public string NewKey { get; }
        }

        #endregion

        #region Private

        /// <summary>
        /// backing field for property Included, contains
        /// true if this monitor is included in the extent used to calculate the DCS resolution
        /// </summary>
        private bool _included;

        /// <summary>
        /// backing field for property Main, contains
        /// true if this monitor is included in Main 3D view
        /// </summary>
        private bool _main;

        /// <summary>
        /// backing field for property UserInterface, contains
        /// true if this monitor is included in UI view showing DCS dialogs and loading screen
        /// </summary>
        private bool _userInterface;

        private int _viewportCount;

        #endregion

        internal ShadowMonitor(IShadowVisualParent parent, Monitor monitor)
            : base(parent, monitor, monitor, false)
        {
            Key = CreateKey(monitor);

            // read settings for monitors matching this geometry
            LoadSettings();
        }

        // the position and size of a shadow is all we care about
        public static string CreateKey(Monitor display) =>
            $"{display.Left}_{display.Top}_{display.Width}_{display.Height}";

        internal static IEnumerable<string> GetAllKeys(Monitor monitor)
        {
            string baseKey = CreateKey(monitor);
            yield return baseKey;
            yield return $"{baseKey}_Main";
            yield return $"{baseKey}_UserInterface";
        }

        public string Key { get; private set; }

        private void LoadSettings()
        {
            _included = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP, Key, true);
            _main = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP, $"{Key}_Main", false);
            _userInterface = ConfigManager.SettingsManager.LoadSetting(MonitorSetup.SETTINGS_GROUP,
                $"{Key}_UserInterface",
                false);
        }

        private void DeleteSettings()
        {
            ISettingsManager2 settings2 = (ISettingsManager2) ConfigManager.SettingsManager;
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, Key);
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, $"{Key}_Main");
            settings2.DeleteSetting(MonitorSetup.SETTINGS_GROUP, $"{Key}_UserInterface");
        }

        /// <summary>
        /// deferred initialization so our factory can index this before we add children to it
        /// </summary>
        internal void Instrument()
        {
            Instrument(Monitor, Monitor);
        }

        protected override void OnVisualModified()
        {
            base.OnVisualModified();

            // the monitor we shadow may have changed enough to where our key
            // doesn't match (during reset monitors)
            // and so we need to reindex in that case and load settings again
            string newKey = CreateKey(_monitor);
            if (newKey != Key)
            {
                string oldKey = Key;
                Key = newKey;
                LoadSettings();
                KeyChanged?.Invoke(this, new KeyChangeEventArgs(oldKey, newKey));
            }
        }

        internal bool AddViewport()
        {
            _viewportCount++;
            if (_viewportCount == 1)
            {
                return true;
            }

            return false;
        }

        internal bool RemoveViewport()
        {
            _viewportCount--;
            if (_viewportCount == 0)
            {
                return true;
            }

            return false;
        }

        internal int ViewportCount => _viewportCount;

        /// <summary>
        /// true if this monitor is included in Main 3D view
        /// </summary>
        public bool Main
        {
            get => _main;
            set
            {
                if (_main == value)
                {
                    return;
                }

                bool oldValue = _main;
                _main = value;
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, $"{Key}_Main", value);
                OnPropertyChanged("Main", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if this monitor is included in UI view showing DCS dialogs and loading screen
        /// </summary>
        public bool UserInterface
        {
            get => _userInterface;
            set
            {
                if (_userInterface == value)
                {
                    return;
                }

                bool oldValue = _userInterface;
                _userInterface = value;
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, $"{Key}_UserInterface", value);
                OnPropertyChanged("UserInterface", oldValue, value, true);
            }
        }

        /// <summary>
        /// true if this monitor is included in the extent used to calculate the DCS resolution
        /// </summary>
        public bool Included
        {
            get => _included;
            set
            {
                if (_included == value)
                {
                    return;
                }

                bool oldValue = _included;
                _included = value;
                ConfigManager.SettingsManager.SaveSetting(MonitorSetup.SETTINGS_GROUP, Key, value);
                OnPropertyChanged("Included", oldValue, value, true);
            }
        }
    }
}