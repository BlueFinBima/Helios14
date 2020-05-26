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

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    /// <summary>
    /// specific naming and handling that differs between a shared monitor setup file and a separate monitor setup file
    /// </summary>
    internal class MonitorSetupTemplate
    {
        public MonitorSetupTemplate(string combinedMonitorSetupName, string profileName, bool combined)
        {
            CombinedMonitorSetupName = combinedMonitorSetupName;
            ProfileName = profileName;
            Combined = combined;
            BaseName = ProfileName?.Replace(" ", "") ?? "";
        }

        public string CombinedMonitorSetupName { get; }
        public string ProfileName { get; }
        public string BaseName { get; }
        public bool Combined { get; }

        public string MonitorSetupFileBaseName => Combined ? "Helios" : $"H_{BaseName}";
        public string MonitorSetupName => Combined ? CombinedMonitorSetupName : $"H_{BaseName}";

        public string SourcesList =>
            Combined ? "compatible Helios Profiles" : $"Helios Profile {ProfileName ?? "<unsaved>"}";

        public string FileName => $"{MonitorSetupFileBaseName}.lua";
    }
}