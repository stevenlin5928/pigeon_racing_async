using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDetail
{
    public class daoSIMInfo
    {
        public string ICCID { get; set; } = "";
        public string Dovecote_sn { get; set; } = "";
        public string IP { get; set; } = "";
        public string Device_UUID { get; set; } = "";

        public int club_id { get; set; } = 0;

        public int is_online { get; set; } = 0;
        public string str_is_online { get; set; } = "";
    }

    public class objSIMInfo
    {
        public ObservableCollection<daoSIMInfo> _SIMInfo_list = new ObservableCollection<daoSIMInfo>();
        public void Add(daoSIMInfo dao)
        {
            _SIMInfo_list.Add(dao);
        }
        public void Clear()
        {
            _SIMInfo_list.Clear();
        }

        public ObservableCollection<daoSIMInfo> Load(int club_id, int is_online)
        {
            string sql = "";
            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                if(is_online != 1)
                {
                    sql = $"select * from pc.view_SIMInfo where club_id={club_id} AND is_online<>1";
                }
                else
                    sql = $"select * from pc.view_SIMInfo where club_id={club_id} and is_online={is_online}";
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    daoSIMInfo dao = new daoSIMInfo();
                    dao.club_id = reader.GetInt32(reader.GetOrdinal("club_id"));
                    dao.Dovecote_sn = reader.GetString(reader.GetOrdinal("dovecote_sn"));
                    dao.ICCID = reader.GetString(reader.GetOrdinal("ICCID"));
                    dao.IP = reader.GetString(reader.GetOrdinal("IP"));
                    dao.Device_UUID = reader.GetString(reader.GetOrdinal("Device_UUID"));
                    dao.is_online = reader.GetInt32(reader.GetOrdinal("is_online"));
                    if(dao.is_online == 1)
                    {
                        dao.str_is_online = "連線";
                    }
                    else
                    {
                        dao.str_is_online = "離線";
                    }
                    _SIMInfo_list.Add(dao);
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }

            return _SIMInfo_list;
        }

    }
}
