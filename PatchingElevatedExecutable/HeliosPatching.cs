﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using GadrocsWorkshop.Helios;
using GadrocsWorkshop.Helios.Patching;
using GadrocsWorkshop.Helios.Patching.DCS;
using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;

namespace HeliosVirtualCockpit.Helios.HeliosPatching
{
    class HeliosPatching
    {
        /// <summary>
        /// elevated executable to apply patches
        ///
        /// currently only supports DCS locations for output
        /// does not use CommandLine to avoid running third party code as root when possible
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            if (args.Length < 2)
            {
                // tell users to go away and not try to use this executable even though it is in the installation folder
                Console.Error.WriteLine("Do not execute this program.  It is a system process used by Helios Profile Editor.");
                return;
            }
            // start only enough of Helios to support logging
            string documentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RunningVersion.IsDevelopmentPrototype ? "HeliosDev" : "Helios");
            HeliosInit.InitializeLogging(documentPath, LogLevel.Info);
            ConfigManager.DocumentPath = documentPath;
            try
            {
                if (args.Length < 5)
                {
                    throw new Exception("must have at least the following arguments: -o PIPENAME -d DCSROOT COMMAND");
                }

                if (args[0] != "-o")
                {
                    throw new Exception("must have the following arguments: -o PIPENAME -d DCSROOT COMMAND");
                }

                if (args[2] != "-d")
                {
                    throw new Exception("must have the following arguments: -o PIPENAME -d DCSROOT COMMAND");
                }

                string pipeName = args[1];
                string dcsRoot = args[3];
                string verb = args[4];
                switch (verb)
                {
                    case "apply":
                        using (ElevatedProcessResponsePipe response = new ElevatedProcessResponsePipe(pipeName))
                        {
                            DcsApply(response, dcsRoot, args.Skip(5));
                            response.WaitToDeliver();
                        }

                        break;
                    case "revert":
                        using (ElevatedProcessResponsePipe response = new ElevatedProcessResponsePipe(pipeName))
                        {
                            DcsRevert(response, dcsRoot, args.Skip(5));
                            response.WaitToDeliver();
                        }

                        break;
                    default:
                        throw new Exception(
                            "must have arguments matching: -o PIPENAME -d DCSROOT apply|revert PATCHFOLDERS...");
                }
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("fatal error while performing patch operations as administrator", ex);
#if DEBUG
                // during debugging, we may be running as a console application, so print to console and wait
                Console.WriteLine(ex);
                Console.WriteLine(ex.StackTrace);

                new ManualResetEvent(false).WaitOne(TimeSpan.FromSeconds(30));
#else
                _ = ex;
#endif
                throw;
            }
        }

        private static PatchList LoadPatches(IEnumerable<string> patchFolders)
        {
            PatchList patches = new PatchList();
            foreach (string patchFolder in patchFolders)
            {
                patches.Merge(PatchList.Load(patchFolder));
            }
            return patches;
        }

        private static void DcsApply(ElevatedProcessResponsePipe response, string outputRoot, IEnumerable<string> patchFolders)
        {
            PatchList patches = LoadPatches(patchFolders);
            string dcsConfig = Path.Combine(outputRoot, "autoupdate.cfg");
            InstallationLocation location = new InstallationLocation(dcsConfig);
            PatchDestination dcs = new PatchDestination(location);
            IList<StatusReportItem> results = patches.Apply(dcs).ToList();
            response.SendReport(results);
        }

        private static void DcsRevert(ElevatedProcessResponsePipe response, string outputRoot, IEnumerable<string> patchFolders)
        {
            PatchList patches = LoadPatches(patchFolders);
            string dcsConfig = Path.Combine(outputRoot, "autoupdate.cfg");
            InstallationLocation location = new InstallationLocation(dcsConfig);
            PatchDestination dcs = new PatchDestination(location);
            IList<StatusReportItem> results = patches.Revert(dcs).ToList();
            response.SendReport(results);
        }
    }
}
