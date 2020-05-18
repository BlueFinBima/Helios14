// Copyright 2020 Helios Contributors
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
using System.IO;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xaml;
using System.Xml;
using NLog;

namespace GadrocsWorkshop.Helios.Util
{
    public class XamlFirewall
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public class DisallowedElementException : Exception
        {
            public DisallowedElementException(string xamlNamespace, string xamlElementName) : base(
                $"disallowed instantiation of '{xamlNamespace}' '{xamlElementName}' from XAML")
            {
                Namespace = xamlNamespace;
                ElementName = xamlElementName;
            }

            public string Namespace { get; }
            public string ElementName { get; }
        }

        /// <summary>
        /// the reference assemblies provided to the XAML reader instead of our ApplicationDomain's assemblies
        /// </summary>
        private readonly Assembly[] _referenceAssemblies =
        {
            // these are two separate assemblies which contain all the types we allow
            Assembly.GetAssembly(typeof(Canvas)),
            Assembly.GetAssembly(typeof(TransformGroup))
        };

        /// <summary>
        /// the XAML schema context settings we use; currently nothing enabled
        /// </summary>
        private readonly XamlSchemaContextSettings _settings = new XamlSchemaContextSettings();

        /// <summary>
        /// our special schema context that implements the firewalling
        /// </summary>
        private readonly XamlSchemaContext _schemaContext;

        /// <summary>
        /// invariant thread-safe instance of XAML firewall
        /// </summary>
        public XamlFirewall()
        {
            _schemaContext = new SchemaContext(_referenceAssemblies, _settings);
        }

        /// <summary>
        /// load a XAML file that may only contain drawing primitives in a whitelist maintained by this class
        /// </summary>
        /// <typeparam name="T">the expected type of the root element of the XAML</typeparam>
        /// <param name="xaml">an input stream for the XAML</param>
        /// <exception cref="DisallowedElementException">for the first XAML element that is not in the whitelist</exception>
        /// <returns>the root element of the loaded XAML</returns>
        public T LoadXamlDefensively<T>(Stream xaml)
        {
            using (XmlReader xmlReader = new XmlTextReader(xaml))
            {
                XamlReader reader = new XamlXmlReader(xmlReader, _schemaContext);
                return (T) System.Windows.Markup.XamlReader.Load(reader);
            }
        }

        /// <summary>
        /// load a XAML file that may only contain drawing primitives in a whitelist maintained by this class
        /// </summary>
        /// <typeparam name="T">the expected type of the root element of the XAML</typeparam>
        /// <param name="xamlFilePath">full path to a XAML in the filesystem</param>
        /// <exception cref="DisallowedElementException">for the first XAML element that is not in the whitelist</exception>
        /// <returns>the root element of the loaded XAML</returns>
        public T LoadXamlDefensively<T>(string xamlFilePath)
        {
            using (Stream inputStream = new FileStream(xamlFilePath, FileMode.Open))
            {
                return LoadXamlDefensively<T>(inputStream);
            }
        }

        /// <summary>
        /// special schema context that only allows white listed XAML elements
        /// </summary>
        private class SchemaContext : XamlSchemaContext
        {
            // map from XAML element name to required namespace (currently always the same)
            private static readonly Dictionary<string, string> AllowedTypes = new Dictionary<string, string>();

            static SchemaContext()
            {
                // questionable: <Image> is used in some drawing XAML, should review it
                foreach (string name in new[]
                {
                    "Canvas", "Compound", "Ellipse", "GradientStop", "GradientStopCollection", "Group", "Line",
                    "LinearGradientBrush", "MatrixTransform", "Path", "PathGeometry", "Polygon",
                    "RadialGradientBrush", "Rectangle", "RotateTransform", "ScaleTransform", "SkewTransform",
                    "TextBlock",
                    "TransformGroup", "TranslateTransform"
                })
                {
                    AllowedTypes[name] = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                }
            }

            public SchemaContext(IEnumerable<Assembly> referenceAssemblies, XamlSchemaContextSettings settings) : base(
                referenceAssemblies, settings)
            {
                // no code
            }

            /// <summary>
            /// dereference a XAML tag and namespace to a XAML type
            /// </summary>
            /// <param name="xamlNamespace"></param>
            /// <param name="name"></param>
            /// <param name="typeArguments"></param>
            /// <exception cref="XamlFirewall.DisallowedElementException">for any XAML element that is not in the whitelist</exception>
            /// <returns></returns>
            protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
            {
                Logger.Debug($"GetXamlType {xamlNamespace} {name} {typeArguments}");
                if (!AllowedTypes.TryGetValue(name, out string requiredNamespace) ||
                    xamlNamespace != requiredNamespace ||
                    typeArguments != null)
                {
                    throw new DisallowedElementException(xamlNamespace, name);
                }

                return base.GetXamlType(xamlNamespace, name, null);
            }

            public override ICollection<XamlType> GetAllXamlTypes(string xamlNamespace)
            {
                Logger.Debug($"GetAllXamlTypes {xamlNamespace}");
                return base.GetAllXamlTypes(xamlNamespace);
            }

            public override IEnumerable<string> GetAllXamlNamespaces()
            {
                Logger.Debug("GetAllXamlNamespaces");
                return base.GetAllXamlNamespaces();
            }

            public override string GetPreferredPrefix(string xmlns)
            {
                Logger.Debug($"GetPreferredPrefix {xmlns}");
                return base.GetPreferredPrefix(xmlns);
            }

            public override XamlDirective GetXamlDirective(string xamlNamespace, string name)
            {
                Logger.Debug($"GetXamlDirective {xamlNamespace} {name}");
                return base.GetXamlDirective(xamlNamespace, name);
            }

            public override bool TryGetCompatibleXamlNamespace(string xamlNamespace, out string compatibleNamespace)
            {
                Logger.Debug($"TryGetCompatibleXamlNamespace {xamlNamespace}");
                return base.TryGetCompatibleXamlNamespace(xamlNamespace, out compatibleNamespace);
            }

            public override XamlType GetXamlType(Type type)
            {
                Logger.Debug($"GetXamlType {type}");
                return base.GetXamlType(type);
            }

            protected override Assembly OnAssemblyResolve(string assemblyName)
            {
                Logger.Debug($"OnAssemblyResolve {assemblyName}");
                return base.OnAssemblyResolve(assemblyName);
            }
        }
    }
}