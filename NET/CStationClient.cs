using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using iNervCore.NET;
using iNervCore.UTIL;

namespace iNervCore.Module
{
    public class CStationClient
    {
        CWinSock pClient = null;
        string sID = "";

        string IP;
        int PORT;

        bool bExit = false;
        int nRetryCnt = 0;

        public string LastRecvStatusData = "";
        public string LastSendData = "";

        System.Timers.Timer pTimer = new System.Timers.Timer();
        System.Timers.Timer pTimeOutTimer = new System.Timers.Timer();

        public delegate void DF_Parse(string sRCV);
        public DF_Parse dfParse = null;

        public delegate void DF_ConStats(bool bConn);
        public DF_ConStats dfConStats = null;

        public string ID
        {
            get { return sID; }
            set { sID = value; }
        }

        public void Init(string sIP, int nPort, string sID)
        {
            bExit = false;
            ID = sID;
            IP = sIP;
            PORT = nPort;
            pClient = new CWinSock(this.ConnComplete, this.SendComplete, this.RecvComplete);
            pClient.Connect(IP, PORT);

            pTimer.Interval = 10000;
            pTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_Elapsed);
            pTimer.Start();

            pTimeOutTimer.Interval = 6000; //6초
            pTimeOutTimer.Elapsed += new System.Timers.ElapsedEventHandler(Timer_TimeOutCheck);
        }

        public void Close()
        {
            bExit = true;

            pTimer.Stop();
            pTimer.Close();

            pTimeOutTimer.Stop();
            pTimeOutTimer.Close();

            if (pClient != null)
                pClient.Close();
        }

        public void ReConnect()
        {
            CLog.LOG(LOG_TYPE.WSK, "Not Connected - ReTry Close");
            pClient.Close();
            //System.Threading.Thread.Sleep(1000);

            Stopwatch pStopwatch = new Stopwatch();
            pStopwatch.Start();
            while (pStopwatch.Elapsed.Seconds < 1)
            {
                //Application.DoEvents();
                System.Threading.Thread.Sleep(10);
            }
            pStopwatch.Stop();

            CLog.LOG(LOG_TYPE.WSK, "Not Connected - ReTry Connect");
            pClient.Connect(IP, PORT);
        }

        public void StartSendTimeOutChecker()
        {
            StopSendTimerOutChecker();
            pTimeOutTimer.Start();
        }

        public void StopSendTimerOutChecker()
        {
            pTimeOutTimer.Stop();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            pTimer.Stop();

            if(!bExit)
            {
                //SendMsg("OK");
                if (pClient.CONNECT_STATE != 7)
                {
                    if (dfConStats != null)
                        dfConStats(false);

                    ReConnect();
                }
                pTimer.Start();
            }
        }

        private void Timer_TimeOutCheck(object sender, System.Timers.ElapsedEventArgs e)
        {
            pTimeOutTimer.Stop();
            //Timer Out
            if (pClient.CONNECT_STATE == 7)
            {
                if (nRetryCnt < 2)
                {
                    //retry send
                    ++nRetryCnt;
                    SendJson(LastSendData);
                    CLog.LOG(LOG_TYPE.WSK, "Send ReTry - " + LastSendData);
                    return;
                }
                pClient.Close();
                pClient.CONNECT_STATE = 0;
                CLog.LOG(LOG_TYPE.WSK, "TimeOut - Close");
            }
        }

        public void ConnComplete()
        {
            if (pClient.CONNECT_STATE == 7)
            {
                //접속 성공
                if (dfConStats != null)
                    dfConStats(true);
            }
            else
            {
                //접속 실패
                if (dfConStats != null)
                    dfConStats(false);
            }
        }

        public void SendComplete()
        {
        }

        public void RecvComplete(string sRCV)
        {
            StopSendTimerOutChecker();
            if (dfParse != null)
                dfParse(sRCV);
        }

        public void SendCmd(string sID, string sCMD, string sRTN, string sBody)
        {
            string sData = "";
            try
            {
                sData = string.Format("{0}{1}{2}{3}", sID, sCMD, sRTN, sBody);
                CLog.LOG(LOG_TYPE.WSK, "SendCmd: " + sData);

                if (pClient == null)
                    return;

                //if (pClient.CONNECT_STATE == 7)
                {
                    //접속 성공
                    pClient.Send(sData);
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "Send Failed: " + e.Message);
            }
        }

        public void SendMsg(string sMsg)
        {
            try
            {
                CLog.LOG(LOG_TYPE.WSK, "SendMsg: " + sMsg);

                if (pClient == null)
                    return;

                //if (pClient.CONNECT_STATE == 7)
                {
                    //접속 성공
                    pClient.Send(sMsg);
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "SendMsg Failed: " + e.Message);
            }
        }

        public bool SendJson(string sJson)
        {
            try
            {
                //if(LastSendData != sJson)
                CLog.LOG(LOG_TYPE.WSK, "SendJson: " + sJson);
                if (sJson.IndexOf("\"Vno\": 100,") < 0)
                {
                    LastSendData = sJson;
                    CLog.LOG(LOG_TYPE.WSK, "재전송 가능 메세지");
                } 

                if (pClient == null)
                    return false;

                //if (pClient.CONNECT_STATE == 7)
                {
                    sJson = sJson;
                    //접속 성공
                    pClient.Send(sJson);
                    StartSendTimeOutChecker();
                }
            }
            catch (Exception e)
            {
                //2022.04.22 swyang: 예외 발생 시 예외코드 로그에 추가
                CLog.LOG(LOG_TYPE.ERR, "SendJson Failed(" + e.HResult.ToString() + "): " + e.Message);
                return false;
            }
            return true;
        }

        public void SendJson_Retry(string sJson, int nRetryCnt = 3)
        {
            try
            {
                int i = 0;
                for (i = 0; i < nRetryCnt; ++i)
                {
                    SendJson(sJson);

                    CFunc.Sleep(3);


                    //응답없음 다시 시도
                    if (i == nRetryCnt-1)
                    {
                        //연결 끊고 다시 연결 후 Send
                        ReConnect();
                    }
                    CFunc.Sleep(1);
                }
            }
            catch (Exception e)
            {
                CLog.LOG(LOG_TYPE.ERR, "SendJson_Retry Failed: " + e.Message);
            }
        }
    }
}
