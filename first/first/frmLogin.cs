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
    public partial class frmLogin : Form
    {

        public static cwitOLEDB ocsDB = new cwitOLEDB();
        public static DataTable dtCode = new DataTable(); 

        private string userid;
        private string passWord;


        public frmLogin()
        {
            InitializeComponent();
        }

        private void DBConnect()
        {
            this.textBox3.Text = "DJH";
            this.textBox1.Text = "dreamer";
            this.textBox2.Text = "dsdvp";

            ocsDB.DBType = DATABASE_TYPE.ORACLE;
            ocsDB.OConnInfo.SechemaName = textBox3.Text;
            ocsDB.OConnInfo.UserID = textBox1.Text;
            ocsDB.OConnInfo.UserPassword = textBox2.Text;
            ocsDB.DBProvider = DATABASE_PROVIDER.ORACLE_PROVIDER;
        }

    }
}
