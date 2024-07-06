using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Shared.Extensions
{
    public partial class TouchableScrollViewer
    {
        //触摸点的坐标
        private static Point _startPosition;

        //滚动条当前位置
        private static double _startVerticalOffset;
        private static double _startHorizontalOffset;

        #region IsEnabled Property

        private static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(TouchableScrollViewer), new UIPropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(FrameworkElement target, bool value)
        {
            target.SetValue(IsEnabledProperty, value);
        }

        public static bool GetIsEnabled(FrameworkElement target)
        {
            return (bool)target.GetValue(IsEnabledProperty);
        }

        #endregion

        #region OnIsEnabledChanged

        private static void OnIsEnabledChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                scroller.Loaded += new RoutedEventHandler(ScrollerLoaded);
            }
        }

        #endregion

        #region ScrollerLoaded Event Handler

        private static void ScrollerLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                scroller.TouchDown += TouchableScrollViewer_TouchDown;
                scroller.TouchUp += TouchableScrollViewer_TouchUp;
            }
        }

        #endregion ScrollerLoaded Event Handler

        private static void TouchableScrollViewer_TouchDown(object? sender, TouchEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                //添加触摸移动监听
                scroller.TouchMove -= TouchableScrollViewer_TouchMove;
                scroller.TouchMove += TouchableScrollViewer_TouchMove;

                //获取ScrollViewer滚动条当前位置
                _startVerticalOffset = scroller.VerticalOffset;
                _startHorizontalOffset = scroller.HorizontalOffset;

                //获取相对于ScrollViewer的触摸点位置
                TouchPoint point = e.GetTouchPoint(scroller);
                _startPosition = point.Position;
            }
        }

        private static void TouchableScrollViewer_TouchUp(object? sender, TouchEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                //注销触摸移动监听
                scroller.TouchMove -= TouchableScrollViewer_TouchMove;
            }
        }

        private static void TouchableScrollViewer_TouchMove(object? sender, TouchEventArgs e)
        {
            if (sender is ScrollViewer scroller)
            {
                //获取相对于ScrollViewer的触摸点位置
                TouchPoint endPoint = e.GetTouchPoint(scroller);
                //计算相对位置
                double diffOffsetY = endPoint.Position.Y - _startPosition.Y;
                double diffOffsetX = endPoint.Position.X - _startPosition.X;

                //ScrollViewer滚动到指定位置(指定位置=起始位置-移动的偏移量，滚动方向和手势方向相反)
                scroller.ScrollToVerticalOffset(_startVerticalOffset - diffOffsetY);
                scroller.ScrollToHorizontalOffset(_startHorizontalOffset - diffOffsetX);
            }
        }
    }
}
