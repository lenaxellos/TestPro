using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Text;

namespace first.CommonClass
{
    public enum DATABASE_TYPE
    {
        ORACLE = 0,
        MSSQL2K = 1,
        MSSQL2005OVER = 2,
        MSACCESS = 3,
    }

    public struct MSACCESS_CONNECTION_INFORMATION
    {
        public string MDBPath;
        public string UserID;
        public string UserPassword;
    }

    public struct ORACLE_CONNECTION_INFORMATION
    {
        public string SechemaName;
        public string UserID;
        public string UserPassword;
    }

    public enum DATABASE_PROVIDER
    {
        ORACLE_PROVIDER = 0,
        ORACLE_MS_PROVIDER = 1,
        MSSQL_2K_PROVIDER = 2,
        MSSQL_2K5_PROVIDER = 3,
    }

    public struct MSSQL_CONNECTION_INFORMATION
    {
        public string HostIP;
        public string HostPort;
        public string DatabaseName;
        public string UserID;
        public string UserPassword;
    }



    class cwitOLEDB
    {
        #region Event
        //public delegate void AddLogHandler(string CommonMsg, string ErrMsg = "", string Pos = "");
        //private AddLogHandler AddLog = null;
        #endregion


        //public override void DisPose()
        //{
        //    conn.Dispose();
        //}

        public DATABASE_TYPE DBType { get; set; }
        public DATABASE_PROVIDER DBProvider { get; set; }
        public string ConnStr
        {
            get
            {
                return MakeConnectString();
            }
        }
        public ORACLE_CONNECTION_INFORMATION OConnInfo;
        public MSSQL_CONNECTION_INFORMATION MConnInfo;
        public MSACCESS_CONNECTION_INFORMATION MAConnInfo;

        public OleDbConnection conn = new OleDbConnection();
        private OleDbTransaction trans;

        public void initClass()
        {
            MAConnInfo.MDBPath = "";
            MAConnInfo.UserID = "";
            MAConnInfo.UserPassword = "";

            OConnInfo.SechemaName = "";
            OConnInfo.UserID = "";
            OConnInfo.UserPassword = "";

            MConnInfo.DatabaseName = "";
            MConnInfo.HostIP = "";
            MConnInfo.HostPort = "";
            MConnInfo.UserID = "";
            MConnInfo.UserPassword = "";
        }

        private string ProcGetStringProvider(DATABASE_PROVIDER dbProv)
        {
            string retVal = "";

            switch (dbProv)
            {
                case DATABASE_PROVIDER.ORACLE_PROVIDER:
                    retVal = "OraOLEDB.Oracle.1";
                    break;
                case DATABASE_PROVIDER.ORACLE_MS_PROVIDER:
                    retVal = "MSDAORA.1";
                    break;
                case DATABASE_PROVIDER.MSSQL_2K_PROVIDER:
                    retVal = "SQLOLEDB.1";
                    break;
                case DATABASE_PROVIDER.MSSQL_2K5_PROVIDER:
                    retVal = "MSDASQL.1";
                    break;
            }

            return retVal;
        }

        private string MakeConnectString()
        {
            if (DBType == DATABASE_TYPE.ORACLE)     // OraOLEDB.Oracle.1     MSDAORA.1
                return "Provider=" + ProcGetStringProvider(DBProvider) + ";Persist Security Info=False;User ID=" + OConnInfo.UserID +
                                                                    ";Password=" + OConnInfo.UserPassword +
                                                                    ";Data Source=" + OConnInfo.SechemaName;
            else if (DBType == DATABASE_TYPE.MSSQL2K)
                return "Provider=" + ProcGetStringProvider(DBProvider) + ";Persist Security Info=False;User ID=" + MConnInfo.UserID +
                                                                     ";Password=" + MConnInfo.UserPassword +
                                                                     ";Data Source=" + MConnInfo.HostIP + "," + MConnInfo.HostPort +
                                                                     ";Initial Catalog=" + MConnInfo.DatabaseName;
            else if (DBType == DATABASE_TYPE.MSSQL2005OVER)
                return "Provider=" + ProcGetStringProvider(DBProvider) + ";Persist Security Info=False;User ID=" + MConnInfo.UserID +
                                                     ";Password=" + MConnInfo.UserPassword +
                                                     ";Data Source=" + MConnInfo.DatabaseName;
            else if (DBType == DATABASE_TYPE.MSACCESS)
                return "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + MAConnInfo.MDBPath + ";Persist Security Info=False";

            else
                return "";
        }

        public void BeginTrans()
        {
            trans = conn.BeginTransaction();
        }

        public void CommitTrans()
        {
            trans.Commit();
        }

        public void RollbackTrans()
        {
            trans.Rollback();
        }

        public bool DBConnect()
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    conn.ConnectionString = MakeConnectString();
                    conn.Open();

                    //cwitLog.AddLog("DB Connect Successed", LOG_LEVEL.CriticalError, "DBConnect()");
                }

                return true;
            }
            catch (Exception ex)
            {
                //cwitLog.AddLog(ex.Message, LOG_LEVEL.CriticalError, "DBConnect()");
                //cwitLog.AddLog("DB Connect Error", LOG_LEVEL.CriticalError, "DBConnect()");
                //cwitLog.AddLog("Connection String = " + conn.ConnectionString, LOG_LEVEL.CommonMessage);
                return false;
            }
        }

        public bool DBClose()
        {
            try
            {
                if (conn.State != System.Data.ConnectionState.Closed)
                {
                    conn.Close();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //public bool ProcMDBOptimize(string sPath = "")
        //{
        //    // //[COM]Microsoft OLE DB Provider for Jet and Replication Objects
        //    if (sPath == "")
        //    {
        //        sPath = MAConnInfo.MDBPath;
        //        if (conn.State == System.Data.ConnectionState.Open)
        //            conn.Close();
        //    }

        //    if (MAConnInfo.MDBPath == "")
        //        return false;


        //    JRO.JetEngine Jro = new JRO.JetEngine();
        //    string   CnString  = "Provider=Microsoft.Jet.OLEDB.4.0;"; 
        //    string   TmpDbPath  = System.IO.Path.GetTempFileName();
        //    System.IO.File.Delete(TmpDbPath);    

        //    try
        //    {
        //        Jro.CompactDatabase(CnString + "Data Source=" + sPath, CnString + "Data Source=" + TmpDbPath);
        //        System.IO.File.Copy(TmpDbPath, sPath, true);
        //        DBConnect();
        //        return true;
        //    }
        //    catch(Exception ex)
        //    {
        //        if(System.IO.File.Exists(TmpDbPath))
        //        {
        //            System.IO.File.Delete(TmpDbPath);
        //        }

        //        Console.Write(ex.Message);

        //        return false;
        //    }


        //}

        public bool ExecuteQry(string strQry)
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    DBConnect();
                }

                OleDbCommand Comm = new OleDbCommand(strQry, conn);

                Comm.ExecuteNonQuery();
                Comm.Transaction = trans;
                Comm = null;

                //DBClose();
                return true;
            }
            catch (Exception ex)
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    DBConnect();
                }

                //cwitLog.AddLog(ex.Message, LOG_LEVEL.CriticalError, "ExecuteQry()");
                //cwitLog.AddLog("ExecuteQry(" + strQry + ")", LOG_LEVEL.CriticalError, "ExecuteQry()");
                return false;
            }
        }

        public OleDbDataReader ExecuteOpenQry(string strQry)
        {
            try
            {
                if (conn.State == System.Data.ConnectionState.Closed)
                {
                    DBConnect();
                }

                OleDbCommand Cmd = new OleDbCommand();
                Cmd.Connection = conn;
                Cmd.CommandText = strQry.Trim();
                Cmd.CommandType = System.Data.CommandType.Text;

                OleDbDataReader Read = Cmd.ExecuteReader();

                return Read;
            }
            catch (Exception ex)
            {
                //cwitLog.AddLog(ex.Message, LOG_LEVEL.CriticalError, "ExecuteOpenQry()");
                //cwitLog.AddLog("ExecuteOpenQry(" + strQry + ")", LOG_LEVEL.CriticalError, "ExecuteOpenQry()");
                return null;
            }
        }
    }
}
