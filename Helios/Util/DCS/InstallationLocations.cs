using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using NLog;

namespace GadrocsWorkshop.Helios.Util.DCS
{
    public class InstallationLocations : DependencyObject
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public event EventHandler<LocationEvent> Added;
        public event EventHandler<LocationEvent> Disabled;
        public event EventHandler<LocationEvent> Enabled;
        public event EventHandler<LocationEvent> Removed;

        private static InstallationLocations _singleton;

        public static InstallationLocations Singleton => _singleton ?? (_singleton = new InstallationLocations());

        public class LocationEvent : EventArgs
        {
            internal LocationEvent(InstallationLocation location)
            {
                Location = location;
            }

            #region Properties

            public InstallationLocation Location { get; }

            #endregion
        }

        private InstallationLocations()
        {
            LoadAll();
            if (ConfigManager.SettingsManager is ISettingsManager2 settings)
            {
                settings.Synchronized += Settings_Synchronized;
            }
        }

        private void Settings_Synchronized(object sender, EventArgs e)
        {
            IList<InstallationLocation> removed = Items.ToList();
            Items.Clear();
            foreach (InstallationLocation location in removed)
            {
                Removed?.Invoke(this, new LocationEvent(location));
            }
            LoadAll();
        }

        private void LoadAll()
        {
            // load from settings XML
            foreach (InstallationLocation item in InstallationLocation.ReadSettings())
            {
                DoAdd(item);
            }
        }

        internal bool TryAdd(InstallationLocation newItem)
        {
            // scan list; O(n) but this is a UI action
            foreach (InstallationLocation existing in Items)
            {
                if (existing.Path.Equals(newItem.Path, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }

            // add to our collection and register handlers
            DoAdd(newItem);

            // write it to settings file
            newItem.UpdateSettings();

            // notify our customers
            Added?.Invoke(this, new LocationEvent(newItem));
            return true;
        }

        private void DoAdd(InstallationLocation newItem)
        {
            newItem.ChangeEnabled += (sender, e) =>
            {
                InstallationLocation location = (InstallationLocation) sender;
                if (location.IsEnabled)
                {
                    Enabled?.Invoke(this, new LocationEvent(location));
                }
                else
                {
                    Disabled?.Invoke(this, new LocationEvent(location));
                }
            };
            Items.Add(newItem);
        }

        internal bool TryRemove(InstallationLocation oldItem)
        {
            if (oldItem == null)
            {
                return false;
            }

            if (!Items.Remove(oldItem))
            {
                return false;
            }

            oldItem.DeleteSettings();
            Removed?.Invoke(this, new LocationEvent(oldItem));
            return true;
        }

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<InstallationLocation>),
                typeof(InstallationLocations), new PropertyMetadata(new ObservableCollection<InstallationLocation>()));

        #region Properties

        public IList<InstallationLocation> Active => Items.Where(l => l.IsEnabled).ToList();

        public ObservableCollection<InstallationLocation> Items
        {
            get => (ObservableCollection<InstallationLocation>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        #endregion

        public static List<string> GenerateDcsRootDirectoryGuesses()
        {
            HashSet<string> existing = new HashSet<string>(
                Singleton.Items.Select(item => item.Path), 
                StringComparer.OrdinalIgnoreCase);

            // typical file system locations
            HashSet<string> guessPaths = new HashSet<string> {
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Eagle Dynamics", "DCS World"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "DCSWorld"),
                System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Steam", "steamapps", "common", "DCSWorld.OpenBeta"),
                "c:\\DCS World",
                "c:\\DCS World.OpenBeta",
                "c:\\DCS",
                "c:\\DCS.OpenBeta",
                "c:\\Eagle Dynamics\\DCS World",
                "c:\\Eagle Dynamics\\DCS World.OpenBeta"
            };

            // any locations from registry indicate where DCS was initially installed
            foreach (string keyPath in new[] {@"Software\Eagle Dynamics\DCS World OpenBeta", @"Software\Eagle Dynamics\DCS World"})
            {
                foreach (RegistryKey rootKey in new[] { Registry.CurrentUser, Registry.LocalMachine })
                {
                    try
                    {
                        using (RegistryKey pathKey = rootKey.OpenSubKey(keyPath))
                        {
                            GenerateGuessesFromRegistry(guessPaths, pathKey);
                        }
                    }
                    catch (System.Security.SecurityException)
                    {
                        // this is a normal case, the user may not be able to read the registry
                        Logger.Debug("Registry access denied; Helios won't guess DCS location based on this key");
                    }
                }
            }

            // generate a lot of guesses where it might be
            HashSet<string> guesses = new HashSet<string>(guessPaths);
            foreach (System.IO.DriveInfo drive in System.IO.DriveInfo.GetDrives()
                .Where(d => (d.DriveType == System.IO.DriveType.Fixed) && (d.IsReady))
                .Where(d => d.Name.Substring(1) == ":\\"))
            {
                string letter = drive.Name.Substring(0, 1);

                // generate guess for every drive
                foreach (string guess in guessPaths
                    .Where(g => g.Substring(1, 2) == ":\\"))
                {
                    guesses.Add($"{letter}{guess.Substring(1)}");
                }
            }

            // now filter to existing directories we haven't already added
            return guesses.Where(guess => (!existing.Contains(guess)) && (System.IO.Directory.Exists(guess))).ToList();
        }

        private static void GenerateGuessesFromRegistry(HashSet<string> guessPaths, RegistryKey pathKey)
        {
            object value = pathKey?.GetValue("Path");
            if (value != null)
            {
                if (pathKey.GetValueKind("Path") == RegistryValueKind.String)
                {
                    guessPaths.Add((string) value);
                }
            }
            pathKey?.Close();
        }
    }
}