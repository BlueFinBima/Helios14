//  Copyright 2014 Craig Courtney
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System.Runtime.InteropServices;

namespace GadrocsWorkshop.Helios.Interfaces.Falcon
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TextLines
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
        public string line1;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
        public string line2;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
        public string line3;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
        public string line4;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 26)]
        public string line5;
    }
}
