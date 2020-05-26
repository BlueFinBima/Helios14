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

    using GadrocsWorkshop.Helios;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class HeliosInterfaceAttribute : Attribute
    {
        private Type _interfaceEditorType;
        private string _typeIdentifier;
        private string _name;
        private bool _autoAdd;
        private Type _factory;

        /// <param name="typeIdentifier">Unique identifier used for persistance.
        /// Recommended to follow conventions of {module name}.{interface}.  Helios.* is reserved for helios's included controls.</param>
        /// <param name="name">Display name used for this interface in the ui.</param>
        /// <param name="interfaceEditor">Instance factory for interface editor dialog.</param>
        /// 
        public HeliosInterfaceAttribute(string typeIdentifier, string name, Type interfaceEditor) : this(typeIdentifier, name, interfaceEditor, typeof(HeliosInterfaceFactory))
        {
        }

        /// <param name="typeIdentifier">Unique identifier used for persistance.
        /// Recommended to follow conventions of {module name}.{interface}.  Helios.* is reserved for helios's included controls.
        /// This value must never change once an interface is shipped.</param>
        /// <param name="name">Default instance name for this interface in the UI and Profile.  This value may be changed in future versions of the interface.</param>
        /// <param name="interfaceEditor">Instance factory for interface editor dialog.</param>
        /// <param name="factory">Instance factory used to populate "Add Interface" dialog with available interfaces.</param>
        /// 
        public HeliosInterfaceAttribute(string typeIdentifier, string name, Type interfaceEditor, Type factory) 
        {
            _typeIdentifier = typeIdentifier;
            _interfaceEditorType = interfaceEditor;
            _name = name;
            _factory = factory;
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

        public Type InterfaceEditorType
        {
            get
            {
                return _interfaceEditorType;
            }
        }

        /// <summary>
        /// If true an isntance of this control will automatically be added to a new profile.
        /// </summary>
        public bool AutoAdd
        {
            get
            {
                return _autoAdd;
            }
            set
            {
                _autoAdd = value;
            }
        }

        public Type Factory
        {
            get
            {
                return _factory;
            }
            set
            {
                _factory = value;
            }
        }
    }
}
