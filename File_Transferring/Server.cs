using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Transferring
{
    class Server
    {

        public Server(MainWindow window)
        {
            this.window = window;
        }

        MainWindow window;
        SocketPermission permission;
        Socket socketListener;
        Socket handler;
        IPEndPoint ipEndPoint;
        Thread listen;
        FileStream fileStream;

        const int chunk = 1400;
        bool serverStatus = false;
        string dirPath;
        string fileName;
        byte[] tempArr;
        int toWrite;
        public void MainServer()
        {
            if (serverStatus == false)
            {
                int port = -1;
                bool goodPort = int.TryParse(window.textBox2.Text, out port);
                if (port < 1 || port > 65535)
                {
                    goodPort = false;
                }

                if (goodPort == true)
                {
                    StartServer();
                }
                else
                {
                    MessageBox.Show("Entered string isn't Port");
                }
            }
            else
            {
                StopServer();
            }
        }

        void StartServer()
        {
            try
            {
                // Creates one SocketPermission object for access restrictions
                permission = new SocketPermission(
                NetworkAccess.Accept,     // Allowed to accept connections 
                TransportType.Tcp,        // Defines transport types 
                "",                       // The IP addresses of local host 
                SocketPermission.AllPorts // Specifies all ports 
                );

                // Listening Socket object 
                socketListener = null;

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance 
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList[0];

                // Creates a network endpoint 
                ipEndPoint = new IPEndPoint(ipAddr, 4510);

                // Create one Socket object to listen the incoming connection 
                socketListener = new Socket(
                    ipAddr.AddressFamily,
                    SocketType.Stream,
                    ProtocolType.Tcp
                    );

                socketListener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

                // Associates a Socket with a local endpoint 
                socketListener.Bind(ipEndPoint);

                //Change status to Started
                window.textBox3.Enabled = false;
                window.button4.Enabled = true;
                window.button3.Text = "Stop";
                window.label5.Text = "Started";

                serverStatus = true;
                listen = new Thread(ReciveData);
                listen.IsBackground = true;
                listen.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                StopServer();
            }
        }

        void StopServer()
        {
            try
            {
                serverStatus = false;

                if (listen != null && listen.IsAlive == true)
                {
                    listen.Abort();
                }

                if (socketListener.Connected == true)
                {
                    socketListener.Shutdown(SocketShutdown.Both);
                }
                socketListener.Close();
                socketListener.Dispose();

                permission = null;
                socketListener = null;
                ipEndPoint = null;
                handler = null;

                //Change status to Stopped
                window.textBox3.Enabled = true;
                window.button4.Enabled = false;
                window.button3.Text = "Start";
                window.label5.Text = "Not started";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void ReciveData()
        {
            try
            {
                if (serverStatus == true)
                {
                    // Places a Socket in a listening state and specifies the maximum 
                    // Length of the pending connections queue 
                    socketListener.Listen(10);

                    // Begins an asynchronous operation to accept an attempt 
                    AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                    socketListener.BeginAccept(aCallback, socketListener);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void AcceptCallback(IAsyncResult ar)
        {
            if (serverStatus == true)
            {
                Socket listener = null;

                // A new Socket to handle remote host communication 
                Socket handler = null;
                try
                {
                    // Receiving byte array 
                    byte[] buffer = new byte[chunk];
                    // Get Listening Socket object 
                    listener = (Socket)ar.AsyncState;
                    // Create a new socket 
                    handler = listener.EndAccept(ar);

                    // Using the Nagle algorithm 
                    handler.NoDelay = false;

                    // Creates one object array for passing data 
                    object[] obj = new object[2];
                    obj[0] = buffer;
                    obj[1] = handler;

                    // Begins to asynchronously receive data 
                    handler.BeginReceive(
                        buffer,        // An array of type Byt for received data 
                        0,             // The zero-based position in the buffer  
                        chunk, // The number of bytes to receive 
                        SocketFlags.None,// Specifies send and receive behaviors 
                        new AsyncCallback(ReceiveCallback),//An AsyncCallback delegate 
                        obj            // Specifies infomation for receive operation 
                        );

                    // Begins an asynchronous operation to accept an attempt 
                    AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                    listener.BeginAccept(aCallback, listener);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }

            }
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                // Fetch a user-defined object that contains information 
                object[] obj = new object[2];
                obj = (object[])ar.AsyncState;

                // Received byte array 
                byte[] buffer = (byte[])obj[0];

                // A Socket to handle remote host communication. 
                handler = (Socket)obj[1];

                // Received message 
                string content = string.Empty;

                // The number of bytes received. 
                int bytesRead = handler.EndReceive(ar);

                if (bytesRead > 0)
                {
                    if (string.IsNullOrEmpty(dirPath) == true)
                    {
                        Send("<!Stop_Transfering!>");
                    }
                    else
                    {
                        content += Encoding.Unicode.GetString(buffer, 0, bytesRead);

                        if (content.IndexOf("<!Transfer_Started!>") > -1)
                        {
                        }

                        if (content.IndexOf("<!Transfer_Finished!>") > -1)
                        {
                            Finished();
                        }

                        if (string.IsNullOrEmpty(fileName) == false)
                        {
                            tempArr = new byte[] { buffer[0], buffer[1], buffer[2], buffer[3]};
                            toWrite = BitConverter.ToInt32(tempArr,0);

                            fileStream.Write(buffer, 4, toWrite);
                        }

                        if (content.IndexOf("<!File_Name!>") > -1)
                        {
                            fileName = content.Substring(2,content.IndexOf("<!File_Name!>")-2);
                            fileStream = new FileStream(dirPath + "\\" + fileName, FileMode.Create);
                        }


                        // Continues to asynchronously receive data
                        byte[] bufferNew = new byte[chunk];
                        obj[0] = bufferNew;
                        obj[1] = handler;
                        handler.BeginReceive(bufferNew, 0, chunk, SocketFlags.None, new AsyncCallback(ReceiveCallback), obj);

                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void Send(string message)
        {
            try
            {
                // byteData - data to send 
                byte[] byteData = new byte[chunk];

                byteData = Encoding.Unicode.GetBytes(message);


                // Sends data asynchronously to a connected Socket 
                handler.BeginSend(byteData, 0, chunk, 0,
                    new AsyncCallback(SendCallback), handler);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void SendCallback(IAsyncResult ar)
        {
            try
            {
                // A Socket which has sent the data to remote host 
                Socket handler = (Socket)ar.AsyncState;
                int bytesSend = handler.EndSend(ar);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void Finished()
        {
            fileName = string.Empty;
            fileStream.Dispose();
        }

        public void SelectDirectory()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    dirPath = dialog.SelectedPath;
                }
            }
        }


    }
}
