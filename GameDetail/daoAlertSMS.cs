using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDetail
{
    public class daoAlertSMS
    {
        public DateTime Racing_date { get; set; } 
        public int SerialNo1 { get; set; } = 0;
        public string telephone { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public int SendStatus { get; set; } = 0;

    }

    public class objAlertSMS
    {
        public List<daoAlertSMS> _add_alertsms = new List<daoAlertSMS>();
        public List<daoAlertSMS> _Load_alertsms = new List<daoAlertSMS>();

        public void Add(daoAlertSMS dao)
        {
            _add_alertsms.Add(dao);
        }

        public void Clear()
        {
            _add_alertsms.Clear();
            _Load_alertsms.Clear();
        }

        public void SaveAlertSMS()
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                foreach (var dao in _add_alertsms)
                {
                    sql = $"insert into pc.AlertSMS (Racing_date,SerialNo1,Tel,msg,SendStatus,club_id) values " +
                          $"('{dao.Racing_date.ToString("yyyy-MM-dd")}',{dao.SerialNo1},'{dao.telephone}','{dao.Message}',{dao.SendStatus},{Setting.ClubID})";
                    using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }
        }

        public async void SendSMSfromAlertSMS()
        {
            List<daoAlertSMS> _alertsms = new List<daoAlertSMS>();
            utility util = new utility();

            LoadAlertSMS();
            foreach (var dao in _Load_alertsms)
            {
                
                String result = util.SendSms(dao.telephone, "測試訊息：" + dao.Message);
                Thread.Sleep(100); // 避免短時間內發送過多簡訊

                dao.SendStatus = 9; // 已發送

                //Setting.PopMessage_queue.Enqueue($"已發送簡訊至 {dao.telephone}");

                if(result == "OK")
                {
                    _alertsms.Add(dao);
                }
                
            }

            UpdateAlertSMS(_alertsms);
        }

        public void LoadAlertSMS()
        {
            _Load_alertsms.Clear();

            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                sql = $"select * from pc.AlertSMS where SendStatus=1 and club_id={Setting.ClubID}";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    daoAlertSMS dao = new daoAlertSMS();
                    dao.Racing_date = reader.GetDateTime(reader.GetOrdinal("Racing_date"));
                    dao.SerialNo1 = reader.GetInt32(reader.GetOrdinal("SerialNo1"));
                    dao.telephone = reader.GetString(reader.GetOrdinal("Tel"));
                    dao.Message = reader.GetString(reader.GetOrdinal("msg"));
                    dao.SendStatus = reader.GetInt32(reader.GetOrdinal("SendStatus"));
                    _Load_alertsms.Add(dao);
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }
        }

        public void UpdateAlertSMS(List<daoAlertSMS> _list)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                foreach (var dao in _list)
                {
                    sql = $"update pc.AlertSMS set SendStatus={dao.SendStatus} " +
                          $"where Racing_date='{dao.Racing_date.ToString("yyyy-MM-dd")}' and SerialNo1={dao.SerialNo1} and Tel='{dao.telephone}' and club_id={Setting.ClubID}";
                    using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }
        }

    }
}
