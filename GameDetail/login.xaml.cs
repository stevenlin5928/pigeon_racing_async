using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GameDetail
{
    /// <summary>
    /// login.xaml 的互動邏輯
    /// </summary>
    public partial class login : Window
    {
        public string Club_name { get; private set; }
        public int Club_id { get; private set; }

        public login()
        {
            InitializeComponent();

            comboBox_club.Items.Add("102-台南迎勝");
            comboBox_club.Items.Add("091-屏東青田(春)");
            comboBox_club.Items.Add("103-佳冬民族");
            comboBox_club.SelectedIndex = 0;
        }

        private void Btn_login_Click(object sender, RoutedEventArgs e)
        {
            Club_name = comboBox_club.Text.Substring(4);
            Club_id = int.Parse( comboBox_club.Text.Substring(0,3) );
            DialogResult = true;   // 會自動關閉視窗

        }
    }
}
