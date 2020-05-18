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
using System.Collections.Generic;

namespace GadrocsWorkshop.Helios.Interfaces.Capabilities
{
    namespace ProfileAwareInterface
    {
        public class ProfileHint : EventArgs
        {
            public string Tag { get; set; }
        }

        public class DriverStatus : EventArgs
        {
            public string DriverType { get; set; }
        }

        public class ClientChange : EventArgs
        {
            /// <summary>
            /// the only handle value which we may interpret, all other values are opaque
            /// </summary>
            public static string NO_CLIENT = "";

            public string FromOpaqueHandle { get; set; }
            public string ToOpaqueHandle { get; set; }
        }

        /// <summary>
        /// Interface instances that support this interface are loosely aware of the existence
        /// of profiles.  They may receive information that helps select a profile and they
        /// may also make their containing profile appropriate for selection or not.
        /// Finally, they are aware that profiles may require certain drivers to be loaded.
        /// </summary>
        public interface IProfileAwareInterface
        {
            /// <summary>
            /// Fired to indicate the interface is using a driver of the specified type.
            /// This event can be fired with or without a previous RequestDriver call.
            /// </summary>
            event EventHandler<DriverStatus> DriverStatusReceived;

            /// <summary>
            /// Fired to indicate the interface would like a profile with a tag matching
            /// the hint received.
            /// </summary>
            event EventHandler<ProfileHint> ProfileHintReceived;

            /// <summary>
            /// Fired to indicate that the interface may no longer be connected to the same
            /// endpoint as before.
            /// </summary>
            event EventHandler<ClientChange> ClientChanged;

            /// <summary>
            /// Tags that can be used to match a profile containing this interface to a future
            /// profile hint.
            /// </summary>
            IEnumerable<string> Tags { get; }

            /// <summary>
            /// Request that the interface switch to using the driver type appropriate to
            /// the given profile identified by its short name (without extension)
            /// and send a DriverStatus event when this is accomplished.
            /// </summary>
            /// <param name="profileShortName"></param>
            void RequestDriver(string profileShortName);
        }
    }
}