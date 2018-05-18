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

namespace GadrocsWorkshop.Helios.ControlCenter
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Navigation;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const long TOPMOST_TICK_COUNT = 3 * TimeSpan.TicksPerSecond;

        private List<string> _profiles = new List<string>();
        private int _profileIndex = -1;

        private DispatcherTimer _dispatcherTimer;
        private List<MonitorWindow> _windows = new List<MonitorWindow>();
        private bool _deletingProfile = false;
        private long _lastTick = 0;

        private WindowInteropHelper _helper;
        private bool _prefsShown = false;

        private HotKey _hotkey = null;

        public MainWindow()
        {
            InitializeComponent();

            ConfigManager.LogManager.LogInfo("Initializing Main Window");

            MinimizeCheckBox.IsChecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "StartMinimized", false);
            if (MinimizeCheckBox.IsChecked == true)
            {
                Minimize();
            }
            else
            {
                Maximize();
            }

            LoadProfileList(ConfigManager.SettingsManager.LoadSetting("ControlCenter", "LastProfile", ""));
            if (_profileIndex == -1 && _profiles.Count > 0)
            {
                _profileIndex = 0;
            }
            if (_profileIndex > -1 && _profileIndex < _profiles.Count)
            {
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
            else
            {
                SelectedProfileName = "- No Profiles Available -";
            }

            try
            {
                RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                using (pathKey)
                {
                    if (pathKey.GetValue("Helios") != null)
                    {
                        AutoStartCheckBox.IsChecked = true;
                    }
                    pathKey.Close();
                }
            }
            catch (Exception e)
            {
                AutoStartCheckBox.IsChecked = false;
                AutoStartCheckBox.IsEnabled = false;
                AutoStartCheckBox.ToolTip = "Unable to read/write registry for auto start.  Try running Control Center as an administrator.";
                ConfigManager.LogManager.LogError("Error checking for auto start.", e);
            }

            AutoHideCheckBox.IsChecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "AutoHide", false);

            SetLicenseMessage();
        }

        #region Properties

        public HeliosProfile ActiveProfile
        {
            get { return (HeliosProfile)GetValue(ActiveProfileProperty); }
            set { SetValue(ActiveProfileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ActiveProfile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ActiveProfileProperty =
            DependencyProperty.Register("ActiveProfile", typeof(HeliosProfile), typeof(MainWindow), new UIPropertyMetadata(null));

        public string SelectedProfileName
        {
            get { return (string)GetValue(SelectedProfileNameProperty); }
            set { SetValue(SelectedProfileNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectedProfileName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedProfileNameProperty =
            DependencyProperty.Register("SelectedProfileName", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Message.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        public string HotKeyDescription
        {
            get { return (string)GetValue(HotKeyDescriptionProperty); }
            set { SetValue(HotKeyDescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HotKeyDescription.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HotKeyDescriptionProperty =
            DependencyProperty.Register("HotKeyDescription", typeof(string), typeof(MainWindow), new PropertyMetadata(""));


        #endregion

        private void Minimize()
        {
            WindowState = System.Windows.WindowState.Minimized;
        }

        internal void Maximize()
        {
            PowerButton.IsChecked = true;
            WindowState = System.Windows.WindowState.Normal;
            if (_helper != null)
            {
                NativeMethods.BringWindowToTop(_helper.Handle);
            }
        }

        #region Commands

        private void RunProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;

            string profileToLoad = e.Parameter as string;
            if (profileToLoad == null || !File.Exists(profileToLoad))
            {
                NativeMethods.BringWindowToTop(_helper.Handle);
            }
            else
            {
                if (ActiveProfile != null)
                {
                    if (ActiveProfile.IsStarted)
                    {
                        ActiveProfile.Stop();
                    }
                    ActiveProfile = null;
                }

                LoadProfile(profileToLoad);
                StartProfile();
            }
        }

        public void StartProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StartProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_deletingProfile)
            {
                if (_profileIndex < _profiles.Count)
                {
                    File.Delete(_profiles[_profileIndex]);
                    _profiles.RemoveAt(_profileIndex);
                    if (_profileIndex == _profiles.Count)
                    {
                        _profileIndex--;
                        if (_profileIndex > -1)
                        {
                            SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
                        }
                        else
                        {
                            SelectedProfileName = "- No Profiles Available -";
                        }
                        ActiveProfile = null;
                    }
                    LoadProfileList();
                }
            }
            else
            {
                if (ActiveProfile != null)
                {
                    if (ActiveProfile.IsStarted)
                    {
                        return;
                    }

                    if (File.GetLastWriteTime(ActiveProfile.Path) > ActiveProfile.LoadTime)
                    {
                        LoadProfile(ActiveProfile.Path);
                    }
                }
                else if (_profileIndex >= 0)
                {
                    LoadProfile(_profiles[_profileIndex]);
                }

                StartProfile();
            }
        }

        public void StopProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void StopProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            StopProfile();
            SetLicenseMessage();
        }

        public void ResetProfile_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ResetProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_deletingProfile)
            {
                _deletingProfile = false;
                SetLicenseMessage();
            }
            ResetProfile();
        }

        private void OpenControlCenter_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Maximize();
        }

        private void TogglePreferences_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _prefsShown = !_prefsShown;
            if (_prefsShown)
            {
                PreferencesCanvas.Visibility = System.Windows.Visibility.Visible;
                Height = 477;
            }
            else
            {
                PreferencesCanvas.Visibility = System.Windows.Visibility.Collapsed;
                Height = 277;
            }
        }

        private void PrevProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            SetLicenseMessage();

            LoadProfileList();

            if (_profileIndex > 0 && _profiles.Count > 0)
            {
                if (ActiveProfile != null && ActiveProfile.IsStarted)
                {
                    StopProfile();
                }

                ActiveProfile = null;
                _profileIndex--;
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
        }

        private void NextProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = false;
            SetLicenseMessage();

            LoadProfileList();

            if (_profileIndex < _profiles.Count - 1)
            {
                if (ActiveProfile != null && ActiveProfile.IsStarted)
                {
                    StopProfile();
                }

                ActiveProfile = null;
                _profileIndex++;
                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(_profiles[_profileIndex]);
            }
        }

        private void DeleteProfile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _deletingProfile = true;
            Message = "!!WARNING!!\nYou are about to permanetly delete this profile.  Please press start to confirm.";
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #region Profile Running

        private void StartProfile()
        {
            if (ActiveProfile != null && !ActiveProfile.IsStarted)
            {
                ActiveProfile.ControlCenterShown += Profile_ShowControlCenter;
                ActiveProfile.ControlCenterHidden += Profile_HideControlCenter;
                ActiveProfile.ProfileStopped += new EventHandler(Profile_ProfileStopped);

                ActiveProfile.Dispatcher = Dispatcher;
                ActiveProfile.Start();

                if (_dispatcherTimer != null)
                {
                    _dispatcherTimer.Stop();
                }

                _dispatcherTimer = new DispatcherTimer();
                _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 33);
                _dispatcherTimer.Tick += new EventHandler(DispatcherTimer_Tick);
                _dispatcherTimer.Start();

                foreach (Monitor monitor in ActiveProfile.Monitors)
                {
                    try
                    {
                        if (monitor.Children.Count > 0 || monitor.FillBackground || !String.IsNullOrWhiteSpace(monitor.BackgroundImage))
                        {
                            ConfigManager.LogManager.LogDebug("Creating window (Monitor=\"" + monitor.Name + "\")");
                            MonitorWindow window = new MonitorWindow(monitor, true);
                            window.Show();
                            _windows.Add(window);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConfigManager.LogManager.LogError("Error creating monitor window (Monitor=\"" + monitor.Name + "\")", ex);
                    }
                }

                //App app = Application.Current as App;
                //if (app == null || (app != null && !app.DisableTouchKit))
                //{
                //    try
                //    {
                //        EGalaxTouch.CaptureTouchScreens(_windows);
                //    }
                //    catch (Exception ex)
                //    {
                //        ConfigManager.LogManager.LogError("Error capturing touchkit screens.", ex);
                //    }
                //}

                Message = "Running Profile";

                if (AutoHideCheckBox.IsChecked == true)
                {
                    Minimize();
                }
                else
                {
                    NativeMethods.BringWindowToTop(_helper.Handle);
                }
            }

        }

        private void StopProfile()
        {
            if (ActiveProfile != null && ActiveProfile.IsStarted)
            {
                ActiveProfile.Stop();
            }
        }

        private void ResetProfile()
        {
            if (ActiveProfile != null)
            {
                ActiveProfile.Reset();
            }
        }

        private void Profile_ShowControlCenter(object sender, EventArgs e)
        {
            Maximize();
        }

        private void Profile_HideControlCenter(object sender, EventArgs e)
        {
            Minimize();
        }

        void Profile_ProfileStopped(object sender, EventArgs e)
        {
            foreach (MonitorWindow window in _windows)
            {
                window.Close();
            }

            _windows.Clear();

            HeliosProfile profile = sender as HeliosProfile;
            if (profile != null)
            {
                profile.ControlCenterShown -= Profile_ShowControlCenter;
                profile.ControlCenterHidden -= Profile_HideControlCenter;
                profile.ProfileStopped -= Profile_ProfileStopped;
            }

            if (_dispatcherTimer != null)
            {
                _dispatcherTimer.Stop();
                _dispatcherTimer = null;
            }

            SetLicenseMessage();

            //EGalaxTouch.ReleaseTouchScreens();
        }

        void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (ActiveProfile != null)
            {
                try
                {
                    ActiveProfile.Tick();
                    long currentTick = DateTime.Now.Ticks;
                    if (currentTick - _lastTick > TOPMOST_TICK_COUNT)
                    {
                        for (int i = 0; i < _windows.Count; i++)
                        {
                            if (_windows[i].Monitor.AlwaysOnTop)
                            {
                                NativeMethods.SetWindowPos(_windows[i].Handle, HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                            }
                        }
                        NativeMethods.SetWindowPos(_helper.Handle, HWND_TOPMOST, 0, 0, 0, 0, NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE);
                        _lastTick = currentTick;
                    }
                }
                catch (Exception exception)
                {
                    ConfigManager.LogManager.LogError("Error processing profile tick or refresh.", exception);
                }
            }
        }

        #endregion

        #region Profile Persistance

        private void LoadProfile(string path)
        {
            if (ActiveProfile == null || (ActiveProfile != null && ActiveProfile.LoadTime < Directory.GetLastWriteTime(ActiveProfile.Path)))
            {
                Message = "Loading Profile...";
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Loaded, (Action)delegate { });
                ActiveProfile = ConfigManager.ProfileManager.LoadProfile(path, Dispatcher);
            }

            if (ActiveProfile != null)
            {
                NativeMethods.SHAddToRecentDocs(0x00000003, path);

                SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(ActiveProfile.Path);
#if !DEBUG
                if (!ActiveProfile.IsValidMonitorLayout)
                {
                    Message = "Can not display this profile because it has an invalid monitor configuration.  Please open the editor and select reset monitors from the profile menu.";
                    ActiveProfile = null;
                    return;
                }
#endif

                if (ActiveProfile.IsInvalidVersion)
                {
                    Message = "Can not display this profile because it was created with a newer version of Helios.  Please upgrade to the latest version.";
                    ActiveProfile = null;
                    return;
                }
            }
            else
            {
                Message = "Error loading profile.";
            }
        }

        private void LoadProfileList()
        {
            string currentProfilePath = "";
            if (_profileIndex >= 0 && _profileIndex < _profiles.Count)
            {
                currentProfilePath = _profiles[_profileIndex];
            }
            LoadProfileList(currentProfilePath);
        }

        private void LoadProfileList(string currentProfileName)
        {
            _profileIndex = -1;
            _profiles.Clear();

            foreach (string file in Directory.GetFiles(ConfigManager.ProfilePath, "*.hpf"))
            {
                if (currentProfileName != null && file.Equals(currentProfileName))
                {
                    _profileIndex = _profiles.Count;
                    SelectedProfileName = System.IO.Path.GetFileNameWithoutExtension(file);
                }
                _profiles.Add(file);
            }
        }

        #endregion

        #region Windows Event Handlers

        private void MoveThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Left = Left + e.HorizontalChange;
            Top = Top + e.VerticalChange;
        }

        private void PowerButton_Unchecked(object sender, RoutedEventArgs e)
        {
            _deletingProfile = false;
            DispatcherTimer minizeTimer = new DispatcherTimer(new TimeSpan(0, 0, 0, 0, 250), DispatcherPriority.Normal, TimedMinimize, Dispatcher);
        }

        void TimedMinimize(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            Dispatcher.Invoke(new Action(Close), null);
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (ActiveProfile != null && ActiveProfile.IsStarted)
            {
                ActiveProfile.Stop();
            }

            ConfigManager.LogManager.LogInfo("Saving control center window position.");

            // Persist window placement details to application settings
            WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
            IntPtr hwnd = new WindowInteropHelper(this).Handle;
            NativeMethods.GetWindowPlacement(hwnd, out wp);

            //Properties.Settings.Default.ControlCenterPlacement = wp;
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "WindowLocation", wp.normalPosition);

            if (ActiveProfile != null && _profileIndex >= 0 && _profileIndex < _profiles.Count)
            {
                ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastProfile", _profiles[_profileIndex]);
            }

            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                // Load window placement details for previous application session from application settings
                // Note - if window was closed on a monitor that is now disconnected from the computer,
                //        SetWindowPlacement will place the window onto a visible monitor.

                if (ConfigManager.SettingsManager.IsSettingAvailable("ControlCenter", "WindowLocation"))
                {
                    WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
                    wp.normalPosition = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "WindowLocation", new RECT(0, 0, (int)Width, (int)Height));
                    wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
                    wp.flags = 0;
                    wp.showCmd = (wp.showCmd == NativeMethods.SW_SHOWMINIMIZED ? NativeMethods.SW_SHOWNORMAL : wp.showCmd);
                    IntPtr hwnd = new WindowInteropHelper(this).Handle;
                    NativeMethods.SetWindowPlacement(hwnd, ref wp);
                }

                ModifierKeys mods = (ModifierKeys)Enum.Parse(typeof(ModifierKeys), ConfigManager.SettingsManager.LoadSetting("ControlCenter", "HotKeyModifiers", "None"));
                Keys hotKey = (Keys)Enum.Parse(typeof(Keys), ConfigManager.SettingsManager.LoadSetting("ControlCenter", "HotKey", "None"));
                if (hotKey != Keys.None)
                {
                    _hotkey = new HotKey(mods, hotKey, this);
                    _hotkey.HotKeyPressed += new Action<HotKey>(HotKeyPressed);
                    HotKeyDescription = KeyboardEmulator.ModifierKeysToString(_hotkey.KeyModifier) + _hotkey.Key.ToString();
                }
                else
                {
                    HotKeyDescription = "None";
                }
            }
            catch { }
        }

        private void Widnow_Loaded(object sender, RoutedEventArgs e)
        {
            //Set the window style to noactivate.
            _helper = new WindowInteropHelper(this);

            NativeMethods.SetWindowLong(_helper.Handle,
                NativeMethods.GWL_EXSTYLE,
                NativeMethods.GetWindowLong(_helper.Handle, NativeMethods.GWL_EXSTYLE) | NativeMethods.WS_EX_NOACTIVATE);
        }

        private void Window_Opened(object sender, EventArgs e)
        {
            Height = _prefsShown ? 477 : 277;
            Width = 504;

            if (Environment.OSVersion.Version.Major > 5 && ConfigManager.SettingsManager.LoadSetting("ControlCenter", "AeroWarning", true))
            {
                bool aeroEnabled;
                NativeMethods.DwmIsCompositionEnabled(out aeroEnabled);
                if (!aeroEnabled)
                {
                    AeroWarning warningDialog = new AeroWarning();
                    warningDialog.Owner = this;
                    warningDialog.ShowDialog();

                    if (warningDialog.DisplayAgainCheckbox.IsChecked == true)
                    {
                        ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AeroWarning", false);
                    }
                }
            }

            App app = Application.Current as App;
            if (app != null && app.StartupProfile != null && File.Exists(app.StartupProfile))
            {
                LoadProfileList(app.StartupProfile);
                LoadProfile(app.StartupProfile);
                StartProfile();
            }

            //VersionChecker.CheckVersion();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
        }

        #endregion

        #region Preferences

        private void AutoStartCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            pathKey.SetValue("Helios", "\"" + System.IO.Path.Combine(ConfigManager.ApplicationPath, "ControlCenter.exe") + "\"");
            pathKey.Close();
        }

        private void MinimizeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "StartMinimized", true);
        }

        private void AutoStartCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            RegistryKey pathKey = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
            pathKey.DeleteValue("Helios", false);
            pathKey.Close();
        }

        private void MinimizeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "StartMinimized", false);
        }

        private void AutoHideCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", true);
        }

        private void AutoHideCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "AutoHide", false);
        }

        private void SetHotkey_Click(object sender, RoutedEventArgs e)
        {
            HotKeyDetector detector = new HotKeyDetector();
            detector.Owner = this;
            detector.ShowDialog();

            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }

            _hotkey = new HotKey(detector.Modifiers, detector.Key, _helper.Handle);
            _hotkey.HotKeyPressed += new Action<HotKey>(HotKeyPressed);

            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKeyModifiers", detector.Modifiers.ToString());
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKey", detector.Key.ToString());

            HotKeyDescription = KeyboardEmulator.ModifierKeysToString(_hotkey.KeyModifier) + _hotkey.Key.ToString();
        }

        private void ClearHotkey_Click(object sender, RoutedEventArgs e)
        {
            if (_hotkey != null)
            {
                _hotkey.UnregisterHotKey();
                _hotkey.Dispose();
            }

            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKeyModifiers", ModifierKeys.None.ToString());
            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "HotKey", Keys.None.ToString());

            HotKeyDescription = "None";
        }

        #endregion

        #region Hotkey Handling

        void HotKeyPressed(HotKey obj)
        {
            Maximize();
        }

        #endregion

        #region Helper Methods

        private void SetLicenseMessage()
        {
            Message = "";
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Minimize();
        }
    }
}
