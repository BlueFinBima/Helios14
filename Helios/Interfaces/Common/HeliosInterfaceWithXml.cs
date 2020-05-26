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

using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    /// <summary>
    /// A Helios Interface implementation that has a model instance that is serialized to the XML
    /// under the interface node in the Profile
    ///
    /// The model class should encapsulate all the configuration of this interface.  Undo support
    /// is provided for all properties in the instance "Model"
    /// </summary>
    /// <typeparam name="T">a default-constructable descendant of HeliosXmlModel</typeparam>
    public class HeliosInterfaceWithXml<T> : HeliosInterface where T : HeliosXmlModel, new()
    {
        public HeliosInterfaceWithXml(string name) : base(name)
        {
            // no code in base
        }

        public T Model { get; protected set; } = new T();

        protected override void OnPropertyChanged(PropertyNotificationEventArgs args)
        {
            if (args.PropertyName == nameof(Model))
            {
                if (args.OldValue != null)
                {
                    ((T) args.OldValue).PropertyChanged -= Configuration_PropertyChanged;
                }

                if (args.NewValue != null)
                {
                    // register to forward any configuration value changes after load
                    ((T)args.NewValue).PropertyChanged += Configuration_PropertyChanged;
                }
            }
            base.OnPropertyChanged(args);
        }

        private void Configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // forward changes to configuration properties as child property events so we get Undo support
            OnPropertyChanged("Configuration", (PropertyNotificationEventArgs)e);
        }

        public override void ReadXml(XmlReader reader)
        {
            Model = HeliosXmlModel.ReadXml<T>(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            HeliosXmlModel.WriteXml(writer, Model);
        }
    }
}