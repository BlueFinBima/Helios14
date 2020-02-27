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
    using System.Reflection;
    using System.Windows;
    using System.Xml;
    using System.Net;
    using System.IO;

    public static class VersionChecker
    {
        private static Version _runningVersion;
        private static Version _lastseenVersion;
        private static Version _currentNetVersion;
        private static string _currentVersion = "";
        private static string _downloadUrl = "";

        private const string VERSION_URL = "https://bluefinbima.github.io/Helios/HeliosCurrentVersionV2.xml";
        //private const string VERSION_URL = "https://bluefinbima.github.io/Helios/HeliosTestVersion.xml";

        static VersionChecker()
        {
            GetRunningVersion();
            _lastseenVersion = ToVersion(ConfigManager.SettingsManager.LoadSetting("ControlCenter", "LastReturnedVersion", "0.0.0.0"));

            if (_runningVersion.CompareTo(_lastseenVersion) >= 0 || _lastseenVersion.Major == 0)
            {
                _currentNetVersion = GetCurrentVersion();  // only get the latest version if the saved version isn't already higher than the running version
            }
            else
            {
                _currentNetVersion = _lastseenVersion;
                _currentVersion = VersionToString(_currentNetVersion);
                _downloadUrl = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "LastestDownloadUrl", "Https://www.digitalcombatsimulator.com/en/files/3302014/");
            }
        }

         public static void CheckVersion()
        {
            if (!string.IsNullOrWhiteSpace(_currentVersion) && _runningVersion != null)
            {
                try
                {
                    string _lastchecked = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "FirstTimeVersionWarningIssued", "");
                    string _lasttimewarned = ConfigManager.SettingsManager.LoadSetting("ControlCenter", "LastTimeVersionWarningIssued", "");
                        if (_runningVersion.CompareTo(_currentNetVersion) < 0)
                        {
                            if (_lastchecked == "")
                            {
                                _lastchecked = DateTime.Today.ToString("yyyyMMdd");
                                _lasttimewarned = "";
                                ConfigManager.SettingsManager.SaveSetting("ControlCenter", "FirstTimeVersionWarningIssued", _lastchecked);
                             }
                        DateTime _now = DateTime.Today;
                        DateTime _firstSeen = new DateTime(Convert.ToInt32(_lastchecked.Substring(0, 4)), Convert.ToInt32(_lastchecked.Substring(4, 2)), Convert.ToInt32(_lastchecked.Substring(6, 2)));
                        double _sinceLastWarned = 0;
                        double _sinceFirstSeen = (_now - _firstSeen).TotalDays;
                        if (_lasttimewarned != "")
                        {
                            DateTime _lastwarned = new DateTime(Convert.ToInt32(_lasttimewarned.Substring(0, 4)), Convert.ToInt32(_lasttimewarned.Substring(4, 2)), Convert.ToInt32(_lasttimewarned.Substring(6, 2)));
                            _sinceLastWarned = (_now - _lastwarned).TotalDays;
                        }

                        //   We want to throttle the number of times that the message is displayed and no more than once a day.
                        if (_lasttimewarned == "" || (_sinceFirstSeen <= 7 && _sinceLastWarned > 0) || (_sinceFirstSeen > 7 && _sinceLastWarned >= 4))
                        {
                            MessageBoxResult result = MessageBox.Show("A newer version (" + _currentVersion + ") of Helios is available. Would you like to download it now?", "Version Check", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastTimeVersionWarningIssued", DateTime.Today.ToString("yyyyMMdd"));
                            if (result == MessageBoxResult.Yes)
                            {
                                System.Diagnostics.Process.Start(_downloadUrl);
                            }
                        }
                    }
                    else if (_runningVersion.CompareTo(_currentNetVersion) > 0)
                    {
                        if (_lastchecked != "")
                        {
                            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "FirstTimeVersionWarningIssued", "");
                            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastTimeVersionWarningIssued", "");
                        }
                        // MessageBoxResult result = MessageBox.Show("Bleeding Edge version (" + VersionToString(_runningVersion) + ") of Helios.", "Version Check", MessageBoxButton.OK, MessageBoxImage.Information);
                        ConfigManager.LogManager.LogInfo("Version Check: Bleeding Edge version " + VersionToString(_runningVersion));
                    }
                    else
                    {
                        if (_lastchecked != "")
                        {
                            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "FirstTimeVersionWarningIssued", "");
                            ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastTimeVersionWarningIssued", "");
                        }
                        // MessageBoxResult result = MessageBox.Show("You're running the latest version (" + _currentVersion + ") of Helios.", "Version Check", MessageBoxButton.OK, MessageBoxImage.Information);
                        ConfigManager.LogManager.LogInfo("Version Check: Running Latest version " + VersionToString(_runningVersion));

                    }
                }
                catch (Exception e)
                {
                    ConfigManager.LogManager.LogError("Error comparing versions", e);
                }
            }
        }

        private static void GetRunningVersion()
        {
            try
            {
                _runningVersion = Assembly.GetEntryAssembly().GetName().Version;
                string _strRevision = VersionToString(_runningVersion);
            }
            catch (Exception e)
            {
                ConfigManager.LogManager.LogError("Error reading running version.", e);
            }
        }

        private static Version GetCurrentVersion()
        {
            try
            {
                // The version checking has been moved off Gadroc's site and onto GitHub (ghpages branch).   This requires querying the version with HTTPS
                // which requires a little more setup
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(VERSION_URL);
                wreq.MaximumAutomaticRedirections = 4;
                wreq.MaximumResponseHeadersLength = 4;
                wreq.Credentials = CredentialCache.DefaultCredentials;
                HttpWebResponse wrsp = (HttpWebResponse)wreq.GetResponse();
                XmlTextReader xmlReader = new XmlTextReader(wrsp.GetResponseStream());
                {
                    xmlReader.ReadStartElement("HeliosVersion");
                    //_currentVersion = xmlReader.ReadElementString("CurrentVersion");
                    //_downloadUrl = xmlReader.ReadElementString("DownloadUrl");
                    while (!xmlReader.Name.Equals("HeliosVersion"))
                    {
                        xmlReader.Read();
                        if (xmlReader.NodeType == XmlNodeType.Element)
                        {

                            switch (xmlReader.Name)
                            {
                                case "CurrentVersion":
                                    if (_currentVersion == "")
                                        _currentVersion = xmlReader.ReadElementString("CurrentVersion");
                                    break;
                                case "CurrentVersionV2":  // this is a newer version format that is not currently used.
                                    _currentVersion = xmlReader.ReadElementString("CurrentVersionV2");
                                    break;
                                case "DownloadUrl":
                                    _downloadUrl = xmlReader.ReadElementString("DownloadUrl");
                                    ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastestDownloadUrl", _downloadUrl);
                                    break;
                                case "HeliosVersion":
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    xmlReader.ReadEndElement();
                    wrsp.Close();
                }
                ConfigManager.SettingsManager.SaveSetting("ControlCenter", "LastReturnedVersion", _currentVersion);
            }
            catch (Exception e)
            {
                ConfigManager.LogManager.LogError("Error retrieving current version.", e);
                return _runningVersion;
            }
            return ToVersion(_currentVersion);
        }
        private static string VersionToString(Version _ver)
        {
            return _ver.Major.ToString() + "." + _ver.Minor.ToString() + "." + _ver.Build.ToString("0000") + "." + _ver.Revision.ToString("0000"); ;
        }

        private static Version ToVersion(string strVersion)
        {
            string[] _ver = strVersion.Split('.');
            Version Ver = new Version(Convert.ToInt32(_ver[0]), Convert.ToInt32(_ver[1]), Convert.ToInt32(_ver[2]), Convert.ToInt32(_ver[3]));
            return Ver;
        }
    }
}
