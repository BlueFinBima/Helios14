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

using GadrocsWorkshop.Helios.ComponentModel;

namespace GadrocsWorkshop.Helios
{
    /// <summary>
    /// Base class for all hardware and software interface objects.
    /// </summary>
    public abstract class HeliosInterface : HeliosObject
    {
        protected string _typeIdentifier;

        protected HeliosInterface(string name)
            : base(name)
        {
        }

        protected override void OnProfileChanged(HeliosProfile oldProfile)
        {
            base.OnProfileChanged(oldProfile);

            if (oldProfile != null)
            {
                DetachFromProfileOnMainThread(oldProfile);
            }

            if (Profile != null)
            {
                AttachToProfileOnMainThread();
            }
        }

        /// <summary>
        /// called on the main thread when the interface is installed in a Profile
        /// rather than just being instantiated for testing in the Add Interfaces dialog
        ///
        /// heavy initialization should be performed here
        /// </summary>
        protected virtual void AttachToProfileOnMainThread()
        {
            // no code in base
        }

        /// <summary>
        /// called on the main thread when the interface is removed from a profile,
        /// usually due to deletion by the user
        ///
        /// shut down the interface and detach from all event handlers so the interface
        /// can be deallocated
        /// </summary>
        /// <param name="oldProfile"></param>
        protected virtual void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            // no code in base
            _ = oldProfile;
        }

        #region Properties

        public override string TypeIdentifier
        {
            get
            {
                if (_typeIdentifier == null)
                {
                    HeliosInterfaceDescriptor descriptor = ConfigManager.ModuleManager.InterfaceDescriptors[this.GetType()];
                    _typeIdentifier = descriptor.TypeIdentifier;
                }
                return _typeIdentifier;
            }
        }

        /// <summary>
        /// this governs the behavior of Helios when loading a profile containing this interface
        /// but the interface is not a supported interface class, for example for a plugin that is
        /// not installed or an interface from a later version of Helios
        /// </summary>
        public ComponentUnsupportedSeverity UnsupportedSeverity { get; set; }

        #endregion
    }
}
