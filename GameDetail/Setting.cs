using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameDetail
{
    public class Setting
    {
        public static bool AutoLoadRecord = false;
        public static bool TopPigeonStart = false;

        public static string ClubName = "";
        public static int ClubID = 0;
        public static int AlertTime = 0;
        public static int InvTime = 0;

        public static bool AutoSendSMS = false;
        public static Dictionary<string, daoPigeon_Dovecote> _dovecote_list = new Dictionary<string, daoPigeon_Dovecote>();

        public static Queue<string> PopMessage_queue = new Queue<string>();

        public static string SMS_USER = "";
        public static string SMS_PASS = "";
        public static string SMS_DEBUG_PHONE = "";

        public static void LoadSMSSetting(string club_id)
        {
            // load from mysql systemconfig table
            string sql = $"SELECT * FROM pc.SystemConfig as A where A.key like 'sms_{club_id}%'";

            utility util = new utility();
            using var conn = util.connectdb();
            try
            {
                using var cmd = new MySql.Data.MySqlClient.MySqlCommand(sql, conn);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    
                    string key = reader.GetString(reader.GetOrdinal("key"));
                    string value = reader.GetString(reader.GetOrdinal("value"));
                    if(key== $"sms_{club_id}_usr")
                    {
                        SMS_USER = value;
                    }
                    else if(key == $"sms_{club_id}_pwd")
                    {
                        SMS_PASS = value;
                    }
                    else if (key == $"sms_{club_id}_debug_phone")
                    {
                        SMS_DEBUG_PHONE = value;
                    }
                }
            }
            catch (Exception ex)
            {
                //Log.Error(ex.ToString());
            }

        }

    }
}
