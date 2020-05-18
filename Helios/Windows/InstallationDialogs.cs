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
using System.Windows;

namespace GadrocsWorkshop.Helios.Windows
{
    public class InstallationDialogs : IInstallationCallbacks
    {
        private readonly DependencyObject _host;

        public InstallationDialogs(DependencyObject host)
        {
            _host = host;
        }

        public InstallationPromptResult DangerPrompt(string title, string message, IList<StatusReportItem> details)
        {
            // XXX create a custom dialog to explore the details
            MessageBoxResult response = MessageBox.Show(Window.GetWindow(_host), message, title,
                MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (response == MessageBoxResult.Cancel)
            {
                return InstallationPromptResult.Cancel;
            }

            return InstallationPromptResult.Ok;
        }

        public void Failure(string title, string message, IList<StatusReportItem> details)
        {
            // XXX create a custom dialog to explore the details
            MessageBox.Show(Window.GetWindow(_host), message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void Success(string title, string message, IList<StatusReportItem> details)
        {
            // no code until we have a custom dialog to browse the details
        }
    }
}