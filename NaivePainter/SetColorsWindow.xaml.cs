using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NaivePainter
{
    /// <summary>
    /// SetColorsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetColorsWindow : Window
    {
        public List<RGBColor> RGBColors { get; set; } = new List<RGBColor>();

        private int SelectedId = 0;

        public SetColorsWindow()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            RGBColors.Clear();
            //从数据库中获取所有已经配置的颜色
            var dtColors = SqlHelper.GetDT("SELECT * FROM tb_Color");

            //然后添加到Colors列表里
            foreach (DataRow rgb in dtColors.Rows)
            {
                RGBColors.Add(new RGBColor()
                {
                    Id = int.Parse(rgb[0].ToString()),
                    Red = int.Parse(rgb[1].ToString()),
                    Green = int.Parse(rgb[2].ToString()),
                    Blue = int.Parse(rgb[3].ToString()),
                    Remark = rgb[4].ToString()
                });
            }
            dgColors.ItemsSource = RGBColors;
            dgColors.Items.Refresh();
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            SolidColorBrush backgroundColor = new SolidColorBrush()
            {
                Color = Color.FromRgb(
                (byte)sliderRed.Value,
                (byte)sliderGreen.Value,
                (byte)sliderBlue.Value)
            };
            lblColor.Background = backgroundColor;
        }

        private void AddColor(object sender, RoutedEventArgs e)
        {
            if (Validator())
                SqlHelper.RunSql("INSERT INTO tb_Color(Red, Green, Blue, Remark) VALUES(@Red, @Green, @Blue, @Remark)",
                    new SqlParameter("@Red", int.Parse(txtRed.Text)),
                    new SqlParameter("@Green", int.Parse(txtGreen.Text)),
                    new SqlParameter("@Blue", int.Parse(txtBlue.Text)),
                    new SqlParameter("@Remark", txtRemark.Text));
            RefreshData();
        }

        private void EditColor(object sender, RoutedEventArgs e)
        {
            var seleted = (RGBColor)dgColors.SelectedItem;
            if (null != seleted)
                SqlHelper.RunSql("UPDATE tb_Color SET Red=@Red, Green=@Green, Blue=@Blue, Remark=@Remark WHERE Id=@Id",
                    new SqlParameter("@Id", SelectedId),
                    new SqlParameter("@Red", int.Parse(txtRed.Text)),
                    new SqlParameter("@Green", int.Parse(txtGreen.Text)),
                    new SqlParameter("@Blue", int.Parse(txtBlue.Text)),
                    new SqlParameter("@Remark", txtRemark.Text));
            else
                MessageBox.Show("请选中要修改的数据");
            RefreshData();
        }

        private void Cancle(object sender, RoutedEventArgs e)
        {
            txtRed.Text = "0";
            txtGreen.Text = "0";
            txtBlue.Text = "0";
            txtRemark.Text = "";
            SelectedId = 0;
        }

        private void DeleteColor(object sender, RoutedEventArgs e)
        {
            var seleted = (RGBColor)dgColors.SelectedItem;
            if (null != seleted)
                SqlHelper.RunSql("DELETE FROM tb_Color WHERE Id=@Id",
                    new SqlParameter("@Id", SelectedId));
            else
                MessageBox.Show("请选中要删除的数据");
            RefreshData();
        }

        private bool Validator()
        {
            return !string.IsNullOrWhiteSpace(txtBlue.Text)
                || !string.IsNullOrWhiteSpace(txtRed.Text)
                || !string.IsNullOrWhiteSpace(txtGreen.Text)
                || ValidatorValue(txtBlue.Text)
                || ValidatorValue(txtRed.Text)
                || ValidatorValue(txtGreen.Text);
        }

        private bool ValidatorValue(string strValue)
        {
            int value = int.Parse(strValue);
            return value < 256 && value > 0;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleted = (RGBColor)dgColors.SelectedItem;
            if (null != seleted)
            {
                txtRed.Text = seleted.Red.ToString();
                txtGreen.Text = seleted.Green.ToString();
                txtBlue.Text = seleted.Blue.ToString();
                txtRemark.Text = seleted.Remark.ToString();
                SelectedId = seleted.Id;
            }
        }
    }

    public class RGBColor
    {
        public int Id { get; set; }
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }
        public string Remark { get; set; }
    }
}
