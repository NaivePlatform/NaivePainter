using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace NaivePainter
{
    /// <summary>
    /// SetSizesWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SetSizesWindow : Window
    {
        public List<PenSize> PenSizes { get; set; } = new List<PenSize>();

        private int SelectedId = 0;

        public SetSizesWindow()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            PenSizes.Clear();
            //从数据库中获取所有已经配置的颜色
            var dtSizes = SqlHelper.GetDT("SELECT * FROM tb_Size");

            //然后添加到Colors列表里
            foreach (DataRow rgb in dtSizes.Rows)
            {
                PenSizes.Add(new PenSize()
                {
                    Id = int.Parse(rgb[0].ToString()),
                    Thickness = int.Parse(rgb[1].ToString()),
                    Remark = rgb[2].ToString()
                });
            }
            dgSizes.ItemsSource = PenSizes;
            dgSizes.Items.Refresh();
        }

        private void AddSize(object sender, RoutedEventArgs e)
        {
            if (Validator())
                SqlHelper.RunSql("INSERT INTO tb_Size(Thickness, Remark) VALUES(@Thickness, @Remark)",
                    new SqlParameter("@Thickness", txtThickness.Text),
                    new SqlParameter("@Remark", txtRemark.Text));
            RefreshData();
        }

        private void EditSize(object sender, RoutedEventArgs e)
        {
            var seleted = (PenSize)dgSizes.SelectedItem;
            if (null != seleted)
                SqlHelper.RunSql("UPDATE tb_Size SET Thickness=@Thickness WHERE Id=@Id",
                    new SqlParameter("@Thickness", txtThickness.Text),
                    new SqlParameter("@Remark", txtRemark.Text),
                    new SqlParameter("@Id", SelectedId));
            else
                MessageBox.Show("请选中要修改的数据");
            RefreshData();
        }

        private void Cancle(object sender, RoutedEventArgs e)
        {
            txtThickness.Text = "4";
            txtRemark.Text = "";
            SelectedId = 0;
        }

        private void DeleteSize(object sender, RoutedEventArgs e)
        {
            var seleted = (PenSize)dgSizes.SelectedItem;
            if (null != seleted)
                SqlHelper.RunSql("DELETE FROM tb_Size WHERE Id=@Id",
                    new SqlParameter("@Id", SelectedId));
            else
                MessageBox.Show("请选中要删除的数据");
            RefreshData();
        }

        private bool Validator()
        {
            return !string.IsNullOrWhiteSpace(txtThickness.Text);
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var seleted = (PenSize)dgSizes.SelectedItem;
            if (null != seleted)
            {
                txtThickness.Text = seleted.Thickness.ToString();
                txtRemark.Text = seleted.Remark.ToString();
                SelectedId = seleted.Id;
            }
        }
    }

    public class PenSize
    {
        public int Id { get; set; }
        public int Thickness { get; set; }
        public string Remark { get; set; }
    }
}
