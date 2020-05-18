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

using System.Text.RegularExpressions;

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.IO;
    using System.Reflection;

    public class LogManager
    {
        public LogManager(LogLevel level)
        {
            _ = level;
            // no code, log level is managed via NLog
        }

        public void LogDebug(string message)
        {
            WriteLogMessage(Helios.LogLevel.Debug, message, null);
        }

        public void Log(string message)
        {
            WriteLogMessage(Helios.LogLevel.All, message, null);
        }

        public void LogWarning(string message)
        {
            WriteLogMessage(LogLevel.Warning, message, null);
        }

        public void LogWarning(string message, Exception exception)
        {
            WriteLogMessage(LogLevel.Warning, message, exception);
        }

        public void LogError(string message)
        {
            WriteLogMessage(LogLevel.Error, message, null);
        }

        public void LogError(string message, Exception exception)
        {
            WriteLogMessage(LogLevel.Error, message, exception);
        }

        public void LogInfo(string message)
        {
            WriteLogMessage(LogLevel.Info, message, null);
        }

        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private void WriteLogMessage(LogLevel level, string message, Exception exception)
        {
            switch (level)
            {
                case LogLevel.All:
                    Logger.Info(message);
                    break;
                case LogLevel.Error:
                    if (exception != null)
                    {
                        Logger.Error(exception, message);
                    }
                    else
                    {
                        Logger.Error(message);
                    }
                    break;
                case LogLevel.Warning:
                    Logger.Warn(message);
                    break;
                case LogLevel.Info:
                    Logger.Info(message);
                    break;
                case LogLevel.Debug:
                    Logger.Debug(message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level), level, null);
            }
        }
    }
}
