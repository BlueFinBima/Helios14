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
using System.Xml;
using System.Xml.Serialization;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// A Helios Object that can be serialized to XML.  This can be used as a child of
    /// a Visual that that non-trivial configuration.
    ///
    /// Descendants of this class must attach the attribute
    /// [System.Xml.Serialization.XmlRoot("Configuration", Namespace = XML_NAMESPACE)]
    /// where "Configuration" is an example name for the XML Element that will contain this data
    /// and XML_NAMESPACE is the same value that is passed to the constructor of this class.
    /// 
    /// WARNING: It is required to register and handle the PropertyChange event of this class
    /// in the parent and forward it to implement Undo functionality.
    /// </summary>
    [Serializable]
    public class HeliosXmlModel: NotificationObject
    {
        public HeliosXmlModel(string xmlNamespace)
        {
            Namespaces = new XmlSerializerNamespaces(new[] {new XmlQualifiedName(string.Empty, xmlNamespace)});
        }

        /// <summary>
        /// declare namespaces so XmlSerializer doesn't add default ones
        /// </summary>
        [XmlNamespaceDeclarations]
        public XmlSerializerNamespaces Namespaces { get; }

        public static T ReadXml<T>(XmlReader reader) where T: HeliosXmlModel, new()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            T model = (T)serializer.Deserialize(reader) ?? new T();
            model.AfterDeserialization();
            return model;
        }

        public static void WriteXml<T>(XmlWriter writer, T model) where T : HeliosXmlModel
        {
            if (model == null)
            {
                return;
            }
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            model.BeforeSerialization();
            serializer.Serialize(writer, model, model.Namespaces);
        }

        /// <summary>
        /// override this to convert any non-serializable configuration into a serializable format
        /// </summary>
        protected virtual void BeforeSerialization()
        {
            // no code in base
        }

        /// <summary>
        /// override this to convert the serialized form of any non-serializable configuration back into its native format
        /// </summary>
        protected virtual void AfterDeserialization()
        {
            // no code in base
        }
    }
}