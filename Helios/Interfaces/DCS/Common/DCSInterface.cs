//  Copyright 2014 Craig Courtney
//  Copyright 2020 Helios Contributors
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;
using GadrocsWorkshop.Helios.Interfaces.Capabilities.ProfileAwareInterface;
using GadrocsWorkshop.Helios.UDPInterface;
using GadrocsWorkshop.Helios.Util;

namespace GadrocsWorkshop.Helios.Interfaces.DCS.Common
{
    public class DCSInterface : BaseUDPInterface, IProfileAwareInterface, IReadyCheck, IStatusReportNotify
    {
        private const string SETTINGS_GROUP = "DCSInterface";
        private const DCSExportModuleFormat DEFAULT_EXPORT_MODULE_FORMAT = DCSExportModuleFormat.HeliosDriver16;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// backing field for ExportModuleFormat
        /// </summary>
        private DCSExportModuleFormat _exportModuleFormat = DEFAULT_EXPORT_MODULE_FORMAT;

        // current state of the Export script, as far as we know, or null if we don't understand it at all
        protected DCSExportModuleFormat? _remoteModuleFormat;

        // phantom monitor fix 
        private DCSPhantomMonitorFix _phantomFix;

        // export configuration generation and checking
        protected DCSExportConfiguration _configuration;

        // vehicle impersonation configuration
        protected DCSVehicleImpersonation _vehicleImpersonation;

        // protocol to talk to DCS Export script (control messages)
        protected DCSExportProtocol _protocol;

        // observers of our status report that need to be told when we change status
        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        /// <summary>
        /// backing field for ImpersonatedVehicleName
        /// </summary>
        private string _impersonatedVehicleName;

        /// <summary>
        /// backing field for property ExportModuleText, contains
        /// the full text of an export module to be attached to the interface in the profile
        /// </summary>
        private string _exportModuleText;

        /// <summary>
        /// backing field for property ExportModuleBaseName, contains
        /// file base name (no path, no extension) for the ExportModuleText, because naming
        /// can differ across module formats
        /// </summary>
        private string _exportModuleBaseName;

        public DCSInterface(string name, string exportDeviceName, string exportFunctionsPath)
            : base(name)
        {
            VehicleName = exportDeviceName;
            ExportFunctionsPath = exportFunctionsPath;

            // make sure we keep our list up to date and don't typo on the name of an export device
            Debug.Assert(DCSVehicleImpersonation.KnownVehicles.Contains(exportDeviceName));

            // create handling for DCS export meta information we handle ourselves
            NetworkTriggerValue activeVehicle = new NetworkTriggerValue(this, "ACTIVE_VEHICLE", "ActiveVehicle",
                "Vehicle currently inhabited in DCS.", "Short name of vehicle");
            AddFunction(activeVehicle);
            activeVehicle.ValueReceived += ActiveVehicle_ValueReceived;
            NetworkTriggerValue activeDriver = new NetworkTriggerValue(this, "ACTIVE_DRIVER", "ActiveDriver",
                "Export driver running on DCS.", "Short name of driver type");
            AddFunction(activeDriver);
            activeDriver.ValueReceived += ActiveDriver_ValueReceived;
            AddFunction(new NetworkTrigger(this, "ALIVE", "Heartbeat",
                "Received periodically if there is no other data received"));
            NetworkTriggerValue alertMessage = new NetworkTriggerValue(this, "ALERT_MESSAGE", "AlertMessage",
                "Export driver running on DCS.", "Most recent alert message");
            AddFunction(alertMessage);
            alertMessage.ValueReceived += AlertMessage_ValueReceived; ;
        }

        #region Events

        #endregion

        #region Properties

        /// <summary>
        /// The vehicle (usually an aircraft) that DCS will report in LoGetSelfData when we are using this interface.
        /// </summary>
        public string VehicleName { get; }

        /// <summary>
        /// If not null, the this interface instance is configured to impersonate the specified vehicle name.  This means
        /// that Helios should select it for the given vehicle, instead of the one that the interface natively supports.
        /// </summary>
        public string ImpersonatedVehicleName
        {
            get => _impersonatedVehicleName;
            set
            {
                _impersonatedVehicleName = value;
                foreach (IStatusReportObserver observer in _observers)
                {
                    string newName = StatusName;
                    InvalidateStatusReport();
                }
            }
        }

        /// <summary>
        /// vehicle-specific file resource to include
        /// </summary>
        public string ExportFunctionsPath { get; }

        public DCSExportConfiguration Configuration => _configuration;

        public DCSVehicleImpersonation VehicleImpersonation => _vehicleImpersonation;

        public string StatusName =>
            ImpersonatedVehicleName != null ? $"{Name} impersonating {ImpersonatedVehicleName}" : Name;

        /// <summary>
        /// the configured format for the export module used by this interface
        /// </summary>
        public DCSExportModuleFormat ExportModuleFormat
        {
            get => _exportModuleFormat;
            set
            {
                if (_exportModuleFormat == value)
                {
                    return;
                }

                // batch change of format with erase of module text and name
                using (new HeliosUndoBatch())
                {
                    DCSExportModuleFormat oldValue = _exportModuleFormat;
                    _exportModuleFormat = value;
                    ExportModuleBaseName = null;
                    ExportModuleText = null;
                    OnPropertyChanged("ExportModuleFormat", oldValue, value, true);
                }
            }
        }

        /// <summary>
        /// the full text of an export module to be attached to the interface in the profile
        /// </summary>
        public string ExportModuleText
        {
            get => _exportModuleText;
            set
            {
                if (value != null)
                {
                    // normalize carriage returns so we can compare it later
                    // NOTE: XMLReader drops all the carriage returns in cdata
                    value = Regex.Replace(value, "\r\n|\n\r|\n|\r", "\r\n");
                }

                if (_exportModuleText == value)
                {
                    return;
                }

                string oldValue = _exportModuleText;
                _exportModuleText = value;
                OnPropertyChanged("ExportModuleText", oldValue, value, true);
            }
        }


        /// <summary>
        /// file base name (no path, no extension) for the ExportModuleText, because naming
        /// can differ across module formats
        /// </summary>
        public string ExportModuleBaseName
        {
            get => _exportModuleBaseName;
            set
            {
                if (_exportModuleBaseName != null && _exportModuleBaseName == value)
                {
                    return;
                }

                string oldValue = _exportModuleBaseName;
                _exportModuleBaseName = value;
                OnPropertyChanged("ExportModuleBaseName", oldValue, value, true);
            }
        }

        #endregion

        internal string LoadSetting(string key, string defaultValue)
        {
            if (ConfigManager.SettingsManager.IsSettingAvailable(SETTINGS_GROUP, key))
            {
                // get from shared location
                return ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, key, defaultValue);
            }

            // get from legacy location
            return ConfigManager.SettingsManager.LoadSetting(Name, key, defaultValue);
        }

        internal T LoadSetting<T>(string key, T defaultValue)
        {
            if (ConfigManager.SettingsManager.IsSettingAvailable(SETTINGS_GROUP, key))
            {
                // get from shared location, using LoadSetting<T>
                return ConfigManager.SettingsManager.LoadSetting(SETTINGS_GROUP, key, defaultValue);
            }

            // get from legacy location, using LoadSetting<T>
            return ConfigManager.SettingsManager.LoadSetting(Name, key, defaultValue);
        }

        internal void SaveSetting(string key, string value)
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, key, value);
        }

        internal void SaveSetting<T>(string key, T value)
        {
            ConfigManager.SettingsManager.SaveSetting(SETTINGS_GROUP, key, value);
        }

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            Profile.ProfileTick += Profile_Tick;
            _configuration = new DCSExportConfiguration(this);
            _vehicleImpersonation = new DCSVehicleImpersonation(this);
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            base.DetachFromProfileOnMainThread(oldProfile);
            oldProfile.ProfileTick -= Profile_Tick;
            _configuration.Dispose();
            _configuration = null;
            _vehicleImpersonation.Dispose();
            _vehicleImpersonation = null;
        }

        private void Profile_Tick(object sender, EventArgs e)
        {
            _phantomFix?.Profile_Tick(sender, e);
        }

        private void ActiveDriver_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            if (Enum.TryParse(e.Text, true, out DCSExportModuleFormat remoteModuleFormat))
            {
                _remoteModuleFormat = remoteModuleFormat;
            }
            else
            {
                if (new Regex("[\\d\\w_-]+").IsMatch(e.Text))
                {
                    Logger.Error("DCS interface received advertisement for unsupported export module format '{e.Text}' from export script");
                }
                _remoteModuleFormat = null;
            }

            _protocol?.OnDriverStatus(_remoteModuleFormat);
            DriverStatusReceived?.Invoke(this, new DriverStatus {DriverType = e.Text});
        }

        private void ActiveVehicle_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            ProfileHintReceived?.Invoke(this, new ProfileHint {Tag = e.Text});
        }

        private void AlertMessage_ValueReceived(object sender, NetworkTriggerValue.Value e)
        {
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(e.Text));
            Logger.Error("Error received from Export.lua: {AlertMessage}", decoded);
        }

        public override void Reset()
        {
            base.Reset();
            _protocol?.Reset();
        }

        protected override void OnProfileStarted()
        {
            // these parts are only used at run time (i.e. not in the Profile Editor)
            _protocol = new DCSExportProtocol(this, Profile.Dispatcher);
            _phantomFix = new DCSPhantomMonitorFix(this);
        }

        protected override void OnProfileStopped()
        {
            // our base classes aren't very logical, so sometimes we get called OnProfileStopped when the profile is not running
            // so the _protocol may not be created
            _protocol?.Stop();
            _protocol = null;
        }

        protected override void OnClientChanged(string fromValue, string toValue)
        {
            base.OnClientChanged(fromValue, toValue);

            // protocol needs to know
            _protocol.OnClientChanged();

            // our information is now out of date
            _remoteModuleFormat = null;
        }

        protected override void OnUnrecognizedFunction(string id, string value)
        {
            // don't log this as warning because our protocol includes times when we 
            // receive data from the wrong module
            if (IsSynchronized)
            {
                // we think we are synchronized
                Logger.Info($"DCS interface received data for missing function (Key=\"{id}\"); we think export is running module of type '{_remoteModuleFormat}'");
            }
        }

        /// <summary>
        /// true if the export script is running the module we requested, as far as we know
        /// </summary>
        private bool IsSynchronized => _remoteModuleFormat.HasValue && _remoteModuleFormat == _exportModuleFormat;

        /// <summary>
        /// true if we have received a remote module format, so we know the other side is capable of this protocol
        /// </summary>
        private bool CanSynchronize => _remoteModuleFormat.HasValue;

        // WARNING: executed on LoadProfile thread
        public override void ReadXml(XmlReader reader)
        {
            base.ReadXml(reader);

            while (reader.NodeType == XmlNodeType.Element)
            {
                switch (reader.Name)
                {
                    case "ExportModuleFormat":
                        TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(DCSExportModuleFormat));
                        ExportModuleFormat =
                            (DCSExportModuleFormat) enumConverter.ConvertFromInvariantString(
                                reader.ReadElementString("ExportModuleFormat"));
                        break;
                    case "ExportModuleBaseName":
                        ExportModuleBaseName = reader.ReadElementString("ExportModuleBaseName");
                        break;
                    case "ImpersonatedVehicleName":
                        ImpersonatedVehicleName = reader.ReadElementString("ImpersonatedVehicleName");
                        break;
                    case "ExportModuleText":
                        ExportModuleText = reader.ReadElementContentAsString();
                        break;
                    case "ExportModuleTextBase64":
                        string encoded = reader.ReadElementContentAsString();
                        ExportModuleText = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
                        break;
                    default:
                        string discard = reader.ReadElementString(reader.Name);
                        Logger.Warn(
                            $"Ignored unsupported DCS Interface setting '{reader.Name}' with value '{discard}'");
                        break;
                }
            }
        }

        public override void WriteXml(XmlWriter writer)
        {
            base.WriteXml(writer);
            TypeConverter enumConverter = TypeDescriptor.GetConverter(typeof(DCSExportModuleFormat));

            if (ExportModuleFormat != DEFAULT_EXPORT_MODULE_FORMAT)
            {
                // write new Xml only if configured, because it may break previous versions
                writer.WriteElementString("ExportModuleFormat",
                    enumConverter.ConvertToInvariantString(ExportModuleFormat));
            }

            if (ExportModuleBaseName != null)
            {
                writer.WriteElementString("ExportModuleBaseName", ExportModuleBaseName);
            }

            if (!string.IsNullOrEmpty(ExportModuleText))
            {
                if (ExportModuleText.Contains("]]>"))
                {
                    string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(ExportModuleText),
                        Base64FormattingOptions.InsertLineBreaks);
                    writer.WriteStartElement("ExportModuleTextBase64");
                    writer.WriteCData(encoded);
                    writer.WriteEndElement();
                }
                else
                {
                    // prefer readable format
                    writer.WriteStartElement("ExportModuleText");
                    writer.WriteCData(ExportModuleText);
                    writer.WriteEndElement();
                }
            }

            if (ImpersonatedVehicleName != null)
            {
                writer.WriteElementString("ImpersonatedVehicleName", ImpersonatedVehicleName);
            }
        }

        #region IReadyCheck

        public IEnumerable<StatusReportItem> PerformReadyCheck()
        {
            // XXX check on our health

            // check on the health of our exports
            foreach (StatusReportItem item in _configuration.PerformReadyCheck())
            {
                yield return item;
            }
        }

        #endregion

        #region IProfileAwareInterface

        // NOTE: ClientChanged event is in base class

        // this event indicates that the interface received an indication that a profile that 
        // matches the specified hint should be loaded
        public event EventHandler<ProfileHint> ProfileHintReceived;

        // this event indicates that the interface received an indication that the specified
        // exports are loaded on the other side of the interface
        public event EventHandler<DriverStatus> DriverStatusReceived;

        // we only support selection based on which vehicle this interface supports
        public IEnumerable<string> Tags => new[] {ImpersonatedVehicleName ?? VehicleName};

        public void RequestDriver(string profileShortName)
        {
            // we don't have per-profile drivers
            _ = profileShortName;

            // the interface is supposed to have called OnProfileStarted before this is called,
            // so don't check for null; we want this to crash if this breaks in the future
            if (_remoteModuleFormat == _exportModuleFormat)
            {
                // already in correct state
                // but we let our UI know
                DriverStatusReceived?.Invoke(this, new DriverStatus {DriverType = _remoteModuleFormat.ToString()});
                return;
            }

            _protocol.SendDriverRequest(_exportModuleFormat);
        }

        #endregion

        #region IStatusReportNotify

        public void Subscribe(IStatusReportObserver observer)
        {
            _observers.Add(observer);
        }

        public void Unsubscribe(IStatusReportObserver observer)
        {
            _observers.Remove(observer);
        }

        public void InvalidateStatusReport()
        {
            if (_observers.Count < 1)
            {
                return;
            }

            IList<StatusReportItem> newReport = _configuration.CheckConfig();
            PublishStatusReport(newReport);
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            string statusName = StatusName;
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(statusName,
                    "Interface listening to UDP updates from DCS export.lua and responding with commands.",
                    statusReport);
            }
        }

        #endregion
    }
}