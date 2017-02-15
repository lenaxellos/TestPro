//****************************************************************************
//
// Author      : Jong Heun, Shin
// Create Info : 2014.08.26
// Modify Info.: 
// Modify Rev. : 1
// Class Name  : cwitGeneral (Cwit Public Function)
//
//****************************************************************************

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Net;
using System.Net.Sockets;

namespace first.CommonClass
{
    class cwitGeneral
    {
        #region Event
        public delegate void AddLogHandler(string CommonMsg, string ErrMsg = "", string Pos = "");
        private static AddLogHandler AddLog = null;
        #endregion

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  API 선언 
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        // INI 파일 사용을 위한 DLL 함수 선언
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileInt(string section, string key, int def, string filePath);
        [DllImport("user32")]
        public static extern int SetWindowPos(int hwnd, int hWndInsertAfter, int x, int y, int cx, int cy, int wFlags);

        [System.Runtime.InteropServices.DllImport("shell32.dll")]
        public static extern int ShellExecute(int hwnd, string lpOperation, string lpFile, string lpParameters,
                                                                string lpDirectory, int nShowcmd);


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  암호화 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        /// Summery
        /// MD5로 암호화하여 반환
        /// Summery
        public static string MD5HashFunc(string str)
        {
            StringBuilder MD5Str = new StringBuilder();
            byte[] byteArr = Encoding.ASCII.GetBytes(str);
            byte[] resultArr = (new MD5CryptoServiceProvider()).ComputeHash(byteArr);

            //for (int cnti = 1; cnti < resultArr.Length; cnti++)
            for (int cnti = 0; cnti < resultArr.Length; cnti++)
            {
                MD5Str.Append(resultArr[cnti].ToString("X2"));
            }

            return MD5Str.ToString();
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  프로세스 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        /// <summary>
        /// 프로그램 실행해주는 함수 
        /// </summary>
        /// <param name="sPath">프로그램 경로</param>
        /// <param name="sArg">프로그램 실행시 파라미터</param>
        /// <param name="bHidden">프로그램 보일 것인지 말것인지 여부 (false=보이기 / true=안보이기)</param>
        /// <returns></returns>
        public static bool ProcRun(string sPath, string sArg, bool bHidden)                        // 프로그램 실행
        {
            // using System.Collections; 변경
            try
            {
                ProcessStartInfo proc = new ProcessStartInfo(sPath, sArg);

                proc.UseShellExecute = true;
                proc.Verb = "open";

                if (bHidden)
                    proc.WindowStyle = ProcessWindowStyle.Hidden;
                else
                    proc.WindowStyle = ProcessWindowStyle.Normal;

                Process.Start(proc);
                return true;
            }
            catch (Exception ex)
            {
                if (AddLog != null)
                    AddLog("ProcRun(" + sPath + " " + sArg + ")", ex.Message, ex.StackTrace);
                return false;
            }

        }

        /// <summary>
        /// URL으로부터 파일을 다운로드 받는 함수
        /// </summary>
        /// <param name="FileNameURL">파일이름이 포함된 전체 URL</param>
        /// <param name="LocalFilePath">다운로드 받을 로컬 경로</param>
        public static bool ProcDownload(string FileNameURL, string LocalFilePath)
        {
            using (WebClient myWebClient = new WebClient())
            {
                try
                {
                    // Download the Web resource and save it into the current filesystem folder.


                    FileNameURL = FileNameURL.Replace(@"\", "/");
                    // 
                    //Uri serverURL = new Uri("http://img.naver.net/static/www/u/2013/0731/nmms_224940510.gif");
                    Uri serverURL = new Uri(FileNameURL);

                    //myWebClient.Encoding
                    myWebClient.DownloadFile(serverURL, LocalFilePath);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.Write(ex.Message);
                    return false;
                }
                
            }
        }

        public static string ProcGetURLFilePath(string URL)
        {
            try
            {
                string tempURL = URL.Substring(7);


                if (tempURL.IndexOf("/") <= 0)
                    return "";

                string[] aRet = tempURL.Split('/');
                string retVal = "";

                for (int i = 0; i < aRet.Count() - 1; i++)
                {
                    retVal += aRet[i] + "/";
                }

                retVal = "http://" + retVal.Substring(0, retVal.Length - 1);

                return retVal;
            }
            catch
            {
                return "";
            }
        }

        public static string ProcGetURLFileName(string URL)
        {
            try
            {
                if (URL.IndexOf("/") <= 0)
                    return "";

                string[] aRet = URL.Split('/');
                return aRet[aRet.Count() - 1];
            }
            catch
            {
                return "";
            }
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  Network 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public static string ProcGetMyIP()
        {
            IPHostEntry host;
            string localIP = "";

            host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    localIP = ip.ToString();
                }
            }

            return localIP;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  System 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        public static string ProcGetMyComputerName()
        {
            return SystemInformation.ComputerName;
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  File 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        /// <summary>
        /// 현재경로 가져오기
        /// </summary>
        /// <returns></returns>
        public static string GetCurPath()
        {
            return System.IO.Directory.GetCurrentDirectory();
        }

        /// <summary>
        /// 파일의 확장자를 가져오는 함수 (LCase)
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string ProcGetFileExt(string strPath)
        {
            string strRet = "";

            if (strPath == "")
                return strRet;

            strRet = Path.GetExtension(strPath).ToLower();
            return strRet;
        }

        /// <summary>
        /// 경로나 파일에서 확장자를 제외한 파일명을 가져오는 함수
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string ProcGetFileName(string strPath)
        {
            string strRet = "";


            if (strPath == "")
                return strRet;

            strRet = Path.GetFileName(strPath);
            string[] aryStr = strRet.Split('.');
            strRet = aryStr[0];
            return strRet;
        }

        /// <summary>
        /// 경로나 파일에서 확장자를 포함한 파일명을 가져오는 함수
        /// </summary>
        /// <param name="strPath"></param>
        /// <returns></returns>
        public static string ProcGetFileName2(string strPath)
        {
            string strRet = "";


            if (strPath == "")
                return strRet;

            strRet = Path.GetFileName(strPath);
            return strRet;
        }

        /// <summary>
        /// 경로를 포함한 파일에서 경로만 가져오는 함수
        /// </summary>
        /// <param name="strFile"></param>
        /// <returns></returns>
        public static string ProcGetFilePath(string strFile)
        {
            string strRet = "";

            if (strFile == "")
                return strRet;

            strRet = Path.GetDirectoryName(strFile);

            return strRet;
        }

        /// <summary>
        /// 경로에서 제일 하위경로를 반환하는 함수
        /// </summary>
        /// <param name="OnlyPath"></param>
        /// <returns></returns>
        public static string ProcGetFilePath2(string OnlyPath)
        {
            string strRet = string.Empty;

            if (OnlyPath == "" || OnlyPath == null)
                return "";

            string sTmp = OnlyPath.Replace(@"\", "|");
            String[] aTmp = sTmp.Split('|');
            strRet = aTmp[aTmp.Count() - 1];

            return strRet;
        }

        public static bool IsFileExist(string strFilePath)
        {
            FileInfo File = new FileInfo(strFilePath);
            return File.Exists;
        }

        // 이거 왜 만들었지?
        /// <summary>
        /// 해당 경로에서 파일이름을 가져온다. NULL일 경우 파일이 없는 거임.
        /// </summary>
        /// <param name="strPath"></param>
        /// <param name="strFilter"></param>
        /// <returns></returns>
        public static string ProcGetFile(string strPath, string strFilter = "")
        {
            try
            {
                string[] flArray = Directory.GetFiles(strPath);

                foreach (string name in flArray)
                {
                    if (name == strFilter)
                        return name;
                }

                string strRet = "";
                return strRet;
            }
            catch (Exception e)
            {
                string strErr = e.Message;
                return null;
            }

        }

        public static bool FileSave(string strPath, string strFileName, string strText, System.Text.Encoding Enc = null)
        {
            try
            {
                ProcMakeDir(strPath);

                if (Enc == null)
                    Enc = Encoding.UTF8;

                StreamWriter sWriter = new StreamWriter(Path.Combine(strPath, strFileName));
                sWriter.Write(strText);
                sWriter.Close();

                return true;
            }
            catch (System.Exception ex)
            {
                string strErr = ex.Message;
                return false;
            }
        }

        public static string FileLoad(string strFile, System.Text.Encoding Enc = null)
        {
            if (!IsFileExist(strFile))
                return "";

            if (Enc == null)
                Enc = Encoding.UTF8;

            try
            {
                StreamReader sReader = new StreamReader(strFile, Enc);
                string strRet = sReader.ReadToEnd();
                sReader.Close();
                return strRet;
            }
            catch (System.Exception ex)
            {
                return "File Load Error - " + ex.Message;
            }

        }

        /// <summary>
        /// 경로의 드라이브가 존재하는지 체크해주는 함수
        /// </summary>
        /// <param name="sPath"></param>
        /// <returns></returns>
        public static bool ProcChkDrive(string sPath)
        {
            if (sPath == "")
                return false;

            string sDrv = sPath.Substring(0, 1);

            if (sDrv == "")
                return false;

            DriveInfo drv = new DriveInfo(sDrv);
            return drv.IsReady;
        }

        public static void ProcGetDriveSize(string DriveStr, out string TotalSize, out string FreeSize)
        {
            long nTotSize = 0;
            long nFreeSize = 0;

            DriveInfo drv = new DriveInfo(DriveStr);
            nTotSize = drv.TotalSize / 1024 / 1024 / 1024;
            nFreeSize = drv.TotalFreeSpace / 1024 / 1024 / 1024;

            TotalSize = nTotSize.ToString() + "Gb";
            FreeSize = nFreeSize.ToString() + "Gb";
        }

        /// <summary>
        /// 전체경로의 하위폴더까지 생성해주는 함수
        /// </summary>
        /// <param name="strPath"></param>
        public static void ProcMakeDir(string strPath)
        {
            if (strPath == "")
                return;

            //string tmpDirPath = Path.GetDirectoryName(strPath);
            DirectoryInfo diDir = new DirectoryInfo(strPath);
            if (!diDir.Exists)
            {
                diDir.Create();
                diDir = new DirectoryInfo(strPath);
            }
        }

        public static void ProcDeleteDir(string strPath)    // 폴더안에 파일이 있어도 삭제하는 함수
        {
            try
            {
                DirectoryInfo TempDirInfo = new DirectoryInfo(strPath);
                if (!TempDirInfo.Exists)
                    return;

                FileInfo[] files = TempDirInfo.GetFiles();

                foreach (FileInfo Fi in files)
                {
                    Fi.Delete();
                }

                //하위 폴더가 있는지 체크 
                string[] folders = Directory.GetDirectories(strPath);
                foreach (string folder in folders)
                {
                    string namename = Path.GetFileName(folder);
                    ProcDeleteDir(folder);
                }

                Directory.Delete(strPath);
            }
            catch (Exception e)
            {
                string strerr = e.Message;
            }
        }

        /// <summary>
        /// 폴더안에 있는 모든 폴더와 파일을 DestFolder로 복사하는 함수
        ///  (2013-08-07 JHShin)
        /// </summary>
        /// <param name="sourceFolder">원본폴더</param>
        /// <param name="destFolder">타겟폴더</param>
        /// <returns>nothing</returns>
        /// <value>이건뭐냐</value>
        public static void ProcCopyDir(string sourceFolder, string destFolder, bool deleteSource = false)
        {
            if (!Directory.Exists(destFolder))
                Directory.CreateDirectory(destFolder);

            string[] files = Directory.GetFiles(sourceFolder);
            string[] folders = Directory.GetDirectories(sourceFolder);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(destFolder, name);
                File.Copy(file, dest);

                if (deleteSource)
                {
                    File.Delete(file);
                }
            }

            // 재귀함수를 통해 하위폴더의 파일들도 복사한다. 그렇기 때문에 폴더를 삭제할 수 없다. 어캐하지?
            foreach (string folder in folders)
            {
                string namename = Path.GetFileName(folder);
                string dest = Path.Combine(destFolder, namename);
                ProcCopyDir(folder, dest);
            }

            if (deleteSource)
            {
                if (folders.Count() == 0)
                {
                    ProcDeleteDir(sourceFolder);
                }
            }
        }

        /// <summary>
        /// 소스폴더의 파일만 타겟폴더에 복사한다. 
        /// </summary>
        /// <param name="sourceFolder">소스폴더</param>
        /// <param name="DestFolder">타겟폴더</param>
        /// <param name="deleteSource">소스폴더의 파일삭제여부</param>
        public static void ProcCopyDir2(string sourceFolder, string DestFolder, bool deleteSource = false)
        {
            if (!Directory.Exists(DestFolder))
                Directory.CreateDirectory(DestFolder);

            string[] files = Directory.GetFiles(sourceFolder);

            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                string dest = Path.Combine(DestFolder, name);
                File.Copy(file, dest);

                if (deleteSource)
                {
                    File.Delete(file);
                }
            }
        }

        /// <summary>
        /// 단일파일을 복사하는 함수. File.Copy는 해당경로에 같은 파일이 있으면 에러남.
        ///  (2013-08-14 JHShin)
        /// </summary>
        public static bool ProcCopy(string SourceFile, string TargetFile)
        {
            bool bRet = false;

            try
            {
                string sTargetDir = ProcGetFilePath(TargetFile);
                ProcMakeDir(sTargetDir);

                if (!File.Exists(TargetFile))
                {
                    if (File.Exists(SourceFile))
                    {
                        File.Copy(SourceFile, TargetFile);
                        bRet = true;
                    }
                }

                return bRet;
            }
            catch (Exception ex)
            {
                Console.Write(ex.Message);
                return bRet;
            }
        }



        /// <summary>
        /// FolderBrowseDialog를 이용하여 폴더를 선택받아 반환하는 함수
        ///  (2013-08-14 JHShin)
        /// </summary>
        public static string ProcGetDirPath()
        {
            FolderBrowserDialog fbDlg = new FolderBrowserDialog();

            fbDlg.ShowDialog();

            if (fbDlg.SelectedPath == "")
                return "";
            else
                return fbDlg.SelectedPath;
        }




        public static string ProcGetFileVer(String strPath)     // 지정된 파일의 버전을 가져오는 함수
        {
            string strRet = "";
            if (strPath == "")
                return strRet;

            Assembly asm = Assembly.LoadFrom(strPath);
            AssemblyName name = asm.GetName();
            return name.Version.ToString();
        }

        public static string ProcGetAppVer()        // 현재 실행 중인 프로그램의 버전을 가져오는 함수
        {
            Assembly assemObj = Assembly.GetExecutingAssembly();
            Version v = assemObj.GetName().Version;

            string strRet = String.Format("{0}.{1}.{2}.{3}", v.Major.ToString(), v.Minor.ToString(), v.Build.ToString(), v.Revision.ToString());
            return strRet;
        }

        public static string ProcGetAppName()        // 현재 실행 중인 프로그램의 이름을 가져오는 함수 여기서 쓰믄 EMClass이름을 가져감.
        {
            Assembly assemObj = Assembly.GetExecutingAssembly();
            string ProgName = assemObj.GetName().Name.ToString();

            return ProgName;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  이미지 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static bool ProcGetClipboardImg(string TargetFileName)
        {
            try
            {
                IDataObject ob = null;
                PictureBox pic = new PictureBox();


                ob = Clipboard.GetDataObject();
                pic.Image = (Image)ob.GetData(DataFormats.Bitmap);
                Image Im = pic.Image;

                if (Im == null)
                    return false;

                FileStream fs = new FileStream(TargetFileName, FileMode.Create, FileAccess.ReadWrite);
                Im.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                fs.Dispose();
                return true;
            }
            catch (Exception e)
            {
                string strerr = e.Message;
                return false;
            }
        }



        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  INI 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static void INI_Write(string Section, string Key, string Val, string INIFile)
        {
            WritePrivateProfileString(Section, Key, Val, INIFile);
        }

        public static string INI_Read(string Section, string Key, string def, string INIFile)
        {
            StringBuilder strTemp = new StringBuilder(4000);
            string strRet;
            GetPrivateProfileString(Section, Key, def, strTemp, 4000, INIFile);
            strRet = strTemp.ToString();
            return strRet;
        }

        public static int INI_Read_Int(String Section, string Key, int def, string INIFile)
        {
            int nRet = GetPrivateProfileInt(Section, Key, Convert.ToInt32(def), INIFile);
            return nRet;
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  문자열 관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        //public static int ProcStructSize(struct struc)
        //{
        //    return Marshal.SizeOf(struc);
        //}


        public static string ProcParserString(string strTot, string strFirst, string strLast)
        {
            string strTemp = "";

            int nPosFirst = strTot.IndexOf(strFirst);
            int nPosLast = strTot.IndexOf(strLast);

            strTemp = strTot.Substring(nPosFirst + 1, nPosLast - (nPosFirst + 1));

            return strTemp;
        }

        public static int ProcGetAge(string BirthDay, bool KoreanAgeType = true) // BirthDay Format - YYYY-MM-DD
        {
            int Age = 0;

            int BirthYear = Convert.ToInt32(BirthDay.Substring(2, 2));

            int NowYear = DateTime.Now.Year;

            if (BirthDay.Substring(0, 2) == "19")
            {
                Age = (NowYear - (1900 + BirthYear));
            }
            else
            {
                Age = (NowYear - (2000 + BirthYear));
            }

            if (KoreanAgeType)
            {
                int BirthMonth = Convert.ToInt32(BirthDay.Substring(5, 2));
                int nowMonth = DateTime.Now.Month;

                if (BirthMonth == nowMonth)
                {
                    int BirthDays = Convert.ToInt32(BirthDay.Substring(8, 2));
                    int nowDay = DateTime.Now.Day;

                    if (BirthDays <= nowDay)
                        Age = Age + 1;
                }
                else if (BirthMonth < nowMonth)
                    Age = Age + 1;
            }
            return Age;
        }


        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  시간관련 함수
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static void Delay(float fSecond)
        {
            // using System.Threading; //추가
            int nSecond = (int)fSecond;

            //Console.Write(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            for (int i = 1; i < nSecond * 100; i++)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }
        }

        public static string GetNow(string sFormat)     // sFormat = yyyy-mm-dd hh:MM:ss 이런형태로 사용
        {
            DateTime dtNow = DateTime.Now;
            return dtNow.ToString(sFormat);
        }

        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  트리뷰 관련
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        public static TreeNode TreeSearchNode(TreeNodeCollection objNodes, string strKey)
        {
            foreach (TreeNode node in objNodes)
            {
                if (node.Name == strKey) return node;

                TreeNode findNode = TreeSearchNode(node.Nodes, strKey);
                return findNode;
            }

            return null;
        }

        public static string TreeGetFullPath(TreeNodeCollection objNodes, string FullPath)
        {
            string[] aryStr = FullPath.Split('.');
            int nLastNode = aryStr.Count() - 1;
            string strLastNode = aryStr[nLastNode];



            foreach (TreeNode node in objNodes)
            {
                if (node.Text == strLastNode)
                    return node.Name;

                if (node.Nodes.Count > 0)
                {
                    string ChildName = TreeGetFullPath(node.Nodes, FullPath);

                    if (ChildName != "")
                    {
                        string retStr = string.Format("{0}.{1}", node.Name, ChildName);
                        return retStr;
                    }
                }
            }

            return "";
        }

        public static void TreeAddNode(TreeNodeCollection objNode, string sCode, string sName, string sRootCode, string sImageKey)
        {
            string[] aryStr;
            string sCngRootCD = "";

            aryStr = sRootCode.Split('.');

            if (aryStr.Count() > 1)
            {
                //2단계 이상 하위코드
                for (int idx = 1; idx < aryStr.Count(); idx++)
                {
                    if (sCngRootCD == "")
                        sCngRootCD = aryStr[idx];
                    else
                        sCngRootCD = sCngRootCD + "." + aryStr[idx];
                }

                TreeAddNode(objNode[aryStr[1]].Nodes, sCode, sName, sCngRootCD, sImageKey);
            }
            else
            {
                //1단계
                if (objNode != null)
                    objNode.Add(sCode, sName, sImageKey);
            }
        }

        public static int[] RedimArrayLength(int[] myArray, int length)
        {
            Array.Resize(ref myArray, length);
            return myArray;
        }

        public static string[] RedimArrayLength(string[] myArray, int length)
        {
            Array.Resize(ref myArray, length);
            return myArray;
        }

        public static void ProcOnTop(object frm, int onTopValue)
        {
            //int HWND_TOP = 0;           // 바로 다음상위로 이동
            int HWND_BOTTOM = 1;        // 최상위로 이동
            int HWND_TOPMOST = -1;      // 최상위로 이동, 포커스를 잃더라도 레벨을 유지
            //int HWND_NOTOPMOST = -2;    // 최상위 바로 다음위치로 이동
            //int SWP_NOMOVE = 2;
            //int SWP_NOSIZE = 1;

            Form thisFrm = (Form)frm;


            if (onTopValue == 1)
            {
                if (thisFrm.Left < 0 && thisFrm.Top < 0)
                    thisFrm.WindowState = FormWindowState.Normal;

                SetWindowPos(thisFrm.Handle.ToInt32(), HWND_TOPMOST, thisFrm.Left, thisFrm.Top, thisFrm.Width, thisFrm.Height, 0);
            }
            else
            {
                SetWindowPos(thisFrm.Handle.ToInt32(), HWND_BOTTOM, thisFrm.Left, thisFrm.Top, thisFrm.Width, thisFrm.Height, 0);
            }
        }


//        Public Function OnTop(frm As Object, Value As Byte, FrmTop As Long, FrmLeft As Long, Optional PosMusi As Boolean = False)
//'-------------------------------------------------------------------------------
//' Function OnTop([폼 오브젝트명], [1: 항상위, 0:해제], [Top위치], [Left위치], [포지션무시])
//' 설  명: 폼을 OS상의 모든 윈도우중 가장 위에 뜨도록 한다.
//' 리턴값: 없음
//'-------------------------------------------------------------------------------

//    Const HWND_TOPMOST = -1
//    Const HWND_NOTOPMOST = -2   ' Not Always top
//    Const SWP_NOMOVE = &H2
//    Const SWP_NOSIZE = &H1
    
//    If Value = 1 Then
        
//        If FrmTop < 0 Or FrmLeft < 0 Then frm.WindowState = vbNormal
//        If PosMusi = False Then
//            If FrmTop < 0 Or FrmLeft < 0 Then
//                FrmTop = 0
//                FrmLeft = 0
//            End If
//        End If
//        SetWindowPos frm.Hwnd, HWND_TOPMOST, FrmTop, FrmLeft, FrmTop, FrmLeft, SWP_NOSIZE
//    Else
//        SetWindowPos frm.Hwnd, HWND_NOTOPMOST, FrmTop, FrmLeft, FrmTop, FrmLeft, SWP_NOSIZE
//    End If

//    frm.Top = FrmTop
//    frm.Left = FrmLeft
//End Function




        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        //  콤보박스 관련
        // ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++


        //public static void ProcSelectedCboBox(ComboBox cbo, string includeStr) // 콤보박스 항목에 포함된 텍스트가 있으면 첫번째로 찾는 아이템을 선택한다.
        //{
        //    if (cbo.Items.Count <= 0)
        //        return;

        //    for (int idx = 0; idx < cbo.Items.Count; idx++)
        //    {
        //        string cboItems = cbo.Items[idx].ToString();

        //        if (cboItems.Contains(includeStr) == true)
        //        {
        //            cbo.SelectedIndex = idx;
        //            return;
        //        }
        //    }

        //    return;
        //}

        //public static void ProcSelectedCboBox(DevExpress.XtraEditors.ComboBoxEdit cbo, string includeStr) // 콤보박스 항목에 포함된 텍스트가 있으면 첫번째로 찾는 아이템을 선택한다.
        //{
        //    if (cbo.Properties.Items.Count <= 0)
        //        return;

        //    for (int idx = 0; idx < cbo.Properties.Items.Count; idx++)
        //    {
        //        string cboItems = cbo.Properties.Items[idx].ToString();

        //        if (cboItems.Contains(includeStr) == true)
        //        {
        //            cbo.SelectedIndex = idx;
        //            return;
        //        }
        //    }

        //    return;
        //}


        //public static void ProcSelectedCboBox2(DevExpress.XtraEditors.ComboBoxEdit cbo, string FirstStr, string SecondStr, string includeStr) // d
        //{
        //    if (cbo.Properties.Items.Count <= 0)
        //        return;

        //    for (int idx = 0; idx < cbo.Properties.Items.Count; idx++)
        //    {
        //        string cboItems = cbo.Properties.Items[idx].ToString();

        //        if (ProcParserString(cboItems, FirstStr, SecondStr) == includeStr)
        //        {
        //            cbo.SelectedIndex = idx;
        //            return;
        //        }
        //    }

        //    return;
        //}
    }
}
