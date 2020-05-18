using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace GadrocsWorkshop.Helios.Windows.Controls
{
    public class DialogWindow : Window
    {
        private bool _initialMeasurement = true;
        private Size? _initialSize;

        protected override Size MeasureOverride(Size availableSize)
        {
            if (_initialMeasurement)
            {
                _initialSize = FindAttachedProperties(this);
            }

            if (!(_initialMeasurement && _initialSize.HasValue))
            {
                return base.MeasureOverride(availableSize);
            }

            // lie on the first measurement of our content about available size to get the content to initial size
            _initialMeasurement = false;
            Size min = new Size(Math.Min(_initialSize.Value.Width, availableSize.Width), Math.Min(_initialSize.Value.Height, availableSize.Height));
            return base.MeasureOverride(min);
        }

        public Size? FindAttachedProperties(DependencyObject node)
        {
            if (node == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(node); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(node, i);
                if (new [] { DialogMaxWidthProperty, DialogMaxHeightProperty, DialogInitialWidthProperty, DialogInitialHeightProperty }.Any(prop => !IsPropertyDefault(child, prop)))
                {
                    // has some property set, so process this element only
                    return ConfigureFrom(child);
                }
                Size? initialSize = FindAttachedProperties(child);
                if (initialSize.HasValue)
                {
                    return initialSize;
                }
            }
            return null;
        }

        private void ConfigureFromChild(FrameworkElement child)
        {
            // set up the dialog to default sizes or sizes specified by child
            MaxWidth = GetDialogMaxWidth(child);
            MaxHeight = GetDialogMaxHeight(child);
            if (!double.IsInfinity(MaxWidth))
            {
                return;
            }

            // enable resizing but stay at initial size
            ResizeMode = ResizeMode.CanResizeWithGrip;
            SizeToContent = SizeToContent.Manual;
        }

        private Size? ConfigureFrom(DependencyObject node)
        {
            if (!(node is FrameworkElement child))
            {
                return new Size(double.PositiveInfinity, double.NegativeInfinity);
            }

            // change some stuff after the current measure/arrange/render cycle
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action<FrameworkElement>(ConfigureFromChild), child);

            return new Size(GetDialogInitialWidth(child), GetDialogInitialHeight(child));
        }

        public static bool IsPropertyDefault(DependencyObject obj, DependencyProperty dp)
        {
            BaseValueSource baseValueSource = DependencyPropertyHelper.GetValueSource(obj, dp).BaseValueSource;
            return baseValueSource == BaseValueSource.Default;
        }

        public static double GetDialogMaxWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(DialogMaxWidthProperty);
        }

        /// <summary>
        /// maximum width of a containing DialogWindow
        ///
        /// if set to "Infinity" then the hosting DialogWindow becomes resizable
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDialogMaxWidth(DependencyObject obj, double value)
        {
            obj.SetValue(DialogMaxWidthProperty, value);
        }

        public static readonly DependencyProperty DialogMaxWidthProperty =
            DependencyProperty.RegisterAttached("DialogMaxWidth", typeof(double), typeof(DialogWindow), new PropertyMetadata(800.0));

        public static double GetDialogInitialWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(DialogInitialWidthProperty);
        }

        /// <summary>
        /// if set on a control in content displayed in a DialogWindow, then the DialogWindow will lay out to that width initially, but
        /// not set a fixed width, so resizing is still possible
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDialogInitialWidth(DependencyObject obj, double value)
        {
            obj.SetValue(DialogInitialWidthProperty, value);
        }

        public static readonly DependencyProperty DialogInitialWidthProperty =
            DependencyProperty.RegisterAttached("DialogInitialWidth", typeof(double), typeof(DialogWindow), new PropertyMetadata(800.0));

        public static double GetDialogMaxHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(DialogMaxHeightProperty);
        }

        /// <summary>
        /// maximum height of containing DialogWindow
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDialogMaxHeight(DependencyObject obj, double value)
        {
            obj.SetValue(DialogMaxHeightProperty, value);
        }

        public static readonly DependencyProperty DialogMaxHeightProperty =
            DependencyProperty.RegisterAttached("DialogMaxHeight", typeof(double), typeof(DialogWindow), new PropertyMetadata(800.0));


        public static double GetDialogInitialHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(DialogInitialHeightProperty);
        }

        /// <summary>
        /// if set on a control in content displayed in a DialogWindow, then the DialogWindow will lay out to that height initially, but
        /// not set a fixed width, so resizing is still possible
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetDialogInitialHeight(DependencyObject obj, double value)
        {
            obj.SetValue(DialogInitialHeightProperty, value);
        }

        public static readonly DependencyProperty DialogInitialHeightProperty =
            DependencyProperty.RegisterAttached("DialogInitialHeight", typeof(double), typeof(DialogWindow), new PropertyMetadata(800.0));
    }
}