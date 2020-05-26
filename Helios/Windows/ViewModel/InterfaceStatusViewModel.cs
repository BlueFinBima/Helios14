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

using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Windows.ViewModel
{
    public class InterfaceStatusViewModel : HeliosViewModel<InterfaceStatusScanner>
    {
        public static RoutedUICommand GoThereCommand { get; } =
            new RoutedUICommand("Opens an associated editor.", "GoThere", typeof(InterfaceStatusViewModel));

        public static RoutedUICommand DeleteInterfaceCommand { get; } =
            new RoutedUICommand("Deletes the interface passed as the parameter.", "DeleteInterface",
                typeof(InterfaceStatusViewModel));

        /// <summary>
        /// backing field for ReloadCommand
        /// </summary>
        private ICommand _reloadCommand;

        public InterfaceStatusViewModel(InterfaceStatusScanner data)
            : base(data)
        {
            Items = new ViewModelCollection<InterfaceStatus, InterfaceStatusViewSection>(data.InterfaceStatuses);
            Items.CollectionChanged += Items_CollectionChanged;
            Data.PropertyChanged += Data_PropertyChanged;
        }

        private void Data_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "DisplayThreshold":
                    // broadcast to all sections
                    foreach (InterfaceStatusViewSection section in Items)
                    {
                        section.ChangeDisplayThreshold(Data.DisplayThreshold);
                    }

                    break;
            }
        }

        private void Items_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // configure each new item
            if (e.NewItems != null)
            {
                foreach (InterfaceStatusViewSection section in e.NewItems)
                {
                    section.Initialize(Data.DisplayThreshold);
                }
            }
        }

        public ViewModelCollection<InterfaceStatus, InterfaceStatusViewSection> Items { get; }

        /// <summary>
        /// backing field for property ShareCommand, contains
        /// handler for Share button
        /// </summary>
        private ICommand _shareCommand;

        /// <summary>
        /// handler for Share button
        /// </summary>
        public ICommand ShareCommand
        {
            get
            {
                _shareCommand = _shareCommand ?? new RelayCommand(parameter =>
                {
                    // execute a modal dialog
                    Dialog.ShowModalCommand.Execute(
                        new ShowModalParameter
                        {
                            Content = new ShareInterfaceStatus(Data.InterfaceStatuses)
                        },
                        parameter as IInputElement);
                });
                return _shareCommand;
            }
        }

        public ICommand ReloadCommand
        {
            get
            {
                _reloadCommand = _reloadCommand ?? new RelayCommand(parameter => { Data.PerformChecks(); });
                return _reloadCommand;
            }
        }
    }
}