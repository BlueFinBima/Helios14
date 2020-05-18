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

using System;
using System.Reflection;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// This utility exists because VersionChecker can only be static initalized after Helios is started.
    /// REVISIT: After we fix VersionChecker, this class can be integrated into VersionChecker
    /// </summary>
    public class RunningVersion
    {
        public static bool IsDevelopmentPrototype
        {
            get
            {
                Version running = Assembly.GetEntryAssembly().GetName().Version;
                return running.Build >= 1000 && running.Build < 2000;
            }
        }
    }
}