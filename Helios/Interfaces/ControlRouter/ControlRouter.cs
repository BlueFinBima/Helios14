// Copyright 2020 Ammo Goettsch
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

using GadrocsWorkshop.Helios.ComponentModel;
using GadrocsWorkshop.Helios.Interfaces.Common;

namespace GadrocsWorkshop.Helios.Interfaces.ControlRouter
{
    [HeliosInterface("Helios.Base.ControlRouter", "Control Router", typeof(ControlRouterEditor),
        typeof(UniqueHeliosInterfaceFactory))]
    public class ControlRouter : HeliosInterfaceWithXml<ControlRouterModel>
    {
        private HeliosVisual _mostRecentlySelected;

        // deserialization constructor
        public ControlRouter() : this("Control Router")
        {
            // no code
        }

        public ControlRouter(string name) : base(name)
        {
            // no code
        }

        protected override void AttachToProfileOnMainThread()
        {
            base.AttachToProfileOnMainThread();
            if (Model.Ports.Count == 0)
            {
                // create some ports
                Model.CreateDefaultPorts(4);
            }

            Model.Ports.ForEach(model =>
            {
                model.Initialize(this);

                // we slave these collections so we can clear them later if we remove the port
                Actions.AddSlave(model.Actions);
                Triggers.AddSlave(model.Triggers);
                Values.AddSlave(model.Values);
            });

            // sign up for any compatible controls being touched
            Profile.RoutableControlSelected += Profile_RoutableControlSelected;
        }

        private void Profile_RoutableControlSelected(object sender, Controls.ControlEventArgs e)
        {
            _mostRecentlySelected = e.Visual;
        }

        protected override void DetachFromProfileOnMainThread(HeliosProfile oldProfile)
        {
            oldProfile.RoutableControlSelected -= Profile_RoutableControlSelected;
            base.DetachFromProfileOnMainThread(oldProfile);
        }

        internal bool TryClaimControl(out HeliosVisual visual)
        {
            visual = _mostRecentlySelected;
            if (_mostRecentlySelected == null)
            {
                return false;
            }

            // claimed
            _mostRecentlySelected = null;
            return true;
        }
    }
}