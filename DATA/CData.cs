using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Media;

namespace iNervCore.DATA
{
    

    public class CData
    {

        public static string sDataDir = Environment.CurrentDirectory + @"\DATA";
        public static string sSetDir = Environment.CurrentDirectory + @"\SET";
        public static string sIniPath = sSetDir + "\\" + "Setting.Ini";
        public static string sSetDB = sSetDir + "\\" + "setting.db";

        //public delegate void GDF_ShowErrMsg_DcTicket();
        //public static GDF_ShowErrMsg_DcTicket gDf_ShowErrMsg_DcTicket;
    }
}
