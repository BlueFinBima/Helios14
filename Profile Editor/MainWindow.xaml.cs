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
    using GadrocsWorkshop.Helios.ComponentModel;
    using GadrocsWorkshop.Helios.Controls;
    using GadrocsWorkshop.Helios.ProfileEditor.UndoEvents;
    using GadrocsWorkshop.Helios.ProfileEditor.ViewModel;
    using GadrocsWorkshop.Helios.Windows.Controls;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Threading;
    using Xceed.Wpf.AvalonDock.Layout;
    using Xceed.Wpf.AvalonDock.Layout.Serialization;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Internal Class used to track open documents
        /// </summary>
        private class DocumentMeta
        {
            public HeliosObject hobj;
            public LayoutDocument document;
            public HeliosEditorDocument editor;
        }

        private delegate HeliosProfile LoadProfileDelegate(string filename);
        private delegate void LayoutDelegate(string filename);

        private XmlLayoutSerializer _layoutSerializer;
        private string _systemDefaultLayout;
        private string _defalutLayoutFile;

        private LoadingAdorner _loadingAdorner;
        private List<DocumentMeta> _documents = new List<DocumentMeta>();

        private bool _initialLoad = true;

        public MainWindow()
        {
            InitializeComponent();

            DockManager.ActiveContentChanged += new EventHandler(DockManager_ActiveDocumentChanged);
            NewProfile();

            _layoutSerializer = new XmlLayoutSerializer(this.DockManager);
            _layoutSerializer.LayoutSerializationCallback += LayoutSerializer_LayoutSerializationCallback;

            _defalutLayoutFile = System.IO.Path.Combine(ConfigManager.DocumentPath, "DefaultLayout.hply");
        }

        void LayoutSerializer_LayoutSerializationCallback(object sender, LayoutSerializationCallbackEventArgs e)
        {            
            if (Profile != null && e.Model is LayoutDocument)
            {
                HeliosObject profileObject = HeliosSerializer.ResolveReferenceName(Profile, e.Model.ContentId);
                if (profileObject != null)
                {
                    HeliosEditorDocument editor = CreateDocumentEditor(profileObject);
                    profileObject.PropertyChanged += DocumentObject_PropertyChanged;
                    e.Content = CreateDocumentContent(editor);
                    //DocumentPane.Children.Add((LayoutDocument)e.Model);
                    e.Model.Closed += Document_Closed;
                    AddDocumentMeta(profileObject, (LayoutDocument)e.Model, editor);
                } else
                {
                    ConfigManager.LogManager.LogDebug("Unable to resolve Layout Document " + e.Model.ContentId);
                }
            }
        }

        #region Properties

        public HeliosProfile Profile
        {
            get { return (HeliosProfile)GetValue(ProfileProperty); }
            set { SetValue(ProfileProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Profile.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ProfileProperty =
            DependencyProperty.Register("Profile", typeof(HeliosProfile), typeof(MainWindow), new PropertyMetadata(null));

        public int Monitor
        {
            get { return (int)GetValue(MonitorProperty); }
            set { SetValue(MonitorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Monitor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MonitorProperty =
            DependencyProperty.Register("Monitor", typeof(int), typeof(MainWindow), new PropertyMetadata(0));

        public string StatusBarMessage
        {
            get { return (string)GetValue(StatusBarMessageProperty); }
            set { SetValue(StatusBarMessageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StatusBarMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StatusBarMessageProperty =
            DependencyProperty.Register("StatusBarMessage", typeof(string), typeof(MainWindow), new PropertyMetadata(""));

        public HeliosEditorDocument CurrentEditor
        {
            get { return (HeliosEditorDocument)GetValue(CurrentEditorProperty); }
            set { SetValue(CurrentEditorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentEditor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentEditorProperty =
            DependencyProperty.Register("CurrentEditor", typeof(HeliosEditorDocument), typeof(MainWindow), new PropertyMetadata(null));

        #endregion

        #region Event Handlers

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            if (_initialLoad)
            {
                _initialLoad = false;
                Helios.ProfileEditor.App app = Application.Current as Helios.ProfileEditor.App;
                if (app != null && app.StartupProfile != null && System.IO.File.Exists(app.StartupProfile))
                {
                    LoadProfile(app.StartupProfile);
                }
            }
        }

        void DockManager_ActiveDocumentChanged(object sender, EventArgs e)
        {
            HeliosEditorDocument activeDocument = null;

            // Interface Editor documents are embeded in a scrollviewer.  Unwrap them if they are
            // the current content.
            ScrollViewer viewer = DockManager.ActiveContent as ScrollViewer;
            if (viewer != null)
            {
                activeDocument = viewer.Content as HeliosEditorDocument;
            }
            else
            {
                activeDocument = DockManager.ActiveContent as HeliosEditorDocument;
            }            
            if (activeDocument != null)
            {
                if (activeDocument != CurrentEditor)
                {
                    CurrentEditor = activeDocument;
                }
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ProfileProperty)
            {
                HeliosProfile oldProfile = e.OldValue as HeliosProfile;
                if (oldProfile != null)
                {
                    oldProfile.PropertyChanged -= new PropertyChangedEventHandler(Profile_PropertyChanged);
                }

                foreach (DocumentMeta meta in _documents.ToArray())
                {
                    meta.document.Close();
                    meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
                }
                _documents.Clear();

                if (Profile != null)
                {
                    Profile.PropertyChanged += new PropertyChangedEventHandler(Profile_PropertyChanged);
                }
            }
            base.OnPropertyChanged(e);
        }

        void Profile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyNotificationEventArgs args = e as PropertyNotificationEventArgs;
            if (args != null)
            {
                ConfigManager.UndoManager.AddPropertyChange(sender, args);
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!CheckSave())
            {
                e.Cancel = true;
            }

            if (e.Cancel == false)
            {
                ConfigManager.UndoManager.ClearHistory();

                // Persist window placement details to application settings
                NativeMethods.WINDOWPLACEMENT wp = new NativeMethods.WINDOWPLACEMENT();
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.GetWindowPlacement(hwnd, out wp);

                ConfigManager.SettingsManager.SaveSetting("ProfileEditor", "WindowLocation", wp.normalPosition);

                // Close all open documents so they can clean up
                while (_documents.Count > 0)
                {
                    _documents[0].document.Close();
                }
            }
            base.OnClosing(e);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            // Load window placement details for previous application session from application settings
            // Note - if window was closed on a monitor that is now disconnected from the computer,
            //        SetWindowPlacement will place the window onto a visible monitor.

            if (ConfigManager.SettingsManager.IsSettingAvailable("ProfileEditor", "WindowLocation"))
            {
                NativeMethods.WINDOWPLACEMENT wp = new NativeMethods.WINDOWPLACEMENT();
                wp.normalPosition = ConfigManager.SettingsManager.LoadSetting("ProfileEditor", "WindowLocation", new NativeMethods.RECT(0, 0, (int)Width, (int)Height));
                wp.length = Marshal.SizeOf(typeof(NativeMethods.WINDOWPLACEMENT));
                wp.flags = 0;
                wp.showCmd = (wp.showCmd == NativeMethods.SW_SHOWMINIMIZED ? NativeMethods.SW_SHOWNORMAL : wp.showCmd);
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                NativeMethods.SetWindowPlacement(hwnd, ref wp);
            }

            base.OnSourceInitialized(e);
        }

        #endregion

        #region Adorners

        private LoadingAdorner GetLoadingAdorner()
        {
            if (_loadingAdorner == null)
            {
                _loadingAdorner = new LoadingAdorner(PrimaryGrid);
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(PrimaryGrid);
                layer.Add(_loadingAdorner);
            }
            return _loadingAdorner;
        }

        private void RemoveLoadingAdorner()
        {
            if (_loadingAdorner != null)
            {
                _loadingAdorner.Visibility = Visibility.Hidden;
                AdornerLayer layer = AdornerLayer.GetAdornerLayer(PrimaryGrid);
                layer.Remove(_loadingAdorner);
                _loadingAdorner = null;
            }
        }

        #endregion

        #region Helper Methods

        private DocumentMeta AddDocumentMeta(HeliosObject profileObject, LayoutDocument document, HeliosEditorDocument editor)
        {
            DocumentMeta meta = new DocumentMeta();
            meta.editor = editor;
            meta.document = document;
            meta.hobj = profileObject;

            _documents.Add(meta);

            return meta;
        }

        private DocumentMeta FindDocumentMeta(HeliosObject profileObject)
        {
            foreach(DocumentMeta meta in _documents)
            {
                if (meta.hobj == profileObject)
                {
                    return meta;
                }
            }

            return null;
        }

        private DocumentMeta FindDocumentMeta(LayoutDocument document)
        {
            foreach (DocumentMeta meta in _documents)
            {
                if (meta.document == document)
                {
                    return meta;
                }
            }

            return null;
        }

        private DocumentMeta FindDocumentMeta(HeliosEditorDocument editor)
        {
            foreach (DocumentMeta meta in _documents)
            {
                if (meta.editor == editor)
                {
                    return meta;
                }
            }

            return null;
        }

        private DocumentMeta AddNewDocument(HeliosObject profileObject)
        {

            DocumentMeta meta = FindDocumentMeta(profileObject);
            if (meta != null)
            {
                meta.document.IsSelected = true;
                return meta;
            }


            HeliosEditorDocument editor = CreateDocumentEditor(profileObject);
            if (editor != null)
            {
                LayoutDocument document = new LayoutDocument();

                document.Title = editor.Title;
                document.IsSelected = true;
                document.ContentId = HeliosSerializer.GetReferenceName(profileObject);
                document.Content = CreateDocumentContent(editor);
                // Since a new LayoutRoot object is created upon de-serialization, the Child LayoutDocumentPane no longer belongs to the LayoutRoot 
                // therefore the LayoutDocumentPane 'DocumentPane' must be referred to dynamically
                // change added by yzfanimal
                LayoutDocumentPane DocumentPane = this.DockManager.Layout.Descendents().OfType<LayoutDocumentPane>().FirstOrDefault();
                if (DocumentPane != null)
                {
                    DocumentPane.Children.Add(document);
                }
                document.Closed += Document_Closed;

                meta = AddDocumentMeta(profileObject, document, editor);
                profileObject.PropertyChanged += DocumentObject_PropertyChanged;
            }

            return meta;
        }
        
        private HeliosEditorDocument CreateDocumentEditor(HeliosObject profileObject)
        {
            HeliosEditorDocument editor = null;

            if (profileObject is Monitor)
            {
                editor = new MonitorDocument((Monitor)profileObject);
            }
            else if (profileObject is HeliosPanel)
            {
                editor = new PanelDocument((HeliosPanel)profileObject);
            }
            else if (profileObject is HeliosInterface)
            {
                editor = ConfigManager.ModuleManager.CreateInterfaceEditor((HeliosInterface)profileObject, Profile);
                if (editor != null)
                {
                    editor.Style = App.Current.Resources["InterfaceEditor"] as Style;
                }
            }
            else
            {
                throw new ArgumentException("Cannot create a editor document for profileobject requested.", "profileObject");
            }

            return editor;
        }

        private object CreateDocumentContent(HeliosEditorDocument editor)
        {
            if (editor.HandlesScroll)
            {
                return editor;
            }
            else
            {
                ScrollViewer scroller = new ScrollViewer();
                scroller.Content = editor;
                scroller.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
                scroller.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                return scroller;
            }
        }

        void Document_Closed(object sender, EventArgs e)
        {
            DocumentMeta meta = FindDocumentMeta(sender as LayoutDocument);

            if (meta == null)
            {
                throw new InvalidOperationException("Document closed called for a document not found in meta data.");
            }

            meta.editor.Closed();
            meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
            meta.document.Closed -= Document_Closed;

            _documents.Remove(meta);
        }

        void DocumentObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            DocumentMeta meta = FindDocumentMeta(sender as HeliosObject);
            if (meta == null)
            {
                throw new InvalidOperationException("Property Changed closed called for a profile object not found in meta data.");
            }

            if (e.PropertyName.Equals("Name"))
            {
                meta.document.Title = meta.editor.Title;
                meta.document.ContentId = "doc:" + HeliosSerializer.GetReferenceName(meta.hobj);
            }
        }

        #endregion

        #region Profile Persistance

        private void NewProfile()
        {
            if (CheckSave())
            {
                Profile = new HeliosProfile();
                ConfigManager.UndoManager.ClearHistory();

                AddNewDocument(Profile.Monitors[0]);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private bool CheckSave()
        {
            if (ConfigManager.UndoManager.CanUndo || (Profile != null && Profile.IsDirty))
            {
                MessageBoxResult result = MessageBox.Show(this, "There are changes to the current profile.  If you continue without saving your changes, they will be lost.  Would you like to save the current profile?", "Save Changes", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    return SaveProfile();
                }
                return (result != MessageBoxResult.Cancel);
            }
            else
            {
                return true;
            }
        }

        private void OpenProfile()
        {
            if (CheckSave())
            {
                Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
                dlg.FileName = Profile.Name; // Default file name
                dlg.DefaultExt = ".hpf"; // Default file extension
                dlg.Filter = "Helios Profiles (.hpf)|*.hpf"; // Filter files by extension
                dlg.InitialDirectory = ConfigManager.ProfilePath;
                dlg.ValidateNames = true;
                dlg.AddExtension = true;
                dlg.Multiselect = false;
                dlg.Title = "Open Profile";

                // Show open file dialog box
                Nullable<bool> result = dlg.ShowDialog(this);

                // Process open file dialog box results
                if (result == true)
                {
                    LoadProfile(dlg.FileName);
                }
            }
        }

        private void LoadProfile(string path)
        {
            StatusBarMessage = "Loading Profile...";
            GetLoadingAdorner();

            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                HeliosProfile profile = ConfigManager.ProfileManager.LoadProfile(path, Dispatcher);

                ConfigManager.UndoManager.ClearHistory();

                // Load the graphics so everything is more responsive after load
                if (profile != null)
                {
                    foreach (Monitor monitor in profile.Monitors)
                    {
                        LoadVisual(monitor);
                    }
                }

                Dispatcher.Invoke(DispatcherPriority.Send, (System.Threading.SendOrPostCallback)delegate { SetValue(ProfileProperty, profile); }, profile);

                Dispatcher.Invoke(DispatcherPriority.Background, new Action(RemoveLoadingAdorner));

                Dispatcher.Invoke(DispatcherPriority.Background, (System.Threading.SendOrPostCallback)delegate { SetValue(StatusBarMessageProperty, ""); }, "");

                // TODO Restore docking panel layout
                if (profile != null)
                {
                    string layoutFileName = Path.ChangeExtension(profile.Path, "hply");
                    if (File.Exists(layoutFileName))
                    {
                        Dispatcher.Invoke(DispatcherPriority.Background, (LayoutDelegate)_layoutSerializer.Deserialize, layoutFileName);
                    }
                } else
                {
                    ConfigManager.LogManager.LogDebug("Docking Panel Layout Problem.  Profile Object Null during restore of hply for " + path);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            });
            t.Start();

        }

        private void LoadVisual(HeliosVisual visual)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new Action<HeliosVisual, Dispatcher>(PreLoadrenderer), visual, Dispatcher);
            foreach (HeliosVisual control in visual.Children)
            {
                LoadVisual(control);
            }
        }

        private void PreLoadrenderer(HeliosVisual visual, Dispatcher dispatcher)
        {
            visual.Renderer.Dispatcher = dispatcher;
            visual.Renderer.Refresh();
        }

        private bool SaveProfile()
        {
            if (Profile.Path == null || Profile.Path.Length == 0)
            {
                return SaveAsProfile();
            }
            else
            {
                WriteProfile(Profile);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return true;
            }
        }

        private void WriteProfile(HeliosProfile profile)
        {
            StatusBarMessage = "Saving Profile...";
            GetLoadingAdorner();

            System.Threading.Thread t = new System.Threading.Thread(delegate()
            {
                if (!ConfigManager.ProfileManager.SaveProfile(profile))
                {
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)ErrorDuringSave);
                }
                ConfigManager.UndoManager.ClearHistory();
                profile.IsDirty = false;
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(RemoveLoadingAdorner));
                Dispatcher.Invoke(DispatcherPriority.Background, (System.Threading.SendOrPostCallback)delegate { SetValue(StatusBarMessageProperty, ""); }, "");

                string layoutFileName = Path.ChangeExtension(profile.Path, "hply");
                if (File.Exists(layoutFileName))
                {
                    File.Delete(layoutFileName);
                }
                Dispatcher.Invoke(DispatcherPriority.Background, (LayoutDelegate)_layoutSerializer.Serialize, layoutFileName);
            });
            t.Start();
        }

        private void ErrorDuringSave()
        {
            MessageBox.Show(this, "There was an error saving your profile.  Please contact support.", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool SaveAsProfile()
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = Profile.Name; // Default file name
            dlg.DefaultExt = ".hpf"; // Default file extension
            dlg.Filter = "Helios Profiles (.hpf)|*.hpf"; // Filter files by extension
            dlg.InitialDirectory = ConfigManager.ProfilePath;
            dlg.OverwritePrompt = true;
            dlg.ValidateNames = true;
            dlg.AddExtension = true;
            dlg.Title = "Save Profile As";

            // Show save file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process save file dialog box results
            if (result == true)
            {
                // Save document
                Profile.Path = dlg.FileName;
                Profile.Name = System.IO.Path.GetFileNameWithoutExtension(Profile.Path);
                WriteProfile(Profile);
                GC.Collect();
                GC.WaitForPendingFinalizers();
                return true;
            }

            return false;
        }

        #endregion

        private void About_Click(object sender, RoutedEventArgs e)
        {
            About dialog = new About();
            dialog.Owner = this;
            dialog.ShowDialog();
        }

        #region Commands

        private void New_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            NewProfile();
        }

        private void Open_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            OpenProfile();
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveProfile();
        }

        private void SaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            SaveAsProfile();
        }

        private void Close_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void OpenProfileItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DocumentMeta meta = AddNewDocument(e.Parameter as HeliosObject);
            if (meta != null)
            {
                if (meta.document.Content is HeliosVisualContainerEditor)
                {
                    meta.editor.SetBindingFocus((HeliosObject)e.Parameter);
                }
            }
        }

        private void CloseProfileItem_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CloseProfileItem(e.Parameter);
        }

        private void CloseProfileItem(object item)
        {
            DocumentMeta meta = FindDocumentMeta(item as HeliosObject);

            if (meta != null)
            {
                meta.document.Close();
                meta.hobj.PropertyChanged -= DocumentObject_PropertyChanged;
            }
        }

        public void OnExecuteUndo(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigManager.UndoManager.Undo();
        }

        public void OnCanExecuteUndo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ConfigManager.UndoManager.CanUndo;
        }

        public void OnExecuteRedo(object sender, ExecutedRoutedEventArgs e)
        {
            ConfigManager.UndoManager.Redo();
        }

        public void OnCanExecuteRedo(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ConfigManager.UndoManager.CanRedo;
        }

        private void AddInterface_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddInterfaceDialog dialog = new AddInterfaceDialog(Profile);
            dialog.Owner = this;

            try
            {
                Nullable<bool> result = dialog.ShowDialog();
                if (result == true && dialog.SelectedInterface != null)
                {
                    string name = dialog.SelectedInterface.Name;
                    int count = 0;
                    while (Profile.Interfaces.ContainsKey(name))
                    {
                        name = dialog.SelectedInterface.Name + " " + ++count;
                    }
                    dialog.SelectedInterface.Name = name;

                    ConfigManager.UndoManager.AddUndoItem(new InterfaceAddUndoEvent(Profile, dialog.SelectedInterface));
                    Profile.Interfaces.Add(dialog.SelectedInterface);
                    AddNewDocument(dialog.SelectedInterface);
                }
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("AddInterface - Error during add Interface dialog or creation", ex);
            }
        }

        private void SaveLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defalutLayoutFile))
            {
                File.Delete(_defalutLayoutFile);
            }
            _layoutSerializer.Serialize(_defalutLayoutFile);
        }

        private void LoadLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defalutLayoutFile))
            {
                _layoutSerializer.Deserialize(_defalutLayoutFile);
            }
            else
            {
                StringReader reader = new StringReader(_systemDefaultLayout);
                _layoutSerializer.Deserialize(reader);
            }
        }

        private void RestoreDefaultLayout_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (File.Exists(_defalutLayoutFile))
            {
                File.Delete(_defalutLayoutFile);
            }
            StringReader reader = new StringReader(_systemDefaultLayout);
            _layoutSerializer.Deserialize(reader);
        }

        #endregion

        private void Show_Preview(object sender, RoutedEventArgs e)
        {
            PreviewPane.Show();
        }

        private void Show_Toolbox(object sender, RoutedEventArgs e)
        {
            ToolboxPane.Show();
        }

        private void Show_Explorer(object sender, RoutedEventArgs e)
        {
            ExplorerPane.Show();
        }

        private void Show_Properties(object sender, RoutedEventArgs e)
        {
            PropertiesPane.Show();
        }

        private void Show_Bindings(object sender, RoutedEventArgs e)
        {
            BindingsPane.Show();
        }

        private void Show_Layers(object sender, RoutedEventArgs e)
        {
            LayersPane.Show();
        }

        private void Show_TemplateManager(object sender, RoutedEventArgs e)
        {
            TemplateManagerWindow tm = new TemplateManagerWindow();
            tm.Owner = this;
            tm.ShowDialog();
        }

        private void ResetMonitors_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ResetMonitors resetDialog = new ResetMonitors(Profile);
            resetDialog.Owner = this;
            bool? reset = resetDialog.ShowDialog();

            if (reset.HasValue && reset.Value)
            {
                GetLoadingAdorner();
                // this value comes from a dependency property that can only be 
                // read on this thread
                HeliosProfile profile = Profile;

                // now run monitor import as another thread
                new System.Threading.Thread(() => { ResetMonitorsThread(resetDialog, profile); }).Start();
            }
        }

        private void ResetMonitorsThread(ResetMonitors resetDialog, HeliosProfile profile)
        {
            ConfigManager.UndoManager.StartBatch();
            ConfigManager.LogManager.LogDebug("Resetting Monitors");
            try
            {
                // WARNING: monitor naming is 1-based but indexing and NewMonitor references are 0-based
                Monitor[] localMonitors = ConfigManager.DisplayManager.Displays.ToArray<Monitor>();
                
                // pass1: process all new and/or old monitors in order
                int existingMonitors = Math.Min(localMonitors.Length, profile.Monitors.Count);
                int totalMonitors = Math.Max(localMonitors.Length, profile.Monitors.Count);

                // monitors that are preserved
                for (int monitorIndex = 0; monitorIndex < existingMonitors; monitorIndex++)
                {
                    MonitorResetItem item = resetDialog.MonitorResets[monitorIndex];
                    ResetExistingMonitor(monitorIndex, item);
                }

                // monitors added (may be zero iterations)
                for (int monitorIndex = existingMonitors; monitorIndex < localMonitors.Length; monitorIndex++)
                {
                    // monitorIndex does not refer to any reset item, as we are off the end of the list
                    ResetAddedMonitor(profile, monitorIndex, localMonitors[monitorIndex]);
                }

                // monitors removed (may be zero iterations)
                for (int monitorIndex = existingMonitors; monitorIndex < resetDialog.MonitorResets.Count; monitorIndex++)
                {
                    MonitorResetItem item = resetDialog.MonitorResets[monitorIndex];
                    ResetRemovedMonitor(profile, monitorIndex, item, localMonitors.Length);
                }

                // pass2: place all controls that were temporarily lifted and
                // copy over any settings from source to target monitors
                foreach (MonitorResetItem item in resetDialog.MonitorResets)
                {
                    ConfigManager.LogManager.LogDebug($"Placing controls for old monitor {item.OldMonitor.Name} onto Monitor {item.NewMonitor + 1}");
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action<Monitor>(item.PlaceControls), profile.Monitors[item.NewMonitor]);
                    Dispatcher.Invoke(DispatcherPriority.Background, new Action<Monitor>(item.CopySettings), profile.Monitors[item.NewMonitor]);
                }

                ConfigManager.UndoManager.CloseBatch();
            }
            catch (Exception ex)
            {
                ConfigManager.LogManager.LogError("Reset Monitors - Unhandled exception", ex);
                ConfigManager.LogManager.LogError("Rolling back any undoable operations from monitor reset");
                ConfigManager.UndoManager.UndoBatch();
                MessageBox.Show("Error encountered while resetting monitors; please file a bug with the contents of the application log", "Error");
            }
            finally
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(RemoveLoadingAdorner));
            }
        }

        private void ResetRemovedMonitor(HeliosProfile profile, int monitorIndex, MonitorResetItem item, int monitorToRemove)
        {
            ConfigManager.LogManager.LogDebug($"Removing Monitor {monitorIndex + 1} and saving its controls for replacement");
            Dispatcher.Invoke(DispatcherPriority.Background, new Action<HeliosObject>(CloseProfileItem), profile.Monitors[monitorToRemove]);
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(item.RemoveControls));
            ConfigManager.UndoManager.AddUndoItem(new DeleteMonitorUndoEvent(profile, profile.Monitors[monitorToRemove], monitorToRemove));
            profile.Monitors.RemoveAt(monitorToRemove);
        }

        // WARNING: monitorIndex refers to a new monitor that does not yet exist, and there is no monitor reset item for it
        private void ResetAddedMonitor(HeliosProfile profile, int monitorIndex, Monitor display)
        {
            ConfigManager.LogManager.LogDebug($"Adding Monitor {monitorIndex + 1}");
            Monitor monitor = new Monitor(display);
            monitor.Name = $"Monitor {monitorIndex + 1}";
            monitor.FillBackground = false;
            ConfigManager.UndoManager.AddUndoItem(new AddMonitorUndoEvent(profile, monitor));
            Dispatcher.Invoke(DispatcherPriority.Background, new Action<Monitor>(profile.Monitors.Add), monitor);
        }

        private void ResetExistingMonitor(int monitorIndex, MonitorResetItem item)
        {
            if (item.NewMonitor != monitorIndex)
            {
                ConfigManager.LogManager.LogDebug($"Removing controls from Monitor {monitorIndex + 1} for replacement");
                Dispatcher.Invoke(DispatcherPriority.Background, new Action(item.RemoveControls));
            }
            ConfigManager.LogManager.LogDebug($"Resetting Monitor {monitorIndex + 1}");
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(item.Reset));
        }

        private void DockManager_Loaded(object sender, RoutedEventArgs e)
        {
            StringWriter systemDefaultLayoutWriter = new StringWriter();
            _layoutSerializer.Serialize(systemDefaultLayoutWriter);
            _systemDefaultLayout = systemDefaultLayoutWriter.ToString();
        }

        private void Donate_Click_Gadroc(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=2MMREAY3KDXJ6");
        }
        private void Donate_Click_Current(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://bluefinbima.github.io/helios/donate/");
        }

        private void Explorer_ItemDeleting(object sender, ItemDeleteEventArgs e)
        {
            CloseProfileItem(e.DeletedItem);
        }
    }
}

