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
    using System.Collections;
    using System.Collections.Generic;

    public class HeliosInterfaceDescriptorCollection : ICollection<HeliosInterfaceDescriptor>
    {
        List<HeliosInterfaceDescriptor> _controlDescriptors = new List<HeliosInterfaceDescriptor>();
        Dictionary<string, HeliosInterfaceDescriptor> _typeIdentifiers = new Dictionary<string, HeliosInterfaceDescriptor>();
        Dictionary<Type, HeliosInterfaceDescriptor> _types = new Dictionary<Type, HeliosInterfaceDescriptor>();

        public HeliosInterfaceDescriptor this[string typeIdentifier]
        {
            get
            {
                if (typeIdentifier != null && _typeIdentifiers.ContainsKey(typeIdentifier))
                {
                    return _typeIdentifiers[typeIdentifier];
                }
                return null;
            }
        }

        public HeliosInterfaceDescriptor this[Type type]
        {
            get
            {
                if (_types.ContainsKey(type))
                {
                    return _types[type];
                }
                return null;
            }
        }

        internal void AddAlias(string additionalTypeId, HeliosInterfaceDescriptor item)
        {
            _typeIdentifiers.Add(additionalTypeId, item);
        }

        #region ICollection<HeliosInterfaceDescriptor> Members

        public void Add(HeliosInterfaceDescriptor item)
        {
            _controlDescriptors.Add(item);
            _typeIdentifiers.Add(item.TypeIdentifier, item);
            _types.Add(item.InterfaceType, item);
        }

        public void Clear()
        {
            _controlDescriptors.Clear();
            _typeIdentifiers.Clear();
            _types.Clear();
        }

        public bool Contains(HeliosInterfaceDescriptor item)
        {
            return _controlDescriptors.Contains(item);
        }

        public void CopyTo(HeliosInterfaceDescriptor[] array, int arrayIndex)
        {
            _controlDescriptors.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get 
            {
                return _controlDescriptors.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public bool Remove(HeliosInterfaceDescriptor item)
        {
            _typeIdentifiers.Remove(item.TypeIdentifier);
            _types.Remove(item.InterfaceType);
            return _controlDescriptors.Remove(item);
        }

        #endregion

        #region IEnumerable<HeliosInterfaceDescriptor> Members

        public IEnumerator<HeliosInterfaceDescriptor> GetEnumerator()
        {
            return _controlDescriptors.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _controlDescriptors.GetEnumerator();
        }

        #endregion
    }
}
