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
        public Client(Form1 form)
        {
            this.form = form;
        }

        Form1 form;
        Thread readFile;
        Socket senderSocket;
        const int chunk = 1400;
        string filePath;
        string fileName;


        public void OpenFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = dialog.FileName;
                    fileName = dialog.SafeFileName;
                    ResetThread();
                    form.panel2.Enabled = false;
                    readFile.Start();
                }
            }
        }

        void ResetThread()
        {
            readFile = new Thread(ReadAndProcessLargeFile);
            readFile.IsBackground = true;
        }


        void ReadAndProcessLargeFile()
        {
            byte[] started = Encoding.Unicode.GetBytes("<!Transfer_started!>");
            ProcessChunk(started, 0);

            FileStream fileStram = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using (fileStram)
            {
                byte[] buffer = new byte[chunk];
                fileStram.Seek(0, SeekOrigin.Begin);
                int bytesRead = fileStram.Read(buffer, 0, chunk);
                while (bytesRead > 0)
                {
                    ProcessChunk(buffer, bytesRead);
                    bytesRead = fileStram.Read(buffer, 0, chunk);
                }
            }

            byte[] finished = Encoding.Unicode.GetBytes("<!Transfer_finished!>");
            ProcessChunk(finished, 0);

            Action enable = () => { form.panel2.Enabled = true; };
            form.panel2.Invoke(enable, null);
        }

        void ProcessChunk(byte[] buffer, int bytesRead)
        {
            try
            {
                senderSocket.Send(buffer);
                Receive();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }



        void Connect()
        {
            try
            {
                // Create one SocketPermission for socket access restrictions 
                SocketPermission permission = new SocketPermission(
                    NetworkAccess.Connect,    // Connection permission 
                    TransportType.Tcp,        // Defines transport types 
                    "",                       // Gets the IP addresses 
                    SocketPermission.AllPorts // All ports 
                    );

                // Ensures the code to have permission to access a Socket 
                permission.Demand();

                // Resolves a host name to an IPHostEntry instance            
                IPHostEntry ipHost = Dns.GetHostEntry("");

                // Gets first IP address associated with a localhost 
                IPAddress ipAddr = ipHost.AddressList[0];

                // Creates a network endpoint 
                IPEndPoint ipEndPoint = new IPEndPoint(ipAddr, 4510);

                // Create one Socket object to setup Tcp connection 
                senderSocket = new Socket(
                    ipAddr.AddressFamily,// Specifies the addressing scheme 
                    SocketType.Stream,   // The type of socket  
                    ProtocolType.Tcp     // Specifies the protocols  
                    );

                senderSocket.NoDelay = false;   // Using the Nagle algorithm 

                // Establishes a connection to a remote host 
                senderSocket.Connect(ipEndPoint);


                //Change status to connected
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void Receive()
        {
            try
            {
                byte[] buffer = new byte[chunk];

                int bytesRec = senderSocket.Receive(buffer);

                while (senderSocket.Available > 0)
                {
                    bytesRec = senderSocket.Receive(buffer);
                    //Process here
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        void Disconnect()
        {
            try
            {
                senderSocket.Shutdown(SocketShutdown.Both);

                senderSocket.Close();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
