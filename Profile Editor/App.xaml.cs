//  Copyright 2014 Craig Courtney
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


// assign an alias to this namespace so we can refer to it without the space is the assembly name

using GadrocsWorkshop.Helios.Windows;
using System.Windows.Markup;

// NOTE: URL used to resolve XAML references without breaking on our assembly name that has spaces in it
[assembly: XmlnsDefinition("http://Helios.local/ProfileEditor", "GadrocsWorkshop.Helios.ProfileEditor")]
namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private string _startupProfile = null;

        #region Properties

        public string StartupProfile
        {
            get { return _startupProfile; }
            private set { _startupProfile = value; }
        }

        #endregion

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Current.Dispatcher.UnhandledException += App_DispatcherUnhandledException;

            CommandLineOptions options = Util.CommandLineOptions.Parse(new CommandLineOptions(), e.Args, out int exitCode);

            // react to options or defaults
            if (options.Profiles.Any())
            {
                StartupProfile = options.Profiles.Last();
            }

            // start up Helios
            string documentPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                RunningVersion.IsDevelopmentPrototype ? options.DevDocumentPath : options.DocumentPath);
            HeliosInit.Initialize(documentPath, "ProfileEditor.log", options.LogLevel);

            // need to defer exit until after we initialize Helios or our main window will crash
            if (exitCode < 0)
            {
                Current.Shutdown();
                return;
            }

            if (RunningVersion.IsDevelopmentPrototype)
            {
                // always run the check when in development build
                VersionChecker.CheckVersion();
            }

            // note that we started
            ConfigManager.LogManager.LogInfo("Starting Editor");
        }

        protected override void OnExit(ExitEventArgs e)
        {
            HeliosInit.OnShutdown();
            base.OnExit(e);
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ExceptionViewer.DisplayUnhandledException(e);
        }
    }
}
