using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDetail
{
    public class daoPigeon_Dovecote
    {
        public string Telephone { get; set; } = string.Empty;
        public string Dovecote_SN { get; set; } = string.Empty;
        public int Club_ID { get; set; } = 0;
    }

    public class objPigeon_Dovecote
    {
        

        public void LoadDovecote(int club_id)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                sql = $"select * from pigeon_system_taiwan.pigeon_dovecotes where club_id={club_id}";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    daoPigeon_Dovecote dao = new daoPigeon_Dovecote();
                    dao.Club_ID = reader.GetInt32(reader.GetOrdinal("club_id"));
                    
                    dao.Dovecote_SN = reader.GetString(reader.GetOrdinal("dovecote_sn"));

                    if (club_id == 102)
                    {
                        dao.Dovecote_SN = $"0{dao.Dovecote_SN}";
                    }
                    dao.Telephone = reader.GetString(reader.GetOrdinal("dovecote_tel"));
                    Setting._dovecote_list.Add(dao.Dovecote_SN, dao);
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }
        }
    }
}
