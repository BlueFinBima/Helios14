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

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    public interface IAvailableInterfaces
    {
        /// <summary>
        /// callback to indicate a factory has discovered another instance that is not present in
        /// the profile and which can be created on demand
        /// WARNING: called back on an arbitrary thread
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="displayName"></param>
        /// <param name="context"></param>
        void ReceiveAvailableInstance(IHeliosInterfaceFactoryAsync factory, string displayName, object context);
    }

    /// <summary>
    /// Descendants of HeliosInterfaceFactory can implement this interface to defer the discovery and creation of interface
    /// instances and allow the use of other threads to perform the discovery itself
    /// </summary>
    public interface IHeliosInterfaceFactoryAsync
    {
        void StartDiscoveringInterfaces(IAvailableInterfaces callbacks, HeliosProfile profile);
        HeliosInterface CreateInstance(object context);
        void StopDiscoveringInterfaces();
    }
}