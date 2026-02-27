using Serilog;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace GameDetail
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static string myDate = "";
        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();
        private readonly DispatcherTimer _timer;
        
        int reflash_countdown = 30;
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

            Btn_TestSMS.Visibility = Visibility.Hidden;

            //utility u = new utility();
            //u.connectdb();
            comboBox_date.Items.Add(DateTime.Today.ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-1).ToString("yyyy/MM/dd"));
            comboBox_date.Items.Add(DateTime.Today.AddDays(-2).ToString("yyyy/MM/dd"));
            comboBox_date.SelectedIndex = 0;

           
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
            Txt_InvTime.Text = "";
            Txt_AlertTime.Text = "";

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            
            this.Show();

            Lbl_Message.Content = "";

            login loginWindow = new login();
            loginWindow.Topmost= true;
            bool? result = loginWindow.ShowDialog();
            if (result == true)
            {
                // 子視窗回傳的資料
                Setting.ClubName = loginWindow.Club_name;
                Setting.ClubID = loginWindow.Club_id;
                // 更新 UI
                Lbl_Club.Content = Setting.ClubName;
            }

            objPigeon_Dovecote objDovecote = new objPigeon_Dovecote();
            objDovecote.LoadDovecote(Setting.ClubID);

            if (Setting.ClubID == 102) //迎勝
            {
                Btn_Topigeon_Click(null, null);
                Txt_AlertTime.Text = "2"; // 5
                Txt_InvTime.Text = "10";   //10
                Setting.InvTime = 10;
                Setting.AlertTime = 2;
            }
            else if(Setting.ClubID == 91)
            {
                Txt_AlertTime.Text = "5";
                Txt_InvTime.Text = "15";
                Setting.InvTime = 15;
                Setting.AlertTime = 5;
            }
            else if(Setting.ClubID == 103)
            {
                Txt_AlertTime.Text = "5";
                Txt_InvTime.Text = "15";
                Setting.InvTime = 15;
                Setting.AlertTime = 5;
            }
            else
            {
                Btn_Load.IsEnabled = false;
                Btn_Load.Foreground = Brushes.Gray;
                Lbl_Message.Foreground = Brushes.Red;
                Lbl_Message.Content = "沒有正確登入！";
                return;
            }

            // Load SMS Services user & password
            //Setting.LoadSMSSetting(Setting.ClubID.ToString("D3"));


            Task.Run(() =>
            {
                while (true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        Setting.AutoSendSMS = CheckBox_AutoSendSMS.IsChecked == true;
                    });
                    objAlertSMS objAlert = new objAlertSMS();
                    objAlert.SendSMSfromAlertSMS();
                    System.Threading.Thread.Sleep(10000); // 每10秒檢查一次
                }
            });

            Task.Run(() =>
            {
                while (true)
                {
                    if (Setting.PopMessage_queue.Count > 0)
                    {
                        PopupMessage();
                    }
                    System.Threading.Thread.Sleep(500); // 每0.5秒檢查一次
                }
            });
        }

        private void PopupMessage()
        {             
            Dispatcher.Invoke(() =>
            {
                while (Setting.PopMessage_queue.Count > 0)
                {
                    string msg = Setting.PopMessage_queue.Dequeue();
                    ShowToast(msg);
                    Task.Delay(100).Wait();
                }
            });
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {

            //Setting

            if (CheckBox_AutoLoad.IsChecked == true)
            {
                reflash_countdown--;
                Lbl_ReflashCountdown.Content = $"自動刷新倒數 {reflash_countdown}";
                if(reflash_countdown <= 0)
                {
                    reflash_countdown = 30;
                    Load_Record();
                }
                //Load_Record();
            }
            
        }

        private void Load_Record()
        {
            myDate = comboBox_date.Text;
            

            int serial2 = int.Parse(comboBox_dispsn.Text);
            string club_name = Lbl_Club.Content.ToString();
            string member_no = Txt_memberNo.Text.Trim();

            objRacingRecordF1 objRacing = new objRacingRecordF1();
            if (Setting.ClubID == 102)
            {
                myRecord = objRacing.Read(myDate, serial2, Setting.ClubID, member_no);
            }
            else
            {
                myRecord = objRacing.Read2(myDate, serial2, Setting.ClubID, member_no);
            }
            Lbl_Message.Content = $"{myDate} {club_name} 前 {serial2} 名 資料筆數 {myRecord.Count}";
            listView_record.ItemsSource = myRecord;
        }

        private void Btn_Clean_Click(object sender, RoutedEventArgs e)
        {
            myRecord.Clear();
            Lbl_Message.Content = "";
            Txt_memberNo.Text = "";
        }

        private void Btn_Load_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(Txt_InvTime.Text.Trim(), out int it))
            {
                Setting.InvTime = it;
            }
            else
            {
                MessageBox.Show("請輸入正確的倒數時間(秒)");
                return;
            }

            if (int.TryParse(Txt_AlertTime.Text.Trim(), out int it2))
            {
                Setting.AlertTime = it2;
            }
            else
            {
                MessageBox.Show("請輸入正確的警示時間(秒)");
                return;
            }

            if (CheckBox_AutoSendSMS.IsChecked == true)
            {
                Setting.AutoSendSMS = true;
            }
            else
            {
                Setting.AutoSendSMS = false;
            }

            reflash_countdown = 30;
            //if (CheckBox_AutoLoad.IsChecked == true)
            //{
            //    Btn_Load.IsEnabled = false;
            //    comboBox_club.IsEnabled = false;
            //}
            //else
            //{
            //    Btn_Load.IsEnabled = true;
            //    comboBox_club.IsEnabled = true;
            //}

            Load_Record();
            _timer.Start();
        }

        private void Btn_Topigeon_Click(object sender, RoutedEventArgs e)
        {
            //topigeon_Load topigeonWindow = new topigeon_Load();
            //topigeonWindow.Show();

            if (topigeon_Load.Instance == null)
            {
                var win = new topigeon_Load();
                win.Closed += (s, e2) => topigeon_Load.Instance = null;
                win.Show();
                win.Owner = this;
            }
            else
            {
                topigeon_Load.Instance.Activate();   // 已存在 → bring to front
            }

        }

        private void Btn_ld_Click(object sender, RoutedEventArgs e)
        {
            //ld_Load ld_Load = new ld_Load();
            //ld_Load.Show();
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

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Setting.AutoLoadRecord = false;
        }

        private void CheckBox_AutoLoad_Checked(object sender, RoutedEventArgs e)
        {

                //Btn_Load.IsEnabled = false;
                //comboBox_club.IsEnabled = false;

        }

        private void CheckBox_AutoLoad_Unchecked(object sender, RoutedEventArgs e)
        {
            //Btn_Load.IsEnabled = true;
            //comboBox_club.IsEnabled = true;
        }

        private void Btn_TestSMS_Click(object sender, RoutedEventArgs e)
        {
            utility _u = new utility();
            _u.SendSms("0975637910", "來自【青田信鴿】的提醒，環號0123456鴿環，還未感應第二鴿鐘。", true);
            ShowToast("SMS已經送出！");

        }

        public async void ShowToast(string msg)
        {
            ToastText.Text = msg;

            var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
            Toast.BeginAnimation(UIElement.OpacityProperty, fadeIn);

            await Task.Delay(4000);

            var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            Toast.BeginAnimation(UIElement.OpacityProperty, fadeOut);
        }

        private void SendSms_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext;
            // 3.從 ListView 的 Items 找出 index
            int index = listView_record.Items.IndexOf(item);
            AddSMS(index);

        }


        public void AddSMS(int index)
        {
            daoRacingRecordF1 record = new daoRacingRecordF1();
            record = myRecord[index];

            objAlertSMS objSMS = new objAlertSMS();
            daoAlertSMS sms = new daoAlertSMS();

            sms.Racing_date = record.RacingDate;
            sms.SerialNo1 = record.Serialno1;
            sms.telephone = getTel(record.MemberNo);
            sms.Message = $"來自【{record.ClubName}】提醒，您的鴿子 {record.feet_no} 已超過 {Setting.InvTime-Setting.AlertTime} 分鐘未感應第二套鴿鐘，請盡速感應！";
            sms.SendStatus = 9;
            
            // 發送簡訊
            
            if(Setting.SMS_DEBUG_PHONE != "")
            {
                sms.telephone = Setting.SMS_DEBUG_PHONE;
            }
                
            utility _u = new utility();
            string r = _u.SendSms(sms.telephone, sms.Message, true);
            if(r == "OK")
            {
                objSMS.Add(sms);
                record.is_NotifySMS = true;
                record.str_NotifySMS = "   已通知   ";
                record.NotifyColor = "Red";
                myRecord.RemoveAt(index);
                myRecord.Insert(index, record); // 更新 ObservableCollection 中的資料，觸發 UI 更新
                // 發送成功
                objSMS.SaveAlertSMS();
                ShowToast($"已對 {record.MemberNo}的{record.feet_no} 發送簡訊！");
            }
            else {
                ShowToast($"{sms.telephone}-簡訊發送失敗！");
            }

        }
        public string getTel(string dovecote_sn)
        {
            string telephone = "";
            if (Setting._dovecote_list.ContainsKey(dovecote_sn))
            {
                telephone = Setting._dovecote_list[dovecote_sn].Telephone;
            }
            return telephone;
        }

        private void Btn_Export_Click(object sender, RoutedEventArgs e)
        {
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            string file = Path.Combine(folder, $"records_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.csv");

            var lines = new List<string>();
            lines.Add($"序號1,序號2,會員號,環號," +
                    $"比賽日期," +
                    $"第一套鴿鐘時間," +
                    $"第二套鴿鐘時間,間隔時間");
            foreach (var r in myRecord)
            {
                lines.Add($"{r.Serialno1},{r.Serialno2},{r.MemberNo},{r.RingId}," +
                    $"{r.RacingDate.ToString("yyyy-MM-dd")}," +
                    $"{r.ArrivedDatetime.ToString("yyyy-MM-dd HH:mm:ss")}," +
                    $"{r.FST?.ToString("yyyy-MM-dd HH:mm:ss")}," +
                    $"{r.str_interval_minutes}");
            }

            File.WriteAllLines(file, lines, Encoding.UTF8);

            ShowToast("檔案匯出完成！");
        }
    }
}