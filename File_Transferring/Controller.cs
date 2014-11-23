using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Transferring
{
    class Controller
    {
        public Controller(Form1 form)
        {
            this.form = form;
        }

        Form1 form;
        Thread readFile;
        string fileName;
        const int chunk = 1400;
        long counter = 0;

        public void OpenFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fileName = dialog.FileName;
                    ResetThread();
                    readFile.Start();
                }
            }
        }

        void ResetThread()
        {
            readFile = new Thread(ReadAndProcessLargeFile);
        }


        void ReadAndProcessLargeFile()
        {
            FileStream fileStram = new FileStream(fileName, FileMode.Open, FileAccess.Read);
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
            MessageBox.Show(counter.ToString());
        }

        void ProcessChunk(byte[] buffer, int bytesRead)
        {
            // Do the processing here
            counter++;
        }

    }
}
