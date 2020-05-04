using GadrocsWorkshop.Helios.Util;
using GadrocsWorkshop.Helios.Util.DCS;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GadrocsWorkshop.Helios.Windows.Controls.DCS
{
    /// <summary>
    /// Interaction logic for InstallationLocationsControl.xaml
    /// </summary>
    public partial class InstallationLocationsControl : ItemsControl
    {
        static InstallationLocationsControl()
        {
            System.Type ownerType = typeof(InstallationLocationsControl);
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(AddInstallationLocationCommand, Add_Executed));
            CommandManager.RegisterClassCommandBinding(ownerType, new CommandBinding(RemoveInstallationLocationCommand, Remove_Executed));
        }

        public InstallationLocationsControl()
        {
            DataContext = InstallationLocations.Singleton;
            InitializeComponent();
            UpdateStatus();
            InstallationLocations.Singleton.Enabled += Locations_Changed;
            InstallationLocations.Singleton.Disabled += Locations_Changed;
        }

        public void Dispose()
        {
            InstallationLocations.Singleton.Enabled -= Locations_Changed;
            InstallationLocations.Singleton.Disabled -= Locations_Changed;
        }

        private void Locations_Changed(object sender, InstallationLocations.LocationEvent e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            Status = InstallationLocations.Singleton.Active.Any() ? StatusCodes.UpToDate : StatusCodes.NoLocations;
        }

        #region Commands
        public static readonly RoutedUICommand AddInstallationLocationCommand = new RoutedUICommand("Adds a new DCS installation location.", "AddInstallationLocation", typeof(InstallationLocationsControl));
        public static readonly RoutedUICommand RemoveInstallationLocationCommand = new RoutedUICommand("Removes a DCS installation location.", "RemoveInstallationLocation", typeof(InstallationLocationsControl));

        private static void Add_Executed(object target, ExecutedRoutedEventArgs e)
        {
            List<string> guesses = InstallationLocations.GenerateDcsRootDirectoryGuesses();

            string guessName = InstallationLocation.AUTO_UPDATE_CONFIG;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = $"Navigate to DCS Installation and select {InstallationLocation.AUTO_UPDATE_CONFIG} file",
                InitialDirectory = "",
                FileName = guessName,
                DefaultExt = ".cfg",
                CheckPathExists = true,
                DereferenceLinks = true,
                Multiselect = false,
                ValidateNames = true,
                Filter = $"DCS|{InstallationLocation.AUTO_UPDATE_CONFIG}",
                CheckFileExists = true
            };

            foreach (string guess in guesses)
            {
                if (openFileDialog.InitialDirectory == "")
                {
                    openFileDialog.InitialDirectory = guesses[0];
                }
                else
                {
                    openFileDialog.CustomPlaces.Add(new Microsoft.Win32.FileDialogCustomPlace(guess));
                }
            }

            if (openFileDialog.ShowDialog() == true)
            {
                InstallationLocations.Singleton.TryAdd(new InstallationLocation(openFileDialog.FileName));
            }
            ((InstallationLocationsControl)target).UpdateStatus();
        }

        private static void Remove_Executed(object target, ExecutedRoutedEventArgs e)
        {
            InstallationLocations.Singleton.TryRemove(e.Parameter as InstallationLocation);
            ((InstallationLocationsControl)target).UpdateStatus();
        }
        #endregion

        #region Properties
        public StatusCodes Status
        {
            get => (StatusCodes)GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(StatusCodes), typeof(InstallationLocationsControl), new PropertyMetadata(StatusCodes.Unknown));
        #endregion
    }
}
