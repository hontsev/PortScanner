using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Collections;
using System.Net;
using System.Threading; 

namespace PortScan
{
    public partial class Form1 : Form
    {
        bool isRun = false;
        const int threadNum = 500;
        Thread[] threads;
        IPAddress ip;
        int portNum = 0;
        delegate void sendStringDelegate(string str);
        delegate int getIntDelegate();
        sendStringDelegate printEvent;
        sendStringDelegate printPortEvent;
        getIntDelegate getNextPortEvent;

        public Form1()
        {
            InitializeComponent();

            printEvent = new sendStringDelegate(print);
            printPortEvent = new sendStringDelegate(printPort);
            getNextPortEvent = new getIntDelegate(getNextPort);
        }

        private int getNextPort()
        {
            portNum++;
            return portNum;
        }

        private void print(string str)
        {
            if (textBox2.Text.Length >= 999) textBox2.Text = "";
            textBox2.AppendText(str + "\r\n");
        }

        private void printPort(string str)
        {
            textBox3.AppendText(str + "\r\n");
        }

        private void workScan()
        {
            try
            {
                TcpClient client = null;
                int nowPort = (int)(Invoke(getNextPortEvent));
                while (nowPort <= 65535)
                {
                    try
                    {
                        //扫描指定的端口范围
                        //Thread.Sleep(500);
                        client = new TcpClient();
                        client.Connect(ip, nowPort);
                        Invoke(printPortEvent, (object)(ip.ToString() + ":" + nowPort));
                    }
                    catch (SocketException)
                    {
                        Invoke(printEvent, (object)(ip.ToString() + ":" + nowPort + " 关闭"));
                    }
                    nowPort = (int)(Invoke(getNextPortEvent));
                }
                //client.connection.Close();
            }
            catch(Exception ex)
            {
                Invoke(printEvent, (object)(" - " + ex.Message));
            }
        }
        private void beginScan()
        {
            portNum = 0;
            textBox2.Text = "";
            textBox3.Text = "";
            isRun = true;
            button1.Text = "停止";
            threads = new Thread[threadNum];
            string host = textBox1.Text;
            try
            {
                IPAddress[] hostinfo = Dns.GetHostAddresses(host);
                foreach (var a in hostinfo)
                {
                    //print("ip地址")
                }
                if (hostinfo.Length >= 1)
                {
                    ip = hostinfo[0];
                    print("开始扫描ip：" + ip);
                    for (int i = 0; i < threadNum; i++)
                    {
                        Thread th = new Thread(workScan);
                        threads[i] = th;
                        th.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                print(ex.Message);
            }
        }

        private void endScan()
        {
            foreach (Thread th in threads)
            {
                if (th != null && th.IsAlive)
                {
                    th.Abort();
                }
            }
            button1.Text = "扫描";
            isRun = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                endScan();
            }
            else
            {
                beginScan();
            }
        }

        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                if (isRun)
                {
                    endScan();
                }
                else
                {
                    beginScan();
                }
            }
        }
    }
}
