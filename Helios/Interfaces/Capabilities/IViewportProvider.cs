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
    /// This interface is supported by HeliosInterface instances that provide
    /// additional simulator viewports.  It is used to make sure that viewport
    /// extentss that require patches can be checked against the included patches.
    /// </summary>
    public interface IViewportProvider
    {
        bool IsViewportAvailable(string viewportName);
    }
}