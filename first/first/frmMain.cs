using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using first.CommonClass;

namespace first
{
    public partial class frmMain : Form
    {
        frmLogin frmLogin;

        //public static cwitOLEDB ocsDB = new cwitOLEDB();
        //public static DataTable dtCode = new DataTable(); 

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        { 
            this.splitContainer1.SplitterDistance = 600;
            frmLogin = new frmLogin();
            frmLogin.Show();

        }
    }
}
