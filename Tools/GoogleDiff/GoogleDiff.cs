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

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DiffMatchPatch;

namespace GoogleDiff
{
    // XXX port to command line and allow diff between any files or stdin or git previous commit
    internal class GoogleDiff
    {
        /// <summary>
        /// performs a google diff between stdin and the file identified by args[0] and outputs to stdout
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                throw new Exception("modified file path argument is required");
            }

            diff_match_patch googleDiff = new diff_match_patch();

            // read stdin raw, including separators
            string source = ReadStdin();
            string target;
            using (StreamReader streamReader = new StreamReader(args[0], Encoding.UTF8))
            {
                target = streamReader.ReadToEnd();
            }

            // NOTE: do our own diffs so we just do semantic cleanup. We don't want to optimize for efficiency.
            List<Diff> diffs;
            if (args.Length > 1 && args[1] == "--reverse")
            {
                diffs = googleDiff.diff_main(target, source);
            }
            else
            {
                diffs = googleDiff.diff_main(source, target);
            }

            googleDiff.diff_cleanupSemantic(diffs);
            List<Patch> patches = googleDiff.patch_make(diffs);
            Console.Write(googleDiff.patch_toText(patches));
        }

        private static string ReadStdin()
        {
            List<string> strings = new List<string>();
            Stream inputStream = Console.OpenStandardInput();
            using (inputStream)
            {
                byte[] buffer = new byte[2 * 1024 * 1024];
                for (;;)
                {
                    int read = inputStream.Read(buffer, 0, buffer.Length);
                    if (read < 1)
                    {
                        break;
                    }

                    strings.Add(Encoding.UTF8.GetString(buffer, 0, read));
                }

                return string.Join("", strings);
            }
        }
    }
}