using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace File_Transferring
{
    public partial class MainWindow : Form
    {
        public MainWindow()
        {
            InitializeComponent();

            client = new Client(this);
            server = new Server(this);
        }

        Client client;
        Server server;

        private void button1_Click(object sender, EventArgs e)
        {
            client.MainClient();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            client.OpenFile();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            server.MainServer();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            server.SelectDirectory();
        }
    }
}
