using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using iNervCore.DB;
using iNervCore.UTIL;
using System.IO;

namespace iNervCore.NET
{
    class CSendQueue
    {
        ConcurrentQueue<string> qSendMsg = new ConcurrentQueue<string>();

        CSqlite pDB = new CSqlite();

        bool bLoop = true;
        Thread thDoWork;

        public CSendQueue()
        {   
            thDoWork = new Thread(new ParameterizedThreadStart(Thread_Pool));
            thDoWork.Start(this);

            string sPath = Directory.GetCurrentDirectory() + "\\Queue.db";
            if( !pDB.Open(sPath) )
            {
                pDB.CreateDatabase(sPath);
                if (!pDB.Open(sPath))
                {
                    CLog.LOG(LOG_TYPE.DATA, "Queue DB Open Failed!!");
                }
            }
        }

        private static void Thread_Pool(object obj)
        {
            CSendQueue pObj = (CSendQueue)obj;
            string sSendMsg = "";
            while(pObj.bLoop)
            {
                if( pObj.pDB.LoadData("SELECT * FROM tblQueue") > 0)
                {
                    //idx, msg
                    //읽어와서 Send
                }
                Thread.Sleep(500);
            }
        }

        public int AddMessage(string sMsg)
        {
            qSendMsg.Enqueue(sMsg);

            return qSendMsg.Count;
        }

        public bool PopMessage(out string sRes)
        {
            bool bRs = false;

            bRs = qSendMsg.TryDequeue(out sRes);
            return bRs;
        }
    }
}
