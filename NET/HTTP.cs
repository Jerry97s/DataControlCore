using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace iNervCore.NET
{
    public class RequestState
    {
        public string sCmd;
        public byte[] data;
        public HttpWebRequest request;
        public RequestState()
        {
            sCmd = "";
            data = null;
            request = null;
        }
    }

    public class HTTP
    {
        public static string sLastError = "";
        public static int nRequestTimeOut = 1000;
        public static HttpWebResponse pResponse;
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        public delegate void DeleHttpParsing(string sResponse);
        private static DeleHttpParsing deleHttpParse = null;

        public static string POST(string sUrl, StringBuilder sbData, ref string sToken)
        {
            return POST(sUrl, sbData.ToString(), ref sToken);
        }

        public static string POST(string sUrl, string sData, ref string sToken)
        {
            string response = "";

            try
            {
                byte[] sendData = Encoding.UTF8.GetBytes(sData);
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(sUrl);
                httpRequest.Timeout = nRequestTimeOut;                
                httpRequest.Headers.Add("Cache-Control", "no-cache");
                httpRequest.Accept = @"text/html,application/xhtml+xml,application/xml,image/webp,image/apng,*/*";
                httpRequest.ContentType = "application/x-www-form-urlencoded";
                httpRequest.UserAgent = ".NET Framework";
                //httpRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/83.0.4103.106 Safari/537.36";
                //httpRequest.Referer = sUrl.Substring(0, sUrl.IndexOf("tpms/")+5);
                //httpRequest.Expect = "";
                //httpRequest.KeepAlive = true;
                //httpRequest.AllowAutoRedirect = false;
                if (sToken != "")
                {
                    httpRequest.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
                }
                httpRequest.Method = "POST";
                httpRequest.ContentLength = sendData.Length;

                Stream reqStream = httpRequest.GetRequestStream();
                reqStream.Write(sendData, 0, sendData.Length);
                reqStream.Close();

                HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream(), Encoding.GetEncoding("UTF-8"));
                response = streamReader.ReadToEnd();

                //헤더에서 토큰 추출
                if (sToken == "")
                {
                    sToken = httpResponse.Headers.Get("X-TPMS-AUTH-TOKEN");
                }

                streamReader.Close();
                httpResponse.Close();
            }
            catch (Exception e)
            {
                //string s = e.Message;
                //throw e;
                sLastError = "POST: " + e.Message;
            }

            return response;
        }

        public static string GET(string url, string param)
        {
            return GET(url + "?" + param);
        }
        public static string GET(string url)
        {
            string request = "";
            return request;
        }

        public static bool POST(string sUrl, string sData, string sToken, DeleHttpParsing deleParseFunc = null)
        {
            try
            {
                deleHttpParse = deleParseFunc;
                //byte[] sendData = UTF8Encoding.UTF8.GetBytes(sData);
                
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(sUrl);
                request.Timeout = 4500;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                //if (sToken != "")
                //{
                //    request.Headers.Add("X-TPMS-AUTH-TOKEN", sToken);
                //}
                request.Method = "POST";
                //request.ContentLength = sendData.Length;

                RequestState state = new RequestState();
                state.data = UTF8Encoding.UTF8.GetBytes(sData);
                request.ContentLength = state.data.Length;

                state.request = request;
                //Stream reqStream = request.GetRequestStream();
                //reqStream.Write(sendData, 0, sendData.Length);
                //reqStream.Close();

                //request.BeginGetResponse(new AsyncCallback(GetResponseCallback), state);
                request.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sLastError = "POST: " + e.Message;
                return false;
            }
            return true;
        }

        private static void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                RequestState state = (RequestState)asynchronousResult.AsyncState;
                HttpWebRequest request = (HttpWebRequest)state.request;

                // End the operation
                Stream postStream = request.EndGetRequestStream(asynchronousResult);

                // Write to the request stream.
                postStream.Write(state.data, 0, state.data.Length);
                postStream.Close();

                // Start the asynchronous operation to get the response
                request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                sLastError = "GetRequestStreamCallback: " + e.Message;
            }
        }

        private static void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

                // End the operation
                HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
                Stream streamResponse = response.GetResponseStream();
                StreamReader streamRead = new StreamReader(streamResponse, Encoding.Default, true);
                string sResponse = streamRead.ReadToEnd();
                pResponse = response;
                Console.WriteLine(sResponse);
                // Close the stream object
                streamResponse.Close();
                streamRead.Close();
                // Release the HttpWebResponse
                response.Close();

                if (deleHttpParse != null)
                {
                    if (sResponse != "")
                        deleHttpParse(sResponse);
                }

                allDone.Set();
            }
            catch (Exception e)
            {
                sLastError = "GetResponseCallback: " + e.Message;
            }
        }
    }
}
