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

namespace GadrocsWorkshop.Helios
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using System.Windows.Input;

    /// <summary>
    /// Keyboard emulator which can send keyboard events to the application with focus.  Special keys can be sent by
    /// sending their names in brackets.
    /// 
    /// BACKSPACE, TAB, CLEAR, RETURN, LSHIFT, RSHIFT, LCONTROL, RCONTROL, LALT, RALT, PAUSE, CAPSLOCK, ESCAPE, SPACE,
    /// PAGEUP, PAGEDOWN, END, HOME, LEFT, UP, RIGHT, DOWN, PRINTSCREEN, INSERT, DELETE, LWIN, RWIN, APPS, NUMPAD0,
    /// NUMPAD1, NUMPAD2, NUMPAD3, NUMPAD4, NUMPAD5, NUMPAD6, NUMPAD7, NUMPAD8, NUMPAD9, MULTIPLY, ADD, SEPARATOR,
    /// SUBTRACT, DECIMAL, DIVIDE, F1, F2, F3, F4, F5, F6, F7, F8, F9, F10, F11, F12, F13, F14, F15, F16, F17, F18, F19,
    /// F20, F21, F22, F23, F24, NUMLOCK, SCROLLLOCK
    /// </summary>
    public class KeyboardEmulator
    {
        // selected keyboard layout for mapping of characters to key codes
        private static IntPtr _hkl;

        // keyboard layout of current thread at static init
        private static IntPtr _defaultLayout;

        private static bool _forceQwerty;

        private static KeyboardThread _keyboardThread;

        private static Dictionary<string, ushort> _keycodes = new Dictionary<string, ushort>{
            {"BACKSPACE", 0x08},
            {"TAB", 0x09},
            {"CLEAR", 0x0C},
            {"RETURN", 0x0D},
            {"LSHIFT", 0xA0},
            {"RSHIFT", 0xA1},
            {"LCONTROL", 0xA2},
            {"RCONTROL", 0xA3},
            {"LALT", 0xA4},
            {"RALT", 0xA5},
            {"PAUSE", 0x13},
            {"CAPSLOCK", 0x14},
            {"ESCAPE", 0x1B},
            {"SPACE", 0x20},
            {"PAGEUP", 0x21},
            {"PAGEDOWN", 0x22},
            {"END", 0x23},
            {"HOME", 0x24},
            {"LEFT", 0x25},
            {"UP", 0x26},
            {"RIGHT", 0x27},
            {"DOWN", 0x28},
            {"PRINTSCREEN", 0x2C},
            {"INSERT", 0x2D},
            {"DELETE", 0x2E},
            {"LWIN", 0x5B},
            {"RWIN", 0x5C},
            {"APPS", 0x5D},
            {"NUMPAD0", 0x60},
            {"NUMPAD1", 0x61},
            {"NUMPAD2", 0x62},
            {"NUMPAD3", 0x63},
            {"NUMPAD4", 0x64},
            {"NUMPAD5", 0x65},
            {"NUMPAD6", 0x66},
            {"NUMPAD7", 0x67},
            {"NUMPAD8", 0x68},
            {"NUMPAD9", 0x69},
            {"MULTIPLY", 0x6A},
            {"ADD", 0x6B},
            {"SEPARATOR", 0x6C},
            {"SUBTRACT", 0x6D},
            {"DECIMAL", 0x6E},
            {"DIVIDE", 0x6F},
            {"F1", 0x70},
            {"F2", 0x71},
            {"F3", 0x72},
            {"F4", 0x73},
            {"F5", 0x74},
            {"F6", 0x75},
            {"F7", 0x76},
            {"F8", 0x77},
            {"F9", 0x78},
            {"F10", 0x79},
            {"F11", 0x7A},
            {"F12", 0x7B},
            {"F13", 0x7C},
            {"F14", 0x7D},
            {"F15", 0x7E},
            {"F16", 0x7F},
            {"F17", 0x80},
            {"F18", 0x81},
            {"F19", 0x82},
            {"F20", 0x83},
            {"F21", 0x84},
            {"F22", 0x85},
            {"F23", 0x86},
            {"F24", 0x87},
            {"NUMLOCK", 0x90},
            {"SCROLLLOCK", 0x91},
            {"ENTER", 0x0D},                  // synonym for "RETURN".  This is deliberate. 
            {"NUMENTER", 0x10D },             // not a real keycode, high byte used to indicate an extended mapping which results in the scancode for ENTER on the numeric keypad.
            {"NUMPADENTER", 0x10D }
        };

        static KeyboardEmulator()
        {
            _keyboardThread = new KeyboardThread(30);
            _defaultLayout = NativeMethods.GetKeyboardLayout(0);
            _hkl = _defaultLayout;
        }

        private KeyboardEmulator()
        {
        }

        /// <summary>
        /// somewhat expensive operation to check keyboard layouts, not cached
        /// </summary>
        /// <returns></returns>
        public static bool CheckIfForceQwertyAvailable()
        {
            return FindQwertyLayout().HasValue;
        }

        private static IntPtr? FindQwertyLayout()
        {
            Int32 allocSize = NativeMethods.GetKeyboardLayoutList(0, null);

            // array of int ptrs, which are not actually pointers but machine word-sized integer handles 
            // containing padded int32 values that we don't have to deallocate
            IntPtr[] hkls = new IntPtr[allocSize];

            // native method to fill array with static handles
            Int32 count = NativeMethods.GetKeyboardLayoutList(hkls.Length, hkls);
            if (count != allocSize)
            {
                // broken native method wrapper? don't use the values
                return null;
            }
            foreach (IntPtr hkl in hkls)
            {
                ConfigManager.LogManager.LogDebug($"KeyboardEmulator: keyboard layout 0x{((UInt32)hkl):X8} detected (not necessarily active)");
                if ((((UInt32)hkl) & 0xffff0000) == 0x04090000)
                {
                    return hkl;
                }
            }
            return null;
        }

        public static int KeyDelay
        {
            get
            {
                return _keyboardThread.KeyDelay;
            }
            set
            {
                _keyboardThread.KeyDelay = value;
            }
        }

        public static bool ControlCenterSession
        {
            get
            {
                return _keyboardThread.ControlCenterSession;
            }
            set
            {
                _keyboardThread.ControlCenterSession = value;
            }
        }

        /// <summary>
        /// If enabled, always tries to use an available QWERTY mapping, since various software
        /// (such as BMS) actually bind to specific key codes instead of key names.  This means that
        /// hitting 'A' on an AZERTY keyboard and 'Q' on a QWERTY keyboard activate the same BMS key binding.
        /// To be compatible with this, we can enable this feature and send 'Q' on either keyboard and get
        /// the right result.   If sending keys to a windows application, this will of course send 'A' 
        /// if run on an AZERTY layout, since that is the key we are really sending.        /// 
        /// </summary>
        public static bool ForceQwerty
        {
            get
            {
                return _forceQwerty;
            }
            set
            {
                if (value == _forceQwerty)
                {
                    return;
                }
                // this will be configured and used only on the main thread,
                // since the keyboard thread gets pre-cooked events and does not
                // access _hkl.  therefore, we can safely change it here
                if (value)
                {
                    _hkl = FindQwertyLayout() ?? _defaultLayout;
                    ConfigManager.LogManager.LogInfo($"KeyboardEmulator: keyboard layout 0x{((UInt32)_hkl):X8} selected for forced QWERTY mode");
                }
                else
                {
                    _hkl = _defaultLayout;
                }
                _forceQwerty = value;
            }
        }

        private static List<NativeMethods.INPUT> CreateEvents(string keys, bool keyDown, bool reverse)
        {
            List<NativeMethods.INPUT> eventList = new List<NativeMethods.INPUT>();
            int length = keys.Length;
            for (int index = 0; index < length; index++)
            {
                char character = keys[index];
                if (character.Equals('{'))
                {
                    int endIndex = keys.IndexOf('}', index + 1);
                    if (endIndex > -1)
                    {
                        string keycode = keys.Substring(index + 1, endIndex - index - 1).ToUpper();
                        if (_keycodes.ContainsKey(keycode))
                        {
                            eventList.Add(CreateInput(_keycodes[keycode], keyDown));
                        }
                        index = endIndex;
                    }
                    else
                    {
                        index = length + 1;
                    }
                }
                else
                {
                    eventList.Add(CreateInput((ushort)NativeMethods.VkKeyScanEx(character, _hkl), keyDown));
                }
            }

            if (reverse)
            {
                eventList.Reverse();
            }

            return eventList;
        }

        private static NativeMethods.INPUT CreateInput(ushort virtualKeyCode, bool keyDown)
        {
            ushort ourCode = virtualKeyCode;
            if (ourCode > 0xff)
            {
                virtualKeyCode = (ushort)(virtualKeyCode & 0x00ff);
            }
            NativeMethods.INPUT input = new NativeMethods.INPUT();
            input.type = NativeMethods.INPUT_KEYBOARD;
            input.mkhi.ki.wVk = virtualKeyCode;
            input.mkhi.ki.time = 0;
            input.mkhi.ki.wScan = 0;
            input.mkhi.ki.dwExtraInfo = IntPtr.Zero;
            input.mkhi.ki.dwFlags = NativeMethods.KEY_SCANCODE;

            if (ourCode > 0xff ||
                (virtualKeyCode >= 0x21 && virtualKeyCode <= 0x28) ||
                virtualKeyCode == 0x2D ||
                virtualKeyCode == 0x2E ||
                (virtualKeyCode >= 0x5B && virtualKeyCode <= 0x5D) ||
                virtualKeyCode == 0x6F ||
                virtualKeyCode == 0x90 ||
                virtualKeyCode == 0xA3 ||
                virtualKeyCode == 0xA5)
            {
                input.mkhi.ki.dwFlags |= NativeMethods.KEY_EXTENDED;
            }

            uint scanCode = NativeMethods.MapVirtualKeyEx(virtualKeyCode, 0, _hkl);
            if (virtualKeyCode == 0x13)
            {
                scanCode = 0x04C5;                  // extended scancode for Pause
            }

            if (keyDown)
            {
                input.mkhi.ki.wScan = (ushort)(scanCode & 0xFF);
            }
            else
            {
                input.mkhi.ki.wScan = (ushort)scanCode;
                input.mkhi.ki.dwFlags |= NativeMethods.KEY_UP;
            }

            return input;
        }

        /// <summary>
        /// Sends key down events for the given keys.
        /// </summary>
        /// <param name="keys">Keys to send keydown events for.</param>
        public static void KeyDown(string keys)
        {            
            string[] keyList = Regex.Split(keys, @"\s+");
            foreach (string keyCombo in keyList)
            {
                _keyboardThread.AddEvents(CreateEvents(keys, true, false));
            }
        }

        /// <summary>
        /// Sends key up events for the given keys.
        /// </summary>
        /// <param name="keys">Keys to send keyup events for.</param>
        public static void KeyUp(string keys)
        {
            string[] keyList = Regex.Split(keys, @"\s+");
            foreach (string keyCombo in keyList)
            {
                _keyboardThread.AddEvents(CreateEvents(keys, false, false));
            }
        }

        /// <summary>
        /// Sends key down and up events for the given keys.
        /// </summary>
        /// <param name="keys">Keys to send key presses for.</param>
        public static void KeyPress(string keys)
        {
            List<NativeMethods.INPUT> events = new List<NativeMethods.INPUT>();

            string[] keyList = Regex.Split(keys, @"\s+");
            foreach (string keyCombo in keyList)
            {
                events.AddRange(CreateEvents(keyCombo, true, false));
                events.AddRange(CreateEvents(keyCombo, false, true));
            }

            _keyboardThread.AddEvents(events);
        }

        public static string KeyNameForVK(ushort VK)
        {
            foreach (KeyValuePair<string, ushort> kvp in _keycodes)
            {
                if (kvp.Value == VK)
                {
                    return kvp.Key;
                }
            }
            if (VK > 0)
            {
                return ((char)VK).ToString();
            }
            return "";
        }

        public static ushort KVKForKeyname(string name)
        {
            if (_keycodes.ContainsKey(name))
            {
                return _keycodes[name];
            }
            else if (name.Length == 1)
            {
                return (ushort)name[0];
            }
            else
            {
                return 0;
            }
        }

        public static string ModifierKeysToString(ModifierKeys modifiers)
        {
            string ret = "";
            if (modifiers.HasFlag(ModifierKeys.Shift)) ret += "SHIFT + ";
            if (modifiers.HasFlag(ModifierKeys.Control)) ret += "CONTROL + ";
            if (modifiers.HasFlag(ModifierKeys.Alt)) ret += "ALT + ";
            if (modifiers.HasFlag(ModifierKeys.Windows)) ret += "WINDOWS + ";
            return ret;
        }
    }
}
