using System;
using System.Collections.Generic;
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
        public Server(Form1 form)
        {
            this.form = form;
        }

        Form1 form;
        SocketPermission permission;
        Socket socketListener;
        IPEndPoint ipEndPoint;
        Socket handler;
        Thread listen;
        const int chunk = 1400;
        bool serverStatus = false;

        long counter = 0;

        public void MainServer()
        {
            if (serverStatus == false)
            {
                StartServer();
            }
            else
            {
                StopServer();
            }
            serverStatus = serverStatus ^ true;
        }

        void Prepare()
        {
            listen = new Thread(Listen);
            listen.IsBackground = true;
        }

        void StartServer()
        {
            try
            {
                Prepare();

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

                // Associates a Socket with a local endpoint 
                socketListener.Bind(ipEndPoint);

                listen.Start();

                //Change status to Started
                form.textBox3.Enabled = false;
                form.button4.Enabled = true;
                form.button3.Text = "Stop";
                form.label5.Text = "Started";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void Listen()
        {
            try
            {
                // Places a Socket in a listening state and specifies the maximum 
                // Length of the pending connections queue 
                socketListener.Listen(10);

                // Begins an asynchronous operation to accept an attempt 
                AsyncCallback aCallback = new AsyncCallback(AcceptCallback);
                socketListener.BeginAccept(aCallback, socketListener);

                //Change status to Listening
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void AcceptCallback(IAsyncResult ar)
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
                    buffer.Length, // The number of bytes to receive 
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
                    counter++;

                    content += Encoding.Unicode.GetString(buffer, 0,
                        bytesRead);

                    // If message contains "<!Transfer_Finished!>", finish receiving
                    if (content.IndexOf("<!Transfer_Finished!>") > -1)
                    {
                        MessageBox.Show("Server: " + counter.ToString());
                    }
                    else
                    {
                        // Continues to asynchronously receive data
                        byte[] bufferNew = new byte[chunk];
                        obj[0] = bufferNew;
                        obj[1] = handler;
                        handler.BeginReceive(bufferNew, 0, buffer.Length,
                            SocketFlags.None,
                            new AsyncCallback(ReceiveCallback), obj);

                        //Do smth with bufferNew
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }


        private void StopServer()
        {
            try
            {
                if (socketListener.Connected)
                {
                    socketListener.Shutdown(SocketShutdown.Receive);
                    socketListener.Close();

                    socketListener.Dispose();
                }

                permission = null;
                socketListener = null;
                ipEndPoint = null;
                handler = null;
                listen = null;

                //Change status to Stopped
                form.textBox3.Enabled = true;
                form.button4.Enabled = false;
                form.button3.Text = "Start";
                form.label5.Text = "Not started";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
