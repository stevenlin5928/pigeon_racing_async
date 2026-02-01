
using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

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

        public async void SendSms(string phone, string msg)
        {
            try
            {
                string username = "we55666";
                string password = "78xwy16g19txke2";
                string mobile = phone;
                string message = msg;

                // .NET 6 / .NET Core 用這個
                string encodedMessage = Uri.EscapeDataString(message);

                string url =
                    $"http://api.twsms.com/json/sms_send.php?username={username}&password={password}&mobile={mobile}&message={encodedMessage}";

                using var client = new HttpClient();
                string response = await client.GetStringAsync(url);

                Console.WriteLine(response);

            
            }
            catch (Exception ex)
            {
                Log.Debug($"SMS 發送失敗: {ex.Message}");
            }
        }
    }
}
