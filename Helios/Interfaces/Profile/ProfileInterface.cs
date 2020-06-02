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

namespace GadrocsWorkshop.Helios.Interfaces.Profile
{
    using System;
    using System.Diagnostics;
    using GadrocsWorkshop.Helios.ComponentModel;

    [HeliosInterface("Helios.Base.ProfileInterface", "Profile", null, typeof(UniqueHeliosInterfaceFactory), AutoAdd = true)]
    public class ProfileInterface : HeliosInterface
    {
        private HeliosTrigger _profileStartedTrigger;
        private HeliosTrigger _profileResetTrigger;
        private HeliosTrigger _profileStoppedTrigger;

        public ProfileInterface()
            : base("Profile")
        {
            HeliosAction resetAction = new HeliosAction(this, "", "", "reset", "Resets the profile to default state.");
            resetAction.Execute += new HeliosActionHandler(ResetAction_Execute);
            Actions.Add(resetAction);

            HeliosAction stopAction = new HeliosAction(this, "", "", "stop", "Stops the profile from running.");
            stopAction.Execute += new HeliosActionHandler(StopAction_Execute);
            Actions.Add(stopAction);

            HeliosAction showControlCenter = new HeliosAction(this, "", "", "show control center", "Shows the control center.");
            showControlCenter.Execute += new HeliosActionHandler(ShowAction_Execute);
            Actions.Add(showControlCenter);

            HeliosAction hideControlCenter = new HeliosAction(this, "", "", "hide control center", "Shows the control center.");
            hideControlCenter.Execute += new HeliosActionHandler(HideAction_Execute);
            Actions.Add(hideControlCenter);

            HeliosAction launchApplication = new HeliosAction(this, "", "", "launch application", "Launches an external application", "Full path to appliation or document you want to launch or URL to a web page.", BindingValueUnits.Text);
            launchApplication.Execute += LaunchApplication_Execute;
            Actions.Add(launchApplication);

            HeliosAction killApplication = new HeliosAction(this, "", "", "kill application", "Kills an external process", "Process Image name of the process to be killed.", BindingValueUnits.Text);
            killApplication.Execute += KillApplication_Execute;
            Actions.Add(killApplication);

            _profileStartedTrigger = new HeliosTrigger(this, "", "", "Started", "Fired when a profile is started.");
            Triggers.Add(_profileStartedTrigger);

            _profileResetTrigger = new HeliosTrigger(this, "", "", "Reset", "Fired when a profile has been reset.");
            Triggers.Add(_profileResetTrigger);

            _profileStoppedTrigger = new HeliosTrigger(this, "", "", "Stopped", "Fired when a profile is stopped.");
            Triggers.Add(_profileStoppedTrigger);
        }

        void LaunchApplication_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                Process.Start(e.Value.StringValue);
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("Error caught launching external application (path=\"" + e.Value.StringValue + "\")", ex);
            }
        }

        void KillApplication_Execute(object action, HeliosActionEventArgs e)
        {
            try
            {
                Process[] _localProcessesByName = Process.GetProcessesByName(e.Value.StringValue);
                foreach (Process _proc in _localProcessesByName) {
                    _proc.Kill();
                }
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("Error caught killing process image \"" + e.Value.StringValue + "\")", ex);
            }
        }

        void HideAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.HideControlCenter();
            }
        }

        void ShowAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.ShowControlCenter();
            }
        }

        void StopAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.Stop();
            }
        }

        void ResetAction_Execute(object action, HeliosActionEventArgs e)
        {
            if (Profile != null)
            {
                Profile.Reset();
            }
        }
        public void Start()
        {
            _profileStartedTrigger.FireTrigger(BindingValue.Empty);
        }

        public override void Reset()
        {
            _profileResetTrigger.FireTrigger(BindingValue.Empty);
            base.Reset();
        }
        public void Stop()
        {
            _profileStoppedTrigger.FireTrigger(BindingValue.Empty);
        }

        public override void ReadXml(System.Xml.XmlReader reader)
        {
            // No-Op
        }

        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            // No-Op
        }
    }
}
