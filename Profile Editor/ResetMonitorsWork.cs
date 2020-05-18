using System;
using System.Collections.Generic;
using System.Linq;

namespace GadrocsWorkshop.Helios.ProfileEditor
{
    internal class ResetMonitorsWork
    {
        internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public interface ICallbacks
        {
            void CloseProfileItem(object item);
        }

        public static IEnumerable<string> ResetMonitors(ICallbacks parent, ResetMonitors resetDialog, HeliosProfile profile)
        {
            Logger.Debug("resetting Monitors");

            // WARNING: monitor naming is 1-based but indexing and NewMonitor references are 0-based
            Monitor[] localMonitors = ConfigManager.DisplayManager.Displays.ToArray<Monitor>();
            yield return "acquired displays";

            // pass1: process all new and/or old monitors in order
            int existingMonitors = Math.Min(localMonitors.Length, profile.Monitors.Count);

            // monitors that are preserved
            for (int monitorIndex = 0; monitorIndex < existingMonitors; monitorIndex++)
            {
                ViewModel.MonitorResetItem item = resetDialog.MonitorResets[monitorIndex];
                foreach (string progress in ResetExistingMonitor(monitorIndex, item))
                {
                    yield return progress;
                }
                yield return $"reset existing monitor {monitorIndex + 1}";
            }

            // monitors added (may be zero iterations)
            for (int monitorIndex = existingMonitors; monitorIndex < localMonitors.Length; monitorIndex++)
            {
                // monitorIndex does not refer to any reset item, as we are off the end of the list
                ResetAddedMonitor(profile, monitorIndex, localMonitors[monitorIndex]);
                yield return $"reset added monitor {monitorIndex + 1}";
            }

            // monitors removed (may be zero iterations)
            for (int monitorIndex = existingMonitors; monitorIndex < resetDialog.MonitorResets.Count; monitorIndex++)
            {
                ViewModel.MonitorResetItem item = resetDialog.MonitorResets[monitorIndex];
                foreach (string progress in ResetRemovedMonitor(parent, profile, monitorIndex, item, localMonitors.Length))
                {
                    yield return progress;
                }
                yield return $"reset removed monitor {monitorIndex + 1}";
            }

            // pass2: place all controls that were temporarily lifted and
            // copy over any settings from source to target monitors
            foreach (ViewModel.MonitorResetItem item in resetDialog.MonitorResets)
            {
                Logger.Debug($"placing controls for old monitor {item.OldMonitor.Name} onto Monitor {item.NewMonitor + 1}");
                foreach (string progress in item.PlaceControls(profile.Monitors[item.NewMonitor]))
                {
                    yield return progress;
                }
                item.CopySettings(profile.Monitors[item.NewMonitor]);
            }
        }

        private static IEnumerable<string> ResetRemovedMonitor(ICallbacks parent, HeliosProfile profile, int monitorIndex, ViewModel.MonitorResetItem item, int monitorToRemove)
        {
            Logger.Debug($"removing Monitor {monitorIndex + 1} and saving its controls for replacement");
            parent.CloseProfileItem(profile.Monitors[monitorToRemove]);
            foreach (string progress in item.RemoveControls())
            {
                yield return progress;
            }
            ConfigManager.UndoManager.AddUndoItem(new UndoEvents.DeleteMonitorUndoEvent(profile, profile.Monitors[monitorToRemove], monitorToRemove));
            profile.Monitors.RemoveAt(monitorToRemove);
        }

        // WARNING: monitorIndex refers to a new monitor that does not yet exist, and there is no monitor reset item for it
        private static void ResetAddedMonitor(HeliosProfile profile, int monitorIndex, Monitor display)
        {
            Logger.Debug($"adding Monitor {monitorIndex + 1}");
            Monitor monitor = new Monitor(display) {Name = $"Monitor {monitorIndex + 1}", FillBackground = false};
            ConfigManager.UndoManager.AddUndoItem(new UndoEvents.AddMonitorUndoEvent(profile, monitor));
            profile.Monitors.Add(monitor);
        }

        private static IEnumerable<string> ResetExistingMonitor(int monitorIndex, ViewModel.MonitorResetItem item)
        {
            if (item.NewMonitor != monitorIndex)
            {
                Logger.Debug($"removing controls from Monitor {monitorIndex + 1} for replacement");
                foreach (string progress in item.RemoveControls())
                {
                    yield return progress;
                }
            }
            Logger.Debug($"resetting Monitor {monitorIndex + 1}");
            foreach (string progress in item.Reset())
            {
                yield return progress;
            }
        }
    }
}
