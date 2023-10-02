using System.Windows;
using System.Windows.Controls;

namespace Cokee.ClassService.Views.Controls
{
    /// <summary>
    /// FloatCard.xaml 的交互逻辑
    /// </summary>
    public partial class FloatCard : UserControl
    {
        public int clkc = 0;
        public bool isDragging = false;
        private Point startPoint, _mouseDownControlPosition;
        public FloatCard()
        {
            InitializeComponent();
            moveBtn.MouseLeftButtonDown += (a, b) =>
            {
                moveBtn.CaptureMouse();
                isDragging = true;
                startPoint = b.GetPosition(this);
                _mouseDownControlPosition = new Point(transT.X, transT.Y);
            };
            moveBtn.MouseMove += (a, b) =>
            {
                if (isDragging)
                {
                    var pos = b.GetPosition(this);
                    // Catalog.ShowInfo($"{pos.ToString()}");
                    var dp = pos - startPoint;
                    if (pos.X >= SystemParameters.FullPrimaryScreenWidth - 10 || pos.Y >= SystemParameters.FullPrimaryScreenHeight - 10) { isDragging = false; transT.X = 0; transT.Y = 0; return; }
                    transT.X = _mouseDownControlPosition.X + dp.X;
                    transT.Y = _mouseDownControlPosition.Y + dp.Y;
                }
            };
            moveBtn.MouseLeftButtonUp += (a, b) =>
            {
                isDragging = false;
                moveBtn.ReleaseMouseCapture();
            };
            moveBtn.MouseRightButtonDown += (a, b) =>
            {
                clkc++;
                if (clkc == 2) Catalog.DeleteObjFromWindow(this);
                else Catalog.ShowInfo("再按一次删除");
            };
            infBtn.Click += (a, b) =>
            {
                content.FontSize+=5;
            };
            defBtn.Click += (a, b) =>
            {
                if(content.FontSize>=5) content.FontSize-=5;
            };
        }

    }
}
