using Mysqlx.Session;
using MySqlX.XDevAPI.Common;
using System.Collections.ObjectModel;
using System.Diagnostics.Eventing.Reader;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;



namespace GameDetail
{
    /// <summary>
    /// ResetDevice.xaml 的互動邏輯
    /// </summary>
    public partial class ResetDevice : Window
    {
        private ObservableCollection<daoSIMInfo> _SIMInfo = new ObservableCollection<daoSIMInfo>();

        public ResetDevice()
        {
            InitializeComponent();
            Lbl_Message.Content = "";

        }

        private void Btn_Load_offline_Click(object sender, RoutedEventArgs e)
        {
            int is_online = 0;
            _SIMInfo.Clear();
            if(CheckBox_Online.IsChecked == true)
            {
                is_online = 1;
            }
            objSIMInfo obj = new objSIMInfo();
            _SIMInfo = obj.Load(Setting.ClubID, is_online);

            ListView_SIMInfo.ItemsSource = _SIMInfo;

            Lbl_Message.Content = $"共 {_SIMInfo.Count} 筆資料";
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            var item = (sender as Button).DataContext;
            // 3.從 ListView 的 Items 找出 index
            int index = ListView_SIMInfo.Items.IndexOf(item);

            string ip = _SIMInfo[index].IP;

            Task.Run(() =>
            {
                bool result = ResetRFIDReader(ip);
                if (result)
                {
                    Log($"成功重置 IP 為 {ip} 的 RFID Reader");
                }
                else
                {
                    Log($"重置 IP 為 {ip} 的 RFID Reader 時發生錯誤");
                }
            });
            
            

        }

        public bool ResetRFIDReader(string ip)
        {
            // 這裡放置重置 RFID Reader 的邏輯
            // 例如，透過網路連線到 RFID Reader 並發送重置指令
            // 這裡僅為示範，實際實現可能需要根據 RFID Reader 的 API 或協定來進行

            //ip = "192.168.100.168";
            try
            {
                int port = 8888;
                //Result ret = Result.OK;
                //AT+CTL=RESTART,23
                string AT_REBOOT = "AT+CTL=REBOOT,75\r\n";

                using (TcpClient client = new TcpClient())
                {
                    Log("Connecting...");
                    client.Connect(ip, port);
                    Log("Connected!");

                    NetworkStream stream = client.GetStream();

                    // 接收資料
                    byte[] buffer = new byte[1024];
                    int bytes = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.UTF8.GetString(buffer, 0, bytes);
                    Log("Received: " + response);

                    // 傳送資料
                    string message = AT_REBOOT;
                    byte[] data = Encoding.ASCII.GetBytes(message);
                    
                    stream.Write(data, 0, data.Length);
                    Log("Sent: " + message);

                    // 接收資料
                    buffer = new byte[1024];
                    bytes = stream.Read(buffer, 0, buffer.Length);
                    response = Encoding.UTF8.GetString(buffer, 0, bytes);
                    Log("Received: " + response);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log($"重置 IP 為 {ip} 的 RFID Reader 時發生錯誤: {ex.Message}");
                return false;
            }
        }

        public void Log(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Listbox_log.Items.Add(msg);
            });
            
        }

        private void Btn_Cls_Click(object sender, RoutedEventArgs e)
        {
            Listbox_log.Items.Clear();
        }
        //private void Btn_ImportSIM_IP_Click(object sender, RoutedEventArgs e)
        //{
        //    var dialog = new OpenFileDialog
        //    {
        //        Title = "選擇檔案",
        //        Filter = "所有檔案 (*.*)|*.*"
        //    };

        //    if (dialog.ShowDialog() == true)
        //    {
        //        string filePath = dialog.FileName;


        //        // 在這裡處理選擇的檔案路徑
        //        using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //        using (StreamReader reader = new StreamReader(fs))
        //        {

        //            string? line;
        //            while ((line = reader.ReadLine()) != null)
        //            {
        //                if(line.StartsWith("UUID") || line.Trim() == "")
        //                {
        //                    continue;
        //                }
        //                string[] parts = line.Split(',');

        //                //Console.WriteLine(parts[0]);
        //                //Console.WriteLine(parts[6]);

        //                using (StreamWriter writer = new StreamWriter(filePath+".sql", append: true))
        //                {
        //                    string sql = $"update pc.SIMInfo set Device_uuid='{parts[0]}' where ICCID like '%{parts[1]}';";
        //                    writer.WriteLine(sql);
        //                }

        //            }

        //        }

        //    }
        //}
    }
}
