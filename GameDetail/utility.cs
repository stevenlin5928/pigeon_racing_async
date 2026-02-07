
using MySql.Data.MySqlClient;
using OfficeOpenXml;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.IO;


namespace GameDetail
{
    public class utility
    {
        MySqlConnection conn;

        public utility()
        {
            

        }

        public MySqlConnection connectdb()
        {
            string dbHost = "202.39.254.129";
            string dbPort = "13316";
            string dbUser = "root";
            string dbPass = "yhiot@2026";
            //string dbUser = "exuser";
            //string dbPass = "exuser@2026$$";
            //string dbName = "pc";
            //string dbHost = "106.52.12.118";
            //string dbPort = "9982";
            //string dbUser = "yya";
            //string dbPass = "91310120MADX94GG3M";
            string dbName = "pigeon_system_taiwan";

            // 如果有特殊的編碼在database後面請加上;CharSet=編碼, utf8請使用utf8_general_ci
            string connStr = $"Server={dbHost};Port={dbPort};Uid={dbUser};Pwd={dbPass};Database={dbName};";
            conn = new MySqlConnection(connStr);

            // 連線到資料庫
            try
            {
                conn.Open();
                Log.Debug("資料庫連線 ok!");
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 0:
                        Log.Debug("無法連線到資料庫.");
                        break;
                    case 1045:
                        Log.Debug("使用者帳號或密碼錯誤,請再試一次.");
                        break;
                }

                Log.Debug("資料庫連線失敗: " + ex.Message);
            }

            return conn;
        }

        public async Task<string> SendSms(string phone, string msg)
        {
            string result = "";

            if (Setting.AutoSendSMS == false)
            {
                return "AutoSendSMS false!";
            }

            try
            {
                //string username = "we55666";
                //string password = "78xwy16g19txke2";
                //string mobile = phone;
                //string message = msg;

                // .NET 6 / .NET Core 用這個
                string encodedMessage = Uri.EscapeDataString(msg);

                string url =
                    $"http://api.twsms.com/json/sms_send.php?username={Setting.SMS_USER}&password={Setting.SMS_PASS}&mobile={phone}&message={encodedMessage}";

                using var client = new HttpClient();
                result = await client.GetStringAsync(url);

                //Console.WriteLine(result);

            
            }
            catch (Exception ex)
            {
                Log.Debug($"SMS 發送失敗: {ex.Message}");
                result = "SMS 發送失敗";
            }

            return result;
        }

        public static void ExportToExcel(string filePath)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("Report");

                // 標題列
                ws.Cells[1, 1].Value = "ID";
                ws.Cells[1, 2].Value = "Name";
                ws.Cells[1, 3].Value = "Score";

                // 假資料
                ws.Cells[2, 1].Value = 1;
                ws.Cells[2, 2].Value = "Steven";
                ws.Cells[2, 3].Value = 95;

                ws.Cells[3, 1].Value = 2;
                ws.Cells[3, 2].Value = "Mary";
                ws.Cells[3, 3].Value = 88;

                // 自動調整欄寬
                ws.Cells.AutoFitColumns();

                // 儲存
                File.WriteAllBytes(filePath, package.GetAsByteArray());
            }
        }

    }
}
