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

using System;
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public static class PatchVersion
    {
        public static string SortableString(string versionString)
        {
            List<int> numbers;
            try
            {
                numbers = versionString.Split('.')
                    .Select(int.Parse)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"invalid version format '{versionString}' read from DCS autoupdate file; update Helios to support current DCS version",
                    ex);
            }

            switch (numbers.Count)
            {
                case 3:
                    numbers.Add(0);
                    numbers.Add(0);
                    break;
                case 4:
                    numbers.Add(0);
                    break;
                case 5:
                    break;
                default:
                    throw new Exception(
                        $"unsupported version number length '{versionString}' read from DCS autoupdate file; update Helios to support current DCS version");
            }

            return $"{numbers[0]:000}_{numbers[1]:000}_{numbers[2]:00000}_{numbers[3]:00000}_{numbers[4]:00000}";
        }
    }
}