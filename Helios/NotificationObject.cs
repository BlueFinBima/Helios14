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

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.ComponentModel;

    using GadrocsWorkshop.Helios.ComponentModel;

    public class NotificationObject : IPropertyNotification
    {
        #region INotifyPropertyChanged Members

        /// <summary>
        /// WARNING:  This is an event that is declared to have the correct signature for
        /// IPropertyNotification, but we actually pass our own extended PropertyNotificationEventArgs
        /// argument instead, to tunnel the old and new values.  This means any all of our handlers
        /// assume this event is only raised from our classes and cast this event DOWN to our derived
        /// event.
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }

        public void OnPropertyChanged(string childPropertyName, PropertyNotificationEventArgs args)
        {
            OnPropertyChanged(new PropertyNotificationEventArgs(this, childPropertyName, args));
        }

        protected void OnPropertyChanged(string propertyName, object oldValue, object newValue, bool undoable)
        {
            OnPropertyChanged(new PropertyNotificationEventArgs(this, propertyName, oldValue, newValue, undoable));
        }

        #endregion
    }
}
