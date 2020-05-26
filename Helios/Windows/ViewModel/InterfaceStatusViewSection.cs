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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    /// <summary>
    /// a view model for the status of an interface, as viewed in the Interface Status view
    /// </summary>
    public class InterfaceStatusViewSection : HeliosViewModel<InterfaceStatus>
    {
        #region Private

        private readonly List<StatusReportItem> _report = new List<StatusReportItem>();
        private StatusReportItem.SeverityCode _displayThreshold;

        #endregion

        public InterfaceStatusViewSection(InterfaceStatus data)
            : base(data)
        {
            Items = new ObservableCollection<InterfaceStatusViewItem>();
            Recommendations = new ObservableCollection<string>();
            if (data.Interface is IExtendedDescription extendedInfo)
            {
                RemovalNarrative = extendedInfo.RemovalNarrative;
            }
        }

        /// <summary>
        /// late init from parent
        /// </summary>
        /// <param name="displayThreshold"></param>
        internal void Initialize(StatusReportItem.SeverityCode displayThreshold)
        {
            _displayThreshold = displayThreshold;
            Update(Data.Report);
            Data.PropertyChanged += Data_PropertyChanged;
        }

        /// <summary>
        /// called by parent if the display threshold is changed after we have already been initialized
        /// </summary>
        /// <param name="displayThreshold"></param>
        internal void ChangeDisplayThreshold(StatusReportItem.SeverityCode displayThreshold)
        {
            _displayThreshold = displayThreshold;
            Update(Data.Report);
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Report":
                    Update(((InterfaceStatus) sender).Report);
                    break;
            }
        }

        private void Update(IList<StatusReportItem> freshReport)
        {
            _report.Clear();
            Items.Clear();
            Recommendations.Clear();
            RecommendationsVisibility = Visibility.Collapsed;

            // scan all items for status codes, even if filtered
            StatusReportItem.SeverityCode newStatus = freshReport
                .Select(item => item.Severity)
                .DefaultIfEmpty(StatusReportItem.SeverityCode.None)
                .Max();

            // filter report
            _report.AddRange(freshReport.Where(i => i.Severity >= _displayThreshold));

            if (_report.Count < 1)
            {
                // done
                SetStatus(newStatus);
                UpdateDetailsVisibility();
                return;
            }

            // create a unique set of recommendations
            HashSet<string> uniqueRecommendations = new HashSet<string>(
                _report.Select(item => item.Recommendation).Where(text => !string.IsNullOrEmpty(text)),
                StringComparer.OrdinalIgnoreCase);
            foreach (string recommendation in uniqueRecommendations)
            {
                Recommendations.Add(recommendation);
                RecommendationsVisibility = Visibility.Visible;
            }

            // create view models for status items
            IEnumerable<InterfaceStatusViewItem> checklistItems = _report
                .Select(item => new InterfaceStatusViewItem(item));
            foreach (InterfaceStatusViewItem checklistItem in checklistItems)
            {
                Items.Add(checklistItem);
            }

            SetStatus(newStatus);
            UpdateDetailsVisibility();
        }

        private void UpdateDetailsVisibility()
        {
            DetailsVisibility = Items.Count > 0 && _displayThreshold < StatusReportItem.SeverityCode.None
                ? Visibility.Visible
                : Visibility.Collapsed;
            string[] attentionStatuses =
                {StatusReportItem.SeverityCode.Warning.ToString(), StatusReportItem.SeverityCode.Error.ToString()};
            DetailsExpanded = Items.Any(i => attentionStatuses.Contains(i.Status));
        }

        /// <summary>
        /// Instead of a setter for the corresponding string, we use this method to
        /// ensure the underlying enum can be renamed and otherwise changed safely.
        /// </summary>
        /// <param name="code"></param>
        public void SetStatus(StatusReportItem.SeverityCode code)
        {
            SetValue(StatusProperty, code.ToString());
            switch (code)
            {
                case StatusReportItem.SeverityCode.None:
                    StatusNarrative = "This component is not reporting any status.";
                    break;
                case StatusReportItem.SeverityCode.Info:
                    StatusNarrative = "This component is configured correctly.";
                    break;
                case StatusReportItem.SeverityCode.Warning:
                    StatusNarrative = "This component is reporting warnings that should be corrected.";
                    break;
                case StatusReportItem.SeverityCode.Error:
                    StatusNarrative = "This component is reporting errors and requires attention.";
                    break;
            }
        }

        #region Properties

        public Visibility GoThereVisibility => Data.HasEditor ? Visibility.Visible : Visibility.Hidden;

        public Visibility DetailsVisibility
        {
            get => (Visibility) GetValue(DetailsVisibilityProperty);
            set => SetValue(DetailsVisibilityProperty, value);
        }

        public bool DetailsExpanded
        {
            get => (bool) GetValue(DetailsExpandedProperty);
            set => SetValue(DetailsExpandedProperty, value);
        }

        public ObservableCollection<InterfaceStatusViewItem> Items
        {
            get => (ObservableCollection<InterfaceStatusViewItem>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public ObservableCollection<string> Recommendations
        {
            get => (ObservableCollection<string>) GetValue(RecommendationsProperty);
            set => SetValue(RecommendationsProperty, value);
        }

        public Visibility RecommendationsVisibility
        {
            get => (Visibility) GetValue(RecommendationsVisibilityProperty);
            set => SetValue(RecommendationsVisibilityProperty, value);
        }

        public string Status => (string) GetValue(StatusProperty);

        public string StatusNarrative
        {
            get => (string) GetValue(StatusNarrativeProperty);
            set => SetValue(StatusNarrativeProperty, value);
        }

        public string RemovalNarrative
        {
            get => (string) GetValue(RemovalNarrativeProperty);
            set => SetValue(RemovalNarrativeProperty, value);
        }

        #endregion

        #region DependencyProperties

        public static readonly DependencyProperty DetailsVisibilityProperty =
            DependencyProperty.Register("DetailsVisibility", typeof(Visibility), typeof(InterfaceStatusViewSection),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(ObservableCollection<InterfaceStatusViewItem>),
                typeof(InterfaceStatusViewSection),
                new PropertyMetadata(null));

        public static readonly DependencyProperty RecommendationsProperty =
            DependencyProperty.Register("Recommendations", typeof(ObservableCollection<string>),
                typeof(InterfaceStatusViewSection), new PropertyMetadata(null));

        public static readonly DependencyProperty RecommendationsVisibilityProperty =
            DependencyProperty.Register("RecommendationsVisibility", typeof(Visibility),
                typeof(InterfaceStatusViewSection),
                new PropertyMetadata(Visibility.Visible));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(InterfaceStatusViewSection),
                new PropertyMetadata("None"));

        public static readonly DependencyProperty StatusNarrativeProperty =
            DependencyProperty.Register("StatusNarrative", typeof(string), typeof(InterfaceStatusViewSection),
                new PropertyMetadata(""));

        public static readonly DependencyProperty RemovalNarrativeProperty =
            DependencyProperty.Register("RemovalNarrative", typeof(string), typeof(InterfaceStatusViewSection),
                new PropertyMetadata("Delete this interface and remove all of its bindings from the Profile."));

        public static readonly DependencyProperty DetailsExpandedProperty =
            DependencyProperty.Register("DetailsExpanded", typeof(bool), typeof(InterfaceStatusViewSection),
                new PropertyMetadata(false));

        #endregion
    }
}