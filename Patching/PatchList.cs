﻿// Copyright 2020 Ammo Goettsch
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

namespace GadrocsWorkshop.Helios.Patching
{
    public class PatchList
    {
        private readonly List<PatchFile> _patches = new List<PatchFile>();

        // target paths in this patch list
        private readonly HashSet<string> _existing = new HashSet<string>();

        public void Merge(PatchList from)
        {
            from._patches.ForEach(patch =>
            {
                if (_existing.Contains(patch.TargetPath))
                {
                    // don't replace existing patches
                    return;
                }

                // merge patch for new target
                _existing.Add(patch.TargetPath);
                _patches.Add(patch);
            });
        }

        public IEnumerable<StatusReportItem> SimulateApply(IPatchDestination destination) =>
            DoApply(destination, null, Mode.Simulate);

        public IEnumerable<StatusReportItem> Apply(IPatchDestinationWritable destination) => DoApply(destination, destination, Mode.Apply);

        public IEnumerable<StatusReportItem> Verify(IPatchDestination destination)
        {
            if (!destination.TryLock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot acquire lock on {destination.LongDescription} to verify patches",
                    Recommendation = "close any programs that are holding a lock on this location",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            foreach (PatchFile patch in _patches)
            {
                if (!destination.TryGetSource(patch.TargetPath, out string source))
                {
                    ConfigManager.LogManager.LogDebug(
                        $"{patch.TargetPath} does not exist in {destination.LongDescription}; patch does not apply");
                    continue;
                }

                if (patch.IsApplied(source))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {patch.TargetPath} patch is already applied",
                        // there will be a lot of these, don't show them in small views
                        Flags = StatusReportItem.StatusFlags.Verbose
                    };
                }
                else
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {patch.TargetPath} is missing some patches",
                        Recommendation = "Apply patches",
                        Link = StatusReportItem.ProfileEditor,
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }
            }

            if (!destination.TryUnlock())
            {
                ConfigManager.LogManager.LogError(
                    $"cannot release lock on {destination.LongDescription} after verifying patches");
            }
        }

        internal bool IsEmpty() => !_patches.Any();

        private enum Mode
        {
            Simulate,
            Apply
        }

        private IEnumerable<StatusReportItem> DoApply(IPatchDestination destination, IPatchDestinationWritable writable, Mode mode)
        {
            // set up according to mode
            string verb;
            string verbing;

            switch (mode)
            {
                case Mode.Simulate:
                    verb = "simulate";
                    verbing = "simulating";
                    break;
                case Mode.Apply:
                    verb = "apply";
                    verbing = "applying";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            if (!destination.TryLock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot acquire lock on {destination.LongDescription} to {verb} patches",
                    Recommendation = "close any programs that are holding a lock on this location",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            foreach (PatchFile patch in _patches)
            {
                if (!destination.TryGetSource(patch.TargetPath, out string source))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{patch.TargetPath} does not exist in {destination.Description}; ignoring patch"
                    };
                    continue;
                }

                string patched;
                if (patch.IsApplied(source))
                {
                    // already applied, go to next patch
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {patch.TargetPath} is already patched"
                    };
                    continue;
                }

                if (!patch.TryApply(source, out patched, out string failureStatus))
                {
                    // could not patch; fatal
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {failureStatus}",
                        Recommendation =
                            "please install a newer Helios distribution or patches with support for this DCS version",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                    destination.TryUnlock();
                    yield break;
                }

                if (mode == Mode.Simulate)
                {
                    continue;
                }

                if (!writable.TrySaveOriginal(patch.TargetPath))
                {
                    // abort
                    yield return new StatusReportItem
                    {
                        Status =
                            $"{destination.Description} could not save original copy of {patch.TargetPath}",
                        Recommendation =
                            "please check write permissions to make sure you can write files in the DCS intallation location",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                    destination.TryUnlock();
                    yield break;
                }

                if (!WriteBack(writable, patch, patched, verbing, out StatusReportItem status))
                {
                    // abort
                    yield return status;
                    destination.TryUnlock();
                    yield break;
                }

                // success result
                yield return status;
            }

            if (!destination.TryUnlock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot release lock on {destination.LongDescription} after {verbing} patches",
                    Recommendation = $"please restart and try the {verb} process again",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }
        }

        public static PatchList Load( string fromFolder)
        {
            PatchList patches = new PatchList();
            if (!Directory.Exists(fromFolder))
            {
                return patches;
            }
            foreach (string patchPath in Directory.EnumerateFiles(fromFolder, "*.gpatch",
                SearchOption.AllDirectories))
            {
                PatchFile patch = new PatchFile
                {
                    TargetPath = patchPath.Substring(fromFolder.Length + 1,
                        patchPath.Length - (fromFolder.Length + 8))
                };
                patch.Load(patchPath);
                patches.Add(patch);
            }
            return patches;
        }

        public IEnumerable<StatusReportItem> Revert(IPatchDestinationWritable destination)
        {
            if (!destination.TryLock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot acquire lock on {destination.LongDescription} to revert patches",
                    Recommendation = "close any programs that are holding a lock on this location",
                    Severity = StatusReportItem.SeverityCode.Error
                };
                yield break;
            }

            foreach (PatchFile patch in _patches)
            {
                if (!destination.TryGetSource(patch.TargetPath, out string source))
                {
                    yield return new StatusReportItem
                    {
                        Status = $"{patch.TargetPath} does not exist in {destination.Description}; ignoring patch"
                    };
                    continue;
                }

                if (!patch.IsApplied(source))
                {
                    // patch not present in this file any more, go to next patch
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {patch.TargetPath} does not have our patches"
                    };
                    continue;
                }

                if (destination.TryRestoreOriginal(patch.TargetPath))
                {
                    // restored original file for this version, no need to revert patch
                    yield return new StatusReportItem
                    {
                        Status = $"restored {destination.LongDescription} {patch.TargetPath} restored to original file",
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    };
                    continue;
                }

                if (!patch.TryRevert(source, out string patched, out string failureStatus))
                {
                    // could not apply reverse patch; keep going trying to fix up as much as possible
                    yield return new StatusReportItem
                    {
                        Status = $"{destination.Description} {failureStatus}",
                        Recommendation = "please repair DCS installation using dcs_updater.exe",
                        Severity = StatusReportItem.SeverityCode.Error
                    };
                }

                if (!WriteBack(destination, patch, patched, "reverting", out StatusReportItem status))
                {
                    yield return status;
                    destination.TryUnlock();
                    yield break;
                }

                yield return status;
            }

            if (!destination.TryUnlock())
            {
                yield return new StatusReportItem
                {
                    Status = $"cannot release lock on {destination.LongDescription} after reverting patches",
                    Recommendation = "please restart and try the revert patches process again",
                    Severity = StatusReportItem.SeverityCode.Error
                };
            }
        }

        private static bool WriteBack(IPatchDestinationWritable destination, PatchFile patch, string patched, string verbing,
            out StatusReportItem status)
        {
            if (destination.TryWritePatched(patch.TargetPath, patched))
            {
                status = new StatusReportItem
                {
                    Status = $"{destination.Description} {patch.TargetPath} successfully written after {verbing}"
                };
                return true;
            }

            status = new StatusReportItem
            {
                Status =
                    $"{destination.Description} {patch.TargetPath} could not be written to target destination after {verbing}",
                Severity = StatusReportItem.SeverityCode.Error,
                Recommendation = "please ensure you have write permission to all the files in the target location"
            };
            return false;
        }

        public void Add(PatchFile patch)
        {
            _patches.Add(patch);
        }
    }
}