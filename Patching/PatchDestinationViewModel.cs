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

using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching
{
    /// <summary>
    /// generic IPatchDestination, currently can only be constructed from a DCS InstallationLocation
    /// </summary>
    public class PatchDestinationViewModel
    {
        public IPatchDestination Destination;
        public StatusCodes Status;
        public PatchList Patches;
        public bool Enabled;

        public PatchDestinationViewModel(InstallationLocation location, string patchSet)
        {
            Destination = new PatchDestination(location);
            Status = StatusCodes.Unknown;
            Patches = PatchList.LoadPatches(Destination, patchSet);
            Enabled = location.IsEnabled;
        }

        internal void CheckApplied()
        {
            bool notInstalled = false;
            bool installed = false;
            foreach (StatusReportItem result in Patches.Verify(Destination))
            {
                // don't log these results, because Verify considers being out of date to be an error
                if (result.Severity > StatusReportItem.SeverityCode.Warning)
                {
                    // errors indicate patch needs work
                    notInstalled = true;
                }
                else
                {
                    // any indication other than error means patch is installed
                    installed = true;
                }
            }

            if (installed && notInstalled)
            {
                Status = StatusCodes.ResetRequired;
            }
            else if (installed)
            {
                Status = StatusCodes.UpToDate;
            }
            else
            {
                Status = StatusCodes.OutOfDate;
            }
        }
    }
}