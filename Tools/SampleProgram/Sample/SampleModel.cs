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

using GadrocsWorkshop.Helios;

namespace net.derammo.Helios.SampleProgram.Sample
{
    /// <summary>
    /// Helios notification object supports IPropertyNotification and automatically creates undo
    /// items for any property changes if it is in the tree of Helios objects that are part of
    /// the profile.
    /// </summary>
    public partial class SampleModel : NotificationObject
    {
        #region Private

        /// <summary>
        /// backing field for property SomeData, contains
        /// some sort of string thingy that gets undo
        /// </summary>
        private string _someData = "some text";

        /// <summary>
        /// backing field for property SomeInteger, contains
        /// an example integer value with undo support
        /// </summary>
        private int _someInteger;

        /// <summary>
        /// backing field for property SomeOtherThing, contains
        /// yet another thing
        /// </summary>
        private object _someOtherThing = new object();

        #endregion

        public SampleModel()
        {
            // no code
        }

        #region Properties

        /// <summary>
        /// some sort of string thingy that gets undo
        /// </summary>
        public string SomeData
        {
            get => _someData;
            set
            {
                if (_someData != null && _someData == value)
                {
                    return;
                }

                string oldValue = _someData;
                _someData = value;
                OnPropertyChanged("SomeData", oldValue, value, true);
            }
        }

        /// <summary>
        /// an example integer value with undo support
        /// </summary>
        public int SomeInteger
        {
            get => _someInteger;
            set
            {
                if (_someInteger == value)
                {
                    return;
                }

                int oldValue = _someInteger;
                _someInteger = value;
                OnPropertyChanged("SomeInteger", oldValue, value, true);
            }
        }

        /// <summary>
        /// yet another thing
        /// </summary>
        public object SomeOtherThing
        {
            get => _someOtherThing;
            set
            {
                if (_someOtherThing != null && _someOtherThing == value)
                {
                    return;
                }

                object oldValue = _someOtherThing;
                _someOtherThing = value;
                OnPropertyChanged("SomeOtherThing", oldValue, value, true);
            }
        }

        #endregion
    }
}