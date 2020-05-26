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

using System.ComponentModel;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public enum SourceOfAdditionalViewports
    {
        [Description("Patched by Helios via DCS Additional Viewports interface in this profile")]
        AdditionalViewportsInterface,

        [Description("Will be created by third party solution and can't be verified by Helios")]
        ThirdPartySolution
    }
}