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

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    /// <summary>
    /// callback interface for changes that happen without a call to PerformReadyCheck
    /// </summary>
    public interface IStatusReportObserver
    {
        /// <summary>
        /// called when interface status changes
        /// </summary>
        /// <param name="statusName">the human-readable name the interface would like to use in status reporting</param>
        /// <param name="description">a longer, human-readable description of the interface</param>
        /// <param name="statusReport">the current status report</param>
        void ReceiveStatusReport(string statusName, string description, IList<StatusReportItem> statusReport);
    }

    /// <summary>
    /// Objects that implement this interface may generate a list of user-readable
    /// issues without being called on IReadyCheck, i.e. due to UI interaction or
    /// other events
    /// </summary>
    public interface IStatusReportNotify
    {
        /// <param name="observer"></param>
        // REVISIT: once on language 8.0, these can all be default implemented?
        void Subscribe(IStatusReportObserver observer);

        void Unsubscribe(IStatusReportObserver observer);

        /// <summary>
        /// alternative to InvalidateStatusReport: send this status report to all subscribers,
        /// asserting that it is fresh
        /// </summary>
        /// <param name="statusReport"></param>
        void PublishStatusReport(IList<StatusReportItem> statusReport);

        /// <summary>
        /// invalidate the status report, so that a fresh one will be generated
        /// and sent to all subscribers
        /// optionally, this functionality may be implemented asynchronously, so that the new status report
        /// is not sent immediately
        /// </summary>
        void InvalidateStatusReport();
    }
}