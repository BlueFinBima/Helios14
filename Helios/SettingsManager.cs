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
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Xml;

    internal class SettingsManager : ISettingsManager
    {
        private class Setting
        {
            public string Name;
            public string Value;
        }

        private class SettingsColleciton : KeyedCollection<string, Setting>
        {
            protected override string GetKeyForItem(Setting item)
            {
                return item.Name;
            }
        }

        private class Group
        {
            private SettingsColleciton _settings = new SettingsColleciton();

            public string Name;
            public SettingsColleciton Settings { get { return _settings; } }
        }

        private class GroupCollection : KeyedCollection<string, Group>
        {
            protected override string GetKeyForItem(Group item)
            {
                return item.Name;
            }
        }

        private string _settingsFile;
        private GroupCollection _settings = null;

        public SettingsManager(string settingsFile)
        {
            _settingsFile = settingsFile;
        }

        private void LoadSettings()
        {
            if (_settings != null)
            {
                // only load once
                return;
            }

            // start with empty settings
            _settings = new GroupCollection();

            try
            {
                if (File.Exists(_settingsFile))
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.IgnoreComments = true;
                    settings.IgnoreWhitespace = true;

                    TextReader reader = new StreamReader(_settingsFile);
                    XmlReader xmlReader = XmlReader.Create(reader, settings);

                    try
                    {
                        xmlReader.ReadStartElement("HeliosSettings");
                        if (!xmlReader.IsEmptyElement)
                        {
                            while (xmlReader.NodeType != XmlNodeType.EndElement)
                            {
                                xmlReader.ReadStartElement("Group");
                                string name = xmlReader.ReadElementString("Name");
                                Group group = GetGroup(name);

                                if (!xmlReader.IsEmptyElement)
                                {
                                    xmlReader.ReadStartElement("Settings");
                                    while (xmlReader.NodeType != XmlNodeType.EndElement)
                                    {
                                        Setting setting = new Setting();

                                        xmlReader.ReadStartElement("Setting");
                                        setting.Name = xmlReader.ReadElementString("Name");
                                        setting.Value = xmlReader.ReadElementString("Value");
                                        xmlReader.ReadEndElement();

                                        group.Settings.Add(setting);
                                    }
                                    xmlReader.ReadEndElement();
                                }
                                else
                                {
                                    xmlReader.Read();
                                }
                                xmlReader.ReadEndElement();
                            }
                        }
                        else
                        {
                            xmlReader.Read();
                        }

                        xmlReader.ReadEndElement();
                    }
                    finally
                    {
                        xmlReader.Close();
                        reader.Close();
                    }
                }
            } 
            catch (System.Exception ex)
            {
                ConfigManager.LogManager.LogError($"the settings file '{_settingsFile}' cannot be read; all settings will be reset", ex);
                // reset to defaults (empty settings) and let it overwrite the settings file when we next save
                _settings = new GroupCollection();
            }
        }

        private void SaveSettings()
        {
            LoadSettings();

            // Delete tmp file if exists
            if (File.Exists(_settingsFile))
            {
                File.Delete(_settingsFile);
            }

            TextWriter writer = new StreamWriter(_settingsFile, false);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;

            XmlWriter xmlWriter = XmlWriter.Create(writer, settings);

            xmlWriter.WriteStartElement("HeliosSettings");
            foreach (Group group in _settings)
            {
                xmlWriter.WriteStartElement("Group");
                xmlWriter.WriteElementString("Name", group.Name);
                xmlWriter.WriteStartElement("Settings");
                foreach (Setting setting in group.Settings)
                {
                    xmlWriter.WriteStartElement("Setting");
                    xmlWriter.WriteElementString("Name", setting.Name);
                    xmlWriter.WriteElementString("Value", setting.Value); 
                    xmlWriter.WriteEndElement();
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }
            xmlWriter.WriteEndElement();

            xmlWriter.Close();
            writer.Close();
        }

        private Group GetGroup(string groupName)
        {
            Group retValue;
            if (_settings.Contains(groupName))
            {
                retValue = _settings[groupName];
            }
            else
            {
                retValue = new Group();
                retValue.Name = groupName;
                _settings.Add(retValue);
            }
            return retValue;
        }

        public void SaveSetting(string group, string name, string value)
        {
            LoadSettings();

            Group settingGroup = GetGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                setting.Value = value;
            }
            else
            {
                setting = new Setting();
                setting.Name = name;
                setting.Value = value;
                settingGroup.Settings.Add(setting);
            }

            SaveSettings();
        }

        public string LoadSetting(string group, string name, string defaultValue)
        {
            LoadSettings();

            Group settingGroup = GetGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                return setting.Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public T LoadSetting<T>(string group, string name, T defaultValue)
        {
            LoadSettings();

            Group settingGroup = GetGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
                TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
                return (T)conv.ConvertFromInvariantString(setting.Value);
            }
            else
            {
                return defaultValue;
            }
        }


        public void SaveSetting<T>(string group, string name, T value)
        {
            LoadSettings();

            Group settingGroup = GetGroup(group);
            Setting setting;
            if (settingGroup.Settings.Contains(name))
            {
                setting = settingGroup.Settings[name];
            }
            else
            {
                setting = new Setting();
                setting.Name = name;
                settingGroup.Settings.Add(setting);
            }

            TypeConverter conv = TypeDescriptor.GetConverter(typeof(T));
            setting.Value = conv.ConvertToInvariantString(value);

            SaveSettings();
        }

        public bool IsSettingAvailable(string group, string name)
        {
            LoadSettings();

            Group settingGroup = GetGroup(group);
            return settingGroup.Settings.Contains(name);
        }
    }
}
