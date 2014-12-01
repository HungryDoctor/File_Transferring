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
    class Client
    {

        public Client(MainWindow window)
        {
            this.window = window;
        }

        MainWindow window;
        Thread readFile;
        Thread listen;
        Socket senderSocket;

        const int chunk = 1396;
        byte[] tempArr;
        string filePath;
        string fileName;
        bool clientStatus = false;
        bool listening = false;
        volatile bool stopped = false;

        public void MainClient()
        {
            if (clientStatus == false)
            {
                IPAddress ipAddr;
                int port = -1;
                bool goodIp = IPAddress.TryParse(window.textBox1.Text, out ipAddr);
                bool goodPort = int.TryParse(window.textBox2.Text, out port);
                if (port < 1 || port > 65535)
                {
                    goodPort = false;
                }

                if (goodIp == true && goodPort == true)
                {
                    Connect();
                }
                else
                {
                    MessageBox.Show("Entered strings aren't IP or Port");
                }
            }
            else
            {
                Disconnect();
            }
        }

        void Connect()
        {
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(NetworkAccess.Connect, TransportType.Tcp, "", SocketPermission.AllPorts);

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr;
                IPAddress.TryParse(window.textBox1.Text, out ipAddr);

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, Convert.ToInt32(window.textBox2.Text));

                senderSocket = null;

                // Create one Socket object to setup Tcp connection 
                senderSocket = new Socket(ipAddr.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                senderSocket.NoDelay = false;   // Using the Nagle algorithm 

                // Establishes a connection to a remote host 
                senderSocket.Connect(ipEndPoint);


                window.textBox1.Enabled = false;
                window.textBox2.Enabled = false;
                window.button2.Enabled = true;
                window.button1.Text = "Disconnect";
                window.label2.Text = "Connected";

                clientStatus = true;
                listening = true;
                listen = new Thread(Listen);
                listen.IsBackground = true;
                listen.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                Disconnect();
            }
        }

        void Disconnect()
        {
            try
            {
                listening = false;
                clientStatus = false;

                if (readFile != null && readFile.IsAlive == true)
                {
                    readFile.Abort();
                }

                if (listen != null && listen.IsAlive == true)
                {
                    listen.Abort();
                }

                if (senderSocket.Connected == true)
                {
                    senderSocket.Shutdown(SocketShutdown.Both);
                }
                senderSocket.Close();
                senderSocket.Dispose();

                window.textBox1.Enabled = true;
                window.textBox2.Enabled = true;
                window.button2.Enabled = false;
                window.button1.Text = "Connect";
                window.label2.Text = "Not connected";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void Listen()
        {
            while (listening == true)
            {
                ReceiveData();
            }
        }

        void ReceiveData()
        {
            try
            {
                byte[] byteData = new byte[chunk];
                // Receives data from a bound Socket. 
                int bytesRec = senderSocket.Receive(byteData);

                // Converts byte array to string 
                String theMessageToReceive = Encoding.Unicode.GetString(byteData, 0, bytesRec);

                stopped = true;
                //// Continues to read the data till data isn't available 
                //while (senderSocket.Available > 0)
                //{
                //    bytesRec = senderSocket.Receive(byteData);
                //    theMessageToReceive += Encoding.Unicode.GetString(byteData, 0, bytesRec);
                //}
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void ReadAndProcessLargeFile()
        {
            try
            {
                stopped = false;

                byte[] started = Encoding.Unicode.GetBytes("<!Transfer_Started!>");
                ProcessChunk(started, started.Length);

                byte[] fileNameByte = Encoding.Unicode.GetBytes(this.fileName + "<!File_Name!>");
                ProcessChunk(fileNameByte, fileNameByte.Length);

                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[chunk];
                    fileStream.Seek(0, SeekOrigin.Begin);
                    int bytesRead = fileStream.Read(buffer, 0, chunk);

                    while (bytesRead > 0 && stopped == false)
                    {
                        ProcessChunk(buffer, bytesRead);
                        bytesRead = fileStream.Read(buffer, 0, chunk);
                    }
                }

                byte[] finished = Encoding.Unicode.GetBytes("<!Transfer_Finished!>");
                ProcessChunk(finished, finished.Length);

                window.button2.Invoke(new Action(() => window.button2.Enabled = true), null);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void ProcessChunk(byte[] buffer, int bytesRead)
        {
            try
            {
                tempArr = new byte[chunk + 4];
                BitConverter.GetBytes(bytesRead).CopyTo(tempArr, 0);
                buffer.CopyTo(tempArr, 4);

                senderSocket.Send(tempArr);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public void OpenFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = dialog.FileName;
                    fileName = dialog.SafeFileName;
                    window.button2.Enabled = false;
                    readFile = new Thread(ReadAndProcessLargeFile);
                    readFile.IsBackground = true;
                    readFile.Start();
                }
            }
        }



    }
}
