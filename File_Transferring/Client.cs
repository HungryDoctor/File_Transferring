﻿using System;
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
        bool clientStatus = false;

        long counter = 0;

        public void MainClient()
        {
            if (clientStatus == false)
            {
                clientStatus = true;
                Connect();
            }
            else
            {
                clientStatus = false;
                Disconnect();
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
                    ResetThread();
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
            byte[] started = Encoding.Unicode.GetBytes("<!Transfer_Started!>");
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

            byte[] finished = Encoding.Unicode.GetBytes("<!Transfer_Finished!>");
            ProcessChunk(finished, 0);

            MessageBox.Show("Clent: " + counter.ToString());
        }

        void ProcessChunk(byte[] buffer, int bytesRead)
        {
            try
            {
                senderSocket.Send(buffer);
                counter++;
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


                form.textBox1.Enabled = false;
                form.textBox2.Enabled = false;
                form.button1.Enabled = true;
                form.button2.Text = "Disconnect";
                form.label2.Text = "Connected";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());

                form.textBox1.Enabled = true;
                form.textBox2.Enabled = true;
                form.button1.Enabled = false;
                form.button2.Text = "Connect";
                form.label2.Text = "Not connected";
            }
        }


        void Disconnect()
        {
            try
            {
                senderSocket.Shutdown(SocketShutdown.Both);
                senderSocket.Close();
                senderSocket.Dispose();

                senderSocket = null;

                form.textBox1.Enabled = true;
                form.textBox2.Enabled = true;
                form.button1.Enabled = false;
                form.button2.Text = "Connect";
                form.label2.Text = "Not connected";
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }
    }
}
