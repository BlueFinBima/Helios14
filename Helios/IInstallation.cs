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

namespace GadrocsWorkshop.Helios
{
    public enum InstallationPromptResult
    {
        Ok,
        Cancel
    }

    public enum InstallationResult
    {
        Success,
        Fatal,
        Canceled
    }

    /// <summary>
    /// callbacks from installation that may be implemented by a UI or otherwise
    /// </summary>
    public interface IInstallationCallbacks
    {
        InstallationPromptResult DangerPrompt(string title, string message, IList<StatusReportItem> details);
        void Failure(string title, string message, IList<StatusReportItem> details);
        void Success(string title, string message, IList<StatusReportItem> details);
    }

    /// <summary>
    /// installation of some resources by a Helios component
    /// </summary>
    public interface IInstallation
    {
        InstallationResult Install(IInstallationCallbacks callbacks);
    }
}