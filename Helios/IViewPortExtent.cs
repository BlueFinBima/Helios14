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

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// Visuals that implement this interface display a Simulator Viewport at their location (Top, Left, Width, Height)
    /// </summary>
    public interface IViewportExtent
    {
        /// <summary>
        /// a string that must match the name of the viewport in the simulator
        /// </summary>
        string ViewportName { get; }

        /// <summary>
        /// true if the viewport is marked as requiring patches rather than being supported by unmodified simulator installation
        /// </summary>
        bool RequiresPatches { get; }
    }
}