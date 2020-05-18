// Copyright 2014 Craig Courtney
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

using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// commands that are used across plugins and therefore must be declared in the SDK
    /// </summary>
    public static class SharedCommands
    {
        public static RoutedUICommand ResetMonitors { get; } = new RoutedUICommand(
            "Resets the monitors in this profile to those of the current system.", "ResetMonitors",
            typeof(SharedCommands));
    }
}