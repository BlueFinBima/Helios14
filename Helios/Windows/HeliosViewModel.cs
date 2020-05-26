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

using System.Windows;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Windows
{
    /// <summary>
    /// a view model class supporting dependency properties, referencing a
    /// Helios model object supporting automatic undo
    /// this type of view model also supports generating undo events for dependency properties
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeliosViewModel<T> : DependencyObject where T : NotificationObject
    {
        protected HeliosViewModel(T data)
        {
            Data = data;
        }

        protected static void GenerateHeliosUndoForProperty(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // pretend this dependency object is a NotificationObject in the tree under our data object and
            // generate an Undo record that will set our property back if called
            HeliosViewModel<T> sourceObject = (HeliosViewModel<T>) d;
            sourceObject.Data.OnPropertyChanged(
                e.Property.Name,
                new PropertyNotificationEventArgs(sourceObject, e.Property.Name, e.OldValue, e.NewValue));
        }

        #region Properties

        public T Data { get; }

        #endregion
    }

    /// <summary>
    /// a view model class using dependency properties, referencing a model class that
    /// is not a notifaction object and is assumed not to change
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class HeliosStaticViewModel<T> : DependencyObject where T : class
    {
        protected HeliosStaticViewModel(T data)
        {
            Data = data;
        }

        #region Properties

        public T Data { get; }

        #endregion
    }
}