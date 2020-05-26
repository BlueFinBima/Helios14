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

namespace GadrocsWorkshop.Helios.ComponentModel
{
    using System;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class HeliosControlAttribute : Attribute
    {
        private readonly string _typeIdentifier;
        private readonly string _name;
        private readonly string _category;
        private readonly Type _renderer;

        private string _requires;

        /// <param name="typeIdentifier">Unique identifier used for persistance.
        /// Recommended to follow conventions of {module name}.{interface}.  Helios.* is reserved for helios's included controls.</param>
        /// <param name="name">Display name used for this control in the ui.</param>
        public HeliosControlAttribute(string typeIdentifier, string name, string category, Type renderer)
        {
            _typeIdentifier = typeIdentifier;
            _name = name;
            _category = category;
            _renderer = renderer;
        }

        public string TypeIdentifier
        {
            get
            {
                return _typeIdentifier;
            }
        }

        public string Name
        {
            get
            {
                return _name;
            }
        }

        public string Category
        {
            get
            {
                return _category;
            }
        }

        public Type Renderer
        {
            get
            {
                return _renderer;
            }
        }

        public string Requires
        {
            get
            {
                return _requires;
            }
            set
            {
                _requires = value;
            }
        }
    }
}
