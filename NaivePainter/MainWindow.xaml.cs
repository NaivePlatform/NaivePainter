using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace NaivePainter
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Brush> Colors = new List<Brush>();
        private List<int> Sizes = new List<int>();
        private Brush CurrentColor { get; set; }
        private int CurrentSize { get; set; }
        private bool ShouldPaint { get; set; }
        private bool ShouldErase { get; set; }
        private Point lastPosition = new Point();
        private int CurrentLineCount = 0;
        private int TotalLineCount = 0;

        public MainWindow()
        {
            InitializeComponent();

            InitializeColors();
            InitializeSizes();

            InitializeColorsGroupBox();
            InitializeSizesGroupBox();

            ShouldPaint = false;
            ShouldErase = false;
        }

        /// <summary>
        /// 从数据库获取颜色信息
        /// </summary>
        private void InitializeColors()
        {
            Colors.Clear();
            //从数据库中获取所有已经配置的颜色
            var dtColors = SqlHelper.GetDT("SELECT * FROM tb_Color");

            //然后添加到Colors列表里
            foreach (DataRow rgb in dtColors.Rows)
            {
                Colors.Add(new SolidColorBrush(Color.FromRgb(byte.Parse(rgb["Red"].ToString()),
                    byte.Parse(rgb["Green"].ToString()),
                    byte.Parse(rgb["Blue"].ToString()))));
            }

            //如果数据库没有颜色数据，把默认颜色设置为黑色；否则设置为第一个颜色
            if (0 < Colors.Count)
                CurrentColor = Colors[0];
            else
                CurrentColor = Brushes.Black;
        }

        /// <summary>
        /// 从数据库获取画笔信息
        /// </summary>
        private void InitializeSizes()
        {
            Sizes.Clear();
            //从数据库中获取所有已经配置的画笔
            var dtSizes = SqlHelper.GetDT("SELECT * FROM tb_Size");

            //然后添加到Sizes列表里
            foreach (DataRow size in dtSizes.Rows)
            {
                Sizes.Add(int.Parse(size["Thickness"].ToString()));
            }

            //如果数据库没有画笔数据，把默认画笔粗细为4；否则设置为第一个画笔粗细值
            if (0 < Sizes.Count)
                CurrentSize = Sizes[0];
            else
                CurrentSize = 4;
        }

        /// <summary>
        /// 在界面上添加颜色
        /// </summary>
        private void InitializeColorsGroupBox()
        {
            stackColors.Children.Clear();
            foreach (var color in Colors)
            {
                stackColors.Children.Add(new Button()
                {
                    Width = 30,
                    Height = 30,
                    Background = color,
                    BorderBrush = Brushes.Black,
                    Margin = new Thickness(2)
                });
            }
        }

        /// <summary>
        /// 在界面上添加画笔
        /// </summary>
        private void InitializeSizesGroupBox()
        {
            stackSizes.Children.Clear();
            foreach (var size in Sizes)
            {
                stackSizes.Children.Add(new Button()
                {
                    Width = 25,
                    Tag = size,
                    Background = Brushes.White,
                    Margin = new Thickness(20, 0, 20, 0),
                    Content = new Path()
                    {
                        Fill = Brushes.Black,
                        Data = Geometry.Parse($"M 0 0 L 0 40 L {size} 40 L {size} 0")
                    }
                });
            }
        }

        /// <summary>
        /// 刷新界面
        /// </summary>
        private void RefreshUI()
        {
            InitializeColors();
            InitializeSizes();
            InitializeColorsGroupBox();
            InitializeSizesGroupBox();
        }

        private void DrawLine(Brush color, Point point1, Point point2)
        {
            canvasPaint.Children.Add(new Line()
            {
                Stroke = color,
                X1 = point1.X,
                Y1 = point1.Y,
                X2 = point2.X,
                Y2 = point2.Y,
                StrokeThickness = CurrentSize
            });
        }

        private void PaintCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Pen;
            ShouldPaint = true;

            lastPosition = e.GetPosition(canvasPaint);
            CurrentLineCount = 0;
        }

        private void PaintCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            ShouldPaint = false;
        }

        private void PaintCanvas_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Hand;
            ShouldErase = true;
        }

        private void PaintCanvas_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Cursor = Cursors.Arrow;
            ShouldErase = false;
        }

        private void PaintCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (ShouldPaint)
            {
                Point mousePosition = e.GetPosition(canvasPaint);
                DrawLine(CurrentColor, lastPosition, mousePosition);
                lastPosition = mousePosition;
                CurrentLineCount++;
                TotalLineCount++;
            }
            else if (ShouldErase)
            {
                Point mousePosition = e.GetPosition(canvasPaint);
                DrawLine(Brushes.White, lastPosition, mousePosition);
                lastPosition = mousePosition;
                TotalLineCount++;
            }
        }

        private void ChooseColor(object sender, RoutedEventArgs e)
        {
            CurrentColor = (e.OriginalSource as ContentControl)?.Background;
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            int count = canvasPaint.Children.Count;
            if (count > 0)
            {
                if (CurrentLineCount <= TotalLineCount)
                {
                    canvasPaint.Children.RemoveRange(TotalLineCount - CurrentLineCount, CurrentLineCount);
                    TotalLineCount -= CurrentLineCount;
                }
                else
                {
                    canvasPaint.Children.Clear();
                }
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            canvasPaint.Children.Clear();
        }

        private void ChooseSize(object sender, RoutedEventArgs e)
        {
            CurrentSize = int.Parse((e.OriginalSource as Button).Tag.ToString());
        }

        private void SetColor(object sender, RoutedEventArgs e)
        {
            new SetColorsWindow().ShowDialog();
            InitializeColors();
            RefreshUI();
        }

        private void SetSize(object sender, RoutedEventArgs e)
        {
            new SetSizesWindow().ShowDialog();
            InitializeSizes();
            RefreshUI();
        }
    }
}
