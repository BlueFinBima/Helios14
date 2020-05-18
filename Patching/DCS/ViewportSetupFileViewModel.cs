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

using System.Collections.Generic;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Windows;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class ViewportSetupFileViewModel : HeliosViewModel<ViewportSetupFile>
    {
        public ViewportSetupFileViewModel(string profileName, ViewportSetupFile data) : base(data)
        {
            ProfileName = profileName;
        }

        /// <summary>
        /// the short name of the profile for which this monitor setup was generated
        /// </summary>
        public string ProfileName { get; }

        /// <summary>
        /// a suitable label for this item in the UI
        /// </summary>
        public string DisplayName => $"Helios Profile '{ProfileName}'";

        /// <summary>
        /// a label for the separate monitor setup file that will be used if this item is excluded
        /// WARNING: this value is escaped for use in a label
        /// </summary>
        public string MonitorSetupDisplayName => $"Monitor Setup 'H__{ProfileName}'";

        /// <summary>
        /// the current status of this monitor setup file
        /// </summary>
        public ViewportSetupFileStatus Status
        {
            get => (ViewportSetupFileStatus) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(ViewportSetupFileStatus), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(ViewportSetupFileStatus.Unknown));

        /// <summary>
        /// if the status is not OK, then this string has a short human-readable decsription of the problem suitable for display
        /// next to the profile name
        /// </summary>
        public string ProblemShortDescription
        {
            get => (string) GetValue(ProblemShortDescriptionProperty);
            set => SetValue(ProblemShortDescriptionProperty, value);
        }

        public static readonly DependencyProperty ProblemShortDescriptionProperty =
            DependencyProperty.Register("ProblemShortDescription", typeof(string), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(null));

        /// <summary>
        /// if the status is not OK, then this string has a human-readable narrative describing the problem
        /// </summary>
        public string ProblemNarrative
        {
            get => (string) GetValue(ProblemNarrativeProperty);
            set => SetValue(ProblemNarrativeProperty, value);
        }

        public static readonly DependencyProperty ProblemNarrativeProperty =
            DependencyProperty.Register("ProblemNarrative", typeof(string), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(null));


        /// <summary>
        /// true if this monitor setup file is associated with the profile currently open for editing
        /// </summary>
        public bool IsCurrentProfile
        {
            get => (bool) GetValue(IsCurrentProfileProperty);
            set => SetValue(IsCurrentProfileProperty, value);
        }

        public static readonly DependencyProperty IsCurrentProfileProperty =
            DependencyProperty.Register("IsCurrentProfile", typeof(bool), typeof(ViewportSetupFileViewModel),
                new PropertyMetadata(false));

        internal void SetData(ViewportSetupFile localViewports)
        {
            // copy the viewports data without replacing the Data object
            Data.Clear();
            Data.MonitorLayoutKey = localViewports.MonitorLayoutKey;
            IList<StatusReportItem> _ = Data.Merge(ProfileName, localViewports).ToList();
        }
    }
}