// Copyright  Helios Contributors
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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;
using GadrocsWorkshop.Helios.Interfaces.Capabilities;

namespace GadrocsWorkshop.Helios.Interfaces.Common
{
    public class StatusReportNotifyAsyncOnce: IStatusReportNotify
    {
        // observers of our status report that need to be told when we change status
        private readonly HashSet<IStatusReportObserver> _observers = new HashSet<IStatusReportObserver>();

        // the number of status reports we have scheduled, essentially a semaphore but this class is single threaded
        private int _pendingReports;

        // the method to call when it is time to generate the report
        private readonly Action _createStatusReport;

        // accessor for the status name to include with reports
        private readonly Func<string> _getStatusName;

        // accessor for the status name to include with reports
        private readonly Func<string> _getStatusDescription;

        public StatusReportNotifyAsyncOnce(Action createStatusReport, Func<string> getStatusName, Func<string> getStatusDescription)
        {
            _createStatusReport = createStatusReport;
            _getStatusName = getStatusName;
            _getStatusDescription = getStatusDescription;
        }

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

            _pendingReports++;
            DispatcherOperation scheduled =
                Application.Current.Dispatcher.BeginInvoke(new Action(CreateStatusReportIfLast), DispatcherPriority.ApplicationIdle);
            scheduled.Completed += OnReportDone;
            scheduled.Aborted += OnReportDone;
        }

        private void OnReportDone(object sender, EventArgs e)
        {
            _pendingReports--;
        }

        private void CreateStatusReportIfLast()
        {
            if (_pendingReports != 1)
            {
                // we are not the last pending report
                return;
            }

            _createStatusReport();
        }

        public void PublishStatusReport(IList<StatusReportItem> statusReport)
        {
            foreach (IStatusReportObserver observer in _observers)
            {
                observer.ReceiveStatusReport(_getStatusName(), _getStatusDescription(), statusReport);
            }
        }
    }
}