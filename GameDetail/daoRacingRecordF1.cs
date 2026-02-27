//using Microsoft.Data.SqlClient;
using Mysqlx.Crud;
using Serilog;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Windows.Media;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;

namespace GameDetail
{
    
    public class daoRacingRecordF1
    {
        public int Serialno1 { get; set; }
        public int Serialno2 { get; set; }
        //public int Serialno3 { get; set; }

        public string ClubName { get; set; } = string.Empty;
        public string MemberNo { get; set; } = string.Empty;

        public string RingId { get; set; } = string.Empty;

        // racing_date 是 varchar(12)，保持 string
        public DateTime RacingDate { get; set; }
        public string feet_no { get; set; } = "";

        public DateTime ArrivedDatetime { get; set; }
        public string BgColor {  get; set; } = "white";
        public string fgColor { get; set; } = "Black";

        public int club_id { get; set; }
        public int dovecote_id { get; set; }
        public string epc { get; set; } = string.Empty;

        public DateTime? system_time { get; set; }
        public DateTime? FST { get; set; }
        public string strFST { get; set; } = string.Empty;

        public int interval_minutes { get; set; }
        public string str_interval_minutes {  get; set; } = string.Empty;

        public bool is_NotifySMS { get; set; } = false;
        public string  str_NotifySMS { get; set; } = "";
        public string NotifyColor { get; set; } = "Red";

        public string memo { get; set; } = string.Empty;
        public string strcountdown_minutes { get; set; } = string.Empty;
    }

    public class objRacingRecordF1
    {
        List<daoRacingRecordF1> _reacrd_list = new List<daoRacingRecordF1>();
        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();
        List<string> _hid = new List<string>();

        public void AddRecord(daoRacingRecordF1 record)
        {
            _reacrd_list.Add(record);
        }

        public ObservableCollection<daoRacingRecordF1> Read(string RacingDate, int serial2, int club_id, string member_no)
        {
            _hid.Clear();
            objAlertSMS objSMS = new objAlertSMS();

            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            string club_name = "台南迎勝";

            try
            {
                if(serial2 == 0)
                    serial2 = 9999;

                if (member_no != "")
                {
                    sql = $"select * from pc.view_ys_record where racing_date='{RacingDate}' AND serialno2 <={serial2} AND club_name='{club_name}' AND member_no='{member_no}'";
                }
                else
                {
                    sql = $"select * from pc.view_ys_record where racing_date='{RacingDate}' AND serialno2 <={serial2} AND club_name='{club_name}'";
                }
                    
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    // 讀取資料並且顯示出來
                    while (reader.Read())
                    {
                        daoRacingRecordF1 record = new daoRacingRecordF1();
                        record.Serialno1 = reader.GetInt32(reader.GetOrdinal("serialno1"));
                        record.Serialno2 = reader.GetInt32(reader.GetOrdinal("serialno2"));
                        record.ClubName = reader.GetString(reader.GetOrdinal("club_name"));
                        record.MemberNo= reader.GetString(reader.GetOrdinal("member_no"));
                        record.RingId = reader.GetString(reader.GetOrdinal("ring_id"));
                        int idx = reader.GetOrdinal("sendstatus");
                        int SendStatus = reader.IsDBNull(idx)
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("sendstatus"));
                        if (SendStatus == 0)
                        {
                            record.is_NotifySMS = false;
                            record.str_NotifySMS = "   未通知   ";
                            record.NotifyColor = "Blue";
                        }
                        else
                        {
                            record.is_NotifySMS = true;
                            record.str_NotifySMS = "   已通知   ";
                            record.NotifyColor = "Red";
                        }
                            

                        record.RacingDate = reader.GetDateTime(reader.GetOrdinal("racing_date"));
                        idx = reader.GetOrdinal("first_see_time");

                        record.FST = reader.IsDBNull(idx)
                            ? (DateTime?)null
                            : reader.GetDateTime(idx);

                        if(record.FST == null) 
                            record.strFST = "";
                        else
                            record.strFST = record.FST?.ToString("HH:mm:ss") ?? "";

                        record.feet_no = SafeGetString(reader, "feet_no");

                        record.ArrivedDatetime = reader.GetDateTime(reader.GetOrdinal("arrived_datetime"));
                        if(record.Serialno2 == 1)
                            record.BgColor = Brushes.Yellow.ToString();

                        if(record.ArrivedDatetime < record.FST && record.FST != null)
                        {
                            TimeSpan interval =  record.FST.Value- record.ArrivedDatetime;
                            //record.interval_minutes = (int)interval.TotalMinutes;
                            record.interval_minutes = (int)interval.TotalSeconds;
                            int _Minutes = record.interval_minutes / 60;
                            int _Seconds = record.interval_minutes % 60;
                            record.str_interval_minutes = $"{_Minutes}:{_Seconds}";
                        }
                        else
                        {
                            record.interval_minutes = -1;
                            record.FST = null;
                        }

                        if (record.FST == null)
                        {
                            DateTime add15Datetime = record.ArrivedDatetime.AddMinutes(Setting.InvTime);
                            TimeSpan interval = add15Datetime - DateTime.Now;
                            int _seconds = (int)interval.TotalSeconds;
                            if (_seconds < 0)
                            {
                                record.strcountdown_minutes = "  結束倒數";
                                record.memo = $"超過 {Setting.InvTime} 分鐘感應第二套鴿鐘！";
                            }
                            else
                            {
                                record.strcountdown_minutes = $"{_seconds / 60}分 {_seconds % 60}秒";
                                if (_seconds < (Setting.AlertTime * 60))
                                {
                                    // 發簡訊通知
                                    if (record.is_NotifySMS == false && Setting.AutoSendSMS == true)
                                    {
                                        daoAlertSMS sms = new daoAlertSMS();
                                        sms.Racing_date = record.RacingDate;
                                        sms.SerialNo1 = record.Serialno1;
                                        sms.telephone = getTel(record.MemberNo.Substring(1));
                                        sms.Message = $"來自【{club_name}】您的鴿子 {record.feet_no} 已超過 {Setting.InvTime - Setting.AlertTime} 分鐘未感應第二套鴿鐘，請盡速感應！";
                                        sms.SendStatus = 1;
                                        objSMS.Add(sms);
                                        record.is_NotifySMS = true;
                                    }
                                }
                            }

                        }

                        else if (record.interval_minutes > Setting.InvTime*60)
                        {
                            record.memo = $"超過 {Setting.InvTime} 分鐘";
                            record.memo = record.memo.PadRight(200);
                        }

                        if (_hid.Contains(record.Serialno1.ToString()) == false)
                        {
                            _hid.Add(record.Serialno1.ToString());
                            myRecord.Add(record);
                        }
                        //myRecord.Add(record);
                    }

                    // 發送簡訊
                    objSMS.SaveAlertSMS();
                }
            }
            catch (Exception e)
            {
            }

            return myRecord;
        }

        //青田
        //佳冬民族
        //
        public ObservableCollection<daoRacingRecordF1> Read2(string RacingDate, int serial2, int club_id, string member_no)
        {
            objAlertSMS objSMS = new objAlertSMS();

            _hid.Clear();
            string club_name = "";
            string vw_name = "";
            if(club_id == 91)//青田(春)
            {
                vw_name = "pc.view_215_record";
                club_name = "青田";
            }
            else if(club_id == 103)//佳冬民族
            {
                vw_name = "pc.view_229_record";
                club_name = "佳冬民族";
            }
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();

            try
            {
                if (serial2 == 0)
                    serial2 = 9999;

                if (member_no != "")
                {
                    sql = $"select * from {vw_name} where ymd='{RacingDate}' AND ringCode <={serial2} AND mid='{member_no}' order by hid";
                }
                else
                {
                    sql = $"select * from {vw_name} where ymd='{RacingDate}' AND ringCode <={serial2} order by hid";
                }

                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    // 讀取資料並且顯示出來
                    while (reader.Read())
                    {
                        daoRacingRecordF1 record = new daoRacingRecordF1();
                        record.Serialno1 = reader.GetInt32(reader.GetOrdinal("hid"));
                        record.Serialno2 = reader.GetInt32(reader.GetOrdinal("ringCode"));
                        record.ClubName = club_name;
                        record.MemberNo = reader.GetString(reader.GetOrdinal("mid"));
                        record.RingId = reader.GetString(reader.GetOrdinal("ring"));

                        int idx = reader.GetOrdinal("sendstatus");
                        int SendStatus = reader.IsDBNull(idx)
                            ? 0
                            : reader.GetInt32(reader.GetOrdinal("sendstatus"));
                        if (SendStatus == 0)
                        {
                            record.is_NotifySMS = false;
                            record.str_NotifySMS = "   未通知   ";
                            record.NotifyColor = "Blue";
                        }
                        else
                        {
                            record.is_NotifySMS = true;
                            record.str_NotifySMS = "   已通知   ";
                            record.NotifyColor = "Red";
                        }

                        record.RacingDate = reader.GetDateTime(reader.GetOrdinal("ymd"));
                        idx = reader.GetOrdinal("pc_FST");

                        record.FST = reader.IsDBNull(idx)
                            ? (DateTime?)null
                            : reader.GetDateTime(idx);

                        if (record.FST == null)
                            record.strFST = "";
                        else
                            record.strFST = record.FST?.ToString("HH:mm:ss") ?? "";

                        //record.FST = reader.GetDateTime(reader.GetOrdinal("first_see_time"));
                        record.feet_no = record.RingId;

                        record.ArrivedDatetime = reader.GetDateTime(reader.GetOrdinal("f1_datetime"));
                        if (record.Serialno2 == 1)
                            record.BgColor = Brushes.Yellow.ToString();

                        if (record.ArrivedDatetime < record.FST && record.FST != null)
                        {
                            TimeSpan interval =  record.FST.Value- record.ArrivedDatetime;
                            //record.interval_minutes = (int)interval.TotalMinutes;
                            int _seconds = (int)interval.TotalSeconds;
                            record.interval_minutes = (int)interval.TotalSeconds;
                            int _Minutes = record.interval_minutes / 60;
                            int _Seconds = record.interval_minutes % 60;
                            record.str_interval_minutes = $"{_Minutes}分{_Seconds}秒";
                            record.strcountdown_minutes ="";
                        }
                        else
                        {
                            record.interval_minutes = -1;
                            record.FST = null;
                        }

                        if(record.FST == null)
                        {
                            DateTime add15Datetime = record.ArrivedDatetime.AddMinutes(Setting.InvTime);
                            TimeSpan interval = add15Datetime - DateTime.Now;
                            int _seconds = (int)interval.TotalSeconds;
                            if (_seconds < 0)
                            {
                                record.strcountdown_minutes = "結束倒數";

                                record.memo = $"超過 {Setting.InvTime} 分鐘感應第二套鴿鐘！";
                                record.memo = record.memo.PadRight(200);
                            }
                            else
                            {
                                record.strcountdown_minutes = $"{_seconds / 60}分 {_seconds % 60}秒";
                                if(_seconds < (Setting.AlertTime*60))
                                {
                                    // 發簡訊通知
                                    if (record.is_NotifySMS == false && Setting.AutoSendSMS == true)
                                    {
                                        daoAlertSMS sms = new daoAlertSMS();
                                        sms.Racing_date = record.RacingDate;
                                        sms.SerialNo1 = record.Serialno1;
                                        sms.telephone = getTel(record.MemberNo);
                                        sms.Message = $"來自【{club_name}】您的鴿子 {record.feet_no} 已超過 {Setting.InvTime-Setting.AlertTime} 分鐘未感應第二套鴿鐘，請盡速感應！";
                                        sms.SendStatus = 1;
                                        objSMS.Add(sms);
                                        record.is_NotifySMS = true;
                                    }
                                }
                            }
                            
                        }
                        else if(record.interval_minutes > Setting.InvTime * 60)
                        {
                            record.memo = $"超過 {Setting.InvTime} 分鐘";
                            record.memo=record.memo.PadRight(200);
                        }

                        if (_hid.Contains(record.Serialno1.ToString()) == false)
                        {
                            _hid.Add(record.Serialno1.ToString());
                            myRecord.Add(record);
                        }
                        
                    }

                    // 發送簡訊
                    objSMS.SaveAlertSMS();
                    Setting.PopMessage_queue.Enqueue($"查詢{club_name}完成！");
                }
            }
            catch (Exception e)
            {
            }

            return myRecord;
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

        public string SafeGetString(MySql.Data.MySqlClient.MySqlDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);
            return reader.IsDBNull(idx) ? "" : reader.GetString(idx);
        }


        public int GetRecordCount(string RacingDate, string clubname)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            int count = 0;
            try
            {
                sql = $"select count(*) as data_count from racing_records_f1 where racing_date='{RacingDate}' AND club_name='{clubname}'";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    // 讀取資料並且顯示出來
                    while (reader.Read())
                    {
                        count = reader.GetInt32(reader.GetOrdinal("data_count"));
                    }
                }
            }
            catch (Exception e)
            {
            }

            return count;
        }

        public void InsertRecord()
        {
            objFeetNo objFeet = new objFeetNo();
            objFeet.LoadFeetNo(102); // 台南迎勝
            Dictionary<string, dao_vw_feetno> feetno_list = objFeet._feetno_list;

            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            //conn.Open();

            foreach (var record in _reacrd_list)
            {
                try
                {
                    sql = @"
                    INSERT INTO racing_records_f1
                    (serialno1, serialno2, club_name, member_no, ring_id, racing_date, arrived_datetime, club_id,dovecote_id,epc,feet_no)
                    VALUES
                    (@serialno1, @serialno2, @club_name, @member_no, @ring_id, @racing_date, @arrived_datetime, @club_id,@dovecote_id,@epc,@feet_no);
                    ";
                    
                    

                    using var cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@serialno1", record.Serialno1);
                    cmd.Parameters.AddWithValue("@serialno2", record.Serialno2);
                    cmd.Parameters.AddWithValue("@club_name", record.ClubName);
                    cmd.Parameters.AddWithValue("@member_no", record.MemberNo);
                    cmd.Parameters.AddWithValue("@ring_id", record.RingId);
                    cmd.Parameters.AddWithValue("@racing_date", record.RacingDate);
                    cmd.Parameters.AddWithValue("@arrived_datetime", record.ArrivedDatetime);

                    dao_vw_feetno _feetno = new dao_vw_feetno();
                    _feetno = feetno_list.GetValueOrDefault(record.MemberNo+record.RingId)?? new dao_vw_feetno();


                    cmd.Parameters.AddWithValue("@club_id", _feetno.club_id);
                    cmd.Parameters.AddWithValue("@dovecote_id", _feetno.dovecote_id);
                    cmd.Parameters.AddWithValue("@epc", _feetno.EPC);
                    cmd.Parameters.AddWithValue("@feet_no", _feetno.feet_no);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Debug("InsertRecord Error: " + ex.Message);
                }
            }
            
            
        }
    }

}
