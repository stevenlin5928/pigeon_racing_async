//using Microsoft.Data.SqlClient;
using Serilog;
using System.Collections.ObjectModel;
using System.Windows.Media;
using MySqlCommand = MySql.Data.MySqlClient.MySqlCommand;

namespace GameDetail
{
    
    public class daoRacingRecordF1
    {
        public int Serialno1 { get; set; }
        public int Serialno2 { get; set; }
        public int Serialno3 { get; set; }

        public string ClubName { get; set; } = string.Empty;
        public string MemberNo { get; set; } = string.Empty;

        public int RingId { get; set; }

        // racing_date 是 varchar(12)，保持 string
        public string RacingDate { get; set; } = string.Empty;
        public string feet_no { get; set; } = "";

        public string ArrivedDatetime { get; set; } = string.Empty;
        public string BgColor {  get; set; } = "white";
        public string fgColor { get; set; } = "Black";
    }

    public class objRacingRecordF1
    {
        List<daoRacingRecordF1> _reacrd_list = new List<daoRacingRecordF1>();
        private ObservableCollection<daoRacingRecordF1> myRecord = new ObservableCollection<daoRacingRecordF1>();
        public void AddRecord(daoRacingRecordF1 record)
        {
            _reacrd_list.Add(record);
        }

        public ObservableCollection<daoRacingRecordF1> Read(string RacingDate, int serial3, string club_name, string member_no)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();

            try
            {
                if(serial3 == 0)
                    serial3 = 9999;

                if (member_no != "")
                {
                    sql = $"select * from vw_racing_records_f1 where racing_date='{RacingDate}' AND serialno3 <={serial3} AND club_name='{club_name}' AND member_no='{member_no}'";
                }
                else
                {
                    sql = $"select * from vw_racing_records_f1 where racing_date='{RacingDate}' AND serialno3 <={serial3} AND club_name='{club_name}'";
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
                        record.Serialno3 = reader.GetInt32(reader.GetOrdinal("serialno3"));
                        record.ClubName = reader.GetString(reader.GetOrdinal("club_name"));
                        record.MemberNo= reader.GetString(reader.GetOrdinal("member_no"));
                        record.RingId = reader.GetInt32(reader.GetOrdinal("ring_id"));
                        record.RacingDate = reader.GetString(reader.GetOrdinal("racing_date"));
                        //record.feet_no = reader.GetString(reader.GetOrdinal("feet_no"));
                        record.feet_no = SafeGetString(reader, "feet_no");

                        record.ArrivedDatetime = reader.GetString(reader.GetOrdinal("arrived_datetime"));
                        if(record.Serialno3 == 1)
                            record.BgColor = Brushes.Yellow.ToString();
                        myRecord.Add(record);
                    }
                }
            }
            catch (Exception e)
            {
            }

            return myRecord;
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
            int serialni1 = 0;
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
                    (serialno1, serialno2, serialno3, club_name, member_no, ring_id, racing_date, arrived_datetime)
                    VALUES
                    (@serialno1, @serialno2, @serialno3, @club_name, @member_no, @ring_id, @racing_date, @arrived_datetime);
                    ";
                    
                    serialni1 = record.Serialno1;

                    using var cmd = new MySqlCommand(sql, conn);

                    cmd.Parameters.AddWithValue("@serialno1", record.Serialno1);
                    cmd.Parameters.AddWithValue("@serialno2", record.Serialno2);
                    cmd.Parameters.AddWithValue("@serialno3", record.Serialno3);
                    cmd.Parameters.AddWithValue("@club_name", record.ClubName);
                    cmd.Parameters.AddWithValue("@member_no", record.MemberNo);
                    cmd.Parameters.AddWithValue("@ring_id", record.RingId);
                    cmd.Parameters.AddWithValue("@racing_date", record.RacingDate);
                    cmd.Parameters.AddWithValue("@arrived_datetime", record.ArrivedDatetime);

                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Debug("sql serialni1: " + serialni1);
                    Log.Debug("InsertRecord Error: " + ex.Message);
                }
            }
            
            
        }
    }

}
