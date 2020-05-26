// Copyright 2020 Ammo Goettsch
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
// 

using System.ComponentModel;
using System.Windows;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class MonitorViewModel : HeliosViewModel<ShadowMonitor>
    {
        #region Private

        private string _canExcludeNarrative;
        private Vector _globalOffset;
        private double _scale;

        #endregion

        internal MonitorViewModel(ShadowMonitor monitor, Vector globalOffset, double scale)
            : base(monitor)
        {
            _scale = scale;
            _globalOffset = globalOffset;
            Update(monitor.Monitor);

            // changes to the Helios monitor geometry
            monitor.MonitorChanged += Monitor_MonitorChanged;

            // changes to our model
            monitor.PropertyChanged += Monitor_PropertyChanged;
        }

        // handle local view logic not involving other monitors
        // global logic is in MonitorSetupViewModel
        private void Monitor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Main":
                case "UserInterface":
                    if (!HasContent)
                    {
                        Data.Included = false;
                        UpdateIncludedNarrative();
                    }

                    break;
            }
        }

        // not automatically called because we are strongly referenced by event
        internal void Dispose()
        {
            Data.MonitorChanged -= Monitor_MonitorChanged;
        }

        private void Monitor_MonitorChanged(object sender, RawMonitorEventArgs e)
        {
            // some geometry change may have happened to our monitor specifically
            Update(e.Raw);
        }

        internal void Update(Monitor monitor)
        {
            RawRect = new Rect(monitor.Left, monitor.Top, monitor.Width, monitor.Height);
            Update();
        }

        internal void Update(Vector globalOffset)
        {
            _globalOffset = globalOffset;
            Update();
        }

        internal void Update(double scale)
        {
            _scale = scale;
            Update();
        }

        private void Update()
        {
            Rect transformed = RawRect;
            transformed.Offset(_globalOffset);
            transformed.Scale(_scale, _scale);
            Rect = transformed;
            ConfigManager.LogManager.LogDebug($"scaled monitor view {GetHashCode()} for monitor setup UI is {Rect}");
        }

        private void UpdateIncludedNarrative()
        {
            IncludedNarrative = Data.Included
                ? _canExcludeNarrative
                : "This monitor is not being drawn on by DCS because it is excluded by this selection.";
        }

        internal void SetCanExclude(bool value, string narrative)
        {
            CanBeExcluded = value;
            _canExcludeNarrative = narrative;
            if (!Data.Included && !value)
            {
                Data.Included = true;
            }

            UpdateIncludedNarrative();
        }

        public Rect RawRect { get; set; }
        public bool HasContent => Data.Main || Data.UserInterface || Data.ViewportCount > 0;

        public static readonly DependencyProperty CanBeExcludedProperty =
            DependencyProperty.Register("CanBeExcluded", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(false));

        public bool CanBeExcluded
        {
            get => (bool) GetValue(CanBeExcludedProperty);
            set => SetValue(CanBeExcludedProperty, value);
        }

        public static readonly DependencyProperty CanBeRemovedFromMainProperty =
            DependencyProperty.Register("CanBeRemovedFromMain", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(true));

        public bool CanBeRemovedFromMain
        {
            get => (bool) GetValue(CanBeRemovedFromMainProperty);
            set => SetValue(CanBeRemovedFromMainProperty, value);
        }

        public static readonly DependencyProperty CanBeRemovedFromUserInterfaceProperty =
            DependencyProperty.Register("CanBeRemovedFromUserInterface", typeof(bool), typeof(MonitorViewModel),
                new PropertyMetadata(true));

        public bool CanBeRemovedFromUserInterface
        {
            get => (bool) GetValue(CanBeRemovedFromUserInterfaceProperty);
            set => SetValue(CanBeRemovedFromUserInterfaceProperty, value);
        }

        public static readonly DependencyProperty IncludedNarrativeProperty =
            DependencyProperty.Register("IncludedNarrative", typeof(string), typeof(MonitorViewModel),
                new PropertyMetadata(null));

        public string IncludedNarrative
        {
            get => (string) GetValue(IncludedNarrativeProperty);
            set => SetValue(IncludedNarrativeProperty, value);
        }

        public static readonly DependencyProperty RectProperty =
            DependencyProperty.Register("Rect", typeof(Rect), typeof(MonitorViewModel), new PropertyMetadata(null));

        public Rect Rect
        {
            get => (Rect) GetValue(RectProperty);
            set => SetValue(RectProperty, value);
        }
    }
}