using HtmlAgilityPack;
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
    /// topigeon_Load.xaml 的互動邏輯
    /// </summary>
    public partial class topigeon_Load : Window
    {
        private static string mycookie = "";
        private static string mycookie_v = "";
        private static string myDate = "";
        private static string html_head = "";
        private static string html_footer = "";

        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();

        private static int pigeon_count = 0;
        private static int member_count = 0;
        int record_count; // db 中已存在的資料筆數
        List<string> _list = new List<string>();

        public topigeon_Load()
        {
            InitializeComponent();

            comboBox_date.Items.Add(DateTime.Today.ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-2).ToString("yyyy/MM/dd"));
            comboBox_date.SelectedIndex = 0;

        }
        private void disp(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                Listbox_log.Items.Add(msg);
            });
        }

        private void Btn_Async_Click(object sender, RoutedEventArgs e)
        {
            myDate = comboBox_date.Text;
            record_count = 0;
            objRacingRecordF1 objRacing = new objRacingRecordF1();
            record_count = objRacing.GetRecordCount(myDate, "台南迎勝");
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

        private void procHtml(int page_index, bool isCalPage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            int data_count = 1000;
            if (isCalPage == true)
            {
                //計算頁數
                data_count = 20;
            }


            var url = $"https://www.topigeon.com.tw/index.asp?QSysid=2501&QMode=train&QRaceDate={myDate}&qsize={data_count}&QSort=1";

            // 建立 HttpClient + CookieContainer
            var handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer(),
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };


            var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0");
            client.DefaultRequestHeaders.Add("Referer", "https://www.topigeon.com.tw/index.asp");

            disp("Downloading page...");

            var html = client.GetStringAsync(url).Result;
            string tmp = "<table id=\"resultdata\" class=\"table table-striped table-borderless table-header-bg table-vcenter\"  width=\"100%\">";

            int _index = html.IndexOf(tmp);
            // Head
            html_head = html.Substring(0, _index);

            var newHtml = html.Substring(_index);
            _index = newHtml.IndexOf("<tbody>");
            newHtml = newHtml.Substring(_index);

            tmp = "</tbody>";
            _index = newHtml.IndexOf(tmp);
            html_footer = newHtml.Substring(_index+tmp.Length);
            newHtml = newHtml.Substring(0, _index+tmp.Length);

            newHtml = newHtml.Replace("<b>\r", "<b>");
            newHtml = newHtml.Replace("<td class=\"text-left\" style=\"display: none;\"></td>", "");
            //newHtml = newHtml.Replace(" ", "");
            newHtml = newHtml.Replace("\t", "");
            newHtml = newHtml.Replace("\n", "");
            _list = newHtml.Split("<tr>").ToList();

            //
            //
            parsorHead(html_head);
            if (true == isCalPage)
                return;

            prochtml2();

        }

        private void prochtml2()
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

                    record.Serialno2 = int.Parse(tds[0].InnerText.Trim());
                    record.Serialno3 = int.Parse(tds[1].InnerText.Trim());
                    record.Serialno1 = 0;
                    record.ClubName = "台南迎勝";
                    record.MemberNo = tds[2].InnerText.Trim();
                    record.RingId = int.Parse(tds[3].InnerText.Trim());
                    record.RacingDate = myDate;
                    record.ArrivedDatetime = tds[4].InnerText.Trim();

                    if (record.Serialno2 < record_count)
                        continue;

                    disp($"序: {record.Serialno1}, 順序: {record.Serialno2}, 序號2: {record.Serialno3}, 鴿會: {record.ClubName}, 會員: {record.MemberNo}, " +
                        $"腳環號碼: {record.RingId}, 日期: {record.RacingDate}-{record.ArrivedDatetime}");
                    obj.AddRecord(record);
                }
            }

            obj.InsertRecord();

        }

        private void parsorHead(string html)
        {
            string tmp = "筆數:";
            int index = html.IndexOf(tmp);
            int index2 = html.IndexOf("&nbsp;");
            string s_count = html.Substring(index + tmp.Length, index2 - index - tmp.Length).Trim();

            disp($"總筆數: {s_count}");

            pigeon_count = int.Parse(s_count);

        }
    }
}
