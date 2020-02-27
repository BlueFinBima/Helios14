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

namespace GadrocsWorkshop.Helios.Interfaces.Keyboard
{
    using GadrocsWorkshop.Helios.ComponentModel;
    using System.Globalization;
    using System.Xml;

    [HeliosInterface("Helios.Base.Keyboard", "Keyboard", typeof(KeyboardInterfaceEditor), typeof(UniqueHeliosInterfaceFactory), AutoAdd=true)]
    public class KeyboardInterface : HeliosInterface
    {
        public static readonly string SpecialKeyHelp = "\r\n\r\nSpecial keys can be sent by sending their names in brackets, ex: {PAUSE}.\r\nBACKSPACE, TAB, CLEAR, RETURN, ENTER, LSHIFT, RSHIFT, LCONTROL, RCONTROL, LALT, RALT, PAUSE, CAPSLOCK, ESCAPE, SPACE, PAGEUP, PAGEDOWN, END, HOME, LEFT, UP, RIGHT, DOWN, PRINTSCREEN, INSERT, DELETE, LWIN, RWIN, APPS, NUMPAD0, NUMPAD1, NUMPAD2, NUMPAD3, NUMPAD4, NUMPAD5, NUMPAD6, NUMPAD7, NUMPAD8, NUMPAD9, MULTIPLY, ADD, SEPARATOR, SUBTRACT, DECIMAL, DIVIDE, NUMPADENTER, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, F19, F20, F21, F22, F23, F24, NUMLOCK, SCROLLLOCK";

        private HeliosAction _keyPressAction;
        private HeliosAction _keyDownAction;
        private HeliosAction _keyUpAction;

        public KeyboardInterface()
            : base("Keyboard")
        {
            // load global configuration not specific to profile
            KeyboardEmulator.ForceQwerty = ConfigManager.SettingsManager.LoadSetting<bool>("KeyboardInterface", "ForceQwerty", false);

            _keyPressAction = new HeliosAction(this, "", "", "send keys", "Presses and releases a set of keyboard keys.", "Keys which will be sent to the foreground applications.  Whitespace seperates key combos allowing multiple keystroke commands to be sent. \"{LCONTROL}c\" will hold down left control and then press c while \"{LCONTROL} c\" will press and release left control and then press and release c." + SpecialKeyHelp, BindingValueUnits.Text);
            _keyPressAction.Execute += new HeliosActionHandler(KeyPress_ExecuteAction);
            _keyDownAction = new HeliosAction(this, "", "", "press key", "Presses keys.", "Keys which will be pressed down and remain pressed until a release key event is sent." + SpecialKeyHelp, BindingValueUnits.Text);
            _keyDownAction.Execute += new HeliosActionHandler(KeyDown_ExecuteAction);
            _keyUpAction = new HeliosAction(this, "", "", "release key", "Releases keys.", "Keys which will be released." + SpecialKeyHelp, BindingValueUnits.Text);
            _keyUpAction.Execute += new HeliosActionHandler(KeyUp_ExecuteAction);

            Actions.Add(_keyPressAction);
            Actions.Add(_keyDownAction);
            Actions.Add(_keyUpAction);
        }

        public int KeyDelay
        {
            get
            {
                return KeyboardEmulator.KeyDelay;
            }
            set
            {
                int oldValue = KeyboardEmulator.KeyDelay;
                KeyboardEmulator.KeyDelay = value;
                OnPropertyChanged("KeyDelay", oldValue, value, true);
            }
        }

        public bool ForceQwerty
        {
            get
            {
                return KeyboardEmulator.ForceQwerty;
            }
            set
            {
                bool oldValue = KeyboardEmulator.ForceQwerty;
                KeyboardEmulator.ForceQwerty = value;
                ConfigManager.SettingsManager.SaveSetting<bool>("KeyboardInterface", "ForceQwerty", value);

                // no undo, since this isn't part of profile
                OnPropertyChanged("ForceQwerty", oldValue, value, false);
            }
        }

        /// <summary>
        /// the UI only evaluates this once and it is not refreshed/notified, because we don't support changing layouts
        /// while Helios is running
        /// </summary>
        public bool ForceQwertyAvailable => KeyboardEmulator.CheckIfForceQwertyAvailable();

        void KeyPress_ExecuteAction(object action, HeliosActionEventArgs e)
        {
            KeyboardEmulator.KeyPress(e.Value.StringValue);
        }

        void KeyDown_ExecuteAction(object action, HeliosActionEventArgs e)
        {
            KeyboardEmulator.KeyDown(e.Value.StringValue);
        }

        void KeyUp_ExecuteAction(object action, HeliosActionEventArgs e)
        {
            KeyboardEmulator.KeyUp(e.Value.StringValue);
        }

        public override void ReadXml(XmlReader reader)
        {
            // load profile-specific configuration
            KeyDelay = int.Parse(reader.ReadElementString("KeyDelay"), CultureInfo.InvariantCulture);
        }

        public override void WriteXml(XmlWriter writer)
        {
            // save profile-specific configuration
            writer.WriteElementString("KeyDelay", KeyDelay.ToString(CultureInfo.InvariantCulture));
        }
    }
}
