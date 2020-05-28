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
    using GadrocsWorkshop.Helios;
    using GadrocsWorkshop.Helios.ProfileEditor.UndoEvents;
    using GadrocsWorkshop.Helios.ProfileEditor.ViewModel;
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public class ItemDeleteEventArgs : EventArgs
    {
        HeliosObject _deletedItem;

        public ItemDeleteEventArgs(HeliosObject item)
        {
            _deletedItem = item;
        }

        public HeliosObject DeletedItem
        {
            get { return _deletedItem; }
        }
    }

    /// <summary>
    /// Interaction logic for ProjectExplorerPanel.xaml
    /// </summary>
    public partial class ProfileExplorerPanel : UserControl
    {
        public ProfileExplorerPanel()
        {
            DataContext = this;
            ProfileExplorerItems = new ProfileExplorerTreeItemCollection();
            InitializeComponent();
        }

        public event EventHandler<ItemDeleteEventArgs> ItemDeleting;

        #region Properties

        public ProfileExplorerTreeItemCollection ProfileExplorerItems
        {
            get { return (ProfileExplorerTreeItemCollection)GetValue(ProfileExplorerItemsProperty); }
            set { SetValue(ProfileExplorerItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ProfileExplorerItems.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileExplorerItemsProperty =
            DependencyProperty.Register("ProfileExplorerItems", typeof(ProfileExplorerTreeItemCollection), typeof(ProfileExplorerPanel), new PropertyMetadata(null));

        public HeliosProfile Profile
        {
            get { return (HeliosProfile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Profile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(HeliosProfile), typeof(ProfileExplorerPanel), new PropertyMetadata(null, new PropertyChangedCallback(OnItemReload)));

        #endregion

        private void LoadItems()
        {
            ProfileExplorerItems.Disconnect();
            ProfileExplorerItems.Clear();
            if (Profile != null)
            {                
                ProfileExplorerTreeItemType types = ProfileExplorerTreeItemType.Interface | ProfileExplorerTreeItemType.Monitor | ProfileExplorerTreeItemType.Panel;
                ProfileExplorerTreeItem item = new ProfileExplorerTreeItem(Profile, types);
                item.ExpandAll();
                ProfileExplorerItems = item.Children;
            }
        }

        private static void OnItemReload(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            ProfileExplorerPanel p = d as ProfileExplorerPanel;
            p.LoadItems();
        }

        private void TreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ProfileExplorerTreeItem item = ProfileExplorerTree.SelectedItem as ProfileExplorerTreeItem;
            if (item != null &&
                    (item.ItemType.HasFlag(ProfileExplorerTreeItemType.Panel) ||
                    item.ItemType.HasFlag(ProfileExplorerTreeItemType.Monitor) ||
                    item.ItemType.HasFlag(ProfileExplorerTreeItemType.Interface)))
            {
                ProfileEditorCommands.OpenProfileItem.Execute(item.ContextItem, this);
            }
            e.Handled = true;
        }

        private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ProfileExplorerTreeItem item = ProfileExplorerTree.SelectedItem as ProfileExplorerTreeItem;
            e.CanExecute = (item != null &&
                            (item.ItemType.HasFlag(ProfileExplorerTreeItemType.Panel) ||
                            item.ItemType.HasFlag(ProfileExplorerTreeItemType.Visual) ||
                            item.ItemType.HasFlag(ProfileExplorerTreeItemType.Interface)));
        }

        private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ProfileExplorerTreeItem item = ProfileExplorerTree.SelectedItem as ProfileExplorerTreeItem;
            if (item != null)
            {
                if (item.ItemType.HasFlag(ProfileExplorerTreeItemType.Panel) ||
                            item.ItemType.HasFlag(ProfileExplorerTreeItemType.Visual))
                {
                    HeliosVisual visual = item.ContextItem as HeliosVisual;
                    HeliosVisualContainer container = visual.Parent as HeliosVisualContainer;
                    if (container != null)
                    {
                        ConfigManager.UndoManager.AddUndoItem(new ControlDeleteUndoEvent(container, new List<HeliosVisual> { visual }, new List<int> { container.Children.IndexOf(visual) } ));
                        OnDeleting(visual);
                        container.Children.Remove(visual);
                    }
                }
                else if (item.ItemType.HasFlag(ProfileExplorerTreeItemType.Interface))
                {
                    HeliosInterface interfaceItem = item.ContextItem as HeliosInterface;
                    if (interfaceItem != null)
                    {
                        DeleteInterface(interfaceItem);
                    }
                }
            }
        }

        public void DeleteInterface(HeliosInterface interfaceItem)
        {
            if (MessageBox.Show(Window.GetWindow(this), "Are you sure you want to remove the " + interfaceItem.Name + " interface from the profile.  This will remove all bindings associated with this interface.", "Remove Interface", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No, MessageBoxOptions.None) == MessageBoxResult.Yes)
            {
                ConfigManager.UndoManager.AddUndoItem(new InterfaceDeleteUndoEvent(Profile, interfaceItem));
                OnDeleting(interfaceItem);
                Profile.Interfaces.Remove(interfaceItem);
            }
        }

        public void OnDeleting(HeliosObject item)
        {
            EventHandler<ItemDeleteEventArgs> handler = ItemDeleting;
            if (handler != null)
            {
                handler.Invoke(this, new ItemDeleteEventArgs(item));
            }
        }
    }
}
