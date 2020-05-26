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
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// a collection of view model items created for each element in the
    /// source collection, which contains the associated model items
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public class ViewModelCollection<TModel, TViewModel> : ObservableCollection<TViewModel>
        where TModel : NotificationObject where TViewModel : HeliosViewModel<TModel>
    {
        private readonly Dictionary<TModel, TViewModel> _activeMappings = new Dictionary<TModel, TViewModel>();

        public ViewModelCollection(ObservableCollection<TModel> sourceCollection)
        {
            foreach (TModel newItem in sourceCollection)
            {
                AddViewModel(newItem);
            }

            sourceCollection.CollectionChanged += SourceCollection_CollectionChanged;
        }

        private void SourceCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                // Clear executed on source collection
                _activeMappings.Clear();
                Clear();
            }

            if (e.OldItems != null)
            {
                foreach (TModel oldItem in e.OldItems)
                {
                    RemoveViewModel(oldItem);
                }
            }

            if (e.NewItems != null)
            {
                foreach (TModel newItem in e.NewItems)
                {
                    AddViewModel(newItem);
                }
            }
        }

        private void RemoveViewModel(TModel oldItem)
        {
            Remove(_activeMappings[oldItem]);
            _activeMappings.Remove(oldItem);
        }

        private void AddViewModel(TModel newItem)
        {
            TViewModel viewModel = (TViewModel) Activator.CreateInstance(typeof(TViewModel), newItem);
            _activeMappings[newItem] = viewModel;
            Add(viewModel);
        }
    }
}