using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace iNervCore.UTIL
{
    public class CJson
    {
        public void Parse(string sData)
        {
        }

        public static string MakeSendData(int nVno, int nType, int nRes, Dictionary<string, string> drData)
        {
            string sJsonData = "";

            try
            {
                JObject obj = new JObject();
                obj.Add("Vno", nVno);
                obj.Add("Type", nType);
                obj.Add("Snd", "C");
                obj.Add("Res", nRes);
                if (drData == null || drData.Count <= 0)
                {
                    obj.Add("Data", "");
                }
                else
                {
                    string val = "";
                    JObject subObj = new JObject();
                    foreach (string key in drData.Keys)
                    {
                        val = drData[key];
                        subObj.Add(key, val);
                    }
                    obj.Add("Data", subObj);
                }
                sJsonData = obj.ToString();
                sJsonData = sJsonData.Replace("\t", "");
                sJsonData = sJsonData.Replace("\r\n", "");
                sJsonData = sJsonData.Replace(@"\r", "");
                sJsonData = sJsonData.Replace("|r", "");
                //sJsonData = sJsonData.Replace("", "");
                sJsonData = sJsonData.Trim();
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "MakeSendData Error: " + e.Message);
            }
            
            return sJsonData;
        }
    }
}
