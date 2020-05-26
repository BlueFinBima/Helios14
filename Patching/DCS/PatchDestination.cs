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
using System.IO;
using System.Text;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace GadrocsWorkshop.Helios.Patching.DCS
{
    public class PatchDestination : IPatchDestination
    {
        private readonly string _dcsRoot;
        private static readonly Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public PatchDestination(InstallationLocation location)
        {
            _dcsRoot = location.Path;
            Version = PatchVersion.SortableString(location.Version);
            DisplayVersion = location.Version;
        }

        public string Description => $"DCS {DisplayVersion}";
        public string LongDescription => $"DCS installation in '{_dcsRoot}'";

        public string RootFolder => _dcsRoot;

        public string Version { get; }
        public string DisplayVersion { get; }

        public bool TryGetSource(string targetPath, out string source)
        {
            string path = LocateFile(targetPath);
            if (!File.Exists(path))
            {
                source = null;
                return false;
            }

            using (StreamReader streamReader = new StreamReader(path, Utf8WithoutBom))
            {
                source = streamReader.ReadToEnd();
                return true;
            }
        }

        public bool TrySaveOriginal(string targetPath)
        {
            string backupAbsolute = LocateFile(BackupPath(targetPath));
            if (File.Exists(backupAbsolute))
            {
                return true;
            }

            File.Copy(LocateFile(targetPath), backupAbsolute, true);
            return true;
        }

        private string LocateFile(string targetPath) => Path.Combine(_dcsRoot, targetPath);

        // REVISIT what sort of locking can we reasonably do?
        public bool TryLock() => true;

        // REVISIT what sort of locking can we reasonably do?
        public bool TryUnlock() => true;

        public bool TryWritePatched(string targetPath, string patched)
        {
            string path = LocateFile(targetPath);
            if (!File.Exists(path))
            {
                ConfigManager.LogManager.LogError(
                    $"attempt to write back a patched file to a non-existing location: '{path}'");
                return false;
            }

            using (StreamWriter streamWriter = new StreamWriter(path, false, Utf8WithoutBom))
            {
                streamWriter.Write(patched);
                return true;
            }
        }

        public bool TryRestoreOriginal(string targetPath)
        {
            string backupAbsolute = LocateFile(BackupPath(targetPath));
            if (!File.Exists(backupAbsolute))
            {
                return false;
            }

            File.Copy(backupAbsolute, LocateFile(targetPath), true);
            return true;
        }

        private string BackupPath(string targetPath) => $"{targetPath}.{Version}";

        public PatchList SelectPatches(string patchesPath, string patchSet, ref string selectedVersion)
        {
            PatchList patches = new PatchList();
            if (!Directory.Exists(patchesPath))
            {
                return patches;
            }

            string candidateVersion = "";
            string candidatePatchSetPath = "";
            foreach (string versionPath in Directory.EnumerateDirectories(patchesPath, "???_???_?????_*",
                SearchOption.TopDirectoryOnly))
            {
                string directoryVersion = Path.GetFileName(versionPath);
                // ReSharper disable once AssignNullToNotNullAttribute versionPath is returned from directory enumeration and can't be null
                string patchSetPath = Path.Combine(versionPath, patchSet);
                if (!Directory.Exists(patchSetPath))
                {
                    // patch set not included in this update
                    continue;
                }

                if (string.Compare(directoryVersion, Version, StringComparison.InvariantCulture) > 0)
                {
                    // patches are only for later version of DCS
                    continue;
                }

                if (string.Compare(directoryVersion, candidateVersion, StringComparison.InvariantCulture) > 0)
                {
                    if (selectedVersion != null)
                    {
                        // we already committed to a particular patch version
                        if (selectedVersion != directoryVersion)
                        {
                            continue;
                        }
                    }

                    candidateVersion = directoryVersion;
                    candidatePatchSetPath = patchSetPath;
                }
            }

            if (candidatePatchSetPath == "")
            {
                if (selectedVersion != null)
                {
                    ConfigManager.LogManager.LogInfo(
                        $"no additional {patchSet} patches for DCS {DisplayVersion} from {Anonymizer.Anonymize(patchesPath)} based on selected patch version {selectedVersion}");
                }
                else
                {
                    ConfigManager.LogManager.LogInfo(
                        $"current version of DCS {DisplayVersion} is not supported by any installed {patchSet} patch set from {Anonymizer.Anonymize(patchesPath)}");
                }

                return patches;
            }

            ConfigManager.LogManager.LogInfo(
                $"loading {patchSet} patches for DCS {DisplayVersion} from {Anonymizer.Anonymize(patchesPath)} using version {candidateVersion} of the patches");

            foreach (string patchPath in Directory.EnumerateFiles(candidatePatchSetPath, "*.gpatch",
                SearchOption.AllDirectories))
            {
                PatchFile patch = new PatchFile
                {
                    TargetPath = patchPath.Substring(candidatePatchSetPath.Length + 1,
                        patchPath.Length - (candidatePatchSetPath.Length + 8))
                };
                patch.Load(patchPath);
                patches.Add(patch);
            }

            selectedVersion = candidateVersion;
            return patches;
        }
    }
}