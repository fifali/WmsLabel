using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.Collections;


namespace WmsLabel
{
    public class GetTcpListener
    {
        public string ipaddress = "192.168.1.234";//Socket服务器IP地址
        public string address = "192.168.1.234";//Socket服务器IP地址
        public string webserviceip = "192.168.1.234";//webservice接口地址
        public int host = 10001;//Socket服务器端口
        public int bytes = 1024;
        public TcpListener listener = null;
        public long i = 0;
        private bool done = false;
        public ArrayList lt = new ArrayList();
        public ArrayList sc = new ArrayList();
        control ct = new control();
        public ManualResetEvent allDone = new ManualResetEvent(false);
        /// <summary>
        /// 启动新线程监听
        /// </summary>
        public void GetListener()
        {
            //每个客户端请求都在客户端新建一个线程
            TcpClient client = new TcpClient(address, 10000);
            Thread accept = new Thread(new ThreadStart(AcceptClientInfo));
            accept.IsBackground = false;//后台线程
            accept.Start();
            //string ls_msg = "";
            //ls_msg = SendFile("");
            //while (ls_msg != "")
            //{
            //    Console.WriteLine(ls_msg);
            //    ls_msg = Console.ReadLine();
            //    ls_msg = SendFile(ls_msg);
            //}

            //StartListening();
        }

        public string SendFile(string message)
        {
            TcpClient client = new TcpClient(address, 10000);
            NetworkStream stream = client.GetStream();
            try
            {
                //1.发送数据   
                //byte[] messages = strToHexByte(message);
                byte[] messages = Encoding.ASCII.GetBytes(message);//ASC发送
                stream.WriteTimeout = 60000;//15秒发送时间
                stream.Write(messages, 0, messages.Length);
                //2.接收状态,长度<1024字节
                byte[] bytes = new Byte[1024];
                string data = string.Empty;
                stream.ReadTimeout = 60000;//15秒接收返回信息
                int length = stream.Read(bytes, 0, bytes.Length);
                if (length > 0)
                {
                    data = ToHexString(bytes);//十六进制接收
                    data = data.Substring(0, data.IndexOf("7E000000") + 2);
                }
                //3.关闭对象
                return data;
            }
            catch (Exception ex)
            {
                return ex.Message.ToString();
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        public static byte[] strToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        public static string ToHexString(byte[] bytes)
        {
            string hexString = string.Empty;
            if (bytes != null)
            {
                StringBuilder strB = new StringBuilder();

                for (int i = 0; i < bytes.Length; i++)
                {
                    strB.Append(bytes[i].ToString("X2"));
                }
                hexString = strB.ToString();
            }
            return hexString;
        }

        #region 非异步线程
        /// <summary>
        /// 线程执行函数
        /// TCPServer类的HandleConnection函数
        /// </summary>
        private void AcceptClientInfo()
        {
            try
            {
                int port;
                port = host;
                listener = new TcpListener(IPAddress.Any, port);
                listener.Start();
                while (!done)
                {
                    try
                    {
                        TcpClient client = listener.AcceptTcpClient();
                        TCPServer server = new TCPServer();
                        server.client = client;
                        server.listener = listener;
                        Thread clientThread = new Thread(new ThreadStart(server.HandleConnection));
                        clientThread.Start();
                    }
                    catch (Exception e)
                    {

                    }
                    System.GC.Collect();
                }
                listener.Stop();
            }
            catch (Exception e)
            {

            }
        }
        #endregion


        // State object for reading client data asynchronously     
        public class StateObject
        {
            // Client socket.     
            public Socket workSocket = null;
            // Size of receive buffer.     
            public const int BufferSize = 1024;
            // Receive buffer.     
            public byte[] buffer = new byte[BufferSize];
            // Received data string.     
            public StringBuilder sb = new StringBuilder();
        }

        // Thread signal.     
        public void StartListening()
        {
            Console.WriteLine("START......");
            // Data buffer for incoming data.     
            byte[] bytes = new Byte[1024];
            IPAddress ipAddress = IPAddress.Parse(ipaddress);
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, host);
            // Create a TCP/IP socket.     
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //Socket handler = new Socket(
            //Send(
            // Bind the socket to the local     
            //endpoint and listen for incoming connections.     
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);
                while (true)
                {
                    // Set the event to nonsignaled state.     
                    allDone.Reset();
                    // Start an asynchronous socket to listen for connections.     
                    //Console.WriteLine("Waiting for a connection...");
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing.     
                    allDone.WaitOne();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();
        }
        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.     
            allDone.Set();
            // Get the socket that handles the client request.     
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            // Create the state object.     
            StateObject state = new StateObject();
            state.workSocket = handler;
            Console.WriteLine("Geting for a connection..." + ((System.Net.IPEndPoint)handler.RemoteEndPoint).ToString());
            //接口IP地址需要屏蔽
            bool exists = ((IList)sc).Contains(handler);
            if (exists)
            { }
            else
            {
                //if (((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString() != ipaddress)
                //{
                sc.Add(handler);
                //}
            }
            Send(handler, "");
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public class control
        {
            string controlip;//控制器IP
            public string Controlip
            {
                get { return controlip; }
                set { controlip = value; }
            }
            string status;//控制器状态1表示正在执行指令，WEBSERVICE在等待返回值状态
            public string Status
            {
                get { return status; }
                set { status = value; }
            }
            string webserviceip;//WEBSERVICE地址，记录地址是为后续服务器得到返回值后寻找通讯的WEBSERVICE
            public string Webserviceip
            {
                get { return webserviceip; }
                set { webserviceip = value; }
            }
        }

        public void ReadCallback(IAsyncResult ar)
        {
            string ls_func;//功能号
            string ls_controlip;//控制器IP
            string ls_order;//指令
            Socket scnew = null;
            Socket scnew1 = null;
            try
            {
                String content = String.Empty;
                // Retrieve the state object and the handler socket     
                // from the asynchronous state object.     
                StateObject state = (StateObject)ar.AsyncState;
                Socket handler = state.workSocket;
                // Read data from the client socket.     
                int bytesRead = handler.EndReceive(ar);
                Console.WriteLine("Ending for a connection..." + ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString());
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far.     
                    //state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    //if (((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString() != webserviceip)
                    //{
                    state.sb.Append(ToHexString(state.buffer));//Label十六进制通讯指令
                    //}
                    //else
                    //{
                    //    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));//WEBSERVICE通讯指令
                    //}

                    // Check for end-of-file tag. If it is not there, read     
                    // more data.

                    content = state.sb.ToString();
                    Console.WriteLine("please input order:");
                    ls_order = Console.ReadLine();
                    Send(handler, ls_order);
                    if (content.IndexOf("@@") > -1)//@@表示来自WEBSERVICE接口的信息
                    {
                        foreach (var item in lt)
                        {
                            if (((control)item).Status == "1" && ((control)item).Controlip == ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString())
                            {
                                Send(handler, "该控制器正在执行任务，请稍后继续......");//下发信息到WEBSERVICE
                                return;
                            }
                        }
                        ls_func = content.Substring(0, content.IndexOf("@@"));
                        if (ls_func == "3001")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA017D5E807E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发预处理命令
                        }
                        else if (ls_func == "3002")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发拣货命令
                        }
                        else if (ls_func == "3003")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发盘点命令
                        }
                        else if (ls_func == "3004")
                        {
                            ls_controlip = content.Substring(6, content.LastIndexOf("@@") - 6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = content.Substring(content.LastIndexOf("@@") + 2);
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            //Console.WriteLine(ls_order);
                            Send(scnew, ls_order);//下发补货命令
                        }
                        else if (ls_func == "3005")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA057F437E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发完成下发命令
                        }
                        else if (ls_func == "3006")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA063F427E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发电子标签自检命令
                        }
                        else if (ls_func == "3007")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA07FE827E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发电子标签地址显示命令
                        }
                        else if (ls_func == "3008")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA08BE867E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发关闭电子标签地址显示命令
                        }
                        else if (ls_func == "3009")
                        {
                            ls_controlip = content.Substring(6);
                            foreach (var item in sc)
                            {
                                if (((System.Net.IPEndPoint)((Socket)item).RemoteEndPoint).Address.ToString() == ls_controlip)
                                {
                                    scnew = (Socket)item;
                                    break;
                                }
                            }
                            ls_order = "7EAA097F467E";
                            ct = new control();
                            ct.Controlip = ls_controlip;
                            ct.Webserviceip = webserviceip;
                            ct.Status = "1";
                            lt.Add(ct);
                            Send(scnew, ls_order);//下发复位命令
                        }
                        // client. Display it on the console.     
                        Console.WriteLine("Read {0} bytes from webservicesocket. \n Data : {1}", content.Length, content);
                        StateObject statenew = new StateObject();
                        statenew.workSocket = scnew;
                        scnew.BeginReceive(statenew.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), statenew);
                    }
                    else if (content.Substring(0, 4) == "7EAA")//表示来自控制器的信息
                    {
                        content = content.Substring(0, content.IndexOf("7E000000") + 2);
                        Console.WriteLine("Read {0} bytes from controlsocket. \n Data : {1}", content.Length, content);
                        if (content == "")//上架确认、拣货确认等电子标签自动访问的命令
                        {
                            //更新上架单明细的确认数量、确认状态等
                            //执行存储过程（增加库存、设置状态等）
                            return;
                        }
                        //解析消息内容
                        foreach (var item in lt)
                        {
                            if (((control)item).Status == "1" && ((control)item).Controlip == ((System.Net.IPEndPoint)handler.RemoteEndPoint).Address.ToString())
                            {
                                string ls_webserviceip;
                                ls_webserviceip = ((control)item).Webserviceip;
                                foreach (var item1 in sc)
                                {
                                    if (((System.Net.IPEndPoint)((Socket)item1).RemoteEndPoint).Address.ToString() == ls_webserviceip)
                                    {
                                        scnew1 = (Socket)item1;
                                        sc.Remove(item1);//webservice--服务器--控制器--服务器--webservice流程完成后，清除连接对象资源
                                        break;
                                    }
                                }
                                Send(scnew1, content);//下发控制器答复信息到WEBSERVICE
                                lt.Remove(item);//webservice--服务器--控制器--服务器--webservice流程完成后，清除绑定对象资源
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Read {0} bytes from nosocket. \n Data : {1}", content.Length, content);
                        // Not all data received. Get more.     
                        handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        public void Send(Socket handler, String data)
        {
            try
            {
                // Convert the string data to byte data using ASCII encoding.     
                //byte[] byteData = Encoding.ASCII.GetBytes(data);
                byte[] byteData = strToHexByte(data);
                // Begin sending the data to the remote device.     
                handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.     
                Socket handler = (Socket)ar.AsyncState;
                // Complete sending the data to the remote device.     
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
                //handler.Shutdown(SocketShutdown.Both);
                //handler.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }

}
