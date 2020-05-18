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
using System.Runtime.InteropServices;

namespace GadrocsWorkshop.Helios.Util
{
    public class KnownFolders
    {
        private static Guid FolderAppDataRoaming = new Guid("3EB685DB-65F9-4CF6-A03A-E3EF65729F3D");

        public static string AppDataRoaming
        {
            get
            {
                IntPtr pathPtr;
                string appDataRoaming;
                int hr = NativeMethods.SHGetKnownFolderPath(ref FolderAppDataRoaming, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    appDataRoaming = Marshal.PtrToStringUni(pathPtr);
                    Marshal.FreeCoTaskMem(pathPtr);
                }
                else
                {
                    appDataRoaming = Environment.GetEnvironmentVariable("userprofile") + "AppData\\Roaming";
                }

                return appDataRoaming;
            }
        }


        private static Guid FolderSavedGames = new Guid("4C5C32FF-BB9D-43b0-B5B4-2D72E54EAAA4");

        public static string SavedGames
        {
            get
            {
                // We attempt to get the Saved Games known folder from the native method to cater for situations
                // when the locale of the installation has the folder name in non-English.
                IntPtr pathPtr;
                string savedGamesPath;
                int hr = NativeMethods.SHGetKnownFolderPath(ref FolderSavedGames, 0, IntPtr.Zero, out pathPtr);
                if (hr == 0)
                {
                    savedGamesPath = Marshal.PtrToStringUni(pathPtr);
                    Marshal.FreeCoTaskMem(pathPtr);
                }
                else
                {
                    savedGamesPath = Environment.GetEnvironmentVariable("userprofile") + "Saved Games";
                }

                return savedGamesPath;
            }
        }
    }
}