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

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Objects that implement this interface are able to generate a list of user-readable
    /// issues that may require review or actions by the user.
    /// Helios interfaces that implement this interface will be called during "Preflight Check" when
    /// their profile is started, so that they can check their configuration.  Returning any error items
    /// during this check will block the profile from starting unless the user overrides this check.
    /// </summary>
    public interface IReadyCheck
    {
        IEnumerable<StatusReportItem> PerformReadyCheck();
    }
}