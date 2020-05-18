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

using System.Collections.Generic;
using NLog;

namespace GadrocsWorkshop.Helios.Util
{
    internal class OnceLogger
    {
        private readonly Logger _logger;
        private readonly HashSet<string> _idsLogged = new HashSet<string>();

        public OnceLogger(Logger logger)
        {
            _logger = logger;
        }

        internal void InfoOnceUnlessDebugging(string loggingId, string message, params object[] args)
        {
            if (_logger.IsDebugEnabled)
            {
                // log always in debug
                _logger.Info(message, args);
                return;
            }

            if (_idsLogged.Add(loggingId))
            {
                // log once if logging at info
                List<object> modifiedArgs = new List<object> {"InfoOnce"};
                modifiedArgs.AddRange(args);
                _logger.Info($"{{Once}} {message}", modifiedArgs.ToArray());
            }
        }

        internal void WarnOnceUnlessDebugging(string loggingId, string message, params object[] args)
        {
            if (_logger.IsDebugEnabled)
            {
                // log always in debug
                _logger.Warn(message, args);
                return;
            }

            if (_idsLogged.Add(loggingId))
            {
                // log once if logging at info
                List<object> modifiedArgs = new List<object> {"WarnOnce"};
                modifiedArgs.AddRange(args);
                _logger.Warn($"{{Once}} {message}", modifiedArgs.ToArray());
            }
        }
    }
}