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

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    using System.Windows.Input;

    public static class ProfileEditorCommands
    {
        private static RoutedUICommand MoveBackCommand = new RoutedUICommand("Move Back", "MoveBack", typeof(ProfileEditorCommands),new InputGestureCollection() { new KeyGesture(Key.Subtract, ModifierKeys.Control) });
        private static RoutedUICommand MoveForwardCommand = new RoutedUICommand("Move Forward", "MoveFoward", typeof(ProfileEditorCommands), new InputGestureCollection() { new KeyGesture(Key.Add, ModifierKeys.Control) });
        private static RoutedUICommand AlignTopCommand = new RoutedUICommand("Align Top", "AlignTop", typeof(ProfileEditorCommands));
        private static RoutedUICommand AlignBottomCommand = new RoutedUICommand("Align Bottom", "AlignBottom", typeof(ProfileEditorCommands));
        private static RoutedUICommand AlignLeftCommand = new RoutedUICommand("Align Left", "AlignLeft", typeof(ProfileEditorCommands));
        private static RoutedUICommand AlignRightCommand = new RoutedUICommand("Align Right", "AlignRight", typeof(ProfileEditorCommands));
        private static RoutedUICommand AlignHorizontalCenterCommand = new RoutedUICommand("Align Horizontal Center", "AlignHorizontalCenter", typeof(ProfileEditorCommands));
        private static RoutedUICommand AlignVerticalCenterCommand = new RoutedUICommand("Align Vertical Center", "AlignVerticalCenter", typeof(ProfileEditorCommands));
        private static RoutedUICommand DistributeHorizontalCenterCommand = new RoutedUICommand("Distrubute Horizontal Center", "DistributeHorizontalCenter", typeof(ProfileEditorCommands));
        private static RoutedUICommand DistributeVerticalCenterCommand = new RoutedUICommand("Distrubute Vertical Center", "DistrubuteVerticalCenter", typeof(ProfileEditorCommands));
        private static RoutedUICommand SpaceHorizontalCommand = new RoutedUICommand("Evenly space horizontaly", "SpaceHorizontal", typeof(ProfileEditorCommands));
        private static RoutedUICommand SpaceVerticalCommand = new RoutedUICommand("Evenlly space verticaly", "SpaceVertical", typeof(ProfileEditorCommands));
        private static RoutedUICommand SaveTemplateCommand = new RoutedUICommand("Save Current Item as a Template", "SaveTemplate", typeof(ProfileEditorCommands));
        private static RoutedUICommand OpenProfileItemCommand = new RoutedUICommand("Opens a document window to the given item.", "OpenProfileItem", typeof(ProfileEditorCommands));
        private static RoutedUICommand CloseProfileItemCommand = new RoutedUICommand("Closes a document window to the given item.", "CloseProfileItem", typeof(ProfileEditorCommands));
        private static RoutedUICommand AddInterfaceCommand = new RoutedUICommand("Add an interface to this profile.", "AddInterface", typeof(ProfileEditorCommands));
        private static RoutedUICommand SaveLayoutCommand = new RoutedUICommand("Saves current layout as the user default.", "SaveLayout", typeof(ProfileEditorCommands));
        private static RoutedUICommand LoadLayoutCommand = new RoutedUICommand("Loads the user default layout.", "LoadLayout", typeof(ProfileEditorCommands));
        private static RoutedUICommand RestoreDefaultLayoutCommand = new RoutedUICommand("Restores user default layout to system default.", "SaveLayout", typeof(ProfileEditorCommands));

        public static RoutedUICommand MoveBack { get { return MoveBackCommand; } }
        public static RoutedUICommand MoveForward { get { return MoveForwardCommand; } }
        public static RoutedUICommand AlignTop { get { return AlignTopCommand; } }
        public static RoutedUICommand AlignBottom { get { return AlignBottomCommand; } }
        public static RoutedUICommand AlignLeft { get { return AlignLeftCommand; } }
        public static RoutedUICommand AlignRight { get { return AlignRightCommand; } }
        public static RoutedUICommand AlignHorizontalCenter { get { return AlignHorizontalCenterCommand; } }
        public static RoutedUICommand AlignVerticalCenter { get { return AlignVerticalCenterCommand; } }
        public static RoutedUICommand DistributeHorizontalCenter { get { return DistributeHorizontalCenterCommand; } }
        public static RoutedUICommand DistributeVerticalCenter { get { return DistributeVerticalCenterCommand; } }
        public static RoutedUICommand SpaceHorizontal { get { return SpaceHorizontalCommand; } }
        public static RoutedUICommand SpaceVertical { get { return SpaceVerticalCommand; } }
        public static RoutedUICommand SaveTemplate { get { return SaveTemplateCommand; } }
        public static RoutedUICommand OpenProfileItem { get { return OpenProfileItemCommand; } }
        public static RoutedUICommand CloseProfileItem { get { return CloseProfileItemCommand; } }
        public static RoutedUICommand AddInterface { get { return AddInterfaceCommand; } }
        public static RoutedUICommand SaveLayout { get { return SaveLayoutCommand; } }
        public static RoutedUICommand LoadLayout { get { return LoadLayoutCommand; } }
        public static RoutedUICommand RestoreDefaultLayout { get { return RestoreDefaultLayoutCommand; } }
    }
}
