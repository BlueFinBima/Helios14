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

using System.Collections.Generic;
using System.Windows;
using System.Xml;
using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Util;
using NLog;

namespace GadrocsWorkshop.Helios.Interfaces.Unsupported
{
    /// <summary>
    /// This interface class is used when an interface in the profile is unsupported but it is marked as to be
    /// ignored.  We create one of these interfaces instead to catch all the bindings and preserve the configuration XML.
    /// </summary>
    [HeliosInterface(TYPE_IDENTIFIER, "Unsupported Interface", typeof(UnsupportedInterfaceEditor),
        typeof(UnsupportedInterfaceFactory))]
    internal class UnsupportedInterface : HeliosInterface, IDynamicBindings, IExtendedDescription, IStatusReportNotify
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private XmlDocument _content;

        public const string TYPE_IDENTIFIER = "Helios.Base.UnsupportedInterface";

        public string RepresentedTypeIdentifier
        {
            set => _typeIdentifier = value;
        }

        public UnsupportedInterface() : base("Unsupported Interface")
        {
        }

        public override void ReadXml(XmlReader reader)
        {
            _content = new XmlDocument();
            _content.Load(reader);
        }

        public override void WriteXml(XmlWriter writer)
        {
            _content.WriteContentTo(writer);
        }

        public IBindingTrigger ResolveTrigger(string triggerId)
        {
            string[] components = triggerId.Split('.');
            string deviceName = components.Length > 2 ? components[0] : "";
            int deviceSegmentLength = components.Length > 2 ? deviceName.Length + 1 : 0;
            string verb = components[components.Length - 1];
            int consumedLength = deviceSegmentLength + 1 + verb.Length;
            string rest = consumedLength < triggerId.Length
                ? triggerId.Substring(deviceSegmentLength, triggerId.Length - consumedLength)
                : "";
            HeliosTrigger trigger = new HeliosTrigger(this, deviceName, rest, verb,
                "A dummy trigger that never fires its event");

            string key = Triggers.GetKeyForItem(trigger);
            if (Triggers.ContainsKey(key))
            {
                // matching trigger already there
                return Triggers[key];
            }
            Triggers.Add(trigger);
            return trigger;
        }

        public IBindingAction ResolveAction(string actionId)
        {
            // NOTE: action ID generation is not really unique, so we assume a format of either
            //    device.verb.rest including whatever
            //    verb.rest not including periods
            //
            string[] components = Tokenizer.TokenizeAtLeast(actionId, 2, '.');
            string deviceName = components.Length > 2 ? components[0] : "";
            int deviceSegmentLength = components.Length > 2 ? deviceName.Length + 1 : 0;
            string verb = components[1];
            int consumedLength = deviceSegmentLength + 1 + verb.Length;
            string rest = consumedLength < actionId.Length ? actionId.Substring(consumedLength) : "";
            HeliosAction action = new HeliosAction(this, deviceName, rest, verb, "A dummy action that ignores events");

            string key = Actions.GetKeyForItem(action);
            if (Actions.ContainsKey(key))
            {
                // matching action already there
                return Actions[key];
            }
            Actions.Add(action);
            return action;
        }

        public string Description =>
            $"Unsupported interface of type '{TypeIdentifier}' that is present in the profile but won't do anything.";

        public string RemovalNarrative =>
            "Removing this interface will remove all its bindings.  It is safe to keep this here for use on Helios installations that do support this interface.";

        public void Subscribe(IStatusReportObserver observer)
        {
            observer.ReceiveStatusReport($"Unsupported interface of type '{TypeIdentifier}'", Description,
                new List<StatusReportItem>
                {
                    new StatusReportItem
                    {
                        Status =
                            "This interface type is not supported in this installation of Helios.  You may be missing a plugin or a later version of Helios.",
                        Recommendation =
                            "You can safely ignore this interface and keep it here for Helios installations that do support it",
                        Severity = StatusReportItem.SeverityCode.Info,
                        Flags = StatusReportItem.StatusFlags.ConfigurationUpToDate
                    }
                });
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            // no code
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            // no code
        }

        public void InvalidateStatusReport()
        {
            // no code
        }
    }
}