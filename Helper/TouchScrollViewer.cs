using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// 可触摸滚动的ScrollViewer控件
    /// </summary>
    public class TouchableScrollViewer : ScrollViewer
    {
        //触摸点的坐标
        Point _startPosition;
        //滚动条当前位置
        double _startVerticalOffset;
        double _startHorizontalOffset;
        public TouchableScrollViewer()
        {
            TouchDown += TouchableScrollViewer_TouchDown;
            TouchUp += TouchableScrollViewer_TouchUp;
        }

      

        private void TouchableScrollViewer_TouchDown(object sender, TouchEventArgs e)
        {
            TouchPoint point = e.GetTouchPoint(this);
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, point.Position);
            //MessageBox.Show(hitTestResult.VisualHit.ToString());
            if (hitTestResult.VisualHit is ScrollBar)
            {
                return;
            }
            //添加触摸移动监听
            TouchMove -= TouchableScrollViewer_TouchMove;
            TouchMove += TouchableScrollViewer_TouchMove;

            //获取ScrollViewer滚动条当前位置
            _startVerticalOffset = VerticalOffset;
            _startHorizontalOffset = HorizontalOffset;

            //获取相对于ScrollViewer的触摸点位置
            
            _startPosition = point.Position;
        }

        private void TouchableScrollViewer_TouchUp(object sender, TouchEventArgs e)
        {
            //注销触摸移动监听
            TouchMove -= TouchableScrollViewer_TouchMove;
        }

        private void TouchableScrollViewer_TouchMove(object sender, TouchEventArgs e)
        {
            //获取相对于ScrollViewer的触摸点位置
            TouchPoint endPoint = e.GetTouchPoint(this);

            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, endPoint.Position);
            //MessageBox.Show(hitTestResult.VisualHit.ToString());
            if (hitTestResult.VisualHit is ScrollBar)
            {
                return;
            }
            //计算相对位置
            double diffOffsetY = endPoint.Position.Y - _startPosition.Y;
            double diffOffsetX = endPoint.Position.X - _startPosition.X;

            //ScrollViewer滚动到指定位置(指定位置=起始位置-移动的偏移量，滚动方向和手势方向相反)
            ScrollToVerticalOffset(_startVerticalOffset - diffOffsetY);
            ScrollToHorizontalOffset(_startHorizontalOffset - diffOffsetX);
        }
    }
}
