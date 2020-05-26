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

using System.Collections.ObjectModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public partial class CombinedMonitorSetupViewModel
    {
        public CombinedMonitorSetupViewModel()
            : base(new MonitorSetup())
        {
            Combined = new ObservableCollection<ViewportSetupFileViewModel>
            {
                new ViewportSetupFileViewModel("F-A1_existing_profile", new ViewportSetupFile())
                {
                    Status = ViewportSetupFileStatus.OK
                },
                new ViewportSetupFileViewModel("F-A2_conflicting_profile", new ViewportSetupFile())
                {
                    Status = ViewportSetupFileStatus.Conflict,
                    ProblemShortDescription = "conflicts with F-A1_existing_profile",
                    ProblemNarrative = "the viewport LEFT_MFCD has a different screen position"
                },
                new ViewportSetupFileViewModel("F-A3_not_generated_profile", new ViewportSetupFile())
                {
                    Status = ViewportSetupFileStatus.NotGenerated,
                    ProblemShortDescription = "has no current viewport data",
                    ProblemNarrative =
                        "DCS Monitor Setup has to be configured in profile 'F-A3_not_generated_profile' before it can be combined"
                }
            };
            Excluded = new ObservableCollection<ViewportSetupFileViewModel>
            {
                new ViewportSetupFileViewModel("A-B1_separate_profile", new ViewportSetupFile())
                {
                    Status = ViewportSetupFileStatus.OK
                }
            };
        }
    }
}