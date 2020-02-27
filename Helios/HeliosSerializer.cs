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
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Windows.Threading;
    using System.Xml;

    public class HeliosSerializer : BaseDeserializer
    {
        private HeliosBindingCollection _skipbindings = new HeliosBindingCollection();
        public HeliosSerializer(Dispatcher dispatcher)
            : base(dispatcher)
        {
        }

        #region Monitors

        public void SerializeMonitor(Monitor monitor, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Monitor");
            monitor.WriteXml(xmlWriter);
            SerializeControls(monitor.Children, xmlWriter);
            xmlWriter.WriteEndElement();
        }

        public void SerializeMonitors(MonitorCollection monitors, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Monitors");
            foreach (Monitor display in monitors)
            {
                SerializeMonitor(display, xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        public Monitor DeserializeMonitor(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement("Monitor");
            Monitor display = (Monitor)CreateNewObject("Monitor", "");
            display.ReadXml(xmlReader);
            DeserializeControls(display.Children, xmlReader);
            xmlReader.ReadEndElement();

            return display;
        }

        public void DeserializeMonitors(MonitorCollection destination, XmlReader xmlReader)
        {
            int i = 1;
            if (xmlReader.Name.Equals("Monitors"))
            {
                if (!xmlReader.IsEmptyElement)
                {
                    xmlReader.ReadStartElement("Monitors");
                    while (xmlReader.NodeType != XmlNodeType.EndElement)
                    {
                        Monitor display = DeserializeMonitor(xmlReader);
                        if (display != null)
                        {
                            display.Name = "Monitor " + i++;
                            if (display != null)
                            {
                                destination.Add(display);
                            }
                        }
                    }
                    xmlReader.ReadEndElement();
                }
                else
                {
                    xmlReader.Read();
                }
            }
        }

        #endregion

        #region Controls

        public void SerializeControl(HeliosVisual control, XmlWriter xmlWriter)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));

            xmlWriter.WriteStartElement("Control");
            xmlWriter.WriteAttributeString("TypeIdentifier", control.TypeIdentifier);
            xmlWriter.WriteAttributeString("Name", control.Name);
            xmlWriter.WriteAttributeString("SnapTarget", boolConverter.ConvertToInvariantString(control.IsSnapTarget));
            xmlWriter.WriteAttributeString("Locked", boolConverter.ConvertToInvariantString(control.IsLocked));
            control.WriteXml(xmlWriter);
            if (control.PersistChildren)
            {
                SerializeControls(control.Children, xmlWriter);
            }
            else
            {
                xmlWriter.WriteStartElement("Children");
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();
        }

        public void SerializeControls(HeliosVisualCollection controls, XmlWriter xmlWriter)
        {   
            xmlWriter.WriteStartElement("Children");
            foreach (HeliosVisual control in controls)
            {
                SerializeControl(control, xmlWriter);
            }
            xmlWriter.WriteEndElement();  // Controls
        }

        public HeliosVisual DeserializeControl(XmlReader xmlReader)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));

            HeliosVisual control = (HeliosVisual)CreateNewObject("Visual", xmlReader.GetAttribute("TypeIdentifier"));
            if (control != null)
            {
                string name = xmlReader.GetAttribute("Name");
                if (xmlReader.GetAttribute("SnapTarget") != null)
                {
                    control.IsSnapTarget = (bool)boolConverter.ConvertFromInvariantString(xmlReader.GetAttribute("SnapTarget"));
                }
                if (xmlReader.GetAttribute("Locked") != null)
                {
                    control.IsLocked = (bool)boolConverter.ConvertFromInvariantString(xmlReader.GetAttribute("Locked"));
                }

                if (xmlReader.IsEmptyElement)
                {
                    xmlReader.Read();
                }
                else
                {
                    xmlReader.ReadStartElement("Control");
                    control.ReadXml(xmlReader);
                    DeserializeControls(control.Children, xmlReader);
                    xmlReader.ReadEndElement();
                }
                control.Name = name;
            }
            else
            {
                xmlReader.Skip();
            }

            return control;
        }

        public void DeserializeControls(HeliosVisualCollection controls, XmlReader xmlReader)
        {
            if (!xmlReader.IsEmptyElement)
            {
                xmlReader.ReadStartElement("Children");
                while (xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    HeliosVisual control = DeserializeControl(xmlReader);
                    if (control != null)
                    {
                        controls.Add(control);
                    }
                }
                xmlReader.ReadEndElement();
            }
            else
            {
                xmlReader.Read();
            }
        }

        #endregion

        #region Bindings

        public void SerializeBinding(HeliosBinding binding, XmlWriter xmlWriter)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));

            if (binding.Action != null && binding.Trigger != null)
            {

                xmlWriter.WriteStartElement("Binding");

                xmlWriter.WriteAttributeString("BypassCascadingTriggers", boolConverter.ConvertToInvariantString(binding.BypassCascadingTriggers));

                xmlWriter.WriteStartElement("Trigger");
                xmlWriter.WriteAttributeString("Source", GetReferenceName(binding.Trigger.Source));
                xmlWriter.WriteAttributeString("Name", binding.Trigger.TriggerID);
                xmlWriter.WriteEndElement();

                xmlWriter.WriteStartElement("Action");
                xmlWriter.WriteAttributeString("Target", GetReferenceName(binding.Action.Target));
                xmlWriter.WriteAttributeString("Name", binding.Action.ActionID);
                xmlWriter.WriteEndElement();

                switch (binding.ValueSource)
                {
                    case BindingValueSources.StaticValue:
                        xmlWriter.WriteElementString("StaticValue", binding.Value);
                        break;
                    case BindingValueSources.TriggerValue:
                        xmlWriter.WriteStartElement("TriggerValue");
                        xmlWriter.WriteEndElement();
                        break;
                    case BindingValueSources.LuaScript:
                        xmlWriter.WriteElementString("LuaScript", binding.Value);
                        break;
                }

                if (binding.HasCondition)
                {
                    xmlWriter.WriteElementString("Condition", binding.Condition);
                }

                xmlWriter.WriteEndElement(); // Binding
            }
            else
            {
                ConfigManager.LogManager.LogWarning("Dangling bindings found during save.");
            }
        }

        public void SerializeBindings(HeliosBindingCollection bindings, XmlWriter xmlWriter)
        {
            foreach (HeliosBinding binding in bindings)
            {
                //SerializeBinding(binding, xmlWriter);
                //_skipbindings.Add(binding);
                if (!_skipbindings.Contains(binding))
                {
                    SerializeBinding(binding, xmlWriter);
                    _skipbindings.Add(binding);
                }

            }
        }

        public void SerializeBindings(HeliosBindingCollection bindings, XmlWriter xmlWriter, HeliosBindingCollection skipBindings)
        {
            foreach (HeliosBinding binding in bindings)
            {
                if (!skipBindings.Contains(binding))
                {
                    SerializeBinding(binding, xmlWriter);
                    skipBindings.Add(binding);
                }
            }
        }

        public void SerializeBindings(HeliosVisual visual, XmlWriter xmlWriter)
        {
            SerializeBindings(visual.OutputBindings, xmlWriter);
            foreach (HeliosVisual control in visual.Children)
            {
                SerializeBindings(control, xmlWriter);
            }
        }

        private HeliosBinding DeserializeBinding(HeliosProfile profile, HeliosVisual root, string copyRoot, List<HeliosVisual> localObjects, XmlReader xmlReader)
        {
            TypeConverter boolConverter = TypeDescriptor.GetConverter(typeof(bool));

            HeliosBinding binding = (HeliosBinding)CreateNewObject("Binding", "");
            binding.BypassCascadingTriggers = (bool)boolConverter.ConvertFromString(null, System.Globalization.CultureInfo.InvariantCulture, xmlReader.GetAttribute("BypassCascadingTriggers"));
            xmlReader.ReadStartElement("Binding");

            HeliosObject source = ResolveReferenceName(profile, root, copyRoot, localObjects, xmlReader.GetAttribute("Source"));
            if (source != null)
            {
                string trigger = xmlReader.GetAttribute("Name");
                if (source.Triggers.ContainsKey(trigger))
                {
                    binding.Trigger = source.Triggers[trigger];
                }
                else if (source is HeliosVisual)
                {
                    HeliosVisual parent = ((HeliosVisual)source).Parent;
                    if (parent.Triggers.ContainsKey(trigger))
                    {
                        source = parent;
                        binding.Trigger = source.Triggers[trigger];
                    }
                }
            } else
            {
                ConfigManager.LogManager.LogError("Binding Source Reference Unresolved: " + xmlReader.GetAttribute("Source"));
            }
            xmlReader.Read();

            HeliosObject target = ResolveReferenceName(profile, root, copyRoot, localObjects, xmlReader.GetAttribute("Target"));
            if (target != null)
            {
                string action = xmlReader.GetAttribute("Name");
                if (target.Actions.ContainsKey(action))
                {
                    binding.Action = target.Actions[action];
                }
                else if (target is HeliosVisual)
                {
                    HeliosVisual parent = ((HeliosVisual)target).Parent;
                    if (parent.Actions.ContainsKey(action))
                    {
                        target = parent;
                        binding.Action = target.Actions[action];
                    }
                }
            }
            else
            {
                ConfigManager.LogManager.LogError("Binding Target Reference Unresolved: " + xmlReader.GetAttribute("Target"));
            }
            xmlReader.Read();
            switch (xmlReader.Name)
            {
                case "StaticValue":
                    binding.ValueSource = BindingValueSources.StaticValue;
                    binding.Value = xmlReader.ReadElementString("StaticValue");
                    break;
                case "TriggerValue":
                    binding.ValueSource = BindingValueSources.TriggerValue;
                    xmlReader.Read();
                    break;
                case "LuaScript":
                    binding.ValueSource = BindingValueSources.LuaScript;
                    binding.Value = xmlReader.ReadElementString("LuaScript");
                    break;
            }

            if (xmlReader.Name.Equals("Condition"))
            {
                binding.Condition = xmlReader.ReadElementString("Condition");
            }

            xmlReader.ReadEndElement();

            return binding;
        }

        private HeliosBindingCollection DeserializeBindings(HeliosProfile profile, HeliosVisual root, string copyRoot, List<HeliosVisual> localObjects, XmlReader xmlReader)
        {
            HeliosBindingCollection bindings = new HeliosBindingCollection();

            if (!xmlReader.IsEmptyElement)
            {
                xmlReader.ReadStartElement("Bindings");
                while (xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    HeliosBinding binding = DeserializeBinding(profile, root, copyRoot, localObjects, xmlReader);
                    if (binding != null && binding.Action != null && binding.Trigger != null)
                    {
                        bindings.Add(binding);
                    }
                }
                xmlReader.ReadEndElement();
            }
            else
            {
                xmlReader.Read();
            }

            return bindings;
        }

        public HeliosBindingCollection DeserializeBindings(HeliosProfile profile, List<HeliosVisual> localObjects, XmlReader xmlReader)
        {
            return DeserializeBindings(profile, null, null, localObjects, xmlReader);
        }

        public HeliosBindingCollection DeserializeBindings(HeliosVisual root, string copyRoot, List<HeliosVisual> localObjects, XmlReader xmlReader)
        {
            return DeserializeBindings(root.Profile, root, copyRoot, localObjects, xmlReader);
        }

        #endregion

        #region References

        private static List<HeliosVisual> EMPTYLOCALS = new List<HeliosVisual>();

        public static HeliosObject ResolveReferenceName(HeliosProfile profile, string reference)
        {
            return ResolveReferenceName(profile, null, null, EMPTYLOCALS, reference);
        }

        public static HeliosObject ResolveReferenceName(HeliosVisual root, string copyRoot, string reference)
        {
            return ResolveReferenceName(root.Profile, root, copyRoot, EMPTYLOCALS, reference);
        }

        private static HeliosObject ResolveReferenceName(HeliosProfile profile, HeliosVisual root, string copyRoot, List<HeliosVisual> localObjects, string reference)
        {
            string[] components;
            if (reference.StartsWith("{"))
            {
                components = reference.Substring(1, reference.Length - 2).Split(';');
            }
            else
            {
                components = reference.Split(';');
            }
            string refType = components[0];
            string path = components[1];
            string typeId = components[2];
            string name = components[3];

            switch (refType)
            {
                case "Visual":

                    HeliosVisual visual = null;
                    if (!string.IsNullOrWhiteSpace(copyRoot) && !path.Equals(copyRoot, StringComparison.InvariantCultureIgnoreCase) && path.StartsWith(copyRoot, StringComparison.InvariantCultureIgnoreCase))
                    {
                        visual = GetVisualByPath(localObjects, path.Substring(copyRoot.Length + 1));
                    }

                    if (visual == null)
                    {
                        if (root == null)
                        {
                            visual = GetVisualByPath(profile, path);
                        }
                        else
                        {
                            visual = GetVisualByPath(root, path);
                        }
                    }
                    return visual;

                case "Interface":
                    if (profile.Interfaces.ContainsKey(name))
                    {
                        return profile.Interfaces[name];
                    }
                    break;
            }

            return null;
        }

        private static HeliosVisual GetVisualByPath(HeliosProfile profile, string path)
        {
            HeliosVisual visual = null;
            foreach (HeliosVisual monitor in profile.Monitors)
            {
                visual = GetVisualByPath(monitor, path);
                if (visual != null) break;
            }
            return visual;
        }

        private static HeliosVisual GetVisualByPath(HeliosVisual visual, string path)
        {
            return GetVisualByPath(visual, new Queue<string>(path.Split('.')));
        }

        private static HeliosVisual GetVisualByPath(HeliosVisual visual, Queue<string> nameQueue)
        {
            HeliosVisual returnValue = null;

            string name = nameQueue.Dequeue();
            if (visual.Name.Equals(name))
            {
                if (nameQueue.Count == 0)
                {
                    returnValue = visual;
                }
                else
                {
                    string childName = nameQueue.Peek();
                    foreach (HeliosVisual child in visual.Children)
                    {
                        if (childName.Equals(child.Name))
                        {
                            returnValue = GetVisualByPath(child, nameQueue);
                            if (returnValue != null)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return returnValue;
        }

        private static HeliosVisual GetVisualByPath(List<HeliosVisual> visuals, string path)
        {
            HeliosVisual returnVisual = null;
            foreach (HeliosVisual child in visuals)
            {
                returnVisual = GetVisualByPath(child, path);
                if (returnVisual != null) break;
            }
            return returnVisual;
        }

        private static bool ComparePaths(string path1, string path2)
        {
            List<string> path1Components = new List<string>(path1.Split('.'));
            List<string> path2Components = new List<string>(path2.Split('.'));

            path1Components.Reverse();
            path2Components.Reverse();

            for (int i = 0; i < path1Components.Count && i < path2Components.Count; i++)
            {
                if (!path1Components[i].Equals(path2Components[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        public static string GetReferenceName(HeliosObject refObject)
        {
            StringBuilder sb = new StringBuilder("");

            HeliosInterface refInterface = refObject as HeliosInterface;
            if (refInterface != null)
            {
                sb.Append("Interface;;");
            }

            HeliosVisual refControl = refObject as HeliosVisual;
            if (refControl != null)
            {
                sb.Append("Visual;");
                sb.Append(GetVisualPath(refControl));
                sb.Append(";");
            }

            sb.Append(refObject.TypeIdentifier);
            sb.Append(";");
            sb.Append(refObject.Name);
            sb.Append("");

            return sb.ToString();
        }

        public static string GetVisualPath(HeliosVisual visual)
        {
            List<string> names = new List<string>();
            HeliosVisual pathItem = visual;
            while (pathItem != null)
            {
                names.Add(pathItem.Name);
                pathItem = pathItem.Parent;
            }
            names.Reverse();
            return string.Join(".", names);
        }

        #endregion

        #region Interfaces

        public void SerializeInterface(HeliosInterface heliosInterface, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Interface");
            xmlWriter.WriteAttributeString("TypeIdentifier", heliosInterface.TypeIdentifier);
            xmlWriter.WriteAttributeString("Name", heliosInterface.Name);
            heliosInterface.WriteXml(xmlWriter);
            xmlWriter.WriteEndElement(); // Interface
        }

        public HeliosInterface DeserializeInterface(XmlReader xmlReader)
        {
            string interfaceType = xmlReader.GetAttribute("TypeIdentifier");
            HeliosInterface heliosInterface = (HeliosInterface)CreateNewObject("Interface", interfaceType);
            if (heliosInterface != null)
            {
                String name = xmlReader.GetAttribute("Name");
                if (xmlReader.IsEmptyElement)
                {
                    xmlReader.Read();
                }
                else
                {
                    xmlReader.ReadStartElement("Interface");
                    heliosInterface.ReadXml(xmlReader);
                    xmlReader.ReadEndElement();
                }
                heliosInterface.Name = name;
            }
            else
            {
                xmlReader.Skip();
            }

            return heliosInterface;
        }

        public void SerializeInterfaces(HeliosInterfaceCollection interfaces, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Interfaces");
            foreach (HeliosInterface heliosInterface in interfaces)
            {
                SerializeInterface(heliosInterface, xmlWriter);
            }
            xmlWriter.WriteEndElement(); // Interfaces
        }

        public void DeserializeInterfaces(HeliosInterfaceCollection destination, XmlReader xmlReader)
        {
            if (!xmlReader.IsEmptyElement)
            {
                xmlReader.ReadStartElement("Interfaces");
                while (xmlReader.NodeType != XmlNodeType.EndElement)
                {
                    HeliosInterface heliosInterface = DeserializeInterface(xmlReader);
                    if (heliosInterface != null)
                    {
                        destination.Add(heliosInterface);
                    }
                }
                xmlReader.ReadEndElement();
            }
            else
            {
                xmlReader.Read();
            }
        }

        #endregion

        #region Profiles

        public void SerializeProfile(HeliosProfile profile, XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("HeliosProfile");
            xmlWriter.WriteElementString("Version", "3");

            SerializeMonitors(profile.Monitors, xmlWriter);

            SerializeInterfaces(profile.Interfaces, xmlWriter);

            xmlWriter.WriteStartElement("Bindings");
            foreach (HeliosInterface heliosInterface in profile.Interfaces)
            {
                SerializeBindings(heliosInterface.OutputBindings, xmlWriter);
            }

            foreach (HeliosVisual visual in profile.Monitors)
            {
                SerializeBindings(visual, xmlWriter);
            }

            xmlWriter.WriteEndElement();  // Bindings

            xmlWriter.WriteEndElement(); // HeliosProfile
        }

        public int GetProfileVersion(XmlReader xmlReader)
        {
            xmlReader.ReadStartElement("HeliosProfile");
            if (xmlReader.Name.Equals("Version"))
            {
                string version = xmlReader.ReadElementString("Version");
                if (version.Contains('.'))
                {
                    return 3;
                }
                return int.Parse(version, CultureInfo.InvariantCulture);
            }

            return 0;
        }

        public void DeserializeProfile(HeliosProfile profile, XmlReader xmlReader)
        {

            profile.Monitors.Clear();
            DeserializeMonitors(profile.Monitors, xmlReader);

            profile.Interfaces.Clear();
            DeserializeInterfaces(profile.Interfaces, xmlReader);

            foreach (HeliosBinding binding in DeserializeBindings(profile, new List<HeliosVisual>(), xmlReader))
            {
                binding.Trigger.Source.OutputBindings.Add(binding);
                binding.Action.Target.InputBindings.Add(binding);
            }

            xmlReader.ReadEndElement();
        }

        #endregion
    }
}
