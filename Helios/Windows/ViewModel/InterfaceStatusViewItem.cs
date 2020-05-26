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

using System.Windows;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    /// <summary>
    /// A view model for StatusReportItem viewed in the Interface Status view
    /// Not implemented as HeliosViewModel because StatusReportItem (the model) is not a Helios NotificationObject
    /// </summary>
    public class InterfaceStatusViewItem : DependencyObject
    {
        private readonly StatusReportItem item;

        public InterfaceStatusViewItem(StatusReportItem item)
        {
            this.item = item;
        }

        public bool HasRecommendation => item.Recommendation != null;
        public string Status => item.Severity.ToString();
        public string TextLine1 => item.Status;
        public string TextLine2 => item.Recommendation;
    }
}