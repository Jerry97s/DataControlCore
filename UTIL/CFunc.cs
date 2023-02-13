using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

using iNervMCS.DATA;
using System.IO.Ports;

namespace iNervCore.UTIL
{
    public class CFunc
    {
        #region 폴더/파일
        public static bool CheckDir(string sDir)
        {
            DirectoryInfo di = new DirectoryInfo(sDir);
            if (!di.Exists)
            {
                Directory.CreateDirectory(sDir);
            }

            di = new DirectoryInfo(sDir);
            return di.Exists;
        }

        public static bool CheckFile(string sFile)
        {
            //sFile = sFile.Replace("\\", "/");
            FileInfo fi = new FileInfo(sFile);
            return fi.Exists; ;
        }
        #endregion

        #region 체크
        public static bool PortCheck(string sPort, int nSpeed)
        {
            bool bCheck = false;
            SerialPort pCom;
            try
            {
                pCom = new SerialPort();

                pCom.PortName = sPort;
                pCom.BaudRate = nSpeed;
                pCom.Parity = Parity.None;
                pCom.DataBits = 8;
                pCom.StopBits = StopBits.One;

                pCom.Open();
                bCheck = pCom.IsOpen;
            }
            catch (Exception e)
            {
                bCheck = false;
                Console.WriteLine("PortCheck Error: " + e.Message);
            }


            return bCheck;
        }

        public static void Sleep(int nSecond)
        {
            Stopwatch pStopwatch = new Stopwatch();
            pStopwatch.Start();
            while (pStopwatch.Elapsed.Seconds < nSecond)
            {
                //응답을 3초동안 기다림

                System.Threading.Thread.Sleep(1);
            }
            pStopwatch.Stop();
        }
        #endregion

        #region 날짜/시간
        /// <summary>
        /// 분 -> 일 시간 분
        /// 63 -> 1시간 3분
        /// </summary>
        /// <param name="sM"></param>
        /// <returns></returns>
        public static string MtoHM(string sM)
        {
            string sHM = sM;
            if (string.IsNullOrEmpty(sM) || sM == "")
                return "0분";

            int n = int.Parse(sM);

            int nD = 0;
            int nH = 0;
            int nM = 0;

            nD = n / 1440;
            nH = (n % 1440) / 60;
            nM = (n % 1440) % 60;

            sHM = ((nD > 0) ? nD.ToString() + "일" : "") +
                  ((nH > 0) ? nH.ToString() + "시간" : "") +
                  (nM.ToString() + "분");

            return sHM;
        }

        public static string S2D(string sDt)
        {
            string sRet = sDt;

            if (sDt.Length == 8)
            {
                sRet = sDt.Substring(0, 4) + "-" + sDt.Substring(4, 2) + "-" + sDt.Substring(6);
            }
            else if (sDt.Length == 12)
            {
                sRet = DateTime.Now.ToString("yyyy").Substring(0,2) + sDt.Substring(0, 2) + "-" + sDt.Substring(2, 2) + "-" + sDt.Substring(4, 2) + " " + sDt.Substring(6, 2) + ":" + sDt.Substring(8, 2) + ":" + sDt.Substring(10, 2);
                sRet = D2S(DateTime.Parse(sRet), 142);
            }
            else if(sDt.Length == 13)
            {
                sRet = sDt.Substring(0, 2) + "-" + sDt.Substring(2, 2) + "-" + sDt.Substring(4, 2) + " " + sDt.Substring(6, 2) + ":" + sDt.Substring(8, 2) + ":" + sDt.Substring(10, 2);
                sRet = D2S(DateTime.Parse(sRet), 142);
            }
            else if (sDt.Length == 14)
            {
                sRet = sDt.Substring(0, 4) + "-" + sDt.Substring(4, 2) + "-" + sDt.Substring(6, 2) + " " + sDt.Substring(8, 2) + ":" + sDt.Substring(10, 2) + ":" + sDt.Substring(12, 2);
                sRet = D2S(DateTime.Parse(sRet), 142);
            }

            return sRet;
        }

        public static string D2S(DateTime dt, int nCase)
        {
            string sRet = "";
            switch (nCase)
            {
                case 2://두자리 초 : 02초
                    sRet = dt.ToString("ss");
                    break;
                case 21://두자리 분 : 02분
                    sRet = dt.ToString("mm");
                    break;
                case 4://시분 : 1802 - 18시 02분
                    sRet = dt.ToString("HHmm");
                    break;
                case 41://년월 : 1912 - 19년 12월
                    sRet = dt.ToString("yyMM");
                    break;
                case 6://년월일 : 191218 - 19년 12월 18일
                    sRet = dt.ToString("yyMMdd");
                    break;
                case 61://시분초 : 151131 - 15시 11분 31초
                    sRet = dt.ToString("HHmmss");
                    break;
                case 62://시:분:초.00 : 15:11:31.12 - 15시 11분 31.12초
                    sRet = dt.ToString("HH:mm:ss.ff");
                    break;
                case 63://시:분:초
                    sRet = dt.ToString("HH:mm:ss");
                    break;
                case 64:
                    sRet = dt.ToString("MMdd");
                    break;
                case 8://년월일
                    sRet = dt.ToString("yyyyMMdd");
                    break;
                case 81://일 시:분:초
                    sRet = dt.ToString("dd HH:mm:ss");
                    break;
                case 82://년-월-일
                    sRet = dt.ToString("yyyy-MM-dd");
                    break;
                case 10://년월일시분 : 1912181511 - 19년 12월 18일 15시 11분
                    sRet = dt.ToString("yyMMddHHmm");
                    break;
                case 12://년월일시분초 : 191218151131 - 19년 12월 18일 15시 11분 31초
                    sRet = dt.ToString("yyMMddHHmmss");
                    break;
                case 121://년월일시분 : 201912181511 - 2019년 12월 18일 15시 11분
                    sRet = dt.ToString("yyyyMMddHHmm");
                    break;
                case 122://년/월/일 시:분
                    sRet = dt.ToString("yyyy/MM/dd HH:mm");
                    break;
                case 123://년-월-일 시:분
                    sRet = dt.ToString("yyyy-MM-dd HH:mm");
                    break;
                case 14://년월일시분초 : 20191218151131 - 2019년 12월 18일 15시 11분 31초
                    sRet = dt.ToString("yyyyMMddHHmmss");
                    break;
                case 141://년/월/일 시:분:초
                    sRet = dt.ToString("yyyy/MM/dd HH:mm:ss");
                    break;
                case 142://년-월-일 시:분:초
                    sRet = dt.ToString("yyyy-MM-dd HH:mm:ss");
                    break;
                case 16:
                    sRet = dt.ToString("yyyyMMddHHmmssff");
                    break;
                case 161:
                    sRet = dt.ToString("yyyy-MM-dd HH:mm:ss.ff");
                    break;
            }

            return sRet;
        }

        public static int DiffMonth(DateTime dtS, DateTime dtE)
        {
            //int nMon = 0;
            //nMon = 12 * (dtE.Year - dtS.Year) + dtE.Month -  dtS.Month;
            return (12 * (dtE.Year - dtS.Year) + dtE.Month - dtS.Month);
        }

        public static bool IsFullMon(DateTime dtS, DateTime dtE)
        {
            bool bFM = false;

            if (dtS.Day == 1 && dtE.Day == DateTime.DaysInMonth(dtE.Year, dtE.Month))
                bFM = true;

            return bFM;
        }

        #endregion

        #region 문자열관련
        //nData 앞에 0을 붙임: 총길이 nCnt - (11, 5) -> 00011
        public static string Zero(int nData, int nCnt)
        {
            string sRsData = "";
            string sTmp = "{0:D" + nCnt.ToString() + "}";
            sRsData = string.Format(sTmp, nData);
            if (sRsData.Length == 1)
                sRsData = "0" + sRsData;

            return sRsData;
        }
        //sData 뒤에 0x00을 붙임: 총길이 nLen - ("1234", 6) -> 1 2 3 4 0x0 0x0 0x0
        public static string FullZero(string sData, int nLen)
        {
            string sRsData = "";
            int nPLen = nLen - sData.Length;
            sRsData = sData;
            for (int i = 0; i < nPLen; ++i)
            {
                sRsData += Convert.ToString('\x00');
            }

            return sRsData;
        }
        public static string FullSpace(string sData, int nLen)
        {
            string sRsData = "";
            int nRLen = nLen - sData.Length;
            sRsData = sData;
            for (int i = 0; i < nRLen; ++i)
            {
                sRsData += " ";
            }

            return sRsData;
        }
        public static string WON(int nData, int nType)
        {
            string sRTN = "";
            string[] arType = { "", "원", "개", "건", "매", "회", "대" };
            if (nType >= arType.Length)
                nType = 0;

            sRTN = string.Format("{0:N0}{1}", nData, arType[nType]);

            return sRTN;
        }
        public static string ConvertEndian(string sHex)
        {
            string sRs = "";
            int i = 0;
            int j = 0;
            byte btmp;
            byte[] bt = new byte[sHex.Length / 2];
            for (i = 0, j = 0; i < sHex.Length; i += 2, j++)
            {
                bt[j] = H2D(sHex.Substring(i, 2));
            }
            j = bt.Length / 2;
            for (i = 0; i < j; ++i)
            {
                btmp = bt[i];
                bt[i] = bt[bt.Length - 1 - i];
                bt[bt.Length - 1 - i] = btmp;
            }

            sRs = BitConverter.ToString(bt);
            sRs = sRs.Replace("-", "");

            return sRs;
        }
        public static string C2H(string sData)
        {
            string resultHex = "";
            byte[] btTmp = Encoding.Default.GetBytes(sData);
            foreach (byte byteStr in btTmp)
            {
                resultHex += string.Format("{0:X2}", byteStr);
            }

            return resultHex;
        }
        public static List<string> C2HArray(string sData)
        {
            List<string> arHex = new List<string>();
            string resultHex = "";
            byte[] btTmp = Encoding.Default.GetBytes(sData);
            foreach (byte byteStr in btTmp)
            {
                arHex.Add(string.Format("{0:X2}", byteStr));
            }

            return arHex;
        }
        public static Byte H2D(string sData)
        {
            try
            {
                return Convert.ToByte(sData, 16);
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "H2D 에러: " + sData  + " / " + e.Message);
            }
            return 0;
        }
        public static string H2C_ALL(string sData)
        {
            string sRTN = "";
            string sTmp = "";
            string sHan = "";
            bool bHAN = false;
            try
            {
                if (sData != "")
                {
                    sData = sData.Replace(" ", "");
                    sData.Trim();
                    for (int i = 0; i < sData.Length; i += 2)
                    {
                        sTmp = sData.Substring(i, 2);
                        if (H2D(sTmp) >= 127 && !bHAN)
                        {
                            //sHan = Encoding.Default.GetString(H2D(sTmp));
                            sHan = sTmp;
                            bHAN = true;
                        }
                        else
                        {
                            sTmp = sHan + sTmp;
                            sRTN += Convert.ToChar(Convert.ToInt32(sTmp, 16)).ToString();
                            sHan = "";
                            bHAN = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "H2C_ALL 에러: " + e.Message);
            }
            return sRTN;
        }

        public static string H2C(string sData)
        {
            string sRTN = "";
            string sTmp = "";
            string sHan = "";
            bool bHAN = false;
            try
            {
                if (sData != "")
                {
                    sData = sData.Replace(" ", "");
                    sData.Trim();
                    for (int i = 0; i < sData.Length; i += 2)
                    {
                        sTmp = sData.Substring(i, 2);
                        if (H2D(sTmp) >= 127 && !bHAN)
                        {
                            //sHan = Encoding.Default.GetString(H2D(sTmp));
                            sHan = sTmp;
                            bHAN = true;
                        }
                        else
                        {
                            sTmp = sHan + sTmp;
                            sRTN += Convert.ToChar(Convert.ToInt32(sTmp, 16)).ToString();
                            sHan = "";
                            bHAN = false;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "H2C_ALL 에러: " + e.Message);
            }
            return sRTN;
        }

        public static string Hex2Bin(string sHex)
        {
            return Convert.ToString(Convert.ToInt32(sHex, 16), 2).PadLeft(sHex.Length * 4, '0');
        }

        public static int GetByteSize(string strValue)
        {
            if (string.IsNullOrEmpty(strValue))
            {
                return 0;
            }

            var strLen = strValue.Length;

            var ascii = new ASCIIEncoding();
            var encodedBytes = ascii.GetBytes(strValue);
            var decodedString = ascii.GetString(encodedBytes);
            for (var idx = 0; idx <= decodedString.Length - 1; idx++)
            {
                if (Asc(decodedString[idx]).Equals(63))
                {
                    strLen += 1;
                }
            }
            return strLen;
        }
        public static string ToStringKor(byte[] data)
        {
            string toString = Encoding.GetEncoding("EUC-KR").GetString(data);
            //string toString = Encoding.Default.GetString(data);
            return toString.Trim('\0');
        }

        public static string SPACE(int nNum)
        {
            string temp = "";

            temp = temp.PadRight(nNum);
            return temp;
        }

        public static string YnNULL(string sData, int nType=1)
        {
            string sRes = sData;

            if(string.IsNullOrEmpty(sData))
            {
                if (nType == 0)
                    sRes = "0";
            }

            return sRes;
        }

        private static int Asc(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                throw new ArgumentException("Argument Length Zero");
            }
            var ch = str[0];
            return Asc(ch);
        }
        public static int Asc(char chr)
        {
            int num;
            var num2 = Convert.ToInt32(chr);

            if (num2 < 0x80)
            {
                return num2;
            }

            try
            {
                byte[] buffer;
                var fileIOEncoding = Encoding.Default;
                var chars = new[] { chr };
                if (fileIOEncoding.IsSingleByte)
                {
                    buffer = new byte[1];
                    fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0);
                    return buffer[0];
                }
                buffer = new byte[2];
                if (fileIOEncoding.GetBytes(chars, 0, 1, buffer, 0) == 1)
                {
                    return buffer[0];
                }
                if (BitConverter.IsLittleEndian)
                {
                    var num4 = buffer[0];
                    buffer[0] = buffer[1];
                    buffer[1] = num4;
                }
                num = BitConverter.ToInt16(buffer, 0);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return num;
        }

        public static string MakeBCC(string sData)
        {
            string sRes = "";

            Byte bcc = 0;
            Byte[] bData = Encoding.Default.GetBytes(sData);
            for (int i = 0; i < bData.Length; ++i)
            {
                bcc ^= bData[i];
            }

            sRes = sData + (char)bcc;

            return sRes;
        }

        /// <summary>
        /// 한글 2byte
        /// 영문 1byte
        /// </summary>
        /// <param name="str"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string ExSubString(string str, int startIndex, int length)
        {
            byte[] b1 = null;
            byte[] b2 = null;

            try
            {
                if (str == null)
                {
                    return "";
                }

                b1 = Encoding.Default.GetBytes(str);
                b2 = new byte[length];

                if (length > (b1.Length - startIndex))
                {
                    length = b1.Length - startIndex;
                }

                System.Array.Copy(b1, startIndex, b2, 0, length);
            }
            catch (Exception)
            {
                //e.printStackTrace();
            }

            return Encoding.Default.GetString(b2);
        }
        #endregion

        public static void LoadConfig()
        {

        }


        #region MakeData
       
        #endregion
    }
}
