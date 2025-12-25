using MySql.Data.MySqlClient;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public DateTime ArrivedDatetime { get; set; }

    }

    public class objRacingRecordF1
    {
        List<daoRacingRecordF1> _reacrd_list = new List<daoRacingRecordF1>();

        public void AddRecord(daoRacingRecordF1 record)
        {
            _reacrd_list.Add(record);
        }

        public void InsertRecord()
        {

            try
            {
                utility util = new utility();
                using var conn = util.connectdb();
                conn.Open();

                foreach (var record in _reacrd_list)
                {

                    string sql = @"
                INSERT INTO racing_records_f1
                (serialno1, serialno2, serialno3, club_name, member_no, ring_id, racing_date, arrived_datetime)
                VALUES
                (@serialno1, @serialno2, @serialno3, @club_name, @member_no, @ring_id, @racing_date, @arrived_datetime);
                ";

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
            }
            catch (Exception ex)
            {
                Log.Debug("InsertRecord Error: " + ex.Message);
            }
            
        }
    }

}
