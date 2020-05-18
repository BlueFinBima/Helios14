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

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// Helios interfaces that implement this interface are notified when a "Reset Monitors" operation completes
    /// </summary>
    public interface IResetMonitorsObserver
    {
        /// <summary>
        /// indicates a reset of profile monitors has occurred, without indicating whether it was successful
        /// called on main thread after reset monitors thread completes and all change events on Profile.Monitors collection
        /// and properties on Monitor objects have been delivered
        /// </summary>
        void NotifyResetMonitorsComplete();
    }
}