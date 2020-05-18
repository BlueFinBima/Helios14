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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util.DCS;
using Newtonsoft.Json;
using ToolsCommon;

namespace EditViewports
{
    internal class EditViewports
    {
        private static readonly Regex ViewportHandling = new Regex("dofile.*ViewportHandling.lua",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TryFindAssignedViewport =
            new Regex("try_find_assigned_viewport\\(\"([^\"]*)\"(?:, *\"(.*)\")?\\)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static void Main(string[] args)
        {
            string jsonPath;
            if (args.Length < 1)
            {
                jsonPath = FileSystem.FindNearestDirectory("ToolsCommon\\Data\\Viewports") + "ViewportTemplates.json";
            }
            else
            {
                jsonPath = args[0];
            }

            string json = File.ReadAllText(jsonPath);
            List<ViewportTemplate> templates = JsonConvert.DeserializeObject<ViewportTemplate[]>(json).ToList();

            // get DCS location from the Helios utility that manages DCS install locations (have to use Profile Editor to configure it, either running dev build or start with --documents HeliosDev)
            string documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "HeliosDev");
            if (!Directory.Exists(documentPath))
            {
                documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Helios");
            }

            HeliosInit.Initialize(documentPath, "EditViewports.log", LogLevel.Debug);

            InstallationLocations locations = InstallationLocations.Singleton;
            if (!locations.Active.Any())
            {
                throw new Exception("at least one DCS install location must be configured and enabled");
            }

            foreach (InstallationLocation location in locations.Items)
            {
                PatchDestination destination = new PatchDestination(location);
                EditFilesInDestination(templates, destination);
            }
        }

        private static void EditFilesInDestination(List<ViewportTemplate> templates, PatchDestination destination)
        {
            if (!destination.TryLock())
            {
                throw new Exception($"cannot acquire lock on {destination.LongDescription} to edit viewports");
            }

            try
            {
                foreach (ViewportTemplate template in templates)
                {
                    foreach (Viewport viewport in template.Viewports.Where(v => v.IsValid))
                    {
                        if (null == viewport.RelativeInitFilePath)
                        {
                            Debug.WriteLine($"viewport {viewport.ViewportName} has no file path; ignoring");
                            continue;
                        }

                        if (!destination.TryGetSource(viewport.RelativeInitFilePath, out string source))
                        {
                            Debug.WriteLine(
                                $"'{viewport.RelativeInitFilePath}' does not exist in target destination; ignoring patch");
                            continue;
                        }

                        string patched = EditCode(template, viewport, source);
                        if (source == patched)
                        {
                            Debug.WriteLine($"'{viewport.RelativeInitFilePath}' is unchanged");
                            continue;
                        }

                        Debug.WriteLine("---------------------------------------------------------");
                        Debug.WriteLine(patched);
                        Debug.WriteLine("=========================================================");

                        if (!destination.TryWritePatched(viewport.RelativeInitFilePath, patched))
                        {
                            throw new Exception(
                                $"'{viewport.RelativeInitFilePath}' could not be written to target destination after edit");
                        }
                    }
                }
            }
            finally
            {
                if (!destination.TryUnlock())
                {
                    Debug.Fail($"cannot release lock on {destination.LongDescription} after editing viewports");
                }
            }
        }

        private static string EditCode(ViewportTemplate template, Viewport viewport, string source)
        {
            string patched = source;
            if (!ViewportHandling.IsMatch(source))
            {
                Debug.WriteLine($"adding viewport handling to '{viewport.RelativeInitFilePath}'");
                EnsureCrLf(ref patched);
                patched += "dofile(LockOn_Options.common_script_path..\"ViewportHandling.lua\")\r\n";
            }

            // if there are multiple viewports defined in the same file, we need to 
            // use original viewport name to disambiguate and edit each separately
            MatchCollection matches = TryFindAssignedViewport.Matches(source);
            Match assigned = null;

            switch (matches.Count)
            {
                case 0:
                    // nothing found, add new entry to end
                    Debug.WriteLine($"adding viewport selection to '{viewport.RelativeInitFilePath}'");
                    EnsureCrLf(ref patched);
                    patched +=
                        $"try_find_assigned_viewport(\"{template.ViewportPrefix}_{viewport.ViewportName}\", \"{viewport.ViewportName}\")\r\n";
                    return patched;
                case 1:
                    // only a single viewport accessed, ignore name mismatch
                    assigned = matches[0];
                    break;
                default:
                    // ambiguity, match name of viewport or fail
                    foreach (Match match in matches)
                    {
                        string existingName = match.Groups[match.Groups[2].Success ? 2 : 1].Value;
                        if (!viewport.ViewportName.Equals(existingName))
                        {
                            continue;
                        }

                        // found the matching item we can update
                        assigned = match;
                        break;
                    }

                    break;
            }

            if (assigned == null)
            {
                throw new Exception(
                    $"viewport '{viewport.ViewportName}' not found in file '{viewport.RelativeInitFilePath}' accessing multiple viewports; ambiguity cannot be resolved");
            }

            Debug.WriteLine($"changing viewport selection in '{viewport.RelativeInitFilePath}'");
            string abstractName = assigned.Groups[assigned.Groups[2].Success ? 2 : 1].Value;
            patched = patched.Replace(assigned.Groups[0].Value,
                $"try_find_assigned_viewport(\"{template.ViewportPrefix}_{viewport.ViewportName}\", \"{abstractName}\")");
            return patched;
        }

        private static void EnsureCrLf(ref string patched)
        {
            if (!patched.EndsWith("\r\n"))
            {
                patched += "\r\n";
            }
        }
    }
}