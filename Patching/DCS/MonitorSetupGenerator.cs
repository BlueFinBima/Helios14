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
using System.IO;
using System.Linq;
using System.Windows;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    internal class MonitorSetupGenerator : DCSConfiguration
    {
        #region Nested

        private delegate StatusReportItem ProcessMonitorSetupFile(InstallationLocation location, string name,
            string directoryPath, string filePath, string correctContents);

        #endregion

        #region Private

        private readonly MonitorSetup _parent;

        /// <summary>
        /// the entire text of the combined monitor setup lua file
        /// </summary>
        private string _combinedMonitorSetup;

        /// <summary>
        /// the entire text of the monitor setup lua file
        /// </summary>
        private string _monitorSetup;

        /// <summary>
        /// the viewport setup file for the current profile or null, written to disk only if we finish configuration
        /// </summary>
        private ViewportSetupFile _localViewports;

        /// <summary>
        /// the viewport setup file containing all viewports for the current monitor setup
        /// </summary>
        private ViewportSetupFile _allViewports;

        #endregion

        public MonitorSetupGenerator(MonitorSetup parent)
        {
            _parent = parent;
            SubscribeToLocationChanges();
        }

        protected override void Update()
        {
            _parent.InvalidateStatusReport();
        }

        private const StatusReportItem.StatusFlags INFORMATIONAL =
            StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate;

        /// <summary>
        /// generate the monitor setup file but do not write it out yet (verbose version that reports all main windows and monitors
        /// also)
        /// </summary>
        /// <returns></returns>
        private IEnumerable<StatusReportItem> UpdateMonitorSetupVerbose(MonitorSetupTemplate template)
        {
            List<string> lines = CreateHeader(template);

            foreach (StatusReportItem item in GatherViewports(template))
            {
                yield return item;
            }

            // emit in sorted canonical order so we can compare files later
            foreach (KeyValuePair<string, Rect> viewport in _allViewports.Viewports.OrderBy(p => p.Key))
            {
                string code = CreateViewport(lines, viewport);
                yield return new StatusReportItem
                {
                    Status = $"{template.MonitorSetupFileBaseName}: {code}",
                    Flags = INFORMATIONAL
                };
            }

            // find main and ui view extents
            Rect mainView = Rect.Empty;
            Rect uiView = Rect.Empty;
            foreach (ShadowMonitor monitor in _parent.Monitors)
            {
                string monitorDescription =
                    $"Monitor at Windows coordinates ({monitor.Monitor.Left},{monitor.Monitor.Top}) of size {monitor.Monitor.Width}x{monitor.Monitor.Height}";
                if (!monitor.Included)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is not included in monitor setup",
                        Flags = INFORMATIONAL
                    };
                    continue;
                }

                Rect rect = MonitorSetup.VisualToRect(monitor.Monitor);
                if (monitor.Main)
                {
                    mainView.Union(rect);
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is used for main view",
                        Flags = INFORMATIONAL
                    };
                }

                if (monitor.UserInterface)
                {
                    uiView.Union(rect);
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} is used to display the DCS user interface",
                        Flags = INFORMATIONAL
                    };
                }

                if (monitor.ViewportCount > 0)
                {
                    string plural = monitor.ViewportCount > 1 ? "s" : "";
                    yield return new StatusReportItem
                    {
                        Status = $"{monitorDescription} has {monitor.ViewportCount} viewport{plural} from this profile",
                        Flags = INFORMATIONAL
                    };
                }
            }

            mainView.Offset(_parent.GlobalOffset);
            uiView.Offset(_parent.GlobalOffset);

            CreateMainView(lines, mainView);
            yield return new StatusReportItem
            {
                Status =
                    $"MAIN = {{ x = {mainView.Left}, y = {mainView.Top}, width = {mainView.Width}, height = {mainView.Height} }}",
                Flags = INFORMATIONAL
            };

            // check for separate UI view
            yield return CreateUserInterfaceViewIfRequired(lines, mainView, uiView, out string uiViewName);

            // set up required names for viewports (well-known to DCS)
            lines.Add($"UIMainView = {uiViewName}");
            lines.Add("GU_MAIN_VIEWPORT = Viewports.Center");

            foreach (string line in lines)
            {
                ConfigManager.LogManager.LogDebug(line);
            }

            if (template.Combined)
            {
                _combinedMonitorSetup = string.Join("\n", lines);
            }
            else
            {
                _monitorSetup = string.Join("\n", lines);
            }
        }


        /// <summary>
        /// generate the monitor setup file but do not write it out yet
        /// </summary>
        /// <returns></returns>
        private IEnumerable<StatusReportItem> UpdateMonitorSetup(MonitorSetupTemplate template)
        {
            List<string> lines = CreateHeader(template);

            foreach (StatusReportItem item in GatherViewports(template))
            {
                yield return item;
            }

            // emit in sorted canonical order so we can compare files later
            foreach (KeyValuePair<string, Rect> viewport in _allViewports.Viewports.OrderBy(p => p.Key))
            {
                string code = CreateViewport(lines, viewport);
                yield return new StatusReportItem
                {
                    Status = $"{template.MonitorSetupFileBaseName}: {code}",
                    Flags = INFORMATIONAL
                };
            }

            // find main and ui view extents
            Rect mainView = Rect.Empty;
            Rect uiView = Rect.Empty;
            foreach (ShadowMonitor monitor in _parent.Monitors)
            {
                if (!monitor.Included)
                {
                    continue;
                }

                Rect rect = MonitorSetup.VisualToRect(monitor.Monitor);
                if (monitor.Main)
                {
                    mainView.Union(rect);
                }

                if (monitor.UserInterface)
                {
                    uiView.Union(rect);
                }
            }

            mainView.Offset(_parent.GlobalOffset);
            uiView.Offset(_parent.GlobalOffset);

            CreateMainView(lines, mainView);

            // check for separate UI view
            CreateUserInterfaceViewIfRequired(lines, mainView, uiView, out string uiViewName);

            // set up required names for viewports (well-known to DCS)
            lines.Add($"UIMainView = {uiViewName}");
            lines.Add("GU_MAIN_VIEWPORT = Viewports.Center");

            foreach (string line in lines)
            {
                ConfigManager.LogManager.LogDebug(line);
            }

            if (template.Combined)
            {
                _combinedMonitorSetup = string.Join("\n", lines);
            }
            else
            {
                _monitorSetup = string.Join("\n", lines);
            }
        }

        private static string CreateViewport(List<string> lines, KeyValuePair<string, Rect> viewport)
        {
            string code =
                $"{viewport.Key} = {{ x = {viewport.Value.Left}, y = {viewport.Value.Top}, width = {viewport.Value.Width}, height = {viewport.Value.Height} }}";
            lines.Add(code);
            return code;
        }

        private static List<string> CreateHeader(MonitorSetupTemplate template)
        {
            List<string> lines = new List<string>
            {
                // NOTE: why do we need to run this string through a local function?  does this create a ref somehow or prevent string interning?
                "_  = function(p) return p end",
                $"name = _('{template.MonitorSetupName}')",
                $"description = 'Generated from {template.SourcesList}'"
            };
            return lines;
        }

        private static void CreateMainView(List<string> lines, Rect mainView)
        {
            // calling the MAIN viewport "Center" to match DCS' built-in monitor setups, even though it doesn't matter
            lines.Add("Viewports = {");
            lines.Add("  Center = {");
            lines.Add($"    x = {mainView.Left},");
            lines.Add($"    y = {mainView.Top},");
            lines.Add($"    width = {mainView.Width},");
            lines.Add($"    height = {mainView.Height},");
            lines.Add($"    aspect = {mainView.Width / mainView.Height},");
            lines.Add("    dx = 0,");
            lines.Add("    dy = 0");
            lines.Add("  }");
            lines.Add("}");
        }

        private IEnumerable<StatusReportItem> UpdateLocalViewports(MonitorSetupTemplate template)
        {
            _localViewports = new ViewportSetupFile
            {
                MonitorLayoutKey = _parent.MonitorLayoutKey
            };
            foreach (ShadowVisual shadow in _parent.Viewports)
            {
                string name = shadow.Viewport.ViewportName;
                if (_localViewports.Viewports.ContainsKey(name))
                {
                    yield return new StatusReportItem
                    {
                        Status =
                            $"The viewport '{name}' exists more than once in this profile.  Each viewport must have a unique name.",
                        Recommendation = $"Rename one of the duplicated viewports with name '{name}' in this profile",
                        Severity = StatusReportItem.SeverityCode.Warning,
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    };
                    continue;
                }

                Rect rect = MonitorSetup.VisualToRect(shadow.Visual);
                rect.Offset(shadow.Monitor.Left, shadow.Monitor.Top);
                rect.Offset(_parent.GlobalOffset);
                _localViewports.Viewports.Add(name, rect);
            }

            // now check against our saved state, which we also have to update
            ViewportSetupFile saved = _parent.Combined.Load(template.ProfileName);
            if (null == saved)
            {
                yield return new StatusReportItem
                {
                    Status = "The viewport data for this profile does not exist",
                    Severity = StatusReportItem.SeverityCode.Error,
                    Recommendation = $"Configure {_parent.Name}"
                };
            }
            else if (saved.MonitorLayoutKey != _localViewports.MonitorLayoutKey
                     || !saved.Viewports.OrderBy(e => e.Key)
                         .SequenceEqual(_localViewports.Viewports.OrderBy(e => e.Key)))
            {
                // monitor layout key and the viewport rectangles in order must be equal
                yield return new StatusReportItem
                {
                    Status = "The viewport data for this profile is out of date",
                    Severity = StatusReportItem.SeverityCode.Error,
                    Recommendation = $"Configure {_parent.Name}"
                };
            }
        }

        private IEnumerable<StatusReportItem> GatherViewports(MonitorSetupTemplate template)
        {
            foreach (StatusReportItem item in UpdateLocalViewports(template))
            {
                yield return item;
            }

            if (!template.Combined)
            {
                _allViewports = _localViewports;
            }
            else
            {
                _allViewports = new ViewportSetupFile
                {
                    MonitorLayoutKey = _parent.MonitorLayoutKey
                };
                foreach (string name in _parent.Combined.CalculateCombinedSetupNames())
                {
                    if (name == template.ProfileName)
                    {
                        // this is the current profile so we take the data from where we generate it instead
                        // of from a file
                        foreach (StatusReportItem item in _allViewports.Merge(name, _localViewports))
                        {
                            yield return item;
                        }

                        continue;
                    }

                    ViewportSetupFile generated = _parent.Combined.Load(name);
                    if (null == generated)
                    {
                        yield return new StatusReportItem
                        {
                            Status =
                                $"Could not include the viewports for Helios profile '{name}' because no generated viewport data was found",
                            Recommendation =
                                $"Configure DCS Monitor Setup for Helios profile '{name}', then configure DCS Monitor Setup for current Helios profile",
                            Severity = StatusReportItem.SeverityCode.Warning,
                            Link = StatusReportItem.ProfileEditor
                        };
                        continue;
                    }

                    foreach (StatusReportItem item in _allViewports.Merge(name, generated))
                    {
                        yield return item;
                    }
                }
            }
        }

        public object GenerateAnonymousPath(string savedGamesName)
        {
            string savedGamesTranslated = Path.GetFileName(KnownFolders.SavedGames);
            // ReSharper disable once AssignNullToNotNullAttribute not possible with saved games folder
            return Path.Combine(savedGamesTranslated, savedGamesName, "Config", "MonitorSetup");
        }

        private string GenerateSavedGamesPath(string savedGamesName) =>
            Path.Combine(KnownFolders.SavedGames, savedGamesName, "Config", "MonitorSetup");

        public ViewportSetupFile LocalViewports => _localViewports;

        private IEnumerable<StatusReportItem> EnumerateMonitorSetupFiles(ProcessMonitorSetupFile handler,
            MonitorSetupTemplate template)
        {
            string fileName = template.FileName;
            HashSet<string> done = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (InstallationLocation location in InstallationLocations.Singleton.Active)
            {
                if (done.Contains(location.SavedGamesName))
                {
                    // could have defaulted or configured the same variant twice
                    continue;
                }

                string monitorSetupsDirectory = GenerateSavedGamesPath(location.SavedGamesName);
                string monitorSetupPath = Path.Combine(monitorSetupsDirectory, fileName);
                string correctContents = template.Combined ? _combinedMonitorSetup : _monitorSetup;
                yield return handler(location, fileName, monitorSetupsDirectory, monitorSetupPath, correctContents);
            }
        }

        public StatusReportItem InstallFile(InstallationLocation location, string name, string directoryPath,
            string filePath, string correctContents)
        {
            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, correctContents);
            return new StatusReportItem
            {
                Status = $"generated monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)}",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        public StatusReportItem CheckSetupFile(InstallationLocation location, string name,
            string _, string filePath, string correctContents)
        {
            if (!File.Exists(filePath))
            {
                return new StatusReportItem
                {
                    Status =
                        $"{GenerateAnonymousPath(location.SavedGamesName)} does not contain the monitor setup file '{name}'",
                    Recommendation = $"Configure {_parent.Name}",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            string contents = File.ReadAllText(filePath);
            if (contents != correctContents)
            {
                return new StatusReportItem
                {
                    Status =
                        $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} does not match configuration",
                    Recommendation = $"Configure {_parent.Name}",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Warning
                };
            }

            return new StatusReportItem
            {
                Status =
                    $"monitor setup file '{name}' in {GenerateAnonymousPath(location.SavedGamesName)} is up to date",
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        /// <summary>
        /// create separate UI view if not identical to the main view
        /// </summary>
        /// <param name="lines"></param>
        /// <param name="mainView"></param>
        /// <param name="uiView"></param>
        /// <param name="uiViewName"></param>
        /// <returns></returns>
        public StatusReportItem CreateUserInterfaceViewIfRequired(List<string> lines, Rect mainView, Rect uiView,
            out string uiViewName)
        {
            string comment;
            if (uiView != mainView)
            {
                uiViewName = "UI";
                string code =
                    $"{uiViewName} = {{ x = {uiView.Left}, y = {uiView.Top}, width = {uiView.Width}, height = {uiView.Height} }}";
                comment = code;
                lines.Add(code);
            }
            else
            {
                uiViewName = "Viewports.Center";
                comment = "UI = MAIN";
            }

            return new StatusReportItem
            {
                Status = comment,
                Flags = StatusReportItem.StatusFlags.Verbose | StatusReportItem.StatusFlags.ConfigurationUpToDate
            };
        }

        /// <summary>
        /// attempt to write the monitor setup file to all configured Saved Games folders
        /// and also persist our viewports information
        /// </summary>
        /// <param name="callbacks"></param>
        /// <returns></returns>
        public override InstallationResult Install(IInstallationCallbacks callbacks)
        {
            try
            {
                if (!_parent.Profile.IsValidMonitorLayout)
                {
                    throw new Exception(
                        "UI should have disabled monitor setup writing without up to date monitors; implementation error");
                }

                if (string.IsNullOrWhiteSpace(_parent.Profile.Path))
                {
                    throw new Exception(
                        "UI should have disabled monitor setup without a profile name; implementation error");
                }

                // create template for combined monitor setup
                MonitorSetupTemplate combinedTemplate = CreateCombinedTemplate();

                // gather all the results into a list to enumerate the yield returns
                List<StatusReportItem> results =
                    new List<StatusReportItem>(EnumerateMonitorSetupFiles(InstallFile, combinedTemplate));
                if (!_parent.GenerateCombined)
                {
                    // add the same tests for separate monitor setup
                    results.AddRange(EnumerateMonitorSetupFiles(InstallFile, CreateSeparateTemplate()));
                }

                // now scan results
                foreach (StatusReportItem item in results)
                {
                    if (item.Severity >= StatusReportItem.SeverityCode.Error)
                    {
                        callbacks.Failure(
                            "Monitor setup file generation has failed",
                            "Some files may have been generated before the failure and these files were not removed.",
                            results);
                        return InstallationResult.Fatal;
                    }

                    // REVISIT we should have simulated first to gather warnings and errors so we can show a danger prompt
                }

                _parent.Combined.Save(combinedTemplate.ProfileName, _localViewports);

                callbacks.Success(
                    "Monitor setup generation successful",
                    "Monitor setup files have been installed into DCS.",
                    results);
                _parent.InvalidateStatusReport();
                return InstallationResult.Success;
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("failed to install monitor setup", ex);
                callbacks.Failure("Failed to install monitor setup", ex.Message, new List<StatusReportItem>());
                _parent.InvalidateStatusReport();
                return InstallationResult.Fatal;
            }
        }

        public override IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            if (!_parent.Profile.IsValidMonitorLayout)
            {
                yield return new StatusReportItem
                {
                    Status = "Monitor configuration in profile does not match this computer",
                    Recommendation = "Perform 'Reset Monitors' function from the 'Profile' menu",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            if (string.IsNullOrWhiteSpace(_parent.Profile.Path))
            {
                yield return new StatusReportItem
                {
                    Status =
                        "You must save the profile before you can use Monitor Setup, because it uses the profile name as the name of the monitor setup",
                    Recommendation = "Save the profile at least once before configuring Monitor Setup",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            // check if DCS install folders are configured
            InstallationLocations locations = InstallationLocations.Singleton;
            if (!locations.Active.Any())
            {
                yield return new StatusReportItem
                {
                    Status = "No DCS installation locations are configured for monitor setup",
                    Recommendation = "Configure any DCS installation location you use",
                    Link = StatusReportItem.ProfileEditor,
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }

            // gather viewport providers from this profile
            List<IViewportProvider> viewportProviders =
                _parent.Profile.Interfaces.OfType<IViewportProvider>().ToList();
            if (!_parent.UsingViewportProvider)
            {
                foreach (HeliosInterface viewportProvider in viewportProviders.OfType<HeliosInterface>())
                {
                    yield return new StatusReportItem
                    {
                        Status = $"interface '{viewportProvider.Name}' is providing additional viewport patches but this profile uses a third-party solution for those",
                        Severity = StatusReportItem.SeverityCode.Warning,
                        Recommendation = $"Remove the '{viewportProvider.Name}' interface since this profile is configured to expect a third-party solution to provide viewport modifications"
                    };
                }
            } 

            // check if any referenced viewports require patches to work
            foreach (IViewportExtent viewport in _parent.Viewports
                .Select(shadow => shadow.Viewport)
                .Where(v => v.RequiresPatches))
            {
                if (!_parent.UsingViewportProvider)
                {
                    yield return new StatusReportItem
                    {
                        Status =
                            $"viewport '{viewport.ViewportName}' must be provided by third-party solution for additional viewports",
                        Recommendation = 
                            "Verify that your third-party viewport modifications match the viewport names for this profile",
                        Flags = StatusReportItem.StatusFlags.Verbose |
                                StatusReportItem.StatusFlags.ConfigurationUpToDate
                    };
                    continue;
                }
                bool found = false;
                foreach (IViewportProvider provider in viewportProviders)
                {
                    if (provider.IsViewportAvailable(viewport.ViewportName))
                    {
                        yield return new StatusReportItem
                        {
                            Status =
                                $"viewport '{viewport.ViewportName}' is provided by '{((HeliosInterface) provider).Name}'",
                            Flags = StatusReportItem.StatusFlags.Verbose |
                                    StatusReportItem.StatusFlags.ConfigurationUpToDate
                        };
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    yield return new StatusReportItem
                    {
                        Status = $"viewport '{viewport.ViewportName}' requires patches to be installed",
                        Recommendation =
                            "Add an Additional Viewports interface or configure the viewport extent not to require patches",
                        Link = StatusReportItem.ProfileEditor,
                        Severity = StatusReportItem.SeverityCode.Error,
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    };
                }
            }

            bool updated = true;
            bool hasFile = false;

            // calculate shared monitor config
            // and see if it is up to date in all locations
            MonitorSetupTemplate combinedTemplate = CreateCombinedTemplate();
            string monitorSetupName = combinedTemplate.MonitorSetupName;
            foreach (StatusReportItem item in UpdateMonitorSetupVerbose(combinedTemplate))
            {
                yield return item;
            }

            foreach (StatusReportItem item in EnumerateMonitorSetupFiles(CheckSetupFile, combinedTemplate))
            {
                if (!item.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate))
                {
                    updated = false;
                }

                hasFile = true;
                yield return item;
            }

            // also calculate separate config, if applicable
            // and see if it is up to date in all locations
            if (!_parent.GenerateCombined)
            {
                MonitorSetupTemplate separateTemplate = CreateSeparateTemplate();
                monitorSetupName = separateTemplate.MonitorSetupName;
                foreach (StatusReportItem item in UpdateMonitorSetup(separateTemplate))
                {
                    yield return item;
                }

                foreach (StatusReportItem item in EnumerateMonitorSetupFiles(CheckSetupFile, separateTemplate))
                {
                    if (!item.Flags.HasFlag(StatusReportItem.StatusFlags.ConfigurationUpToDate))
                    {
                        updated = false;
                    }

                    hasFile = true;
                    yield return item;
                }
            }

            // XXX check if monitor setup selected in DCS (should be a gentle message but error)
            // XXX check if correct monitor resolution selected in DCS (should be a gentle message but error)
            yield return new StatusReportItem
            {
                Status = "This version of Helios does not select the Resolution in DCS directly",
                Recommendation =
                    $"Using DCS, please set 'Resolution' in the 'System' options to at least {_parent.Resolution.X}x{_parent.Resolution.Y}",
                Severity = StatusReportItem.SeverityCode.Info,
                Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
            };

            // don't tell the user to do this yet if the file isn't done
            if (hasFile && updated)
            {
                yield return new StatusReportItem
                {
                    Status = "This version of Helios does not select the monitor setup in DCS directly",
                    Recommendation =
                        $"Using DCS, please set 'Monitors' in the 'System' options to '{monitorSetupName}'",
                    Severity = StatusReportItem.SeverityCode.Info,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
                if (_parent.GenerateCombined)
                {
                    yield break;
                }

                yield return new StatusReportItem
                {
                    Status = "This profile requires a specific monitor setup file",
                    Recommendation =
                        "You will need to switch 'Monitors' in DCS when you switch Helios Profile",
                    Severity = StatusReportItem.SeverityCode.Info,
                    Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                };
            }
        }

        private MonitorSetupTemplate CreateCombinedTemplate() =>
            new MonitorSetupTemplate(
                _parent.CombinedMonitorSetupName,
                _parent.CurrentProfileName,
                true);

        private MonitorSetupTemplate CreateSeparateTemplate() =>
            new MonitorSetupTemplate(
                _parent.CombinedMonitorSetupName,
                _parent.CurrentProfileName,
                false);
    }
}