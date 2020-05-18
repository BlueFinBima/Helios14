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

namespace GadrocsWorkshop.Helios.Util
{
    public class Tokenizer
    {
        /// <summary>
        /// called to tokenize a string
        /// </summary>
        /// <param name="input"></param>
        /// <param name="minTokens"></param>
        /// <returns>at least minTokens tokens, with any tokens that were not present in the input set to the empty string</returns>
        public static string[] TokenizeAtLeast(string input, int minTokens, params char[] separator)
        {
            string[] raw = input.Split(separator);
            int parsed = raw.Length;
            if (parsed < minTokens)
            {
                Array.Resize(ref raw, minTokens);
            }

            for (int fill = parsed; fill < minTokens; fill++)
            {
                raw[fill] = "";
            }

            return raw;
        }
    }
}