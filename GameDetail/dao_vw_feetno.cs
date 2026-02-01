using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDetail
{
    public class dao_vw_feetno
    {
        public int club_id { get; set; } = 0;
        public int dovecote_id { get; set; } = 0;
        public string feet_no { get; set; } = string.Empty;
        public string PartOfRingNo { get; set; } = string.Empty;
        public string dovecote_No { get; set; } = string.Empty;

        public string EPC { get; set; } = string.Empty;

        public string telephone { get; set; } = string.Empty;

    }

    public class objFeetNo
    {
        public Dictionary<string, dao_vw_feetno> _feetno_list = new Dictionary<string, dao_vw_feetno>();

        public void LoadFeetNo(int club_id)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                sql = $"select * from vw_feetno where club_id={club_id}";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    dao_vw_feetno dao = new dao_vw_feetno();
                    dao.club_id = reader.GetInt32(reader.GetOrdinal("club_id"));
                    dao.dovecote_id = reader.GetInt32(reader.GetOrdinal("dovecote_id"));
                    dao.feet_no = reader.GetString(reader.GetOrdinal("feet_no"));
                    dao.PartOfRingNo = dao.feet_no.Substring(4);
                    dao.dovecote_No = "0"+reader.GetString(reader.GetOrdinal("dovecote_sn"));
                    dao.EPC = reader.GetString(reader.GetOrdinal("epc"));
                    dao.telephone = reader.GetString(reader.GetOrdinal("dovecote_tel"));
                    _feetno_list.Add(dao.dovecote_No + dao.PartOfRingNo, dao);
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }
        }
    }
}
