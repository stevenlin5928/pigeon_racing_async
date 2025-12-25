
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

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
    }
}
