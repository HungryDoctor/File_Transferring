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
            controller = new Controller(this);
        }

        Controller controller;

        private void button1_Click(object sender, EventArgs e)
        {
            controller.OpenFile();
        }
    }
}
