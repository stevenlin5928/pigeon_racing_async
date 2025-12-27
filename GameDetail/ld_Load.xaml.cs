using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
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
    /// ld_Load.xaml 的互動邏輯
    /// </summary>
    public partial class ld_Load : Window
    {
        private static string mycookie = "";
        private static string mycookie_v = "";
        private static string myDate = "";
        private static string html_head = "";

        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();

        private static int pigeon_count = 0;
        private static int member_count = 0;
        int record_count; // db 中已存在的資料筆數

        List<string> _list = new List<string>();

        public ld_Load()
        {
            InitializeComponent();
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            comboBox_date.Items.Add(DateTime.Today.ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-2).ToString("yyyy/MM/dd"));
            comboBox_date.SelectedIndex = 0;
        }

        private void Btn_Async_Click(object sender, RoutedEventArgs e)
        {
            mycookie = Txt_CookieName.Text;
            mycookie_v = Txt_CookieValue.Text;
            myDate = comboBox_date.Text;

            record_count = 0;
            objRacingRecordF1 objRacing = new objRacingRecordF1();
            record_count = objRacing.GetRecordCount(myDate, "屏東青田(春)");
            disp(myDate + " 已有 " + record_count + " 筆資料");


            Task.Run(() =>
            {
                disp("開始讀取比賽資料...");

                procHtml(1, true);

                int start_index = (record_count / 1000) + 1;
                int stop_index = (pigeon_count / 1000) + 1;
                //int total_pages = (int)Math.Ceiling((double)pigeon_count / 1000);
                for (int index = start_index; index <= stop_index; index++)
                {
                    Thread.Sleep(3000);
                    procHtml(index, false);
                }

                disp("讀取比賽資料結束...");

            });
        }
        private void disp(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                Listbox_Log.Items.Add(msg);
            });
        }
        private void procHtml(int page_index, bool isCalPage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            int data_count = 1000;
            if (isCalPage == true)
            {
                //計算頁數
                data_count = 5;
            }


            var url = $"http://www.087780212.tw/msg/dailyGameDetail.asp?udate={myDate}&ucgp=215&uhouse=0&page={page_index}&upgs={data_count}";

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
            var newHtml = html.Replace("bgcolor='#EEEEFF", "bgcolor='#FFFFFF");

            if (html.Contains("default.asp") == true)
            {
                disp("❌ Cookie 已失效，請重新取得 Cookie");
                return;
            }

            _list = newHtml.Split("<TR bgcolor='#FFFFFF' >").ToList();

            html_head = _list[0];
            //html_footer = _list[_list.Count - 1];

            for (int index = 1; index < _list.Count; index++)
            {
                if (index == _list.Count - 1)
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

            parsorHead();

            if (isCalPage == true)
            {
                return;
            }

            ParsorHtml_2(_list);

            if (_list.Count < 1)
            {
                disp("❌ 無資料");
                return;
            }

        }

        private void ParsorHtml_2(List<string> _list)
        {
            objRacingRecordF1 obj = new objRacingRecordF1();

            for (int index = 1; index < _list.Count; index++)
            {
                string s = _list[index];
                var doc = new HtmlAgilityPack.HtmlDocument();
                doc.LoadHtml(s);
                var tds = doc.DocumentNode.SelectNodes("//td");
                if (tds != null)
                {
                    daoRacingRecordF1 record = new daoRacingRecordF1();

                    record.Serialno1 = 0;
                    record.Serialno2 = int.Parse(tds[1].InnerText.Trim());
                    record.Serialno3 = int.Parse(tds[2].InnerText.Trim());
                    record.ClubName = tds[3].InnerText.Trim();
                    record.MemberNo = tds[4].InnerText.Trim();
                    record.RingId = int.Parse(tds[5].InnerText.Trim());
                    record.RacingDate = tds[6].InnerText.Trim();
                    string Time1 = tds[7].InnerText.Trim();
                    //06時42分29.14秒
                    //string myDatetime = record.RacingDate + " " + Time1.Replace("時", ":").Replace("分", ":").Replace("秒", "");
                    //DateTime dt = DateTime.ParseExact(myDatetime, "yyyy/MM/dd HH:mm:ss.ff", null);
                    record.ArrivedDatetime = tds[7].InnerText.Trim();

                    if (record.Serialno2 < record_count)
                        continue;

                    disp($"序: {record.Serialno1}, 順序: {record.Serialno2}, 序號2: {record.Serialno3}, 鴿會: {record.ClubName}, 會員: {record.MemberNo}, " +
                        $"腳環號碼: {record.RingId}, 日期: {record.RacingDate}-{record.ArrivedDatetime}");

                    obj.AddRecord(record);
                }
            }

            obj.InsertRecord();
        }

        // 解析表頭-鴿子數量
        private void parsorHead()
        {
            string keyword = "本日共歸返";
            int index = _list[0].IndexOf(keyword);
            int index2 = _list[0].IndexOf("\n", index);
            string head = _list[0].Substring(index + keyword.Length, index2 - index - keyword.Length - 1);
            // <font color='#800000'>138</font> 舍  <font color='#800000'>4854</font> 鴿</font>
            head = head.Replace("<font color='#800000'>", "").Replace("</font>", "").Trim();
            //138 舍  4875 鴿
            head = head.Replace(" ", "").Replace("舍", ",").Replace("鴿", "");

            string[] parts = head.Split(',');
            head = $"本日共歸返 {parts[0]} 舍, {parts[1]} 鴿";

            pigeon_count = int.Parse(parts[1]);
            member_count = int.Parse(parts[0]);

            disp(head);
            disp($"pigeon_count:{pigeon_count}, member_count: {member_count}");
        }

    }
}
