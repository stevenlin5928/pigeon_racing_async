using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameDetail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string myDate = "";
        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();

        public MainWindow()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            InitializeComponent();

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File(
                    path: "logs/log-.txt",
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 14,   // 保留 14 天
                    encoding: System.Text.Encoding.UTF8
                    ).CreateLogger();

            Log.Debug("程式開始");

            //utility u = new utility();
            //u.connectdb();
            comboBox_date.Items.Add(DateTime.Today.ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-2).ToString("yyyy/MM/dd"));
            comboBox_date.SelectedIndex = 0;

            comboBox_club.Items.Add("台南迎勝");
            comboBox_club.Items.Add("屏東青田(春)");
            comboBox_club.SelectedIndex = 0;

            
            comboBox_dispsn.Items.Add("3");
            comboBox_dispsn.Items.Add("4");
            comboBox_dispsn.Items.Add("5");
            comboBox_dispsn.Items.Add("6");
            comboBox_dispsn.Items.Add("7");
            comboBox_dispsn.Items.Add("8");
            comboBox_dispsn.Items.Add("9");
            comboBox_dispsn.Items.Add("10");
            comboBox_dispsn.Items.Add("0");
            comboBox_dispsn.SelectedIndex = 0;

            this.WindowState = WindowState.Maximized;

            Btn_Load.Foreground = Brushes.Blue;

            Txt_memberNo.Text = "";
        }

        private void disp(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                
            });
        }

        private void Btn_Cookie_Click(object sender, RoutedEventArgs e)
        {
            //var handler = new HttpClientHandler
            //{
            //    UseCookies = true,
            //    CookieContainer = new CookieContainer()
            //};

            //var client = new HttpClient(handler);

            //// 送出登入或一般請求
            //var response = client.GetAsync("http://www.087780212.tw/msg/login_pre.asp").Result;

            //// 抓 Cookie
            //var cookies = handler.CookieContainer.GetCookies(new Uri("http://www.087780212.tw"));
            ////mycookie = cookies["ASPSESSIONIDQQQQRACD"].Value;
            //foreach (Cookie c in cookies)
            //{
            //    mycookie= c.Name;
            //    //mycookie_v= c.Value;
            //    Listbox_Log.Items.Add($"{c.Name} = {c.Value}");
            //}

            //Txt_CookieName.Text = mycookie;
            //mycookie_v = Txt_CookieValue.Text;
        }

        private void Btn_Clean_Click(object sender, RoutedEventArgs e)
        {
            myRecord.Clear();
            Lbl_Message.Content = "";
        }

        private void Btn_Load_Click(object sender, RoutedEventArgs e)
        {
            myDate = comboBox_date.Text;
            int serial3 = int.Parse ( comboBox_dispsn.Text);
            string club_name = comboBox_club.Text;
            string member_no = Txt_memberNo.Text.Trim();

            objRacingRecordF1 objRacing = new objRacingRecordF1();
            myRecord = objRacing.Read(myDate, serial3, club_name, member_no);
            Lbl_Message.Content = $"{myDate} {club_name} 前 {serial3} 名 資料筆數 {myRecord.Count}";
            listView_record.ItemsSource = myRecord;
        }

        private void Btn_Topigeon_Click(object sender, RoutedEventArgs e)
        {
            topigeon_Load topigeonWindow = new topigeon_Load();
            topigeonWindow.Show();
        }

        private void Btn_ld_Click(object sender, RoutedEventArgs e)
        {
            ld_Load ld_Load = new ld_Load();
            ld_Load.Show();
        }

        private void MemberNo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var tb = sender as TextBlock;
            var data = tb.DataContext;   // 就是該列的資料物件

            // 你可以直接取出資料
            // var item = (YourModel)data;

            //MessageBox.Show("你點了：" + tb.Text);

            Txt_memberNo.Text = tb.Text;
        }
    }
}