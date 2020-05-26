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

namespace GadrocsWorkshop.Helios.Controls
{

    public class RotarySwitchPosition : NotificationObject
    {
        private int _index;
        private string _name;
        private double _rotation;
        private HeliosTrigger _enterTrigger;
        private HeliosTrigger _exitTrigger;

        public RotarySwitchPosition(HeliosObject rotarySwitch, int index, string name, double rotation)
        {
            _index = index;
            _name = name;
            _rotation = rotation;

            _enterTrigger = new HeliosTrigger(rotarySwitch, "", "position " + _index, "entered", "Fires when switch enters the " + _name + " position");
            _exitTrigger = new HeliosTrigger(rotarySwitch, "", "position " + _index, "exited", "Fires when switch exits the " + _name + " position");
        }

        #region Properties

        public int Index
        {
            get
            {
                return _index;
            }
            set
            {
                if (!_index.Equals(value))
                {
                    int oldValue = _index;
                    _index = value;
                    OnPropertyChanged("Index", oldValue, value, false);
                }
            }
        }


        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if ((_name == null && value != null)
                    || (_name != null && !_name.Equals(value)))
                {
                    string oldValue = _name;
                    _name = value;
                    _enterTrigger.Name = "position " + _index;
                    _exitTrigger.Name = "position " + _index;
                    _enterTrigger.TriggerDescription = "Fires when switch enters the " + _name + " position";
                    _exitTrigger.TriggerDescription = "Fires when switch exits the " + _name + " position";
                    OnPropertyChanged("Name", oldValue, value, true);
                }
            }
        }

        public double Rotation
        {
            get
            {
                return _rotation;
            }
            set
            {
                if (!_rotation.Equals(value))
                {
                    double oldValue = _rotation;
                    _rotation = value;
                    OnPropertyChanged("Rotation", oldValue, value, true);
                }
            }
        }

        public HeliosTrigger EnterTriggger
        {
            get { return _enterTrigger; }
        }

        public HeliosTrigger ExitTrigger
        {
            get { return _exitTrigger; }
        }

        #endregion
    }
}
