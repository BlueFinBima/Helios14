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
    using System;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;

    public static class DragAndDropManager
    {
        private static readonly string DragOffsetFormat = "DnD.DragOffset";

        public static readonly DependencyProperty DragSourceAdvisorProperty = DependencyProperty.RegisterAttached("DragSourceAdvisor", typeof(IDragSourceAdvisor), typeof(DragAndDropManager),
                                                                                                            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDragSourceAdvisorChagned)));

        public static readonly DependencyProperty DropTargetAdvisorProperty = DependencyProperty.RegisterAttached("DropTargetAdvisor", typeof(IDropTargetAdvisor), typeof(DragAndDropManager),
                                                                                                            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnDropTargetAdvisorChagned)));

        private static Point _adornerPosition;

        private static UIElement _draggedElt;
        private static Point _dragStartPoint;
        private static bool _isMouseDown;
        private static Point _offsetPoint;
        private static DropPreviewAdorner _overlayElement;
        private static IDragSourceAdvisor s_currentDragSourceAdvisor;
        private static IDropTargetAdvisor s_currentDropTargetAdvisor;

        private static IDragSourceAdvisor CurrentDragSourceAdvisor
        {
            get { return s_currentDragSourceAdvisor; }
            set { s_currentDragSourceAdvisor = value; }
        }

        private static IDropTargetAdvisor CurrentDropTargetAdvisor
        {
            get { return s_currentDropTargetAdvisor; }
            set { s_currentDropTargetAdvisor = value; }
        }

        #region Dependency Properties Getter/Setters

        public static void SetDragSourceAdvisor(DependencyObject depObj, IDragSourceAdvisor advisor)
        {
            depObj.SetValue(DragSourceAdvisorProperty, advisor);
        }

        public static void SetDropTargetAdvisor(DependencyObject depObj, IDropTargetAdvisor advisor)
        {
            depObj.SetValue(DropTargetAdvisorProperty, advisor);
        }

        public static IDragSourceAdvisor GetDragSourceAdvisor(DependencyObject depObj)
        {
            return depObj.GetValue(DragSourceAdvisorProperty) as IDragSourceAdvisor;
        }

        public static IDropTargetAdvisor GetDropTargetAdvisor(DependencyObject depObj)
        {
            return depObj.GetValue(DropTargetAdvisorProperty) as IDropTargetAdvisor;
        }

        #endregion

        #region Attached Property Handlers

        private static void OnDragSourceAdvisorChagned(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            UIElement sourceElement = depObj as UIElement;
            if (args.NewValue != null && args.OldValue == null)
            {
                sourceElement.PreviewMouseLeftButtonDown += DragSource_PreviewMouseLeftButtonDown;
                sourceElement.PreviewMouseMove += DragSource_PreviewMouseMove;
                sourceElement.PreviewMouseUp += DragSource_PreviewMouseUp;

                IDragSourceAdvisor advisor = args.NewValue as IDragSourceAdvisor;
                advisor.SourceUI = sourceElement;
            }
            else if (args.NewValue == null && args.OldValue != null)
            {
                sourceElement.PreviewMouseLeftButtonDown -= DragSource_PreviewMouseLeftButtonDown;
                sourceElement.PreviewMouseMove -= DragSource_PreviewMouseMove;
                sourceElement.PreviewMouseUp -= DragSource_PreviewMouseUp;                
            }
        }

        private static void OnDropTargetAdvisorChagned(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            UIElement targetElement = depObj as UIElement;
            if (args.NewValue != null && args.OldValue == null)
            {
                targetElement.PreviewDragEnter += DropTarget_PreviewDragEnter;
                targetElement.PreviewDragOver += DropTarget_PreviewDragOver;
                targetElement.PreviewDragLeave += DropTarget_PreviewDragLeave;
                targetElement.PreviewDrop += DropTarget_PreviewDrop;
                targetElement.AllowDrop = true;

                IDropTargetAdvisor advisor = args.NewValue as IDropTargetAdvisor;
                advisor.TargetUI = targetElement;
            }
            else if (args.NewValue == null && args.OldValue != null)
            {
                targetElement.PreviewDragEnter -= DropTarget_PreviewDragEnter;
                targetElement.PreviewDragOver -= DropTarget_PreviewDragOver;
                targetElement.PreviewDragLeave -= DropTarget_PreviewDragLeave;
                targetElement.PreviewDrop -= DropTarget_PreviewDrop;
                targetElement.AllowDrop = false;
            }
        }

        #endregion

        #region Drag Source

        private static void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Make this the new drag source
            CurrentDragSourceAdvisor = GetDragSourceAdvisor(sender as DependencyObject);

            if (CurrentDragSourceAdvisor.IsDraggable(e.Source as UIElement, e.GetPosition(CurrentDragSourceAdvisor.GetSourceTopContainer())))
            {
                _draggedElt = e.Source as UIElement;
                _dragStartPoint = e.GetPosition(CurrentDragSourceAdvisor.GetSourceTopContainer());
                _offsetPoint = e.GetPosition(_draggedElt);
                _isMouseDown = true;
                e.Handled = true;
            }
            e.Handled = false;
        }

        private static void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMouseDown && IsDragGesture(e.GetPosition(CurrentDragSourceAdvisor.GetSourceTopContainer())))
            {
                DragStarted(sender as UIElement);
            }
        }

        private static void DragSource_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMouseDown && IsDragGesture(e.GetPosition(CurrentDragSourceAdvisor.GetSourceTopContainer())))
            {
                _isMouseDown = false;
                Mouse.Capture(null);
            }
        }

        private static void DragStarted(UIElement uiElt)
        {
            _isMouseDown = false;
            Mouse.Capture(uiElt);

            DataObject data = CurrentDragSourceAdvisor.GetDataObject(_draggedElt, _dragStartPoint);

            // NOTE: observed as being null during testing
            if (data != null)
            {
                data.SetData(DragOffsetFormat, _offsetPoint);
                DragDropEffects supportedEffects = CurrentDragSourceAdvisor.SupportedEffects;

                // Perform DragDrop
                DragDropEffects effects = System.Windows.DragDrop.DoDragDrop(_draggedElt, data, supportedEffects);
                CurrentDragSourceAdvisor.FinishDrag(_draggedElt, _dragStartPoint, effects);
            }

            // Clean up
            RemovePreviewAdorner();
            Mouse.Capture(null);
            _draggedElt = null;
        }

        #endregion

        #region Drop Target

        private static void DropTarget_PreviewDrop(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(sender as UIElement);
            UpdateEffects(e, dropPoint);

            // Calculate displacement for (Left, Top)
            if (_overlayElement != null)
            {
                Point offset = e.GetPosition(_overlayElement);
                dropPoint.X = dropPoint.X - offset.X;
                dropPoint.Y = dropPoint.Y - offset.Y;
            }

            RemovePreviewAdorner();
            _offsetPoint = new Point(0, 0);

            if (e.AllowedEffects != DragDropEffects.None)
            {
                CurrentDropTargetAdvisor.OnDropCompleted(e.Data, dropPoint);
                e.Handled = true;
            }            
        }

        private static void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(sender as UIElement);
            UpdateEffects(e, dropPoint);

            if (e.AllowedEffects != DragDropEffects.None)
            {
                RemovePreviewAdorner();
                e.Handled = true;
            }
        }

        private static void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
        {
            Point dropPoint = e.GetPosition(sender as UIElement);
            UpdateEffects(e, dropPoint);

            // Update position of the preview Adorner
            if (e.AllowedEffects != DragDropEffects.None)
            {
                _adornerPosition = e.GetPosition(sender as UIElement);
                PositionAdorner();

                e.Handled = true;
            }
        }

        private static void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
        {
            // Get the current drop target advisor
            CurrentDropTargetAdvisor = GetDropTargetAdvisor(sender as DependencyObject);

            Point dropPoint = e.GetPosition(sender as UIElement);
            UpdateEffects(e, dropPoint);

            if (e.AllowedEffects != DragDropEffects.None)
            {
                // Setup the preview Adorner
                _offsetPoint = new Point();
                if (CurrentDropTargetAdvisor.ApplyMouseOffset && e.Data.GetData(DragOffsetFormat) != null)
                {
                    _offsetPoint = (Point)e.Data.GetData(DragOffsetFormat);
                }
                CreatePreviewAdorner(sender as UIElement, e.Data);

                e.Handled = true;
            }
        }

        private static void UpdateEffects(DragEventArgs e, Point dropPoint)
        {
            if (CurrentDropTargetAdvisor.IsValidDataObject(e.Data, dropPoint) == false)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) == 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) == 0)
            {
                e.Effects = DragDropEffects.None;
            }

            else if ((e.AllowedEffects & DragDropEffects.Move) != 0 &&
                     (e.AllowedEffects & DragDropEffects.Copy) != 0)
            {
                e.Effects = ((e.KeyStates & DragDropKeyStates.ControlKey) != 0)
                                ? DragDropEffects.Copy
                                : DragDropEffects.Move;
            }
        }

        #endregion

        #region Helpers

        private static bool IsDragGesture(Point point)
        {
            bool hGesture = Math.Abs(point.X - _dragStartPoint.X) >
                            SystemParameters.MinimumHorizontalDragDistance;
            bool vGesture = Math.Abs(point.Y - _dragStartPoint.Y) >
                            SystemParameters.MinimumVerticalDragDistance;

            return (hGesture | vGesture);
        }

        private static void CreatePreviewAdorner(UIElement adornedElt, IDataObject data)
        {
            if (_overlayElement != null)
            {
                return;
            }

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(CurrentDropTargetAdvisor.GetTargetTopContainer());
            UIElement feedbackUI = CurrentDropTargetAdvisor.GetVisualFeedback(data);
            _overlayElement = new DropPreviewAdorner(feedbackUI, adornedElt);
            PositionAdorner();
            layer.Add(_overlayElement);
        }

        private static void PositionAdorner()
        {
            if (_overlayElement != null)
            {
                Point adornerPosition = new Point(_adornerPosition.X - _offsetPoint.X, _adornerPosition.Y - _offsetPoint.Y);
                Point adjustedPosition = CurrentDropTargetAdvisor.GetVisualFeedbackLocation(adornerPosition);
                _overlayElement.Left = adjustedPosition.X;
                _overlayElement.Top = adjustedPosition.Y;
            }
        }

        private static void RemovePreviewAdorner()
        {
            if (_overlayElement != null)
            {
                AdornerLayer.GetAdornerLayer(CurrentDropTargetAdvisor.GetTargetTopContainer()).Remove(_overlayElement);
                _overlayElement = null;
            }
        }

        #endregion
    }
}
