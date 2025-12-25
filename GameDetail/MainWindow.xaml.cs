using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
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
        private static string mycookie = "";
        private static string mycookie_v = "";

        private static string html_head = "";
        private static string html_footer = "";

        private static int pigeon_count = 0;
        private static int member_count = 0;

        List<string> _list = new List<string>();

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

        }

        private void disp(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                Listbox_Log.Items.Add(msg);
            });
        }

        private void Btn_Read_Click(object sender, RoutedEventArgs e)
        {
            mycookie = Txt_CookieName.Text;
            mycookie_v = Txt_CookieValue.Text;

            Task.Run(() =>
            {
                disp("開始讀取比賽資料...");
             
                procHtml(1);

                disp("讀取比賽資料結束...");
                
            });
            

            //int total_pages = (int)Math.Ceiling((double)pigeon_count / 100);
            //for (int page_index = 2; page_index <= total_pages; page_index++)
            //{
            //    Thread.Sleep(5000);
            //    procHtml(page_index);
            //}
        }


        private void procHtml(int page_index)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var url = $"http://www.087780212.tw/msg/dailyGameDetail.asp?udate=2025/12/25&ucgp=215&uhouse=0&page={page_index}";

            // 建立 HttpClient + CookieContainer
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };


            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Referer", "http://www.087780212.tw/msg/main.asp");

            // ★★★ 把瀏覽器的 Cookie 填進來 ★★★
            handler.CookieContainer.Add(
                new Uri("http://www.087780212.tw"),
                new Cookie(mycookie, mycookie_v)
            //new Cookie("ASPSESSIONIDSQQQSCCA", "GDPPEADCBJHANCALEKHCENMP")
            );

            handler.CookieContainer.Add(
                new Uri("http://www.087780212.tw"),
                new Cookie("timeState", "1")
            );

            disp("Downloading page...");

            var html = client.GetStringAsync(url).Result;
            var newHtml=html.Replace("bgcolor='#EEEEFF", "bgcolor='#FFFFFF");

            if(html.Contains("default.asp") == true)
            {
                disp("❌ Cookie 已失效，請重新取得 Cookie");
                return;
            }
            
            _list = newHtml.Split("<TR bgcolor='#FFFFFF' >").ToList();

            html_head = _list[0];
            //html_footer = _list[_list.Count - 1];

            for (int index = 1;index < _list.Count; index++)
            {
                if(index == _list.Count - 1)
                {
                    string s = _list[index];
                    int _i = s.IndexOf("\n");
                    s = s.Substring(0, _i);
                    _list[index] = s;
                    //Listbox_Log.Items.Add(s);
                    break;
                }
                //Listbox_Log.Items.Add(_list[index]);
            }
            if(page_index == 1)
                parsorHead();

            ParsorHtml_2(_list);

            if (_list.Count < 1)
            {
                disp("❌ 無資料");
                return;
            }

        }

        private void ParsorHtml_2(List<string> _list)
        {
            for (int index = 1; index < _list.Count; index++)
            {
                string s = _list[index];
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(s);
                var tds = doc.DocumentNode.SelectNodes("//td");
                if (tds != null )
                {
                    string SerialNo = tds[0].InnerText.Trim();
                    string SerialNo2 = tds[1].InnerText.Trim();
                    string SerialNo3 = tds[2].InnerText.Trim();
                    string ClubName = tds[3].InnerText.Trim();
                    string Member = tds[4].InnerText.Trim();
                    string RingID = tds[5].InnerText.Trim();
                    string Date1 = tds[6].InnerText.Trim();
                    string Time1 = tds[7].InnerText.Trim();
                    //06時42分29.14秒
                    string myDatetime = Date1 + " " + Time1.Replace("時", ":").Replace("分", ":").Replace("秒", "");
                    DateTime dt = DateTime.ParseExact(myDatetime, "yyyy/MM/dd HH:mm:ss.ff", null);

                    disp($"序: {SerialNo}, 順序: {SerialNo2}, 序號2: {SerialNo3}, 鴿會: {ClubName}, 會員: {Member}, " +
                        $"腳環號碼: {RingID}, 日期: {Date1}-{Time1}");
                }
            }
        }

        // 解析表頭-鴿子數量
        private void parsorHead()
        {
            string keyword = "本日共歸返";
            int index = _list[0].IndexOf(keyword);
            int index2 = _list[0].IndexOf("\n", index);
            string head = _list[0].Substring(index+keyword.Length, index2-index-keyword.Length-1);
            // <font color='#800000'>138</font> 舍  <font color='#800000'>4854</font> 鴿</font>
            head = head.Replace("<font color='#800000'>", "").Replace("</font>", "").Trim();
            //138 舍  4875 鴿
            head = head.Replace(" ", "").Replace("舍",",").Replace("鴿", "");

            string[] parts = head.Split(',');
            head = $"本日共歸返 {parts[0]} 舍, {parts[1]} 鴿";

            pigeon_count = int.Parse(parts[1]);
            member_count = int.Parse(parts[0]);

            disp(head);
        }

        private void Btn_Cookie_Click(object sender, RoutedEventArgs e)
        {
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            var client = new HttpClient(handler);

            // 送出登入或一般請求
            var response = client.GetAsync("http://www.087780212.tw/msg/login_pre.asp").Result;

            // 抓 Cookie
            var cookies = handler.CookieContainer.GetCookies(new Uri("http://www.087780212.tw"));
            //mycookie = cookies["ASPSESSIONIDQQQQRACD"].Value;
            foreach (Cookie c in cookies)
            {
                mycookie= c.Name;
                //mycookie_v= c.Value;
                Listbox_Log.Items.Add($"{c.Name} = {c.Value}");
            }

            Txt_CookieName.Text = mycookie;
            mycookie_v = Txt_CookieValue.Text;
        }

        private void Btn_Clean_Click(object sender, RoutedEventArgs e)
        {
            Listbox_Log.Items.Clear();
            _list.Clear();
        }
    }
}