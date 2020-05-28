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

using GadrocsWorkshop.Helios.Windows.Controls;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Reflection;
    using System.Windows;
    using System.Xml;
    using System.Net;
    using System.IO;
    using GadrocsWorkshop.Helios.Util;

    public static class VersionChecker
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static Version _runningVersion;
        private static Version _lastseenVersion;
        private static Version _currentNetVersion;
        private static string _currentVersion = "";
        private static string _downloadUrl = "";
        private static string _ghdownloadUrl = "";
        private static string _nextdatewarn = "";
        private static string _today = "";
        private static Int32 _todayVal = 0;
        private static Boolean _downloadneeded = false;
        private static Boolean _remindnextrelease = false;
        private const string VERSION_URL = "https://github.com/BlueFinBima/HeliosCheckRelease/releases/download/CheckRelease/HeliosCurrentVersionV2.xml";
        
        private static readonly bool _developmentPrototype;
        private static readonly DateTime _timeBombTime;

        // XXX: doing this work at static init time depending on Helios having been initialized is an awful idea.  this could even be a child of ConfigManager
        static VersionChecker()
        {
            _today = DateTime.Today.ToString("yyyyMMdd");
            _todayVal = Convert.ToInt32(_today);
            GetRunningVersion();
            if (Helios.RunningVersion.IsDevelopmentPrototype)
            {
                _developmentPrototype = true;
                _timeBombTime = TimeStamp.CompilationTimestampUtc.AddDays(30.0);
            }
            _lastseenVersion = ToVersion(ConfigManager.SettingsManager.LoadSetting("Helios", "LastReturnedVersion", "0.0.0.0"));
            _nextdatewarn = ConfigManager.SettingsManager.LoadSetting("Helios", "NextDateForVersionWarning", _today);
            _remindnextrelease = ConfigManager.SettingsManager.LoadSetting("Helios", "ReminderForNextRelease", "0")=="1"?true:false;

            // also need to do a periodic (weekly) check for people wanting to stop reminders until the following release 
            if ((!_developmentPrototype) && (
                _runningVersion.CompareTo(_lastseenVersion) >= 0 || 
                _lastseenVersion.Major == 0 || 
                (_remindnextrelease && Convert.ToInt32(_nextdatewarn) <= _todayVal)))
            {
                _currentNetVersion = GetCurrentVersion();  // only get the latest version if the saved version isn't already higher than the running version
                if (_remindnextrelease)
                {
                    if (_runningVersion.CompareTo(_lastseenVersion) < 0 && _currentNetVersion.CompareTo(_lastseenVersion) > 0)
                    {
                        _nextdatewarn = _today;  //There is a new release higher than the las new release we stored so indicate that we need to issue the New Version dialog
                        ConfigManager.SettingsManager.SaveSetting("Helios", "ReminderForNextRelease", "0");
                    } else
                    {
                        _nextdatewarn = DateTime.Today.AddDays(7).ToString("yyyyMMdd");  // Check again in a week's time
                    }
                    ConfigManager.SettingsManager.SaveSetting("Helios", "NextDateForVersionWarning", _nextdatewarn);
                }
            }
            else
            {
                _currentNetVersion = _lastseenVersion;
                _currentVersion = VersionToString(_currentNetVersion);
                _downloadUrl = ConfigManager.SettingsManager.LoadSetting("Helios", "LastestDownloadUrl", "Https://www.digitalcombatsimulator.com/en/files/3302014/");
                // REVISIT: is this default used? then document why it is ok to assume the DCS page is up to date
                _ghdownloadUrl = ConfigManager.SettingsManager.LoadSetting("Helios", "LastestGitHubDownloadUrl", "Https://www.digitalcombatsimulator.com/en/files/3302014/");
            }
        }
        
        public static void CheckVersion()
        {
            if (_developmentPrototype)
            {
                if (_timeBombTime.CompareTo(DateTime.Now) < 0)
                {
                    string message =
                        $"this is a development prototype build that expired on {_timeBombTime.ToLongDateString()}";
                    Logger.Error(message);
                    MessageBox.Show(message, "Please get a new Development build of Helios");
                    Application.Current.Shutdown();
                }
                Logger.Info($"this is a development prototype build that will expire on {_timeBombTime.ToLongDateString()}");
                return;
            }
            if (!string.IsNullOrWhiteSpace(_currentVersion) && _runningVersion != null)
            {
                try
                {
                    _nextdatewarn = ConfigManager.SettingsManager.LoadSetting("Helios", "NextDateForVersionWarning", "");

                    if (_nextdatewarn == "")
                    {
                        _nextdatewarn = _today;
                        ConfigManager.SettingsManager.SaveSetting("Helios", "NextDateForVersionWarning", _nextdatewarn);
                    }
                    if (_runningVersion.CompareTo(_currentNetVersion) < 0 && (Convert.ToInt32(_nextdatewarn) <= _todayVal))
                    {
                        VersionReminderForm dialog = new VersionReminderForm();
                        dialog.NewVersionBlock.Text = VersionToString(_currentNetVersion);
                        dialog.ShowDialog();

                        if (_downloadneeded)
                        {
                            System.Diagnostics.Process.Start(_ghdownloadUrl);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e, "Version Checker: Error comparing versions");
                }
            }
        }

        public static void SetNextCheckDate(DateTime dateNextCheck, Boolean remindNextRelease)
        {
            _nextdatewarn = dateNextCheck.ToString("yyyyMMdd");
            ConfigManager.SettingsManager.SaveSetting("Helios", "NextDateForVersionWarning", _nextdatewarn);
            ConfigManager.SettingsManager.SaveSetting("Helios", "ReminderForNextRelease", remindNextRelease?"1":"0");
        }

        public static void SetDownloadNeeded()
        {
            _downloadneeded = true;

            // if the user has downloaded the new version, set a nag if they have not installed it
            _nextdatewarn = _today;
            ConfigManager.SettingsManager.SaveSetting("Helios", "NextDateForVersionWarning", _nextdatewarn);
        }

        public static Version RunningVersion => _runningVersion;

        private static void GetRunningVersion()
        {
            try
            {
                _runningVersion = Assembly.GetEntryAssembly().GetName().Version;
                string _strRevision = VersionToString(_runningVersion);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Version Checker: Error reading running version. ");
            }
        }

        private static Version GetCurrentVersion()
        {
            try
            {
                // The version checking has been moved off Gadroc's site and onto a dedicated GitHub repo.   This requires querying the version with HTTPS
                // which requires a little more setup
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
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
                                case "DownloadUrl":
                                    _downloadUrl = xmlReader.ReadElementString("DownloadUrl");
                                    ConfigManager.SettingsManager.SaveSetting("Helios", "LastestDownloadUrl", _downloadUrl);
                                    break;
                                case "GitHubDownloadUrl":
                                    _ghdownloadUrl = xmlReader.ReadElementString("GitHubDownloadUrl");
                                    ConfigManager.SettingsManager.SaveSetting("Helios", "LastestGitHubDownloadUrl", _ghdownloadUrl);
                                    break;                                    
                                case "VersionHighlights":
                                    _ghdownloadUrl = xmlReader.ReadElementString("VersionHighlights");
                                    // TODO handle the VersionHighlights data in the XML
                                    //ConfigManager.SettingsManager.SaveSetting("Helios", "LastestGitHubDownloadUrl", _ghdownloadUrl);
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
                ConfigManager.SettingsManager.SaveSetting("Helios", "LastReturnedVersion", _currentVersion);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Version Checker: Error retrieving current version.");
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
            string[] _ver = Tokenizer.TokenizeAtLeast(strVersion, 4, '.');
            Version Ver = new Version(Convert.ToInt32(_ver[0]), Convert.ToInt32(_ver[1]), Convert.ToInt32(_ver[2]), Convert.ToInt32(_ver[3]));
            return Ver;
        }
    }
}
