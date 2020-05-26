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
using Newtonsoft.Json;
using NLog;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// this is a structured log item, with the additional restriction
    /// that it is intended for human consumption, so long messages
    /// can be truncated and implementation-related messages are discouraged
    /// </summary>
    public class StatusReportItem
    {
        private static readonly Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This severity code mirrors the log levels where names are the same.
        /// Note: It should be a nested class of StatusReportItem, but WPF bindings require it to be
        /// a top-level type.
        /// </summary>
        public enum SeverityCode
        {
            Info,
            Warning,
            Error,
            None // no messages should be created at this level, in it used to filter out all messages
        }

        // Uri components used in Links
        public const string HELIOS_SCHEME = "helios";
        public const string PROFILE_EDITOR_HOST = "profileeditor";
        public const string CONTROL_CENTER_HOST = "controlcenter";

        /// <summary>
        /// optional time stamp or null
        /// </summary>
        [JsonProperty("TimeStamp", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeStamp { get; set; } = CreateTimeStamp(DateTime.Now);

        [JsonProperty("Severity", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public SeverityCode Severity { get; set; } = SeverityCode.Info;

        /// <summary>
        /// the status message, which may be more than one line long
        /// but should generally be short
        /// </summary>
        [JsonProperty("Status")]
        public string Status { get; set; }

        /// <summary>
        /// Additional information related to this status, expressed as a set of
        /// flags that may be set to communicate different facts.
        /// </summary>
        [Flags]
        public enum StatusFlags
        {
            // no flags
            None = 0,

            // this status may be numerous or verbose and should be filtered in small status displays
            Verbose = 1,

            // this status indicates that some checked configuration item was up to date and does not need to regenerated
            ConfigurationUpToDate = 2,

            // the timestamp in this item is from a log, so correlation can be done
            // otherwise the time stamp might be off by some milliseconds from related log entries
            TimeStampIsPrecise = 4
        }

        /// <summary>
        /// Any flags set in this value (combined via binary 'or') indicate that
        /// the corresponding fact is true about this status report item.
        /// </summary>
        [JsonProperty("Flags", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public StatusFlags Flags { get; set; }

        /// <summary>
        /// a recommendation to the user or null
        /// </summary>
        [JsonProperty("Recommendation", NullValueHandling = NullValueHandling.Ignore)]
        public string Recommendation { get; set; }

        /// <summary>
        /// in the event that the status message was shortened or cleaned up, this
        /// property will have the original full text, which is not safe for UI display
        /// and should only be used for logging or sharing purposes
        /// </summary>
        [JsonProperty("FullText", NullValueHandling = NullValueHandling.Ignore)]
        public string FullText { get; set; }

        /// <summary>
        /// If this is not null and a consumer of this data has the capability of generating
        /// references or links, this value should be parsed and presented as a link to the
        /// specified entities.
        /// </summary>
        [JsonIgnore] public Uri Link;

        /// <summary>
        /// utility to create a link to the profile editor without referring to any specific UI component
        /// </summary>
        public static Uri ProfileEditor => new Uri($"{HELIOS_SCHEME}://{PROFILE_EDITOR_HOST}/");

        /// <summary>
        /// utility to create a link to the control center without referring to any specific UI component
        /// </summary>
        public static Uri ControlCenter => new Uri($"{HELIOS_SCHEME}://{CONTROL_CENTER_HOST}/");

        /// <summary>
        /// log this result
        /// </summary>
        /// <param name="logManager"></param>
        public void Log(LogManager logManager)
        {
            switch (Severity)
            {
                case SeverityCode.None:
                    throw new Exception(
                        "Severity 'None' must not be used for any status report instances; implementation error");
                case SeverityCode.Info:
                    logManager.LogInfo(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogInfo(Recommendation);
                    }

                    break;
                case SeverityCode.Warning:
                    logManager.LogWarning(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogWarning(Recommendation);
                    }

                    break;
                case SeverityCode.Error:
                    logManager.LogError(Status);
                    if (Recommendation != null)
                    {
                        logManager.LogError(Recommendation);
                    }

                    break;
            }
        }

        public static string CreateTimeStamp(DateTime dateTime) => dateTime.ToString("MM/dd/yyyy hh:mm:ss.fff tt");
    }
}