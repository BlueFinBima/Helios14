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
using System.Windows;
using NLog;
using NLog.Targets;

namespace GadrocsWorkshop.Helios.ControlCenter.StatusViewer
{
    [Target("StatusViewer")]
    public sealed class StatusViewerLogTarget : TargetWithLayout
    {
        public static StatusViewer Parent { get; internal set; }

        protected override void Write(LogEventInfo logEvent)
        {
            if (Parent == null)
            {
                // not hooked up
                return;
            }

            string message = Layout.Render(logEvent);

            // note: exceptions here will bring down the application
            Application.Current.Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    Parent.WriteLogMessage(logEvent, message);
                }
                catch (Exception ex)
                {
                    // can't log from here
                    Console.WriteLine($@"log consumer failed to write message: {ex}");
                }
            });
        }
    }
}