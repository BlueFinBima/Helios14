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
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CommandLine;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util.DCS;
using GadrocsWorkshop.Helios.Windows;
using Newtonsoft.Json;
using ToolsCommon;

namespace EditViewports
{
    internal class EditViewports
    {
        public class CommonOptions
        {
            [Option('d', "dcsroot", Required = true, HelpText = "Full path to DCS installation on which to operate.")]
            public string DcsRoot { get; set; }

            [Option('j', "jsondir", Default=null, HelpText ="Full path to directory containing viewports JSON files for various DCS versions")]
            public string JsonDir { get; set; }
        }

        private static readonly Regex ViewportHandling = new Regex("dofile.*ViewportHandling.lua",
            RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex TryFindAssignedViewport =
            new Regex("try_find_assigned_viewport\\(\"([^\"]*)\"(?:, *\"(.*)\")?\\)",
                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static void Main(string[] args)
        {
            ParserResult<CommonOptions> result = Parser.Default.ParseArguments<CommonOptions>(args)
                .WithParsed(options => { EditInstallation(options.DcsRoot, options.JsonDir); });
        }

        private static void EditInstallation(string dcsRootPath, string jsonDirPath)
        {
            if (jsonDirPath == null)
            {
                jsonDirPath = FileSystem.FindNearestDirectory("ToolsCommon\\Data\\Viewports");
            }

            // open DCS installation location
            InstallationLocation dcs = new InstallationLocation(Path.Combine(dcsRootPath, "autoupdate.cfg"));

            // pick JSON file from the given ones based on version number
            string exactName = $"ViewportTemplates_{PatchVersion.SortableString(dcs.Version)}.json";
            string versionedJsonPath = "";
            foreach (string candidate in Directory.EnumerateFiles(jsonDirPath, "ViewportTemplates_*.json",
                SearchOption.AllDirectories))
            {
                string candidateName = Path.GetFileName(candidate);
                if (string.Compare(candidateName, exactName, StringComparison.InvariantCulture) > 0)
                {
                    continue;
                }

                // applies
                if (string.Compare(candidateName, versionedJsonPath, StringComparison.InvariantCulture) > 0)
                {
                    // new best match
                    versionedJsonPath = candidate;
                }
            }

            string json = File.ReadAllText(Path.Combine(jsonDirPath, "ViewportTemplates.json"));
            List<ViewportTemplate> templates = JsonConvert.DeserializeObject<ViewportTemplate[]>(json).ToList();

            if (versionedJsonPath == "")
            {
                ConfigManager.LogManager.LogInfo($"no ViewportTemplates_*.json file found that is applicable to selected DCS version {dcs.Version}");
            }
            else
            {
                // read version specific changes and replace any entries by ModuleId
                string changesText = File.ReadAllText(versionedJsonPath);
                List<ViewportTemplate> changes = JsonConvert.DeserializeObject<ViewportTemplate[]>(changesText).ToList();
                templates = templates.GroupJoin(changes, t => t.TemplateName, c => c.TemplateName, (original, applicableChanges) => applicableChanges.FirstOrDefault() ?? original).ToList();
            }


            // get DCS location from the Helios utility that manages DCS install locations (have to use Profile Editor to configure it, either running dev build or start with --documents HeliosDev)
            string documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "HeliosDev");
            if (!Directory.Exists(documentPath))
            {
                documentPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Helios");
            }

            HeliosInit.Initialize(documentPath, "EditViewports.log", LogLevel.Debug);

            ConfigManager.LogManager.LogInfo($"Editing viewport in DCS distribution {dcs.Path} of Version {dcs.Version}");
            ConfigManager.LogManager.LogInfo($"Selected ViewportTemplates file {versionedJsonPath}");
            PatchDestination destination = new PatchDestination(dcs);
            EditFilesInDestination(templates, destination);

            HeliosInit.OnShutdown();
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
                            ConfigManager.LogManager.LogDebug($"viewport {viewport.ViewportName} has no file path; ignoring");
                            continue;
                        }

                        if (!destination.TryGetSource(viewport.RelativeInitFilePath, out string source))
                        {
                            ConfigManager.LogManager.LogDebug(
                                $"'{viewport.RelativeInitFilePath}' does not exist in target destination; ignoring patch");
                            continue;
                        }

                        string patched = EditCode(template, viewport, source);
                        if (source == patched)
                        {
                            ConfigManager.LogManager.LogDebug($"'{viewport.RelativeInitFilePath}' is unchanged");
                            continue;
                        }

                        ConfigManager.LogManager.LogDebug("---------------------------------------------------------");
                        ConfigManager.LogManager.LogDebug(patched);
                        ConfigManager.LogManager.LogDebug("=========================================================");

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
                ConfigManager.LogManager.LogDebug($"adding viewport handling to '{viewport.RelativeInitFilePath}'");
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
                    ConfigManager.LogManager.LogDebug($"adding viewport selection to '{viewport.RelativeInitFilePath}'");
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

            ConfigManager.LogManager.LogDebug($"changing viewport selection in '{viewport.RelativeInitFilePath}'");
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