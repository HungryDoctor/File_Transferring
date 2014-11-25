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
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            client = new Client(this);
            server = new Server(this);
        }

        Client client;
        Server server;

        private void button1_Click(object sender, EventArgs e)
        {
            client.OpenFile();
        }

        private void button2_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {

        }
    }
}
