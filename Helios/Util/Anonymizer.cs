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
using System.Collections;
using System.Collections.Specialized;

namespace GadrocsWorkshop.Helios.Util
{
    public class Anonymizer
    {
        private static readonly OrderedDictionary Replacements = new OrderedDictionary
        {
            // replace home directory reference
            {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "%USERPROFILE%"},
            // and file:// URIS
            {Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).Replace("\\", "/"), "%USERPROFILE%"},
            // just in case user name appears in other paths
            {$"\\{Environment.UserName}\\", "\\%USERNAME%\\"},
            // and URIs
            {$"/{Environment.UserName}/", "/%USERNAME%/"}
        };

        public static string Anonymize(string value)
        {
            string working = value;
            IDictionaryEnumerator iterator = Replacements.GetEnumerator();
            while (iterator.MoveNext())
            {
                working = working.Replace((string) iterator.Key, (string) iterator.Value);
            }

            return working;
        }

        // nothing special so far
        public static string Anonymize(Uri uri) => Anonymize(uri.ToString());
    }
}