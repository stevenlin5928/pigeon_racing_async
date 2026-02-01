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

        public static Dictionary<string, daoPigeon_Dovecote> _dovecote_list = new Dictionary<string, daoPigeon_Dovecote>();

        public static Queue<string> PopMessage_queue = new Queue<string>();

    }
}
