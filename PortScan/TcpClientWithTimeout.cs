using System;
using System.Net.Sockets;
using System.Threading;
using System.Net;

/// <summary>
/// TcpClientWithTimeout 用来设置一个带连接超时功能的类
/// 使用者可以设置毫秒级的等待超时时间 (1000=1second)
/// 例如:
/// TcpClient connection = new TcpClientWithTimeout('127.0.0.1',80,1000).Connect();
/// </summary>
public class TcpClientWithTimeout
{
    private static bool IsConnectionSuccessful = false;
    private static Exception socketexception;
    private static ManualResetEvent TimeoutObject = new ManualResetEvent(false);
    public static TcpClient TryConnect(IPEndPoint remoteEndPoint, int timeoutMiliSecond)
    {
        TimeoutObject.Reset();
        socketexception = null; 
        string serverip = Convert.ToString(remoteEndPoint.Address);
        int serverport = remoteEndPoint.Port;           
        TcpClient tcpclient = new TcpClient();

        tcpclient.BeginConnect(serverip, serverport, 
            new AsyncCallback(CallBackMethod), tcpclient);
        if (TimeoutObject.WaitOne(timeoutMiliSecond, false))
        {
            if (IsConnectionSuccessful)
            {
                return tcpclient;
            }
            else
            {
                //throw socketexception;
                return null;
            }
        }
        else
        {
            tcpclient.Close();
            throw new TimeoutException("TimeOut Exception");
        }
    }
    private static void CallBackMethod(IAsyncResult asyncresult)
    {
        try
        {
            IsConnectionSuccessful = false;
            TcpClient tcpclient = asyncresult.AsyncState as TcpClient;

            if (tcpclient.Client != null)
            {
                tcpclient.EndConnect(asyncresult);
                IsConnectionSuccessful = true;
            }
        }
        catch (Exception ex)
        {
            IsConnectionSuccessful = false;
            socketexception = ex;
        }
        finally
        {
            TimeoutObject.Set();
        }
    }
}