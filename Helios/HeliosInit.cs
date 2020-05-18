//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
//    
//  Helios is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  Helios is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using NLog;

namespace GadrocsWorkshop.Helios
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Windows.Media;

    /// <summary>
    /// Class to initialize the Helios Runtime.
    /// </summary>
    public static class HeliosInit
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static void Initialize(string docPath, string logFileName, LogLevel logLevel, HeliosApplication application = null)
        {
            // check OS and build
            if (!Environment.Is64BitOperatingSystem)
            {
                throw new PlatformNotSupportedException("Helios must be executed on a 64-bit Windows OS");
            }
            if (!Environment.Is64BitProcess)
            {
                throw new PlatformNotSupportedException("Helios must be executed as a 64-bit application");
            }

            // Create documents directory if it does not exist
            ConfigManager.DocumentPath = docPath;

            // start up logging
            InitializeNLog(logLevel);

            // create this legacy interface for other DLLs that don't have access to NLog
            ConfigManager.LogManager = new LogManager(logLevel);

            Assembly execAssembly = Assembly.GetExecutingAssembly();
            Logger.Info("Helios Version " + execAssembly.GetName().Version);

            ConfigManager.Application = application ?? new HeliosApplication();
            ConfigManager.SettingsManager =
                new SettingsManager(Path.Combine(ConfigManager.DocumentPath, "HeliosSettings.xml"))
                {
                    Writable = ConfigManager.Application.SettingsAreWritable
                };
            ConfigManager.UndoManager = new UndoManager();
            ConfigManager.ProfileManager = new ProfileManager();
            ConfigManager.ImageManager = new ImageManager(ConfigManager.ImagePath);
            ConfigManager.DisplayManager = new DisplayManager();

            ConfigManager.ModuleManager = new ModuleManager(ConfigManager.ApplicationPath);
            ConfigManager.TemplateManager = new TemplateManager(ConfigManager.TemplatePath, ConfigManager.PanelTemplatePath);
            
            Logger.Debug("Searching for Helios modules in libraries");

            LoadModule(Assembly.GetExecutingAssembly());

            if (ConfigManager.Application.AllowPlugins)
            {
                string pluginsFolder = Path.Combine("Plugins");
                if (Directory.Exists(pluginsFolder))
                {
                    foreach (string pluginPath in Directory.EnumerateFiles(pluginsFolder, "*.dll", SearchOption.AllDirectories))
                    {
                        LoadModule(pluginPath);
                    }
                }

                // REVISIT: move this to plugins folder and get rid of special case if we can add the check for system libraries
                // to HeliosModuleAttribute.  This would be relevant if we create other plugins for DCSFlightPanels and the like
                String phidgetsDllPath = Path.Combine(Environment.SystemDirectory, "phidget21.dll");
                if (File.Exists("Phidgets.dll") && File.Exists(phidgetsDllPath))
                {
                    LoadModule("Phidgets.dll");
                }
            }

            if (RenderCapability.Tier == 0)
            {
                Logger.Warn("Hardware rendering is not available on this machine.  Helios will consume large amounts of CPU.");
            }
        }

        private static void InitializeNLog(LogLevel logLevel)
        {
            // tell NLog config where to log
            GlobalDiagnosticsContext.Set("documentsPath", ConfigManager.DocumentPath);

            // tell NLog config the log level
            NLog.LogLevel nlogLevel;
            bool badLogLevel = false;
            switch (logLevel)
            {
                case LogLevel.All:
                case LogLevel.Error:
                    nlogLevel = NLog.LogLevel.Error;
                    break;
                case LogLevel.Warning:
                    nlogLevel = NLog.LogLevel.Warn;
                    break;
                case LogLevel.Info:
                    nlogLevel = NLog.LogLevel.Info;
                    break;
                case LogLevel.Debug:
                    nlogLevel = NLog.LogLevel.Debug;
                    break;
                default:
                    nlogLevel = NLog.LogLevel.Info;
                    badLogLevel = true;
                    break;
            }

            GlobalDiagnosticsContext.Set("logLevel", nlogLevel.ToString());

            // now that we are configured, we can log this
            if (badLogLevel)
            {
                Logger.Warn("Log level {Level} is not valid for command line selection; Info level used instead", logLevel);
            }

            NLog.LogManager.Configuration = NLog.LogManager.Configuration?.Reload();
            NLog.LogManager.Flush();
        }

        private static void LoadModule(string moduleFileName)
        {
            if (File.Exists(moduleFileName))
            {
                Logger.Debug($"Loading library: '{moduleFileName}'");
                try
                {
                    Assembly asm = Assembly.LoadFrom(moduleFileName);
                    if (asm != null)
                    {
                        LoadModule(asm);
                    }
                    else
                    {
                        Logger.Warn($"Failed to load library '{moduleFileName}'");
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, $"Failed to load library '{moduleFileName}' due to error");
                }
            }
        }

        private static void LoadModule(Assembly asm)
        {
            string moduleName = asm.GetName().Name;
            Logger.Debug($"Helios is searching for Helios components in library '{moduleName}'");
            ((ModuleManager)ConfigManager.ModuleManager).RegisterModule(asm);

            string directoryName = moduleName;
            HeliosModuleAttribute[] moduleAttributes = (HeliosModuleAttribute[])asm.GetCustomAttributes(typeof(HeliosModuleAttribute), false);
            if (moduleAttributes != null && moduleAttributes.Length > 0)
            {
                directoryName = moduleAttributes[0].Directory;
            }
            else
            {
                Logger.Debug($"No Helios-specific module attribute in library '{directoryName}'; using default path based on library name");
            }

            ((TemplateManager)ConfigManager.TemplateManager).LoadModuleTemplates(directoryName);
        }

        /// <summary>
        /// flush logs and do anything else Helios wants to do before exit
        ///
        /// WARNING: this method is not guaranteed to get called since there are many ways to
        /// exit an application, some of them not allowing us time to call it.  It may also
        /// be called more than once.
        /// </summary>
        public static void OnShutdown()
        {
            NLog.LogManager.Shutdown();
        }
    }
}
