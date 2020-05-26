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

using System.IO;

namespace ToolsCommon
{
    public class FileSystem
    {
        public static bool TryFindNearestDirectory(string path, out string absolutePath)
        {
            string cwd = Directory.GetCurrentDirectory();
            DirectoryInfo info = new DirectoryInfo(cwd);
            while (info != null)
            {
                string candidate = Path.Combine(info.FullName, path);
                if (Directory.Exists(candidate))
                {
                    candidate += Path.DirectorySeparatorChar;
                    absolutePath = candidate;
                    return true;
                }

                info = info.Parent;
            }

            absolutePath = null;
            return false;
        }

        public static string FindNearestDirectory(string path)
        {
            if (!TryFindNearestDirectory(path, out string result))
            {
                throw new DirectoryNotFoundException($"could not find ancestor directory that contains '{path}'");
            }

            return result;
        }
    }
}