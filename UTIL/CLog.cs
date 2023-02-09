using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace iNervCore.UTIL
{
    public enum LOG_TYPE
    {
        ERR = 0,
        IO,
        SCREEN,
        DATA,
        PRT,
        BARCODE,
        MC,
        DEBUG,
        IO_MONEY,
        REG,
        SET,
        CHK,
        HOST,
        SERVER,
        WSK,
        LED,
        GT,
        KEY,
        LPR,
        WSK_WS,
        QUEUE,
        EASYFEE,
        SOUND,
        DR = 100
    }

    public class CLog
    {
        public static void LOG(LOG_TYPE tp, string sLog)
        {
            string sDir = CheckLogDir();
            string sFilePath = "";
            try
            {
                sFilePath = sDir + "\\" + DateTime.Today.ToString("yyyyMMdd") + "_ST_" + tp.ToString() + ".log";
                FileInfo fi = new FileInfo(sFilePath);
                if(!fi.Exists)
                {
                    FileStream fs = File.Create(sFilePath);
                    fs.Close();
                }

                string sMsg = string.Format("[{0}] {1}", DateTime.Now.ToString("HH:mm:ss.ff"), sLog);
                using (StreamWriter sw = File.AppendText(sFilePath))
                {
                    sw.WriteLine(sMsg);
                    sw.Close();
                }
            }
            catch (Exception)
            {
            }
        }

        public static string CheckLogDir()
        {
            string sLogDir = Environment.CurrentDirectory + @"\LOG";
            string sNowLogDir = sLogDir + "\\" + DateTime.Today.ToString("yyyyMMdd");

            if( CFunc.CheckDir(sLogDir) )
            {
                if (CFunc.CheckDir(sNowLogDir))
                    return sNowLogDir;

            }
            return "";
        }
    }
}
