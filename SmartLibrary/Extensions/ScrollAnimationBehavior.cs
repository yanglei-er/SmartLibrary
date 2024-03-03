using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace SmartLibrary.Extensions
{
    public partial class ScrollAnimationBehavior
    {
        public static double intendedLocation = 0;

        #region VerticalOffset Property

        public static DependencyProperty VerticalOffsetProperty =
            DependencyProperty.RegisterAttached("VerticalOffset",
                                                typeof(double),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(0.0, OnVerticalOffsetChanged));

        public static void SetVerticalOffset(FrameworkElement target, double value)
        {
            target.SetValue(VerticalOffsetProperty, value);
        }

        public static double GetVerticalOffset(FrameworkElement target)
        {
            return (double)target.GetValue(VerticalOffsetProperty);
        }

        #endregion VerticalOffset Property

        #region TimeDuration Property

        public static DependencyProperty TimeDurationProperty =
            DependencyProperty.RegisterAttached("TimeDuration",
                                                typeof(TimeSpan),
                                                typeof(ScrollAnimationBehavior),
                                                new PropertyMetadata(new TimeSpan(0, 0, 0, 0, 0)));

        public static void SetTimeDuration(FrameworkElement target, TimeSpan value)
        {
            target.SetValue(TimeDurationProperty, value);
        }

        public static TimeSpan GetTimeDuration(FrameworkElement target)
        {
            return (TimeSpan)target.GetValue(TimeDurationProperty);
        }

        #endregion TimeDuration Property

        #region PointsToScroll Property

        public static DependencyProperty PointsToScrollProperty =
            DependencyProperty.RegisterAttached("PointsToScroll",
                                                typeof(double),
                                                typeof(ScrollAnimationBehavior),
                                                new PropertyMetadata(0.0));

        public static void SetPointsToScroll(FrameworkElement target, double value)
        {
            target.SetValue(PointsToScrollProperty, value);
        }

        public static double GetPointsToScroll(FrameworkElement target)
        {
            return (double)target.GetValue(PointsToScrollProperty);
        }

        #endregion PointsToScroll Property

        #region OnVerticalOffset Changed

        private static void OnVerticalOffsetChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (target is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset((double)e.NewValue);
            }
        }

        #endregion OnVerticalOffset Changed

        #region IsEnabled Property

        public static DependencyProperty IsEnabledProperty =
                                                DependencyProperty.RegisterAttached("IsEnabled",
                                                typeof(bool),
                                                typeof(ScrollAnimationBehavior),
                                                new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

        #endregion IsEnabled Property

        #region OnIsEnabledChanged Changed

        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var target = sender;

            if (target != null && target is ScrollViewer)
            {
                if (target is ScrollViewer scroller)
                {
                    scroller.Loaded += new RoutedEventHandler(ScrollerLoaded);
                }
            }
        }

        #endregion OnIsEnabledChanged Changed

        #region AnimateScroll Helper

        private static void AnimateScroll(ScrollViewer scrollViewer, double ToValue)
        {
            scrollViewer.BeginAnimation(VerticalOffsetProperty, null);
            DoubleAnimation verticalAnimation = new()
            {
                From = scrollViewer.VerticalOffset,
                To = ToValue,
                Duration = new Duration(GetTimeDuration(scrollViewer))
            };
            scrollViewer.BeginAnimation(VerticalOffsetProperty, verticalAnimation);
        }

        #endregion AnimateScroll Helper

        #region NormalizeScrollPos Helper

        private static double NormalizeScrollPos(ScrollViewer scroll, double scrollChange, Orientation o)
        {
            double returnValue = scrollChange;

            if (scrollChange < 0)
            {
                returnValue = 0;
            }

            if (o == Orientation.Vertical && scrollChange > scroll.ScrollableHeight)
            {
                returnValue = scroll.ScrollableHeight;
            }
            else if (o == Orientation.Horizontal && scrollChange > scroll.ScrollableWidth)
            {
                returnValue = scroll.ScrollableWidth;
            }

            return returnValue;
        }

        #endregion NormalizeScrollPos Helper

        #region SetEventHandlersForScrollViewer Helper

        private static void SetEventHandlersForScrollViewer(ScrollViewer scroller)
        {
            scroller.PreviewMouseWheel += new MouseWheelEventHandler(ScrollViewerPreviewMouseWheel);
            scroller.PreviewKeyDown += new KeyEventHandler(ScrollViewerPreviewKeyDown);
            scroller.PreviewMouseLeftButtonUp += Scroller_PreviewMouseLeftButtonUp;
        }

        private static void Scroller_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            intendedLocation = ((ScrollViewer)sender).VerticalOffset;
        }

        #endregion SetEventHandlersForScrollViewer Helper

        #region ScrollerLoaded Event Handler

        private static void ScrollerLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                SetEventHandlersForScrollViewer(scroller);
            }
        }

        #endregion ScrollerLoaded Event Handler

        #region ScrollViewerPreviewMouseWheel Event Handler

        private static void ScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            double mouseWheelChange = (double)e.Delta;
            ScrollViewer scroller = (ScrollViewer)sender;
            double newVOffset = intendedLocation - (mouseWheelChange * 2);
            //We got hit by the mouse again. jump to the offset.
            scroller.ScrollToVerticalOffset(intendedLocation);
            if (newVOffset < 0)
            {
                newVOffset = 0;
            }
            if (newVOffset > scroller.ScrollableHeight)
            {
                newVOffset = scroller.ScrollableHeight;
            }

            AnimateScroll(scroller, newVOffset);
            intendedLocation = newVOffset;
            e.Handled = true;
        }

        #endregion ScrollViewerPreviewMouseWheel Event Handler

        #region ScrollViewerPreviewKeyDown Handler

        private static void ScrollViewerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            ScrollViewer scroller = (ScrollViewer)sender;

            Key keyPressed = e.Key;
            double newVerticalPos = GetVerticalOffset(scroller);
            bool isKeyHandled = false;

            if (keyPressed == Key.Down)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + GetPointsToScroll(scroller)), Orientation.Vertical);
                intendedLocation = newVerticalPos;
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageDown)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos + scroller.ViewportHeight), Orientation.Vertical);
                intendedLocation = newVerticalPos;
                isKeyHandled = true;
            }
            else if (keyPressed == Key.Up)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - GetPointsToScroll(scroller)), Orientation.Vertical);
                intendedLocation = newVerticalPos;
                isKeyHandled = true;
            }
            else if (keyPressed == Key.PageUp)
            {
                newVerticalPos = NormalizeScrollPos(scroller, (newVerticalPos - scroller.ViewportHeight), Orientation.Vertical);
                intendedLocation = newVerticalPos;
                isKeyHandled = true;
            }

            if (newVerticalPos != GetVerticalOffset(scroller))
            {
                intendedLocation = newVerticalPos;
                AnimateScroll(scroller, newVerticalPos);
            }

            e.Handled = isKeyHandled;
        }
    }

    #endregion ScrollViewerPreviewKeyDown Handler
}