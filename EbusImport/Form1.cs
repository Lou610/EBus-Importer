using EbusImport.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Data.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Globalization;
using EBusXmlParser.XmlModels;
using System.Collections;
using EbusImport.Services;
using EbusImport.Classes;
using System.Timers;
using System.Security.Permissions;
using System.Security;
using System.Net.Mail;
using System.Collections.Specialized;
using System.Net;

namespace EbusImport
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public bool Importresult;

        delegate int SetErrorLabel();
        private BackgroundWorker worker;
        public int CheckErrorCount()
        {
            int num = 0;
            int count = 0;
            IEnumerable<string> list = Directory.GetDirectories(Settings.Default.FilePath + @"\");
            foreach (string dbName in list)
            {
                string lastFolderName = Path.GetFileName(dbName);
                DirectoryInfo di = new DirectoryInfo(Settings.Default.FilePath + @"\" + lastFolderName + @"\" + @"Error\");
                count = di.GetFiles("*.xml", SearchOption.AllDirectories).Length;
                num = num + count;
            }
            return num;
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = "Ebus Audit File Importer V2 20151124";
            bool bImportData = true;

            worker = new BackgroundWorker();
            worker.DoWork += WorkThreadFunction;
            System.Timers.Timer timer = new System.Timers.Timer(1000);
            timer.Elapsed += timer_Elapsed;
            timer.Start();
        }
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (!worker.IsBusy)
                worker.RunWorkerAsync();
        }
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void pictureBox4_MouseHover(object sender, EventArgs e)
        {
            // pictureBox4.Load("../../Images/Stop-red-icon1.png");
        }

        private void pictureBox4_MouseLeave(object sender, EventArgs e)
        {
            // pictureBox4.Load("../../Images/Stop-red-icon.png");
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        public void WorkThreadFunction(object sender, DoWorkEventArgs e)
        {
            int ErrorCount = 0;
            int count = 0;
            string lastFolderName = "";
            string sname = "";
            try
            {
                SetErrorLabel error = new SetErrorLabel(CheckErrorCount);

                //string[] xmlFile = Directory.GetFiles(@"D:\eBusSuppliesTGX150AuditFiles\Incoming\Ntambanana\In\", "*.xml");
                // IEnumeral for 
                IEnumerable<string> list = Directory.GetDirectories(Settings.Default.FilePath + @"\");
                foreach (string dbName in list)
                {

                    richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Green));
                    richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                    richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Start " + DateTime.Now));
                    lastFolderName = Path.GetFileName(dbName);
                    //  DirectoryInfo di = new DirectoryInfo(Settings.Default.FilePath + @"\" + lastFolderName + @"\" + @"Error\");
                    //  ErrorCount = di.GetFiles("*.xml", SearchOption.AllDirectories).Length;
                    string[] xmlFile = Directory.GetFiles(Settings.Default.FilePath + @"\" + lastFolderName + @"\" + @"In\", "*.xml");
                    string[] directory = Directory.GetFiles(Settings.Default.FilePath + @"\" + lastFolderName + @"\" + @"In\", "*.csv");


                    foreach (string item in directory)
                    {
                        bool csvimp = CSVImport(item, lastFolderName);

                        if (csvimp == true)
                        {
                            MoveSuccessFile(item, dbName);
                            richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Red));
                            richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                            richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Sleep " + DateTime.Now));
                            Thread.Sleep(100);
                        }
                        else if (csvimp == false)
                        {
                            MoveDuplicateFile(item, dbName);
                            richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Red));
                            richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                            richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Sleep " + DateTime.Now));
                            Thread.Sleep(100);
                        }
                    }

                    foreach (string name in xmlFile)
                    {
                        sname = name;
                        DateTime dt = DateTime.Now;
                        string file = "Error " + dt.ToString("yyyyMMdd");
                        string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
                        //cashier file import
                        using (StreamWriter writer = new StreamWriter(errorPath, true))
                        {
                            writer.WriteLine("Thread Started " + Environment.NewLine + "Client: " + dbName + "FileName: " + name +
                               "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                            writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                        }
                        //Audit file import
                        FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, name);
                        if (SecurityManager.IsGranted(writePermission))
                        {
                            // you have permission



                            Importresult = ImportModule(name, lastFolderName);

                            if (Importresult == true)
                            {
                                MoveSuccessFile(name, dbName);
                                richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Red));
                                richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                                richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Sleep " + DateTime.Now));
                                Thread.Sleep(100);
                            }
                            else if (Importresult == false)
                            {
                                MoveDuplicateFile(name, dbName);
                                richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Red));
                                richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                                richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Sleep " + DateTime.Now));
                                Thread.Sleep(100);
                            }
                        }
                        else
                        {
                            Console.WriteLine("File still being processed");
                        }
                    }
                }

                int counts = error();
                lblFullCount.Invoke(new Action(() => lblFullCount.Text = " " + counts.ToString()));
                richTextBox1.Invoke(new Action(() => richTextBox1.ForeColor = Color.Red));
                richTextBox1.Invoke(new Action(() => richTextBox1.Font = new Font("Courier", 26, FontStyle.Bold)));
                richTextBox1.Invoke(new Action(() => richTextBox1.Text = "Thread Sleep " + DateTime.Now));
                count = count + ErrorCount;
                //MessageBox.Show("Import Completed", "Complete", MessageBoxButtons.OK);
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Connection fail: " + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                    "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                    MoveErrorFile(sname, lastFolderName);
                }
            }
        }

        #region Declarations
        string transCancellation = "";
        int TransNonRevenue = 0;
        int DutyNonRevenues = 0;
        int tickets = 0;
        int trip = 0;
        string SVTrip = "";
        /// <summary>   The XML module. </summary>
        IEnumerable<XElement> XMLModule;
        /// <summary>   The root. </summary>
        XElement root;
        /// <summary>   The dt import date time. </summary>
        DateTime dtImportDateTime;

        /// <summary>   The files. </summary>
        string[] files;

        string newModuleOutTime = "";
        /// <summary>   The new sign on time. </summary>
        string newSignOnTime = "";
        /// <summary>   The result. </summary>
        string result;
        /// <summary>   your code goes here. </summary>
        string subPath = @"C:\Temp";
        /// <summary>   The XML file. </summary>
        string xmlFile;
        /// <summary>   Full pathname of the file. </summary>
        string path;
        /// <summary>   The sign on date. </summary>
        string SignOnDate;
        /// <summary>   The sign off date. </summary>
        string SignOffDate;
        /// <summary>   The sign on time. </summary>
        string SignOnTime;
        /// <summary>   The sign off time. </summary>
        string SignOffTime;
        /// <summary>   The module out date. </summary>
        string ModuleOutDate;
        /// <summary>   The traffic date. </summary>
        string TrafficDate;
        /// <summary>   The module out time. </summary>
        string ModuleOutTime;
        /// <summary>   The et mptr. </summary>
        string ETMptr;
        /// <summary>   The fptr. </summary>
        string SFptr;
        /// <summary>   The duty sign on date. </summary>
        string DutySignOnDate;
        /// <summary>   The duty sign on time. </summary>
        string DutySignOnTime;
        /// <summary>   The totalnetcash paid. </summary>
        string TotalnetcashPaid;
        /// <summary>   The total net cash passengers. </summary>
        string TotalNetCashPassengers;
        /// <summary>   The total net other passengers. </summary>
        string TotalNetOtherPassengers;
        /// <summary>   List of times of the duty sign ons. </summary>
        string DutySignOnTimes;
        /// <summary>   The duty sign off time. </summary>
        string DutySignOffTime;
        /// <summary>   The esn. </summary>
        string esn;
        /// <summary>   The product. </summary>
        string Product;
        /// <summary>   The route variant no. </summary>
        string RouteVariantNo;

        /// <summary>   The journey no. </summary>
        int JourneyNo = 0;
        /// <summary>   Identifier for the fleet. </summary>
        int FleetID = 0;
        /// <summary>   Identifier for the modifier. </summary>
        int modID = 0;
        /// <summary>   Identifier for the module. </summary>
        int ModuleId = 0;
        /// <summary>   Identifier for the duty. </summary>
        int DutyId = 0;
        /// <summary>   Identifier for the stage. </summary>
        int StageId = 0;
        /// <summary>   Identifier for the journey. </summary>
        int JourneyId = 0;
        /// <summary>   Identifier for the transaction. </summary>
        int TransId = 0;
        /// <summary>   Identifier for the inspector. </summary>
        int InspectorId = 0;
        /// <summary>   The boarding stage. </summary>
        int BoardingStage;

        #endregion

        #region Static Methods
        private SqlConnection ConnectToSql(string sDatasource, string sUsername, string sPassword, string sDatabase)
        {
            try
            {
                SqlConnection sqlConnection = null;
                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();


                csb.DataSource = sDatasource;
                // MY local DB is trusted - uncomment if your db requires a username and password
                csb.UserID = sUsername;
                csb.Password = sPassword;
                csb.InitialCatalog = sDatabase;

                sqlConnection = new SqlConnection(csb.ConnectionString);
                sqlConnection.Open();

                return sqlConnection;
            }
            catch (Exception ex)
            {
                string s = ex.ToString();
                return null;
            }
        }

        public void CopyFile()
        {
            string fileName = "staff.xml";
            string targetPath = @"C:\Temp";
            // Use Path class to manipulate file and directory paths.
            string sourcePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string sourceFile = System.IO.Path.Combine(sourcePath, fileName);
            string destFile = System.IO.Path.Combine(targetPath, fileName);
            // To copy a folder's contents to a new location:
            // Create a new target folder, if necessary.
            if (!System.IO.Directory.Exists(targetPath))
                // To copy a file to another location and 
                // overwrite the destination file if it already exists.
                System.IO.File.Copy(sourceFile, destFile, true);
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Logwriters. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="message">  The message. </param>
        /// <param name="Method">   The method. </param>
        /// <param name="file">     The file. </param>
        ///-------------------------------------------------------------------------------------------------

        public void Logwriter(string message, string Method, string file)
        {


        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Logwriter success. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="Method">   The method. </param>
        /// <param name="file">     The file. </param>
        ///-------------------------------------------------------------------------------------------------

        public void LogwriterSuccess(string Method, string file)
        {
            using (StreamWriter writer = new StreamWriter("Success.txt"))
            {

                writer.WriteLine(Method);
                writer.WriteLine(file);
                writer.WriteLine(DateTime.Now.ToString());
            }
        }

        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Move error file. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="Currentpath">  The currentpath. </param>
        ///-------------------------------------------------------------------------------------------------

        public void MoveErrorFile(string Currentpath, string dbname)
        {
            try
            {
                string path = Currentpath;
                string file = Path.GetFileName(Currentpath);

                string dirPath1 = Settings.Default.FilePath + @"\" + dbname + @"\" + @"Error\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day;
                string path2 = Settings.Default.FilePath + @"\" + dbname + @"\" + @"Error\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\" + file;

                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }

                //  string path2 = @"D:\eBusSuppliesTGX150AuditFiles\Incoming\Ntambanana\Error\" + file;
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                //CheckErrorCount();
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
            }
        }

        public void MoveDuplicateFile(string Currentpath, string dbname)
        {
            try
            {

                string path = Currentpath;
                string file = Path.GetFileName(Currentpath);
                string path2 = dbname + @"\" + @"Duplicate\" + file;

                //  string path2 = @"D:\eBusSuppliesTGX150AuditFiles\Incoming\Ntambanana\Error\" + file;
                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                //        CheckErrorCount();
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Duplicate File moved failed " + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }


        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Move success file. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="Currentpath">  The currentpath. </param>
        ///-------------------------------------------------------------------------------------------------

        public void MoveSuccessFile(string Currentpath, string dbname)
        {
            try
            {
                string result = Path.GetFileNameWithoutExtension(Currentpath);
                string path = Currentpath;
                string file = Path.GetFileName(Currentpath);
                // string path2 = @"D:\eBusSuppliesTGX150AuditFiles\Incoming\Ntambanana\Out\";
                //string path2 = dbname + @"\" + @"Out\" + file;

                string dirPath1 = dbname + @"\" + @"Out\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day;
                string path2 = dbname + @"\" + @"Out\" + DateTime.Now.Year + @"\" + "0" + DateTime.Now.Month + @"\" + DateTime.Now.Day + @"\" + file;

                if (!Directory.Exists(dirPath1))
                {
                    Directory.CreateDirectory(dirPath1);
                }

                if (File.Exists(path2))
                {
                    File.Delete(path2);
                }
                File.Copy(path, path2);
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

            }
            catch (Exception ex)
            {

                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Success file moved failed " + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }

        #endregion

        #region CSV Import
        public bool CSVImport(string sfilePath, string dbname)
        {
            String filePath = sfilePath;
            try
            {
                string filename = Path.GetFileName(sfilePath);
                //check file name in out folder first against file to import.
                string[] csvFiles = Directory.GetFiles(Settings.Default.FilePath + @"\" + dbname + @"\" + @"Out\", "*.csv").Select(path => Path.GetFileName(path)).ToArray();
                if (csvFiles.Length == 0)
                {
                     PipedToDataTable(filePath, dbname);
                        return true;
                }
                foreach (var name in csvFiles)
                {
                    if (name == filename)
                    {
                        MoveDuplicateFile(sfilePath, dbname);
                        return false;
                    }
                    else if (name != filename)
                    {
                        PipedToDataTable(filePath, dbname);
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }
        DataTable PipedToDataTable(string PathToDataFile, string dbname)
        {
            DataTable dt = new DataTable();
            string fileName = Path.GetFileName(PathToDataFile);
            string newFile = fileName.Substring(0, 6);
            dt.Columns.AddRange(new DataColumn[6]
            {
                new DataColumn("StaffNumber",typeof(string)),
                new DataColumn("Date",typeof(string)),
                new DataColumn("Time",typeof(string)),
                new DataColumn("Revenue",typeof(string)),
                new DataColumn("CashierID",typeof(string)),
                new DataColumn("ImportDateTime",typeof(string)),
            });
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB"); 
                var lines = File.ReadAllLines(PathToDataFile);
                int dataRowStart = 0;
                for (int i = dataRowStart; i < lines.Length; i++)
                {
                    string line = lines[i];
                    string[] values = line.Split(',');
                    // Do the same for values
                    string date = values[1].ToString();
                    string time = values[2].ToString();
                    string Employee = values[3].ToString();
                    string Revenue = values[5].ToString();



                    string CashierDate = DateTime.ParseExact(date, "yyyyMMdd", null).ToString("dd-MM-yyyy");
                    
                    string test = CashierDate;

                    DateTime date23 = DateTime.ParseExact(test, "dd-MM-yyyy",null);

                   //DateTime time2 = DateTime.Parse(CashierDate);

                    //DateTime dates = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);

                    //DateTime d = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture);


                    string ImportDateTime = DateTime.Now.ToString();

                    string tempTime = DateTime.ParseExact(time, "HHmmss", null).ToString("h:mm:ss tt");

                    string CashierTime = CashierDate + " " + tempTime;
                    DateTime Time12 = DateTime.Parse(CashierTime);


                    dt.Rows.Add(Employee, date23, Time12, Revenue, newFile, ImportDateTime);
                }

                BulkCopyCashier(dt, PathToDataFile, dbname);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return dt;
        }

        bool BulkCopyCashier(DataTable dtData, string FileName, string dbname)
        {
            bool bDidInsert = false;

            try
            {
                SqlBulkCopy sbc = new SqlBulkCopy(ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname));
                sbc.DestinationTableName = "Cashier";
                //value on the left is from datarow[] and value on right is the col index in db
                sbc.ColumnMappings.Add("StaffNumber", "StaffNumber");
                sbc.ColumnMappings.Add("Date", "Date");
                sbc.ColumnMappings.Add("Time", "Time");
                sbc.ColumnMappings.Add("Revenue", "Revenue");
                sbc.ColumnMappings.Add("CashierID", "CashierID");
                sbc.ColumnMappings.Add("ImportDateTime", "ImportDateTime");
                //ImportDateTime
                sbc.WriteToServer(dtData);

                sbc.Close();
                bDidInsert = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return bDidInsert;
        }


        #endregion

        #region Importing Methods
        int ModuleID = 0;
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Import module. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="filePath"> The file path. </param>
        ///
        /// <returns>   true if it succeeds, false if it fails. </returns>
        ///-------------------------------------------------------------------------------------------------
        public bool ImportModule(string filePath, string dbname)
        {
            try
            {
                string mESN = "";
                //nonrevenue and transfer
                DataTable dt = new DataTable();
                dt.Columns.AddRange(new DataColumn[23] {
                    new DataColumn("id_Module",typeof(int)),
                    new DataColumn("int4_ModuleID", typeof(int)),
                    new DataColumn("int4_SignOnID",typeof(int)),
                    new DataColumn("int4_OnReaderID",typeof(int)),
                    new DataColumn("dat_SignOnDate",typeof(DateTime)),
                    new DataColumn("dat_SignOnTime",typeof(DateTime)),
                    new DataColumn("int4_OffReaderID",typeof(int)),
                    new DataColumn("dat_SignOffDate",typeof(DateTime)),
                    new DataColumn("dat_SignOffTime",typeof(DateTime)),
                    new DataColumn("dat_TrafficDate",typeof(DateTime)),
                    new DataColumn("dat_ModuleOutDate",typeof(DateTime)),
                    new DataColumn("dat_ModuleOutTime",typeof(DateTime)),
                    new DataColumn("int4_HdrModuleRevenue",typeof(int)),
                    new DataColumn("int4_HdrModuleTickets",typeof(int)),
                    new DataColumn("int4_HdrModulePasses",typeof(int)),
                    new DataColumn("int4_ModuleRevenue",typeof(int)),
                    new DataColumn("int4_ModuleTickets",typeof(int)),
                    new DataColumn("int4_ModulePasses",typeof(int)),
                    new DataColumn("int4_ModuleNonRevenue",typeof(int)),
                     new DataColumn("int4_ModuleTransfer",typeof(int)),
                    new DataColumn("dat_RecordMod",typeof(DateTime)),
                    new DataColumn("id_BatchNo",typeof(int)),
                    new DataColumn("byt_ModuleType",typeof(byte))
                });

                XDocument xdoc = XDocument.Load(filePath);

                var Items = xdoc.Descendants("ModuleHeader").Select(x => new
                {
                    ModuleESN = (int)x.Element("ModuleESN"),
                    DriverNumber1 = (int)x.Element("DriverNumber1"),
                    HomeDepotID = (int)x.Element("HomeDepotID"),
                    SignOnDate = (string)x.Element("SignOnDate"),
                    SignOnTime = (string)x.Element("SignOnTime"),
                    ETMptr = (string)x.Element("ETMptr"),
                    SFptr = (string)x.Element("SFptr"),
                    HomeDepoID = (string)x.Element("HomeDepotID"),
                    SignOffDate = (string)x.Element("SignOffDate"),
                    SignOffTime = (string)x.Element("SignOffTime"),
                    TrafficDate = (string)x.Element("SignOffDate"),
                    ModuleOutDate = (string)x.Element("SignOffDate"),
                    ModuleOutTime = (string)x.Element("SignOffTime"),
                    TotalNetCashPaid = (int)x.Element("TotalNetCashPaid"),
                    TotalNetCashPassengers = (int)x.Element("TotalNetCashPassengers"),
                    TotalNetOtherPassengers = (int)x.Element("TotalNetOtherPassengers"),
                    TotalGrossCash = (int)x.Element("TotalGrossCash"),
                    TotalGrossCashPassengers = (int)x.Element("TotalGrossCashPassengers"),
                    TotalGrossOtherPassengers = (int)x.Element("TotalGrossOtherPassengers"),
                    ModuleType = (int)x.Element("ModuleType"),
                }).ToList();




                Int32 IDModule = 0;
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {

                    string sql = "SELECT TOP 1 id_Module FROM Module ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, conn);
                    try
                    {
                        conn.Close();
                        conn.Open();

                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            IDModule = Convert.ToInt32(val);
                            IDModule = IDModule + 1;
                        }
                        else if (val == null)
                        {
                            IDModule = Convert.ToInt32(val);
                            IDModule = IDModule + 1;
                        }
                        conn.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);

                        MoveErrorFile(filePath, dbname);
                    }
                }
                foreach (var item in Items)
                {

                    ETMptr = item.ETMptr.ToString();
                    SFptr = item.SFptr.ToString();
                    TotalnetcashPaid = item.TotalNetCashPaid.ToString();
                    TotalNetCashPassengers = item.TotalNetCashPassengers.ToString();
                    TotalNetOtherPassengers = item.TotalNetOtherPassengers.ToString();
                    try
                    {
                        //Taking out the ModuleID
                        ModuleId = Convert.ToInt32(item.ModuleESN);
                        //Getting the datetime of today
                        dtImportDateTime = DateTime.Now;
                        //Parsing DateTime to correct format
                        SignOnDate = DateTime.ParseExact(item.SignOnDate, "ddMMyy", null).ToString("yyyy-MM-dd");
                        SignOffDate = DateTime.ParseExact(item.SignOffDate, "ddMMyy", null).ToString("yyyy-MM-dd");
                        //SignOnDate = DateTime.ParseExact(item.SignOnDate, "ddMMyyyy", CultureInfo.InvariantCulture);
                        ModuleOutDate = DateTime.ParseExact(item.ModuleOutDate, "ddMMyy", null).ToString("yyyy-MM-dd");
                        TrafficDate = DateTime.ParseExact(item.TrafficDate, "ddMMyy", null).ToString("yyyy-MM-dd");

                        //Setting date format to the correct format for DB
                        int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));
                        SignOnTime = DateTime.ParseExact(item.SignOnTime, "HHmm", null).ToString("h:mm tt");
                        SignOffTime = DateTime.ParseExact(item.SignOffTime, "HHmm", null).ToString("h:mm tt");
                        ModuleOutTime = DateTime.ParseExact(item.ModuleOutTime, "HHmm", null).ToString("h:mm tt");
                        //Collecting only signoffDate
                        newSignOnTime = SignOnDate + " " + SignOnTime;
                        string newSignOffTime = SignOffDate + " " + SignOffTime;
                        newModuleOutTime = ModuleOutDate + " " + ModuleOutTime;

                        dt.Rows.Add(IDModule, item.ModuleESN, item.DriverNumber1, Convert.ToInt32(item.HomeDepoID), Convert.ToDateTime(SignOnDate), Convert.ToDateTime(newSignOnTime), item.HomeDepotID, Convert.ToDateTime(SignOffDate), Convert.ToDateTime(newSignOffTime), Convert.ToDateTime(TrafficDate), Convert.ToDateTime(ModuleOutDate), Convert.ToDateTime(newModuleOutTime), item.TotalNetCashPaid, Convert.ToInt32(item.TotalNetCashPassengers), TotalNetOtherPassengers, item.TotalGrossCash, item.TotalGrossCashPassengers, item.TotalGrossOtherPassengers, 0, 0, Convert.ToDateTime(dtImportDateTime), batchID, item.ModuleType);
                        mESN = item.ModuleESN.ToString();
                        InsertDrivers(item.DriverNumber1.ToString(), dbname);
                    }
                    catch (Exception ex)
                    {
                        Logwriter(ex.Message, "ImportModule", filePath);
                        char[] delimiterChars = { ';' };
                        string[] email = Settings.Default.emails.Split(delimiterChars);

                        foreach (var items in email)
                        {

                            System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                            mail.To.Add(items);



                            mail.IsBodyHtml = true;
                            string body = "";
                            using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                            {
                                body = reader.ReadToEnd();
                            }
                            string files = Path.GetFileName(filePath);
                            body = body.Replace("[*FileName*]", files);
                            body = body.Replace("[*ClientName*]", dbname);
                            body = body.Replace("[*Error*]", ex.Message);
                            MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                            mail.From = address;
                            mail.Subject = "Ebus Import Error";
                            mail.Body = body;
                            mail.IsBodyHtml = true;
                            SmtpClient client = new SmtpClient();
                            client.DeliveryMethod = SmtpDeliveryMethod.Network;
                            client.EnableSsl = false;
                            client.Host = "mail.ebussupplies.co.za";
                            client.Port = 25;

                            //Setup credentials to login to our sender email address ("UserName", "Password")
                            NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                            client.UseDefaultCredentials = true;
                            client.Credentials = credentials;

                            //Send the msg
                            client.Send(mail);

                        }
                        MoveErrorFile(filePath, dbname);
                    }
                }
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Module";

                        sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModuleID", "int4_ModuleID");
                        sqlBulkCopy.ColumnMappings.Add("int4_SignOnID", "int4_SignOnID");
                        sqlBulkCopy.ColumnMappings.Add("int4_OnReaderID", "int4_OnReaderID");
                        sqlBulkCopy.ColumnMappings.Add("dat_SignOnDate", "dat_SignOnDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_SignOnTime", "dat_SignOnTime");
                        sqlBulkCopy.ColumnMappings.Add("int4_OffReaderID", "int4_OffReaderID");
                        sqlBulkCopy.ColumnMappings.Add("dat_SignOffDate", "dat_SignOffDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_SignOffTime", "dat_SignOffTime");
                        sqlBulkCopy.ColumnMappings.Add("dat_TrafficDate", "dat_TrafficDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_ModuleOutDate", "dat_ModuleOutDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_ModuleOutTime", "dat_ModuleOutTime");
                        sqlBulkCopy.ColumnMappings.Add("int4_HdrModuleRevenue", "int4_HdrModuleRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_HdrModuleTickets", "int4_HdrModuleTickets");
                        sqlBulkCopy.ColumnMappings.Add("int4_HdrModulePasses", "int4_HdrModulePasses");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModuleRevenue", "int4_ModuleRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModuleTickets", "int4_ModuleTickets");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModulePasses", "int4_ModulePasses");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModuleNonRevenue", "int4_ModuleNonRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_ModuleTransfer", "int4_ModuleTransfer");
                        sqlBulkCopy.ColumnMappings.Add("dat_RecordMod", "dat_RecordMod");
                        sqlBulkCopy.ColumnMappings.Add("id_BatchNo", "id_BatchNo");
                        sqlBulkCopy.ColumnMappings.Add("byt_ModuleType", "byt_ModuleType");
                        con.Close();
                        con.Open();
                        sqlBulkCopy.WriteToServer(dt);
                        con.Close();
                    }
                }
                TransNonRevenue = 0;
                DutyNonRevenues = 0;
                tickets = 0;
                ModuleID = IDModule;
                InsertModules(mESN, Convert.ToDateTime(newSignOnTime), dbname);
                ImportDuty(filePath, IDModule, xdoc, dbname);
                WaybillImport(filePath, dbname);

                return true;
            }
            catch (SqlException Nullex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");


                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
                string EerrorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                char[] delimiterChars = { ';' };
                string[] email = Settings.Default.emails.Split(delimiterChars);

                //foreach (var item in email)
                //{

                //    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                //    mail.To.Add(item);

                //    string files = Path.GetFileName(filePath);

                //    mail.IsBodyHtml = true;
                //    string body = "";
                //    using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                //    {
                //        body = reader.ReadToEnd();
                //    }

                //    body = body.Replace("[*FileName*]", files);
                //    body = body.Replace("[*ClientName*]", dbname);
                //    body = body.Replace("[*Error*]", Nullex.Message);
                //    MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                //    mail.From = address;
                //    mail.Subject = "Ebus Import Error";
                //    mail.Body = body;
                //    mail.IsBodyHtml = true;
                //    SmtpClient client = new SmtpClient();
                //    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //    client.EnableSsl = false;
                //    client.Host = "mail.ebussupplies.co.za";
                //    client.Port = 25;

                //    //Setup credentials to login to our sender email address ("UserName", "Password")
                //    NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                //    client.UseDefaultCredentials = true;
                //    client.Credentials = credentials;

                //    //Send the msg
                //    client.Send(mail);

                //}


                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + Nullex.Message + "<br/>" + "Import Module into DB:" + dbname + "<br/>" + Environment.NewLine + "StackTrace :" + Nullex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);

                }

                return false;
            }
        }
        ///-------------------------------------------------------------------------------------------------
        /// <summary>   Import duty. </summary>
        ///
        /// <remarks>   Lourens, 9/11/2015. </remarks>
        ///
        /// <param name="filePath"> The file path. </param>
        /// <param name="IDModule"> The identifier module. </param>
        ///-------------------------------------------------------------------------------------------------
        public void ImportDuty(string filePath, int IDModule, XDocument doc, string dbname)
        {
            int IDDuty = 0;
            try
            {
                DataTable dt1 = new DataTable();
                DataTable dt2 = new DataTable();
                DataTable dt3 = new DataTable();
                //trafficdate non revenue transfers recordmod
                // 
                dt1.Columns.AddRange(new DataColumn[18] {
                    new DataColumn("id_Duty",typeof(int)),
                    new DataColumn("id_Module", typeof(int)),
                    new DataColumn("int4_DutyID",typeof(int)),
                    new DataColumn("int4_OperatorID",typeof(int)),
                    new DataColumn("str_ETMID",typeof(string)),
                    new DataColumn("int4_GTValue",typeof(int)),
                    new DataColumn("int4_NextTicketNumber",typeof(int)),
                    new DataColumn("int4_DutySeqNum",typeof(int)),
                    new DataColumn("dat_DutyStartDate",typeof(DateTime)),
                    new DataColumn("dat_DutyStartTime",typeof(DateTime)),
                    new DataColumn("dat_TrafficDate",typeof(DateTime)),
                    new DataColumn("str_BusID",typeof(string)),
                    new DataColumn("str_FirstRouteID",typeof(string)),
                    new DataColumn("int2_FirstJourneyID",typeof(int)),
                    new DataColumn("id_BatchNo",typeof(int)),
                    new DataColumn("str_EpromVersion",typeof(string)),
                    new DataColumn("str_OperatorVersion",typeof(string)),

                    new DataColumn("str_SpecialVersion",typeof(string))
                });

                dt2.Columns.AddRange(new DataColumn[7] {

                    new DataColumn("dat_DutyStopDate",typeof(DateTime)),
                    new DataColumn("dat_DutyStopTime",typeof(DateTime)),
                    new DataColumn("int4_DutyRevenue",typeof(int)),
                    new DataColumn("int4_DutyTickets",typeof(int)),
                    new DataColumn("int4_DutyPasses",typeof(int)),
                    new DataColumn("int4_DutyNonRevenue",typeof(int)),
                      new DataColumn("int4_DutyTransfer",typeof(int)),
                });


                XDocument xdoc1 = doc;

                var Items = xdoc1.Descendants("StartOfDuty").Select(x => new
                {
                    DutyDate = (string)x.Element("DutyDate"),
                    DutyTime = (string)x.Element("DutyTime"),
                    DriverNumber = (int)x.Element("DriverNumber"),
                    DutyNo = (int)x.Element("DutyNo"),
                    DutySeqNo = (int)x.Element("DutySeqNo."),
                    Route = (int)x.Element("Route"),
                    FleetID = (int)x.Element("FleetID"),
                    RunningBoard = (string)x.Element("RunningBoard"),
                    ETMCashTotal = (int)x.Element("ETMCashTotal"),
                    NextTicketNo = (int)x.Element("NextTicketNo"),
                }).ToList();

                var waybil = xdoc1.Descendants("ExtendedWaybill").Select(x => new
                {

                    ETMNumber = (string)x.Element("ETMNumber"),
                    BusNumber = (int)x.Element("BusNumber"),


                }).ToList();
                var EndOfDuty = xdoc1.Descendants("EndOfDuty").Select(x => new
                {

                    SignOffDate = (string)x.Element("SignOffDate"),
                    SignOffTime = (string)x.Element("SignOffTime"),
                    DutyCashTotal = (int)x.Element("DutyCashTotal"),
                    DutyTicketTotal = (int)x.Element("DutyTicketTotal"),
                    DutyAnnulCashTotal = (int)x.Element("DutyAnnulCashTotal"),
                    DutyPassTotal = (int)x.Element("DutyPassTotal"),

                }).ToList();

                var StartOfJourney = xdoc1.Descendants("StartOfJourney").Select(x => new
                {

                    RouteVariantNo = (string)x.Element("RouteVariantNo"),
                    JourneyNo = (int)x.Element("JourneyNo"),

                }).ToList();

                foreach (var item in StartOfJourney)
                {
                    RouteVariantNo = item.RouteVariantNo;
                    JourneyNo = item.JourneyNo;
                }

                foreach (var item1 in EndOfDuty)
                {
                    int second = int.Parse(item1.SignOffTime);
                    double seconds = second;
                    DutySignOnDate = DateTime.ParseExact(item1.SignOffDate, "ddMMyy", null).ToString("yyyy-MM-dd");

                    TimeSpan t = TimeSpan.FromSeconds(seconds);

                    string formatedTime1 = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

                    string time = DateTime.ParseExact(item1.SignOffTime, "HHmmss", null).ToString("h:mm:ss tt");
                    // string dateTime = DateTime.ParseExact(formatedTime, "HH:mm:ss", CultureInfo.InvariantCulture).ToString("h:mm:ss tt");

                    DutySignOffTime = DutySignOnDate + " " + time;

                    dt2.Rows.Add(Convert.ToDateTime(DutySignOnDate), Convert.ToDateTime(DutySignOffTime), item1.DutyCashTotal, item1.DutyTicketTotal, item1.DutyPassTotal, item1.DutyAnnulCashTotal, 0);
                }

                string way = "";


                string formatedTime = "";

                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    string sql = "SELECT TOP 1 id_Duty FROM Duty ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();
                        object val = cmd.ExecuteScalar();

                        if (val != null)
                        {
                            IDDuty = Convert.ToInt32(val);
                            IDDuty = IDDuty + 1;
                        }
                        else if (val == null)
                        {
                            IDDuty = Convert.ToInt32(val);
                            IDDuty = IDDuty + 1;
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                foreach (var item in Items)
                {
                    int bussno = 0;
                    foreach (var items in waybil)
                    {
                        way = items.ETMNumber.ToString();
                        bussno = items.BusNumber;
                    }
                    //Getting the datetime of today
                    int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));
                    //Parsing DateTime to correct format
                    int second = int.Parse(item.DutyTime);
                    double seconds = second;
                    DutySignOnDate = DateTime.ParseExact(item.DutyDate, "ddMMyy", null).ToString("yyyy-MM-dd");
                    TimeSpan t = TimeSpan.FromSeconds(seconds);

                    formatedTime = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Hours, t.Minutes, t.Seconds);

                    string time = DateTime.ParseExact(item.DutyTime, "HHmmss", null).ToString("h:mm:ss tt");
                    // string dateTime = DateTime.ParseExact(formatedTime, "HH:mm:ss", CultureInfo.InvariantCulture).ToString("h:mm:ss tt");
                    DutySignOnTimes = DutySignOnDate + " " + time;
                    DutySignOffTime = SignOnDate + " " + SignOffTime;

                    dt1.Rows.Add(IDDuty, IDModule, item.DutyNo, item.DriverNumber, way, item.ETMCashTotal, item.NextTicketNo, item.DutySeqNo, Convert.ToDateTime(DutySignOnDate), Convert.ToDateTime(DutySignOnTimes), Convert.ToDateTime(DutySignOnDate), item.FleetID.ToString(), RouteVariantNo, Convert.ToInt16(JourneyNo), batchID, ETMptr, item.DutyDate.ToString(), SFptr);
                    //IDDuty++;
                }

                var columns = dt1.Columns.Cast<DataColumn>()
                                  .Concat(dt2.Columns.Cast<DataColumn>());

                foreach (var column in columns)
                {
                    dt3.Columns.Add(column.ColumnName, column.DataType);
                }

                for (int i = 0; i < dt1.Rows.Count; i++)
                {
                    var row = dt3.NewRow();
                    row.ItemArray = dt1.Rows[i].ItemArray
                                       .Concat(dt2.Rows[i].ItemArray).ToArray();

                    dt3.Rows.Add(row);
                }

                using (SqlConnection con1 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con1))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Duty";

                        sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                        sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyID", "int4_DutyID");
                        sqlBulkCopy.ColumnMappings.Add("int4_OperatorID", "int4_OperatorID");
                        sqlBulkCopy.ColumnMappings.Add("str_ETMID", "str_ETMID");
                        sqlBulkCopy.ColumnMappings.Add("int4_GTValue", "int4_GTValue");
                        sqlBulkCopy.ColumnMappings.Add("int4_NextTicketNumber", "int4_NextTicketNumber");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutySeqNum", "int4_DutySeqNum");
                        sqlBulkCopy.ColumnMappings.Add("dat_DutyStartDate", "dat_DutyStartDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_DutyStartTime", "dat_DutyStartTime");
                        sqlBulkCopy.ColumnMappings.Add("dat_DutyStopDate", "dat_DutyStopDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_DutyStopTime", "dat_DutyStopTime");
                        sqlBulkCopy.ColumnMappings.Add("dat_TrafficDate", "dat_TrafficDate");
                        sqlBulkCopy.ColumnMappings.Add("str_BusID", "str_BusID");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyRevenue", "int4_DutyRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyTickets", "int4_DutyTickets");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyPasses", "int4_DutyPasses");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyNonRevenue", "int4_DutyNonRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_DutyTransfer", "int4_DutyTransfer");
                        sqlBulkCopy.ColumnMappings.Add("str_FirstRouteID", "str_FirstRouteID");
                        sqlBulkCopy.ColumnMappings.Add("int2_FirstJourneyID", "int2_FirstJourneyID");
                        sqlBulkCopy.ColumnMappings.Add("id_BatchNo", "id_BatchNo");
                        sqlBulkCopy.ColumnMappings.Add("str_EpromVersion", "str_EpromVersion");
                        sqlBulkCopy.ColumnMappings.Add("str_SpecialVersion", "str_SpecialVersion");
                        con1.Close();
                        con1.Open();
                        sqlBulkCopy.WriteToServer(dt3);
                        con1.Close();
                    }
                }

                ImportJourney(filePath, IDModule, IDDuty, doc, dbname);
                LogwriterSuccess("ImportDuty", filePath);
            }
            catch (Exception DutyImportException)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");

                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                char[] delimiterChars = { ';' };
                string[] email = Settings.Default.emails.Split(delimiterChars);

                foreach (var item in email)
                {

                    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                    mail.To.Add(item);


                    string files = Path.GetFileName(filePath);

                    mail.IsBodyHtml = true;
                    string body = "";
                    using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("[*FileName*]", files);
                    body = body.Replace("[*ClientName*]", dbname);
                    body = body.Replace("[*Error*]", DutyImportException.Message);
                    MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                    mail.From = address;
                    mail.Subject = "Ebus Import Error";
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    SmtpClient client = new SmtpClient();
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = false;
                    client.Host = "mail.ebussupplies.co.za";
                    client.Port = 25;

                    //Setup credentials to login to our sender email address ("UserName", "Password")
                    NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                    client.UseDefaultCredentials = true;
                    client.Credentials = credentials;

                    //Send the msg
                    client.Send(mail);

                }


                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + DutyImportException.Message + "<br/>" + "Import Duty into DB:" + dbname + "<br/>" + Environment.NewLine + "StackTrace :" + DutyImportException.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                Logwriter(DutyImportException.Message, "ImportDuty", filePath);
                //ImportJourneyfilePath, IDModule, IDDuty, doc);
                MoveErrorFile(filePath, dbname);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="ModId"></param>
        /// <param name="dutID"></param>
        /// <param name="doc"></param>
        public void ImportJourney(string path, int ModId, int dutID, XDocument doc, string dbname)
        {
            int IDJourneies = 0;
            try
            {
                DataTable dt1 = new DataTable();
                dt1.Columns.AddRange(new DataColumn[22] {
                    new DataColumn("id_Journey",typeof(int)),
                    new DataColumn("id_Duty", typeof(int)),
                    new DataColumn("id_Module", typeof(int)),
                    new DataColumn("str_RouteID",typeof(string)),
                    new DataColumn("int2_JourneyID",typeof(int)),
                    new DataColumn("int2_Direction",typeof(int)),
                    new DataColumn("dat_JourneyStartDate",typeof(DateTime)),
                    new DataColumn("dat_JourneyStartTime",typeof(DateTime)),
                    new DataColumn("dat_JourneyStopDate", typeof(DateTime)),
                    new DataColumn("dat_JourneyStopTime", typeof(DateTime)),
                    new DataColumn("dat_TrafficDate", typeof(DateTime)),
                    new DataColumn("int4_Distance", typeof(int)),
                    new DataColumn("int4_Traveled", typeof(int)),
                    new DataColumn("int4_JourneyRevenue", typeof(int)),
                    new DataColumn("int4_JourneyTickets", typeof(int)),
                    new DataColumn("int4_JourneyPasses", typeof(int)),
                    new DataColumn("int4_JourneyNonRevenue", typeof(int)),
                    new DataColumn("int4_JourneyTransfer", typeof(int)),
                    new DataColumn("dat_RecordMod", typeof(DateTime)),
                    new DataColumn("id_BatchNo",typeof(int)),
                    new DataColumn("startPos",typeof(int)),
                    new DataColumn("endPos",typeof(int)),
                });


                int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));

                XDocument xdoc2 = doc;

                string JourneyStopDate = "";
                string JourneyStopTime = "";
                string JourneyStartDate = "";
                int JourneyCashTotal = 0;
                int JourneyTicketTotal = 0;
                int JourneyPassTotal = 0;


                int journeyNo = 0;
                int direction = 0;

                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {

                    string sql = "SELECT TOP 1 id_Journey FROM Journey ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();
                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            IDJourneies = Convert.ToInt32(val);
                            IDJourneies = IDJourneies + 1;
                        }
                        else if (val == null)
                        {
                            IDJourneies = Convert.ToInt32(val);
                            IDJourneies = IDJourneies + 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                var jourNeys = new ParserHelper().GetAllJourneyDetails(doc);

                //order by postions, so we will have start-end start-end,..........
                var allJourneys = (from journey in jourNeys
                                   select new
                                   {
                                       journey,
                                       position = Convert.ToInt32(journey.Attribute("Position").Value)
                                   }).OrderBy(s => s.position).ToList();

                // All assigned below in for each loop 
                int iJounreyNo = 0; // Should never change until new journey

                int iEndOfJourneyPos;

                string StopDate = "";
                string stop = "";
                string Rev = "";
                string ticket = "";
                String pass = "";

                var journeyDetails = new List<JoureyDetails>();

                for (int j = 0; j < allJourneys.Count; j++)
                {
                    if (allJourneys[j].journey.Name.LocalName.Equals("StartOfJourney"))
                    {
                        // Journey Number - StartOfJourney
                        var journeyNumber = allJourneys[j].journey.Elements("JourneyNo");

                        foreach (string sValue in journeyNumber)
                        {
                            iJounreyNo = Int32.Parse(sValue);
                        }
                        String StartDate = allJourneys[j].journey.Element("StartDate").Value.ToString();
                        string startTime = allJourneys[j].journey.Element("StartTime").Value.ToString();

                        RouteVariantNo = allJourneys[j].journey.Element("RouteVariantNo").Value.ToString();
                        JourneyStartDate = DateTime.ParseExact(StartDate, "ddMMyy", null).ToString("yyyy-MM-dd");


                        var istartOfJourneyPos = Int32.Parse(allJourneys[j].journey.Attribute("Position").Value.ToString());

                        //to be read from end of journey tag
                        iEndOfJourneyPos = Int32.Parse(allJourneys[j + 1].journey.Attribute("Position").Value.ToString());
                        StopDate = allJourneys[j + 1].journey.Element("JourneyStopDate").Value.ToString();
                        stop = allJourneys[j + 1].journey.Element("JourneyStopTime").Value.ToString();

                        Rev = allJourneys[j + 1].journey.Element("JourneyCashTotal").Value.ToString();
                        ticket = allJourneys[j + 1].journey.Element("JourneyTicketTotal").Value.ToString();
                        pass = allJourneys[j + 1].journey.Element("JourneyPassTotal").Value.ToString();

                        string Stoptime = DateTime.ParseExact(stop, "HHmmss", null).ToString("h:mm:ss tt");
                        string TrafficDate = DateTime.ParseExact(StopDate, "ddMMyy", null).ToString("yyyy-MM-dd");
                        int Distance = 0;
                        int Travelled = 0;
                        int Transfer = 0;
                        int Revenue = Int32.Parse(Rev);
                        int TicketTotal = Int32.Parse(ticket);
                        int PassTotal = Int32.Parse(pass);
                        int NonRevenue = 0;
                        DateTime RcorMod = DateTime.Now;

                        JourneyStopDate = DateTime.ParseExact(StopDate, "ddMMyy", null).ToString("yyyy-MM-dd");

                        string newStop = JourneyStopDate + " " + Stoptime;

                        //add start and end journey details as one row
                        string times = allJourneys[j].journey.Element("StartTime").Value.ToString();
                        string journeyNumbers = allJourneys[j].journey.Element("JourneyNo").Value.ToString();
                        string time = DateTime.ParseExact(times, "HHmmss", null).ToString("h:mm:ss tt");

                        string StartsTImes = JourneyStartDate + " " + time;
                        string sub = times.Substring(0, 4);
                        Int16 no = Convert.ToInt16(sub);

                        dt1.Rows.Add(IDJourneies, dutID, ModId, RouteVariantNo, Convert.ToInt16(journeyNumbers), Convert.ToInt16(direction), Convert.ToDateTime(JourneyStartDate), Convert.ToDateTime(StartsTImes), Convert.ToDateTime(JourneyStopDate), Convert.ToDateTime(newStop), Convert.ToDateTime(TrafficDate), Distance, Travelled, Revenue, TicketTotal, PassTotal, NonRevenue, Transfer, RcorMod, batchID, istartOfJourneyPos, iEndOfJourneyPos);

                        var jrn = new JoureyDetails();
                        jrn.IDJourneies = IDJourneies;

                        jrn.dutID = dutID;
                        jrn.ModId = ModId;
                        jrn.istartOfJourneyPos = istartOfJourneyPos;
                        jrn.iEndOfJourneyPos = iEndOfJourneyPos;
                        journeyDetails.Add(jrn);

                        IDJourneies++;
                    }
                    else
                    {
                        continue;
                    }
                }

                // bulk insert all above journeys
                try
                {
                    using (SqlConnection con2 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con2))
                        {
                            sqlBulkCopy.DestinationTableName = "dbo.Journey";

                            sqlBulkCopy.ColumnMappings.Add("id_Journey", "id_Journey");
                            sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                            sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                            sqlBulkCopy.ColumnMappings.Add("str_RouteID", "str_RouteID");
                            sqlBulkCopy.ColumnMappings.Add("int2_JourneyID", "int2_JourneyID");
                            sqlBulkCopy.ColumnMappings.Add("int2_Direction", "int2_Direction");
                            sqlBulkCopy.ColumnMappings.Add("dat_JourneyStartDate", "dat_JourneyStartDate");
                            sqlBulkCopy.ColumnMappings.Add("dat_JourneyStartTime", "dat_JourneyStartTime");
                            sqlBulkCopy.ColumnMappings.Add("dat_JourneyStopDate", "dat_JourneyStopDate");
                            sqlBulkCopy.ColumnMappings.Add("dat_JourneyStopTime", "dat_JourneyStopTime");
                            sqlBulkCopy.ColumnMappings.Add("dat_TrafficDate", "dat_TrafficDate");
                            sqlBulkCopy.ColumnMappings.Add("int4_Distance", "int4_Distance");
                            sqlBulkCopy.ColumnMappings.Add("int4_Traveled", "int4_Traveled");
                            sqlBulkCopy.ColumnMappings.Add("int4_JourneyRevenue", "int4_JourneyRevenue");
                            sqlBulkCopy.ColumnMappings.Add("int4_JourneyTickets", "int4_JourneyTickets");
                            sqlBulkCopy.ColumnMappings.Add("int4_JourneyPasses", "int4_JourneyPasses");
                            sqlBulkCopy.ColumnMappings.Add("int4_JourneyNonRevenue", "int4_JourneyNonRevenue");
                            sqlBulkCopy.ColumnMappings.Add("int4_JourneyTransfer", "int4_JourneyTransfer");
                            sqlBulkCopy.ColumnMappings.Add("dat_RecordMod", "dat_RecordMod");
                            sqlBulkCopy.ColumnMappings.Add("id_BatchNo", "id_BatchNo");
                            con2.Close();
                            con2.Open();
                            sqlBulkCopy.WriteToServer(dt1);
                            con2.Close();
                        }
                    }
                }
                catch (Exception ex)
                {

                    DateTime dtime = DateTime.Now;
                    string file = "Error " + dtime.ToString("yyyyMMdd");

                    string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";


                    char[] delimiterChars = { ';' };
                    string[] email = Settings.Default.emails.Split(delimiterChars);

                    foreach (var item in email)
                    {

                        System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                        mail.To.Add(item);



                        mail.IsBodyHtml = true;
                        string body = "";
                        using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                        {
                            body = reader.ReadToEnd();
                        }

                        string files = Path.GetFileName(path);
                        body = body.Replace("[*FileName*]", files);
                        body = body.Replace("[*ClientName*]", dbname);
                        body = body.Replace("[*Error*]", ex.Message);
                        MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                        mail.From = address;
                        mail.Subject = "Ebus Import Error";
                        mail.Body = body;
                        mail.IsBodyHtml = true;
                        SmtpClient client = new SmtpClient();
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;
                        client.EnableSsl = false;
                        client.Host = "mail.ebussupplies.co.za";
                        client.Port = 25;

                        //Setup credentials to login to our sender email address ("UserName", "Password")
                        NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                        client.UseDefaultCredentials = true;
                        client.Credentials = credentials;

                        //Send the msg
                        client.Send(mail);

                    }
                    using (StreamWriter writer = new StreamWriter(errorPath, true))
                    {
                        writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Journey into DB: " + dbname + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                           "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                        writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                    }
                    Logwriter(ex.Message, "ImportJourney", path);
                    MoveErrorFile(path, dbname);
                }

                // Journey(s) done, now move to all stages

                ImportStageDetails(journeyDetails, doc, dbname);

                //WaybillImport(path); //to-do
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");

                string errorPath = file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                Logwriter(ex.Message, "ImportJourney", path);
                //ImportStage(path, ModId, dutID, IDJourneies);
                MoveErrorFile(path, dbname);
            }

            LogwriterSuccess("ImportJourney", path);
            // ImportStage(path, ModId, dutID, IDJourneies);
        }
        /// <summary>
        /// here we will insert all stages [for all journey(s)]
        /// </summary>
        /// <param name="journeyDetails"></param>
        /// <param name="doc"></param>
        private void ImportStageDetails(List<JoureyDetails> journeyDetails, XDocument doc, string dbname)
        {
            var stageDetails = new List<StageDetails>();
            int Mod = 0;
            try
            {
                int IDStages = 0;
                int sID = 0;
                string iStartOfStagePos = "";
                int iStageCount = 0;

                var helper = new ParserHelper();

                List<XElement> allStages = helper.GetStageDetails(doc);
                List<XElement> StageRersult = helper.GetTranseDetails(doc);
                List<XElement> StageRersults = helper.GetMJDetails(doc);
                int iCount = StageRersult.Count;


                DataTable dt3 = new DataTable();

                dt3.Columns.AddRange(new DataColumn[10] {
                    new DataColumn("id_Stage",typeof(int)),
                    new DataColumn("id_Journey", typeof(int)),
                    new DataColumn("id_Duty", typeof(int)),
                    new DataColumn("id_Module",typeof(int)),
                    new DataColumn("int2_StageID",typeof(int)),
                    new DataColumn("dat_StageDate",typeof(DateTime)),
                    new DataColumn("dat_StageTime",typeof(DateTime)),
                    new DataColumn("dat_RecordMod",typeof(DateTime)),
                    new DataColumn("id_BatchNo",typeof(int)),
                    new DataColumn("stagePosition",typeof(int)), //imp: this is new column added, we dont insert into db, only for reference
                });

                DataTable dt9 = new DataTable();
                XDocument xdoc5 = doc;

                dt9.Columns.AddRange(new DataColumn[3] {
                    new DataColumn("id_Stage",typeof(int)),
                    new DataColumn("id_InspectorID", typeof(int)),
                    new DataColumn("datTimeStamp", typeof(DateTime))
                });

                var Itemss1 = xdoc5.Descendants("InspectorTicket").Select(x => new
                {

                    TSN = (int)x.Element("TSN"),
                    InspectorNo = (int)x.Element("InspectorNo"),
                    Time = (string)x.Element("Time"),

                }).ToList();


                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    string sql = "SELECT TOP 1 id_Stage FROM Stage ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();
                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            IDStages = Convert.ToInt32(val);
                            IDStages = IDStages + 1;
                        }
                        else if (val == null)
                        {
                            IDStages = Convert.ToInt32(val);
                            IDStages = IDStages + 1;
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));

                string BoaringStages = "";
                string Time = "";

                //fill all stage details
                foreach (var stage in allStages)
                {
                    BoaringStages = stage.Element("BoardingStage").Value.ToString();
                    Time = stage.Element("Time").Value.ToString();

                    string date = Convert.ToInt16(BoaringStages).ToString("yyyy-MM-dd");
                    DateTime dt = DateTime.Now;
                    int second = Convert.ToInt32(Time);
                    double seconds = second;
                    TimeSpan t = TimeSpan.FromSeconds(seconds);
                    string formatedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                               t.Hours,
                               t.Minutes,
                               t.Seconds);

                    string time = DutySignOnDate + " " + formatedTime;

                    var jd = new ParserHelper().GetAllParentDetailsForStage(journeyDetails, Int32.Parse(stage.Attribute("Position").Value));

                    dt3.Rows.Add(IDStages, jd.IDJourneies, jd.dutID, jd.ModId, Convert.ToInt16(BoaringStages), Convert.ToDateTime(DutySignOnDate), Convert.ToDateTime(time), dt, batchID);

                    var stagedet = new StageDetails();
                    stagedet.dutID = jd.dutID;
                    stagedet.ModId = jd.ModId;
                    stagedet.IDJourneies = jd.IDJourneies;
                    stagedet.iEndOfJourneyPos = jd.iEndOfJourneyPos;
                    stagedet.istartOfJourneyPos = jd.istartOfJourneyPos;
                    stagedet.StageID = IDStages;
                    stagedet.StagePosition = Int32.Parse(stage.Attribute("Position").Value);
                    stagedet.boardingStageID = Int32.Parse(stage.Descendants("BoardingStage").First().Value);
                    Mod = jd.ModId;
                    stageDetails.Add(stagedet);

                    IDStages++;
                }

                //insert stage details to DB
                using (SqlConnection con3 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con3))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Stage";

                        sqlBulkCopy.ColumnMappings.Add("id_Stage", "id_Stage");
                        sqlBulkCopy.ColumnMappings.Add("id_Journey", "id_Journey");
                        sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                        sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                        sqlBulkCopy.ColumnMappings.Add("int2_StageID", "int2_StageID");
                        sqlBulkCopy.ColumnMappings.Add("dat_StageDate", "dat_StageDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_StageTime", "dat_StageTime");
                        sqlBulkCopy.ColumnMappings.Add("dat_RecordMod", "dat_RecordMod");
                        sqlBulkCopy.ColumnMappings.Add("id_BatchNo", "id_BatchNo");
                        con3.Close();
                        con3.Open();

                        sqlBulkCopy.WriteToServer(dt3);
                        con3.Close();
                    }


                    //fill ins ticket details
                    foreach (var item1 in Itemss1)
                    {
                        string time1 = DateTime.ParseExact(item1.Time, "HHmmss", null).ToString("h:mm:ss");
                        string newDate = DutySignOnDate + " " + time1;

                        dt9.Rows.Add(IDStages, item1.InspectorNo, Convert.ToDateTime(newDate));    //to-do                           
                    }
                    //insert ins ticket details to DB
                    using (SqlConnection con4 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con4))
                        {
                            sqlBulkCopy.DestinationTableName = "dbo.Inspector";

                            sqlBulkCopy.ColumnMappings.Add("id_Stage", "id_Stage");
                            sqlBulkCopy.ColumnMappings.Add("id_InspectorID", "id_InspectorID");
                            sqlBulkCopy.ColumnMappings.Add("datTimeStamp", "datTimeStamp");
                            con4.Close();
                            con4.Open();
                            sqlBulkCopy.BatchSize = 30;
                            sqlBulkCopy.WriteToServer(dt9);
                            con4.Close();
                        }
                    }
                    LogwriterSuccess("ImportStages", path);
                    // now done with all stages, import all ticket sales
                    ImportTransactionDetails(stageDetails, doc, dbname);
                    // import all smart cards
                    ImportAllMJs(stageDetails, doc, dbname);



                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                char[] delimiterChars = { ';' };
                string[] email = Settings.Default.emails.Split(delimiterChars);

                foreach (var item in email)
                {

                    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                    mail.To.Add(item);


                    string files = Path.GetFileName(path);
                    mail.IsBodyHtml = true;
                    string body = "";
                    using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("[*FileName*]", files);
                    body = body.Replace("[*ClientName*]", dbname);
                    body = body.Replace("[*Error*]", ex.Message);
                    MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                    mail.From = address;
                    mail.Subject = "Ebus Import Error";
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    SmtpClient client = new SmtpClient();
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = false;
                    client.Host = "mail.ebussupplies.co.za";
                    client.Port = 25;

                    //Setup credentials to login to our sender email address ("UserName", "Password")
                    NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                    client.UseDefaultCredentials = true;
                    client.Credentials = credentials;

                    //Send the msg
                    client.Send(mail);

                }


                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Stage and Inspector into DB: " + dbname + "<br/>" + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                // now done with all stages, import all ticket sales
                ImportTransactionDetails(stageDetails, doc, dbname);
                // import all smart cards
                ImportAllMJs(stageDetails, doc, dbname);
                ////Does the update of NonRevenue

            }
        }
        /// <summary>
        /// here we update all transaction details for TICKET SALE
        /// </summary>
        /// <param name="allStagesDetails"></param>
        /// <param name="stagePos"></param>
        /// <param name="idJourney"></param>
        /// <param name="IDStage"></param>
        /// <param name="idDuty"></param>
        /// <param name="IDmodule"></param>
        /// <param name="BoardingStages"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private void ImportTransactionDetails(List<StageDetails> allStagesDetails, XDocument doc, string dbname)
        {
            try
            {

                // we will read all tickets in XML and insert wwith respective  parent details !
                var allTicketSaleInfo = new ParserHelper().GetTranseDetails(doc);

                string TripBal = "";
                string serial = "";
                int IDTranse = 0;
                int hexClassValue = 0;
                int second = 0;
                double seconds = 0;
                TimeSpan t;
                string formatedTime = "";
                string date = "";
                string issueDate = "";
                string issueTime = "";
                int stageno = 0;
                int HexClasValue = 0;
                int Fare = 0;

                DataTable dt4 = new DataTable();
                dt4.Columns.AddRange(new DataColumn[19] {
                    new DataColumn("id_Trans",typeof(int)),
                    new DataColumn("id_Stage", typeof(int)),
                    new DataColumn("id_Journey", typeof(int)),
                    new DataColumn("id_Duty",typeof(int)),
                    new DataColumn("id_Module",typeof(int)),
                    new DataColumn("str_LocationCode",typeof(string)),
                    new DataColumn("int2_BoardingStageID",typeof(int)),
                    new DataColumn("int2_AlightingStageID",typeof(int)),
                    new DataColumn("int2_Class",typeof(int)),
                    new DataColumn("int4_Revenue",typeof(int)),
                    new DataColumn("int4_NonRevenue",typeof(int)),
                    new DataColumn("int2_TicketCount",typeof(int)),
                    new DataColumn("int2_PassCount",typeof(int)),
                    new DataColumn("int2_Transfers",typeof(int)),
                    new DataColumn("dat_TransDate",typeof(DateTime)),
                    new DataColumn("dat_TransTime",typeof(DateTime)),
                    new DataColumn("str_SerialNumber",typeof(string)),
                    new DataColumn("int4_RevenueBal",typeof(int)),
                    new DataColumn("int4_TripBal",typeof(int))
                });

                int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));
                XDocument xdoc3 = doc; //imp: dont load XML each time , use reference passed in this function arguments
                XDocument xdoc4 = doc; //imp: same as above


                //this should be outside loop, get latest pk value from trans table
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    string sql = "SELECT TOP 1 id_Trans FROM Trans ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();

                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            IDTranse = Convert.ToInt32(val);
                            IDTranse = IDTranse + 1;
                        }
                        else if (val == null)
                        {
                            IDTranse = Convert.ToInt32(val);
                            IDTranse = IDTranse + 1;
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                foreach (var trans in allTicketSaleInfo) //loop all transactions begins here
                {
                    string Hex = trans.Element("TicketType").Value.ToString();
                    hexClassValue = int.Parse(Hex, NumberStyles.HexNumber);

                    if (hexClassValue == 11000)
                    {
                        continue;
                        //imp:since we dont want 11000 just continue with next node !!
                    }

                    var boardingStageDetails = new ParserHelper().GetAllParentDetailsForTransaction(allStagesDetails, Int32.Parse(trans.Attribute("Position").Value));

                    string stageN = trans.Element("StageNo").Value.ToString();
                    string Issuetime = trans.Element("IssueTime").Value.ToString();
                    string fares = trans.Element("Fare").Value.ToString();
                    string Issuedate = trans.Element("IssueDate").Value.ToString();
                    string TicketSerialNo = trans.Element("TicketSerialNo").Value.ToString();

                    //  string TicketType = item.TicketType.ToString();
                    stageno = Int32.Parse(stageN);
                    HexClasValue = hexClassValue;
                    second = Int32.Parse(Issuetime);
                    seconds = second;
                    //t = TimeSpan.FromSeconds(seconds);
                    //formatedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    //          t.Hours,
                    //          t.Minutes,
                    //           t.Seconds);
                    Fare = Int32.Parse(fares);
                    string dateLength = Issuedate;
                    string time = DateTime.ParseExact(Issuetime, "HHmmss", null).ToString("h:mm:ss tt");
                    if (dateLength.Length == 5)
                    {
                        date = "0" + Issuedate;
                        issueDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                        issueTime = issueDate + " " + time;
                    }
                    else
                    {
                        date = Issuedate;
                        issueDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                        issueTime = issueDate + " " + time;
                    }

                    if (HexClasValue == 17 || HexClasValue == 18 || HexClasValue == 33 || HexClasValue == 34 || HexClasValue == 65 || HexClasValue == 66 || HexClasValue == 34)
                    {
                        //Cash
                        // Mokopane , Ntambanana and Gauteng coaches 
                        if (HexClasValue == 17 || HexClasValue == 18 || HexClasValue == 33 || HexClasValue == 34 || HexClasValue == 65 || HexClasValue == 66 || HexClasValue == 34)
                        {
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(HexClasValue), Fare, 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), 0, 0, Convert.ToInt32(0));
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            tickets = tickets + 1;
                            continue; //imp: once we find right hexValue, add to datatble and proceed to next trans, we should write continue so the need not check below if-else conditions
                        }
                    }

                    if (HexClasValue == 9000)
                    {
                        continue;
                    }

                    else if (HexClasValue == 9000 || HexClasValue == 81 || HexClasValue == 82 || HexClasValue == 83 || HexClasValue == 84 || HexClasValue == 85 || HexClasValue == 86 || HexClasValue == 87 || HexClasValue == 115 || HexClasValue == 116 || HexClasValue == 144)
                    {
                        continue;


                    }
                } //imp:loop ends here

                //imp:insert all the above trans in one shot ! 
                using (SqlConnection con4 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con4))
                    {
                        //revenuebal  = 0 
                        sqlBulkCopy.DestinationTableName = "dbo.Trans";

                        sqlBulkCopy.ColumnMappings.Add("id_Trans", "id_Trans");
                        sqlBulkCopy.ColumnMappings.Add("id_Stage", "id_Stage");
                        sqlBulkCopy.ColumnMappings.Add("id_Journey", "id_Journey");
                        sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                        sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                        sqlBulkCopy.ColumnMappings.Add("str_LocationCode", "str_LocationCode");
                        sqlBulkCopy.ColumnMappings.Add("int2_BoardingStageID", "int2_BoardingStageID");
                        sqlBulkCopy.ColumnMappings.Add("int2_AlightingStageID", "int2_AlightingStageID");
                        sqlBulkCopy.ColumnMappings.Add("int2_Class", "int2_Class");
                        sqlBulkCopy.ColumnMappings.Add("int4_Revenue", "int4_Revenue");
                        sqlBulkCopy.ColumnMappings.Add("int4_NonRevenue", "int4_NonRevenue");
                        sqlBulkCopy.ColumnMappings.Add("int2_TicketCount", "int2_TicketCount");
                        sqlBulkCopy.ColumnMappings.Add("int2_PassCount", "int2_PassCount");
                        sqlBulkCopy.ColumnMappings.Add("int2_Transfers", "int2_Transfers");
                        sqlBulkCopy.ColumnMappings.Add("dat_TransDate", "dat_TransDate");
                        sqlBulkCopy.ColumnMappings.Add("dat_TransTime", "dat_TransTime");
                        sqlBulkCopy.ColumnMappings.Add("str_SerialNumber", "str_SerialNumber");
                        sqlBulkCopy.ColumnMappings.Add("int4_RevenueBal", "int4_RevenueBal");
                        sqlBulkCopy.ColumnMappings.Add("int4_TripBal", "int4_TripBal");
                        con4.Close();
                        con4.Open();
                        sqlBulkCopy.WriteToServer(dt4);
                        con4.Close();
                    }
                }
                LogwriterSuccess("ImportTrans", path);
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");

                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                char[] delimiterChars = { ';' };
                string[] email = Settings.Default.emails.Split(delimiterChars);

                foreach (var item in email)
                {

                    System.Net.Mail.MailMessage mail = new System.Net.Mail.MailMessage();
                    mail.To.Add(item);


                    string files = Path.GetFileName(path);
                    mail.IsBodyHtml = true;
                    string body = "";
                    using (StreamReader reader = new StreamReader(System.Windows.Forms.Application.StartupPath + @"\Email\htmlbody.html"))
                    {
                        body = reader.ReadToEnd();
                    }

                    body = body.Replace("[*FileName*]", files);
                    body = body.Replace("[*ClientName*]", dbname);
                    body = body.Replace("[*Error*]", ex.Message);
                    MailAddress address = new MailAddress("lourens@ebussupplies.co.za");
                    mail.From = address;
                    mail.Subject = "Ebus Import Error";
                    mail.Body = body;
                    mail.IsBodyHtml = true;
                    SmtpClient client = new SmtpClient();
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.EnableSsl = false;
                    client.Host = "mail.ebussupplies.co.za";
                    client.Port = 25;

                    //Setup credentials to login to our sender email address ("UserName", "Password")
                    NetworkCredential credentials = new NetworkCredential("lourens@ebussupplies.co.za", "ebus0117836833");
                    client.UseDefaultCredentials = true;
                    client.Credentials = credentials;

                    //Send the msg
                    client.Send(mail);

                }


                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Cash Trans into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }

            }
        }
        /// <summary>
        /// here we update all transaction details for SMART CARD
        /// </summary>
        /// <param name="allTransDetails"></param>
        /// <param name="path"></param>
        /// <param name="curStage"></param>
        /// <param name="nxtStage"></param>
        /// <param name="journeyEnd"></param>
        /// <param name="IDStage"></param>
        /// <param name="IDJourney"></param>
        /// <param name="IDDuty"></param>
        /// <param name="IDModule"></param>
        /// <param name="BoardingStages"></param>
        /// <returns></returns>
        private void ImportAllMJs(List<StageDetails> allStagesDetails, XDocument doc, string dbname)
        {
            try
            {
                TransNonRevenue = 0;
                var allSmartCardInfo = new ParserHelper().GetMJDetails(doc);

                string TripBal = "";
                string transfer = "";
                string serial = "";
                int IDTranse = 0;
                int PosIDTranse = 0;
                int EESN = 0;
                int hexClassValue = 0;
                int second = 0;
                double seconds = 0;
                TimeSpan t;
                string formatedTime = "";
                string date = "";
                string issueDate = "";
                string issueTime = "";
                int stageno = 0;
                int HexClasValue = 0;

                DataTable dt4 = new DataTable();
                DataTable dtPos = new DataTable();

                dt4.Columns.AddRange(new DataColumn[19] {
                    new DataColumn("id_Trans",typeof(int)),
                    new DataColumn("id_Stage", typeof(int)),
                    new DataColumn("id_Journey", typeof(int)),
                    new DataColumn("id_Duty",typeof(int)),
                    new DataColumn("id_Module",typeof(int)),
                    new DataColumn("str_LocationCode",typeof(string)),
                    new DataColumn("int2_BoardingStageID",typeof(int)),
                    new DataColumn("int2_AlightingStageID",typeof(int)),
                    new DataColumn("int2_Class",typeof(int)),
                    new DataColumn("int4_Revenue",typeof(int)),
                    new DataColumn("int4_NonRevenue",typeof(int)),
                    new DataColumn("int2_TicketCount",typeof(int)),
                    new DataColumn("int2_PassCount",typeof(int)),
                    new DataColumn("int2_Transfers",typeof(int)),
                    new DataColumn("dat_TransDate",typeof(DateTime)),
                    new DataColumn("dat_TransTime",typeof(DateTime)),
                    new DataColumn("str_SerialNumber",typeof(string)),
                    new DataColumn("int4_RevenueBal",typeof(string)),
                    new DataColumn("int4_TripBal",typeof(int))
                });

                dtPos.Columns.AddRange(new DataColumn[19] {
                    new DataColumn("id_PosTrans",typeof(int)),
                    new DataColumn("id_Stage", typeof(int)),
                    new DataColumn("id_Journey", typeof(int)),
                    new DataColumn("id_Duty",typeof(int)),
                    new DataColumn("id_Module",typeof(int)),
                    new DataColumn("str_LocationCode",typeof(string)),
                    new DataColumn("int2_BoardingStageID",typeof(int)),
                    new DataColumn("int2_AlightingStageID",typeof(int)),
                    new DataColumn("int2_Class",typeof(int)),
                    new DataColumn("int4_Revenue",typeof(int)),
                    new DataColumn("int4_NonRevenue",typeof(int)),
                    new DataColumn("int2_TicketCount",typeof(int)),
                    new DataColumn("int2_PassCount",typeof(int)),
                    new DataColumn("int2_Transfers",typeof(int)),
                    new DataColumn("dat_TransDate",typeof(DateTime)),
                    new DataColumn("dat_TransTime",typeof(DateTime)),
                    new DataColumn("str_SerialNumber",typeof(string)),
                    new DataColumn("int4_RevenueBal",typeof(string)),
                    new DataColumn("int4_TripBal",typeof(int))
                });

                int batchID = Convert.ToInt32(dtImportDateTime.ToString("yyyyMMdd"));
                XDocument xdoc3 = doc;
                XDocument xdoc4 = doc;//XDocument.Load(path);

                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    string sql = "SELECT TOP 1 id_Trans FROM Trans ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();

                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            IDTranse = Convert.ToInt32(val);
                            IDTranse = IDTranse + 1;
                        }
                        else if (val == null)
                        {
                            IDTranse = Convert.ToInt32(val);
                            IDTranse = IDTranse + 1;
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    string sql = "SELECT TOP 1 id_PosTrans FROM PosTrans ORDER BY 1 DESC";
                    SqlCommand cmd = new SqlCommand(sql, con);
                    try
                    {
                        con.Close();
                        con.Open();

                        object val = cmd.ExecuteScalar();
                        if (val != null)
                        {
                            PosIDTranse = Convert.ToInt32(val);
                            PosIDTranse = PosIDTranse + 1;
                        }
                        else if (val == null)
                        {
                            PosIDTranse = Convert.ToInt32(val);
                            PosIDTranse = PosIDTranse + 1;
                        }
                        con.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }

                int Journies = 0;
                int duties = 0;
                int modules = 0;

                foreach (var tran in allSmartCardInfo)
                {
                    var boardingStageDetails = new ParserHelper().GetAllParentDetailsForTransaction(allStagesDetails, Int32.Parse(tran.Attribute("Position").Value));

                    string Hex = tran.Element("TicketType").Value.ToString();
                    string stageN = tran.Element("StageNo").Value.ToString();
                    string Issuetime = tran.Element("IssueTime").Value.ToString();
                    string fares = tran.Element("Fare").Value.ToString();
                    string Issuedate = tran.Element("IssueDate").Value.ToString();
                    string TicketSerialNo = tran.Element("TicketSerialNo").Value.ToString();
                    string productData1 = tran.Element("Product1Data").Value.ToString();
                    //  string TicketType = item.TicketType.ToString();
                    hexClassValue = int.Parse(Hex, NumberStyles.HexNumber);

                    string newFare = "";
                    newFare = fares.Substring(0, 1);

                    if (Convert.ToInt16(newFare) == 0)
                    {
                        newFare = fares.Substring(1, 5);
                    }
                    else
                    {
                        newFare = fares.ToString();
                    }

                    if (hexClassValue == 11000)
                    {
                        continue;
                        //imp:since we dont want 11000 just continue with next node !!
                    }

                    stageno = Int32.Parse(stageN);
                    HexClasValue = hexClassValue;
                    second = Convert.ToInt32(Issuetime);
                    TripBal = productData1.Substring(2, 2);
                    SVTrip = productData1.Substring(6, 6);

                    trip = int.Parse(TripBal, System.Globalization.NumberStyles.HexNumber);
                    string dateLength = Issuedate;
                    string time = DateTime.ParseExact(Issuetime, "HHmmss", null).ToString("h:mm:ss tt");
                    if (dateLength.Length == 5)
                    {
                        date = "0" + Issuedate;
                        issueDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                        issueTime = issueDate + " " + time;
                    }
                    else
                    {
                        date = Issuedate;
                        issueDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                        issueTime = issueDate + " " + time;
                    }
                    Journies = boardingStageDetails.IDJourneies;
                    duties = boardingStageDetails.dutID;
                    modules = boardingStageDetails.ModId;
                    //stored value inserts here
                    if (HexClasValue == 10000 || HexClasValue == 10001 || HexClasValue == 10002 || HexClasValue == 10003 || HexClasValue == 10004 || HexClasValue == 10022 || HexClasValue == 10024)
                    {
                        string ESN = tran.Element("ESN").Value.ToString();
                        string productData = tran.Element("Product1Data").Value.ToString();


                        //TripBal = "";
                        serial = ESN;
                        //serial = "";
                        if (HexClasValue == 11000)
                        {
                            continue;
                        }
                        using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                        {
                            //string query = "Select int2_Class from Trans Where str_SerialNumber='" + multi.ESN + "'";
                            string query = "select int2_Class,int4_Revenue from Postrans where str_SerialNumber = '" + ESN + "' and int2_Class != 144 and int2_Class != 145 and int2_Class != 731 and int2_Class != 133 and int2_Class != 999 and int2_Class != 117 and int2_Class != 701 and int2_Class != 702 and int2_Class != 703 ";
                            SqlCommand cmd = new SqlCommand(query, con);

                            try
                            {
                                con.Close();
                                con.Open();
                                Int16 val = 0;
                                int val2 = 0;
                                //OLD
                                using (SqlDataReader da = cmd.ExecuteReader())
                                {
                                    while (da.Read())
                                    {
                                        val = da.GetInt16(0);
                                        val2 = da.GetInt32(1);
                                    }
                                }

                                DataTable dt = new DataTable();

                                //NEW
                                //using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                //{
                                //    da.Fill(dt);

                                //    foreach (DataRow dr in dt.Rows)
                                //    {
                                //        val = Int16.Parse(dr[0].ToString());
                                //        val2 = Int16.Parse(dr[1].ToString());
                                //    }

                                //}

                                EESN = Convert.ToInt32(val);
                                //only fare that is = 0
                                int Pos = 0;
                                int NewVal = 0;


                                trip = int.Parse(TripBal, NumberStyles.HexNumber);
                                //sv transactions recharge
                                if (HexClasValue == 10000)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(744), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(744), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    tickets = tickets + 1; PosIDTranse++;
                                    TransNonRevenue = TransNonRevenue + NewVal;
                                }
                                if (HexClasValue == 10001)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(745), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(745), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    tickets = tickets + 1; PosIDTranse++;
                                    TransNonRevenue = TransNonRevenue + NewVal;
                                }
                                if (HexClasValue == 10002)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(746), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(746), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);

                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    tickets = tickets + 1; PosIDTranse++;
                                    TransNonRevenue = TransNonRevenue + NewVal;

                                }
                                if (HexClasValue == 10003)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(747), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(747), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    tickets = tickets + 1; PosIDTranse++;
                                    TransNonRevenue = TransNonRevenue + NewVal;
                                }
                                if (HexClasValue == 10004)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(743), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(743), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    tickets = tickets + 1; PosIDTranse++;
                                    TransNonRevenue = TransNonRevenue + NewVal;
                                }
                                else if (HexClasValue == 10022)
                                {
                                    //89
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(741), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(741), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    tickets = tickets + 1; PosIDTranse++;
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                }
                                else if (HexClasValue == 10024)
                                {
                                    //90
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(742), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(742), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(SVTrip), 0);
                                    tickets = tickets + 1;
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++;
                                    PosIDTranse++;

                                }

                            }
                            catch (Exception ex)
                            {

                                DateTime dtime = DateTime.Now;
                                string file = "Error " + dtime.ToString("yyyyMMdd");

                                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
                                using (StreamWriter writer = new StreamWriter(errorPath, true))
                                {
                                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import SV into DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                                }
                            }
                        }
                    }
                    //////////////////////
                    //SV use
                    if (HexClasValue == 1017 || HexClasValue == 1033 || HexClasValue == 1018)
                    {
                        string ESN = tran.Element("ESN").Value.ToString();
                        string productData = tran.Element("Product1Data").Value.ToString();
                        //take last 4 digits of the total int value that is passed through

                        TripBal = productData.Substring(8, 4);
                        //TripBal = "";
                        serial = ESN;
                        //serial = "";
                        if (HexClasValue == 11000)
                        {
                            continue;
                        }

                        using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                        {
                            //string query = "Select int2_Class from Trans Where str_SerialNumber='" + multi.ESN + "'";
                            string query = "select int2_Class,int4_Revenue from Postrans where str_SerialNumber = '" + ESN + "' and int2_Class != 144 and int2_Class != 145 and int2_Class != 731 and int2_Class != 133 and int2_Class != 999 and int2_Class != 117 and int2_Class != 701 and int2_Class != 702 and int2_Class != 703 ";
                            SqlCommand cmd = new SqlCommand(query, con);

                            try
                            {
                                con.Close();
                                con.Open();
                                Int16 val = 0;
                                int val2 = 0;
                                //OLD
                                using (SqlDataReader da = cmd.ExecuteReader())
                                {
                                    while (da.Read())
                                    {
                                        val = da.GetInt16(0);
                                        val2 = da.GetInt32(1);
                                    }
                                }

                                DataTable dt = new DataTable();

                                //NEW
                                //using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                //{
                                //    da.Fill(dt);

                                //    foreach (DataRow dr in dt.Rows)
                                //    {
                                //        val = Int16.Parse(dr[0].ToString());
                                //        val2 = Int16.Parse(dr[1].ToString());
                                //    }

                                //}

                                EESN = Convert.ToInt32(val);
                                //only fare that is = 0
                                int Pos = 0;
                                int NewVal = 0;

                                trip = int.Parse(TripBal, NumberStyles.HexNumber);
                                if (EESN != 0)
                                {

                                    switch (HexClasValue)
                                    {
                                        // change hex values to 117 and 133
                                        // changed from 117 to 701 SV Adult
                                        case 1017:
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(701), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;

                                            break;
                                        // changed from 118 to 703 SV Pensioner
                                        case 1018:
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(703), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;

                                            break;
                                        // changed from 133 to 702 SV Child
                                        case 1033:
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(702), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        //For UGU added this value in
                                        case 1034:
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(704), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                    }
                                }

                                else if (EESN == 0)
                                {
                                    if (HexClasValue == 1017)
                                    {
                                        dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(701), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                        InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                        tickets = tickets + 1;
                                        TransNonRevenue = TransNonRevenue + NewVal;
                                    }
                                    if (HexClasValue == 1033)
                                    {
                                        dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(702), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                        InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                        tickets = tickets + 1;
                                        TransNonRevenue = TransNonRevenue + NewVal;

                                    }
                                    if (HexClasValue == 1018)
                                    {
                                        dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(703), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                        InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                        tickets = tickets + 1;
                                        TransNonRevenue = TransNonRevenue + NewVal;

                                    }
                                    if (HexClasValue == 1034)
                                    {
                                        dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(704), 0, TripBal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                                        InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                        tickets = tickets + 1;
                                        TransNonRevenue = TransNonRevenue + NewVal;

                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                                DateTime dtime = DateTime.Now;
                                string file = "Error " + dtime.ToString("yyyyMMdd");
                                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
                                using (StreamWriter writer = new StreamWriter(errorPath, true))
                                {
                                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Adult/Child/Pensioner into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                                }
                            }
                        }
                    }
                    // Scholar Use
                    else if (HexClasValue == 9001)
                    {
                        using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                        {
                            //string query = "Select int2_Class from Trans Where str_SerialNumber='" + multi.ESN + "'";
                            string query = "select int2_Class,int4_Revenue from Postrans where str_SerialNumber = '" + esn + "' and int2_Class != 144 and int2_Class != 145 and int2_Class != 731 and int2_Class != 133 and int2_Class != 999 and int2_Class != 117 and int2_Class != 701 and int2_Class != 702 and int2_Class != 703 ";
                            SqlCommand cmd = new SqlCommand(query, con);

                            con.Close();
                            con.Open();

                            Int16 val = 0;
                            int val2 = 0;
                            //OLD
                            using (SqlDataReader da = cmd.ExecuteReader())
                            {
                                while (da.Read())
                                {
                                    val = da.GetInt16(0);
                                    val2 = da.GetInt32(1);
                                }
                            }

                            DataTable dt = new DataTable();

                            //NEW
                            //using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            //{
                            //    da.Fill(dt);

                            //    foreach (DataRow dr in dt.Rows)
                            //    {
                            //        val = Int16.Parse(dr[0].ToString());
                            //        val2 = Int16.Parse(dr[1].ToString());
                            //    }

                            //}
                            transfer = productData1.Substring(6, 2);
                            transCancellation = productData1.Substring(4, 2);
                            EESN = Convert.ToInt32(val);
                            //only fare that is = 0
                            int Pos = 0;
                            int NewVal = 0;

                            int i3 = 0;
                            int i4 = 0;
                            i3 = Convert.ToInt32(transCancellation);
                            i4 = Convert.ToInt32(transfer);

                            // scholar MJ transactions
                            trip = int.Parse(TripBal, NumberStyles.HexNumber);
                            //////////////////////////////////////////////////
                            if (EESN != 0)
                            {
                                //customer with no transfer in operation (Mokopane,MarbleHall and Gauteng Coaches)
                                if (i3 == 0 && i4 == 0)
                                {
                                    switch (EESN)
                                    {
                                        case 721:
                                            NewVal = val2 / 10;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            //   dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            //  tickets = tickets + 1;
                                            PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;

                                        case 722:
                                            NewVal = val2 / 44;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            //   dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            //   tickets = tickets + 1;PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;


                                    }
                                }
                                //customer with pass count but no transfer(Ntambanana)
                                else if (i3 == 0 && i4 == 1)
                                {
                                    switch (EESN)
                                    {
                                        case 721:
                                            NewVal = val2 / 10;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            //  dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            //  tickets = tickets + 1;PosIDTranse++;
                                            PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;

                                        case 722:
                                            NewVal = val2 / 44;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            // dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            //   tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;


                                    }
                                }
                                //customer with transfer but no pass count(Ntambanana)
                                else if (i3 == 1 && i4 == 0)
                                {
                                    switch (EESN)
                                    {

                                        case 721:

                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            //   dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;

                                            //     TransNonRevenue = TransNonRevenue + NewVal;
                                            break;

                                        case 722:

                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            //  dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            //    tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                    }
                                }

                            }
                            else if (EESN == 0)
                            {
                                // if we dont find pos transaction for this esn number instead of inserting 0 we give it a value of 5
                                //Mokopane , Marblehall and Gautengcoaches has no transfers
                                if (i3 == 0 && i4 == 0)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    //dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++; PosIDTranse++;
                                    //  tickets = tickets + 1;
                                    TransNonRevenue = TransNonRevenue + 5;
                                }
                                //ntambanana mj trip cancellation with a transfer available
                                else if (i3 == 0 && i4 == 1)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    //  dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(997), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++; PosIDTranse++;
                                    //    tickets = tickets + 1;
                                    TransNonRevenue = TransNonRevenue + 5;
                                }
                                //transfer cancellation but not pass cancellation
                                else if (i3 == 1 && i4 == 0)
                                {
                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    //  dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(996), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                    IDTranse++; PosIDTranse++;
                                    //   tickets = tickets + 1;
                                    TransNonRevenue = TransNonRevenue + 0;
                                }
                            }

                        }
                    }
                    ///////////////////////
                    if (HexClasValue == 9000)
                    {
                        string ESN = tran.Element("ESN").Value.ToString();
                        string productData = tran.Element("Product1Data").Value.ToString();
                        TripBal = productData.Substring(2, 2);
                        //TripBal = "";
                        serial = ESN;
                        //serial = "";

                        if (HexClasValue == 11000)
                        {
                            continue;
                        }



                        ////////////////////////////

                        #region commented out


                        else if (HexClasValue == 9000)
                        {


                            using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                            {
                                //string query = "Select int2_Class from Trans Where str_SerialNumber='" + multi.ESN + "'";
                                string query = "select int2_Class,int4_Revenue from trans where str_SerialNumber = '" + ESN + "' and int2_Class != 144 and int2_Class != 145 and int2_Class != 731 and int2_Class != 133 and int2_Class != 999 and int2_Class != 117 and int2_Class != 701 and int2_Class != 702 and int2_Class != 703 and int2_Class != 741 and int2_Class != 742 and int2_Class != 743 and int2_Class != 745 and int2_Class != 746 and int2_Class != 747";
                                SqlCommand cmd = new SqlCommand(query, con);


                                try
                                {
                                    con.Close();
                                    con.Open();
                                    Int16 val = 0;
                                    int val2 = 0;
                                    //OLD
                                    using (SqlDataReader da = cmd.ExecuteReader())
                                    {
                                        while (da.Read())
                                        {
                                            val = da.GetInt16(0);
                                            val2 = da.GetInt32(1);
                                        }
                                    }

                                    DataTable dt = new DataTable();

                                    //NEW
                                    //using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                    //{
                                    //    da.Fill(dt);

                                    //    foreach (DataRow dr in dt.Rows)
                                    //    {
                                    //        val = Int16.Parse(dr[0].ToString());
                                    //        val2 = Int16.Parse(dr[1].ToString());
                                    //    }

                                    //}

                                    transfer = productData.Substring(6, 2);
                                    transCancellation = productData.Substring(4, 2);

                                    if (val != 0)
                                    {
                                        EESN = Convert.ToInt32(val);
                                    }
                                    else if (val == 0)
                                    {
                                        EESN = 0;
                                    }


                                    //only fare that is = 0
                                    int Pos = 0;
                                    int NewVal = 0;

                                    int i3 = 0;
                                    int i4 = 0;
                                    i3 = Convert.ToInt32(transCancellation);
                                    i4 = Convert.ToInt32(transfer);

                                    // Multi Journey Use
                                    trip = int.Parse(TripBal, NumberStyles.HexNumber);

                                    //////////////////////////////////////////////////
                                    if (EESN != 0)
                                    {
                                        //customer with no transfer in operation (Mokopane,MarbleHall and Gauteng Coaches)
                                        if (i3 == 0 && i4 == 0)
                                        {
                                            switch (EESN)
                                            {
                                                case 710:
                                                    NewVal = val2 / 6;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //  tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 711:
                                                    NewVal = val2 / 10;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //  tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 712:
                                                    NewVal = val2 / 12;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;

                                                case 713:
                                                    NewVal = val2 / 14;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //     tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 714:
                                                    NewVal = val2 / 40;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //     tickets = tickets + 1;
                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 715:
                                                    NewVal = val2 / 44;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //   tickets = tickets + 1;
                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 716:
                                                    NewVal = val2 / 48;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 717:
                                                    NewVal = val2 / 52;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //  tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 751:
                                                    NewVal = val2 / 10;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //  tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;


                                            }
                                        }
                                        //customer with pass count but no transfer(Ntambanana)
                                        else if (i3 == 0 && i4 == 1)
                                        {
                                            switch (EESN)
                                            {
                                                case 710:
                                                    NewVal = val2 / 6;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 711:
                                                    NewVal = val2 / 10;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 712:
                                                    NewVal = val2 / 12;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;

                                                case 713:
                                                    NewVal = val2 / 14;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //    tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 714:
                                                    NewVal = val2 / 40;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //   tickets = tickets + 1;
                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 715:
                                                    NewVal = val2 / 44;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //    tickets = tickets + 1;
                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 716:
                                                    NewVal = val2 / 48;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;
                                                case 717:
                                                    NewVal = val2 / 52;
                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //  tickets = tickets + 1;

                                                    TransNonRevenue = TransNonRevenue + NewVal;
                                                    break;

                                            }
                                        }
                                        //customer with transfer but no pass count(Ntambanana)
                                        // non revenue 0 for transfers
                                        else if (i3 == 1 && i4 == 0)
                                        {
                                            switch (EESN)
                                            {
                                                case 710:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    break;
                                                case 711:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    break;
                                                case 712:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    break;

                                                case 713:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                                    //   tickets = tickets + 1;

                                                    break;
                                                case 714:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //     tickets = tickets + 1;

                                                    break;
                                                case 715:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    IDTranse++;
                                                    //    tickets = tickets + 1;

                                                    break;
                                                case 716:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    //   tickets = tickets + 1;
                                                    IDTranse++;
                                                    break;
                                                case 717:

                                                    dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                                    InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                                    //    tickets = tickets + 1;
                                                    IDTranse++;
                                                    break;

                                            }
                                        }

                                    }
                                    else if (EESN == 0)
                                    {
                                        // if we dont find pos transaction for this esn number instead of inserting 0 we give it a value of 5
                                        //Mokopane , Marblehall and Gautengcoaches has no transfers
                                        if (i3 == 0 && i4 == 0)
                                        {
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            // tickets = tickets + 1;


                                            TransNonRevenue = TransNonRevenue + 5;
                                        }
                                        //ntambanana mj trip cancellation with a transfer available
                                        else if (i3 == 0 && i4 == 1)
                                        {
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, 500, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            //  tickets = tickets + 1;


                                            TransNonRevenue = TransNonRevenue + 5;
                                        }
                                        //transfer cancellation but not trip cancellation
                                        else if (i3 == 1 && i4 == 0)
                                        {
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(995), 0, 0, 0, 0, 1, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), serial, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            //  tickets = tickets + 1;

                                            TransNonRevenue = TransNonRevenue + 0;
                                        }
                                    }


                                }
                                catch (Exception ex)
                                {

                                    throw;
                                }
                            }
                            //continue taken out
                            // continue;
                        }

                        //POS Recharge
                        else if (HexClasValue == 83 || HexClasValue == 84 || HexClasValue == 85 || HexClasValue == 86 || HexClasValue == 87 || HexClasValue == 115 || HexClasValue == 116)
                        {
                            //take fares

                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(HexClasValue), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 1);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;

                        }

                        //POS Deposit
                        if (HexClasValue == 144)
                        {
                            //take fares
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, 1, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;
                        }
                        if (HexClasValue == 145)
                        {
                            //take fares
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, 1, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;
                        }
                        //MJ Deposit Gauteng Coaches
                        if (HexClasValue == 11001)
                        {
                            //take fares class value changes to 91
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;
                        }
                        //Staffpass deposit Gauteng Coaches
                        if (HexClasValue == 11005)
                        {
                            //take fares
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(734), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;
                        }
                        //storedvalue only deposit Gauteng Coaches
                        if (HexClasValue == 11006)
                        {
                            //take fares
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(733), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESN, Convert.ToInt32(trip), 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                            continue;
                        }
                    }
                        #endregion
                    //took the else out
                    if (HexClasValue == 9000 || HexClasValue == 81 || HexClasValue == 82 || HexClasValue == 83 || HexClasValue == 84 || HexClasValue == 85 || HexClasValue == 86 || HexClasValue == 87 || HexClasValue == 115 || HexClasValue == 116 || HexClasValue == 144 || HexClasValue == 145 || HexClasValue == 11001 || HexClasValue == 11002 || HexClasValue == 11005 || HexClasValue == 11006 || HexClasValue == 10005 || HexClasValue == 10008 || HexClasValue == 10006 || HexClasValue == 10011 || HexClasValue == 10012 || HexClasValue == 10014 || HexClasValue == 9001 || HexClasValue == 10022 || HexClasValue == 10024 || HexClasValue == 10004 || HexClasValue == 10000 || HexClasValue == 10001 || HexClasValue == 10002 || HexClasValue == 10003 || HexClasValue == 10023 || HexClasValue == 10018 || HexClasValue == 10019 || HexClasValue == 10007 || HexClasValue == 10010 || HexClasValue == 10013 || HexClasValue == 10016)
                    {

                        string ESNs = tran.Element("ESN").Value.ToString();
                        string productDatas = tran.Element("Product1Data").Value.ToString();

                        //issueDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                        //issueTime = issueDate + " " + formatedTime;
                        TripBal = productDatas.Substring(2, 2);

                        //TripBal = "";
                        serial = ESNs;
                        //serial = "";
                        Journies = boardingStageDetails.IDJourneies;
                        duties = boardingStageDetails.dutID;
                        modules = boardingStageDetails.ModId;
                        if (HexClasValue == 11000)
                        {
                            continue;
                        }
                        else if (HexClasValue == 9000)
                        {

                            using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                            {
                                string query = "select int2_Class,int4_Revenue from Postrans where str_SerialNumber = '" + ESNs + "' and int2_Class != 144 and int2_Class != 145 and int2_Class != 731 and int2_Class != 133 and int2_Class != 999 and int2_Class != 117 and int2_Class != 701 and int2_Class != 702 and int2_Class != 703 ";
                                SqlCommand cmd = new SqlCommand(query, con);

                                try
                                {
                                    con.Close();
                                    con.Open();
                                    Int16 val = 0;
                                    int val2 = 0;
                                    //OLD
                                    using (SqlDataReader da = cmd.ExecuteReader())
                                    {
                                        while (da.Read())
                                        {
                                            val = da.GetInt16(0);
                                            val2 = da.GetInt32(1);
                                        }
                                    }

                                    DataTable dt = new DataTable();

                                    //NEW
                                    //using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                                    //{
                                    //    da.Fill(dt);

                                    //    foreach (DataRow dr in dt.Rows)
                                    //    {
                                    //        val = Int16.Parse(dr[0].ToString());
                                    //        val2 = Int16.Parse(dr[1].ToString());
                                    //    }

                                    //}

                                    EESN = Convert.ToInt32(val);
                                    //only fare that is = 0
                                    int Pos = 0;
                                    int NewVal = 0;

                                    trip = int.Parse(TripBal, NumberStyles.HexNumber);

                                    switch (Pos)
                                    {
                                        case 710:
                                            NewVal = val2 / 6;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 711:
                                            NewVal = val2 / 10;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 712:
                                            NewVal = val2 / 12;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;

                                        case 713:
                                            NewVal = val2 / 14;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 714:
                                            NewVal = val2 / 40;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 715:
                                            NewVal = val2 / 44;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                            IDTranse++; PosIDTranse++;
                                            tickets = tickets + 1;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 716:
                                            NewVal = val2 / 48;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;
                                        case 717:
                                            NewVal = val2 / 52;
                                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(999), 0, NewVal, 0, 1, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, Convert.ToInt32(trip));
                                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname); IDTranse++;
                                            tickets = tickets + 1; PosIDTranse++;
                                            TransNonRevenue = TransNonRevenue + NewVal;
                                            break;

                                    }


                                }
                                catch (Exception ex)
                                {

                                    DateTime dtime = DateTime.Now;
                                    string file = "Error " + dtime.ToString("yyyyMMdd");
                                    string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";
                                    using (StreamWriter writer = new StreamWriter(errorPath, true))
                                    {
                                        writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import MJ into DB" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                                           "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                                        writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                                    }
                                }
                            }
                        }

                        else if (HexClasValue == 9100)
                        {
                            //take fares
                            // Scholar Use
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(998), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            tickets = tickets + 1;
                        }
                        else if (HexClasValue == 9002)
                        {
                            //take fares
                            // Scholar Use
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(994), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            tickets = tickets + 1;
                        }
                        //POS Recharge
                        else if (HexClasValue == 83 || HexClasValue == 84 || HexClasValue == 85 || HexClasValue == 86 || HexClasValue == 87 || HexClasValue == 115 || HexClasValue == 116)
                        {

                            if (HexClasValue == 83)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(710), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(710), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 84)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(711), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(711), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 85)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(712), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(712), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 86)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(713), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(713), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 87)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(714), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(714), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 115)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(715), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(715), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }
                            else if (HexClasValue == 116)
                            {
                                //take fares
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(717), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(717), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                                tickets = tickets + 1;
                            }

                        }
                        else if (HexClasValue == 10005 || HexClasValue == 10006 || HexClasValue == 10009 || HexClasValue == 10008 || HexClasValue == 10011 || HexClasValue == 10012 || HexClasValue == 10014 || HexClasValue == 10015 || HexClasValue == 10018 || HexClasValue == 10019 || HexClasValue == 10023 || HexClasValue == 10007 || HexClasValue == 10010 || HexClasValue == 10013 || HexClasValue == 10016)
                        {
                            //take fares and class values needs to change
                            if (HexClasValue == 10005)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(711), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(711), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            if (HexClasValue == 10006)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(721), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(721), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            if (HexClasValue == 10007)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(751), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(751), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10008)
                            {
                                //82
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(712), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(712), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;

                            }
                            else if (HexClasValue == 10009)
                            {
                                //82
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(723), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(723), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;

                            }
                            if (HexClasValue == 10010)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(752), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(752), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10011)
                            {
                                //84
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(714), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(714), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10012)
                            {
                                //84
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(722), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(722), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            if (HexClasValue == 10013)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(753), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(753), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            if (HexClasValue == 10016)
                            {
                                //81
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(754), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(754), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10023)
                            {
                                //85
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(713), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(713), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10014)
                            {
                                //86
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(716), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(716), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10015)
                            {
                                //86
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(724), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(724), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;
                            }
                            else if (HexClasValue == 10018)
                            {
                                //87
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(715), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(715), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;

                            }
                            else if (HexClasValue == 10019)
                            {
                                //88
                                dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(717), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(717), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, Convert.ToInt32(trip));
                                tickets = tickets + 1;
                                InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                                IDTranse++;
                                PosIDTranse++;

                            }

                            continue;


                        }
                        //POS Deposit
                        else if (HexClasValue == 144 || HexClasValue == 145)
                        {
                            //take fares
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            tickets = tickets + 1;
                        }
                        // MJ Deposit
                        else if (HexClasValue == 11001)
                        {
                            //take fares 91
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(731), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            continue;
                        }
                        else if (HexClasValue == 11003)
                        {
                            //take fares 91
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(735), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(735), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            continue;
                        }
                        //Staffpass deposit
                        else if (HexClasValue == 11005)
                        {
                            //take fares 92
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(734), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(734), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            continue;
                        }
                        //Scholar Deposit
                        else if (HexClasValue == 11002)
                        {
                            //take fares 92
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(732), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(732), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            continue;
                        }
                        //storedvalue only deposit
                        else if (HexClasValue == 11006)
                        {
                            //take fares 93
                            dt4.Rows.Add(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(733), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            dtPos.Rows.Add(PosIDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, "_", boardingStageDetails.boardingStageID, Convert.ToInt16(stageN), Convert.ToInt16(733), Convert.ToInt32(newFare), 0, 1, 0, 0, Convert.ToDateTime(issueDate), Convert.ToDateTime(issueTime), ESNs, 0, 0);
                            tickets = tickets + 1;
                            InsertGPSRecord(IDTranse, boardingStageDetails.StageID, boardingStageDetails.IDJourneies, boardingStageDetails.dutID, boardingStageDetails.ModId, dbname);
                            IDTranse++;
                            PosIDTranse++;
                            continue;
                        }
                    }
                }
                if (HexClasValue == 11000)
                {

                }
                else if (dt4.Rows.Count != 0)
                {
                    using (SqlConnection con4 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con4))
                        {
                            //revenuebal  = 0 
                            sqlBulkCopy.DestinationTableName = "dbo.Trans";

                            sqlBulkCopy.ColumnMappings.Add("id_Trans", "id_Trans");
                            sqlBulkCopy.ColumnMappings.Add("id_Stage", "id_Stage");
                            sqlBulkCopy.ColumnMappings.Add("id_Journey", "id_Journey");
                            sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                            sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                            sqlBulkCopy.ColumnMappings.Add("str_LocationCode", "str_LocationCode");
                            sqlBulkCopy.ColumnMappings.Add("int2_BoardingStageID", "int2_BoardingStageID");
                            sqlBulkCopy.ColumnMappings.Add("int2_AlightingStageID", "int2_AlightingStageID");
                            sqlBulkCopy.ColumnMappings.Add("int2_Class", "int2_Class");
                            sqlBulkCopy.ColumnMappings.Add("int4_Revenue", "int4_Revenue");
                            sqlBulkCopy.ColumnMappings.Add("int4_NonRevenue", "int4_NonRevenue");
                            sqlBulkCopy.ColumnMappings.Add("int2_TicketCount", "int2_TicketCount");
                            sqlBulkCopy.ColumnMappings.Add("int2_PassCount", "int2_PassCount");
                            sqlBulkCopy.ColumnMappings.Add("int2_Transfers", "int2_Transfers");
                            sqlBulkCopy.ColumnMappings.Add("dat_TransDate", "dat_TransDate");
                            sqlBulkCopy.ColumnMappings.Add("dat_TransTime", "dat_TransTime");
                            sqlBulkCopy.ColumnMappings.Add("str_SerialNumber", "str_SerialNumber");
                            sqlBulkCopy.ColumnMappings.Add("int4_RevenueBal", "int4_RevenueBal");
                            sqlBulkCopy.ColumnMappings.Add("int4_TripBal", "int4_TripBal");

                            con4.Close();
                            con4.Open();
                            sqlBulkCopy.WriteToServer(dt4);
                            con4.Close();

                        }
                    }
                }
                if (dtPos.Rows.Count != 0)
                {
                    using (SqlConnection con4 = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(con4))
                        {
                            //revenuebal  = 0 
                            sqlBulkCopy.DestinationTableName = "dbo.PosTrans";

                            sqlBulkCopy.ColumnMappings.Add("id_PosTrans", "id_PosTrans");
                            sqlBulkCopy.ColumnMappings.Add("id_Stage", "id_Stage");
                            sqlBulkCopy.ColumnMappings.Add("id_Journey", "id_Journey");
                            sqlBulkCopy.ColumnMappings.Add("id_Duty", "id_Duty");
                            sqlBulkCopy.ColumnMappings.Add("id_Module", "id_Module");
                            sqlBulkCopy.ColumnMappings.Add("str_LocationCode", "str_LocationCode");
                            sqlBulkCopy.ColumnMappings.Add("int2_BoardingStageID", "int2_BoardingStageID");
                            sqlBulkCopy.ColumnMappings.Add("int2_AlightingStageID", "int2_AlightingStageID");
                            sqlBulkCopy.ColumnMappings.Add("int2_Class", "int2_Class");
                            sqlBulkCopy.ColumnMappings.Add("int4_Revenue", "int4_Revenue");
                            sqlBulkCopy.ColumnMappings.Add("int4_NonRevenue", "int4_NonRevenue");
                            sqlBulkCopy.ColumnMappings.Add("int2_TicketCount", "int2_TicketCount");
                            sqlBulkCopy.ColumnMappings.Add("int2_PassCount", "int2_PassCount");
                            sqlBulkCopy.ColumnMappings.Add("int2_Transfers", "int2_Transfers");
                            sqlBulkCopy.ColumnMappings.Add("dat_TransDate", "dat_TransDate");
                            sqlBulkCopy.ColumnMappings.Add("dat_TransTime", "dat_TransTime");
                            sqlBulkCopy.ColumnMappings.Add("str_SerialNumber", "str_SerialNumber");
                            sqlBulkCopy.ColumnMappings.Add("int4_RevenueBal", "int4_RevenueBal");
                            sqlBulkCopy.ColumnMappings.Add("int4_TripBal", "int4_TripBal");

                            con4.Close();
                            con4.Open();
                            sqlBulkCopy.WriteToServer(dtPos);
                            con4.Close();

                        }
                    }
                }
                //Does the update of NonRevenue
                CalculateNonRevenue(ModuleID, dbname, TransNonRevenue, path);
                CalculateRevenue(ModuleID, dbname, TransNonRevenue, path);
                UPdateTransfers(Journies, ModuleID, dbname, path);
                UpdateJourney(Journies, duties, ModuleID, tickets, dbname);
                CalculatePasses(ModuleID, dbname, path);
                LogwriterSuccess("ImportTrans-smartcards", path);


            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import POS into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }
        /// <summary>
        /// waybill import for all modules
        /// </summary>
        /// <param name="path"></param>
        public void WaybillImport(string path, string dbname)
        {
            try
            {
                DataTable dt7 = new DataTable();
                dt7.Columns.AddRange(new DataColumn[8] {
                    new DataColumn("ModuleID", typeof(int)),
                    new DataColumn("dat_Start", typeof(DateTime)),
                    new DataColumn("dat_End",typeof(DateTime)),
                    new DataColumn("int4_Operator",typeof(int)),
                    new DataColumn("str8_BusID",typeof(string)),
                    new DataColumn("str6_EtmID",typeof(string)),
                    new DataColumn("int4_EtmGrandTotal",typeof(int)),
                    new DataColumn("int4_Revenue",typeof(int))
                });

                //ExtendedWaybill
                XDocument xdoc3 = XDocument.Load(path);

                var Items = xdoc3.Descendants("ExtendedWaybill").Select(x => new
                {

                    TSN = (int)x.Element("TSN"),
                    OperatorNumber = (int)x.Element("OperatorNumber"),
                    ModuleNumber = (int)x.Element("ModuleNumber"),
                    StartTime = (string)x.Element("StartTime"),
                    StartDate = (string)x.Element("StartDate"),
                    StopDate = (string)x.Element("StopDate"),
                    StopTime = (string)x.Element("StopTime"),
                    ETMNumber = (string)x.Element("ETMNumber"),
                    BusNumber = (string)x.Element("BusNumber"),
                    ETMTotal = (int)x.Element("ETMTotal"),
                    DutyRevenue = (int)x.Element("DutyRevenue"),

                }).ToList();


                foreach (var item in Items)
                {
                    int second = Convert.ToInt32(item.StartTime);
                    double seconds = second;
                    TimeSpan t = TimeSpan.FromSeconds(seconds);
                    string formatedTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                               t.Hours,
                               t.Minutes,
                               t.Seconds);

                    int second1 = Convert.ToInt32(item.StopTime);
                    double seconds1 = second;
                    TimeSpan t1 = TimeSpan.FromSeconds(seconds);
                    string formatedTime1 = string.Format("{0:D2}:{1:D2}:{2:D2}",
                               t.Hours,
                               t.Minutes,
                               t.Seconds);

                    string date = item.StartDate.ToString();
                    string date1 = item.StopDate.ToString();
                    string StartDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                    string StopDate = DateTime.ParseExact(date, "dMMyy", null).ToString("yyyy-MM-d");
                    string starttime = StartDate + " " + formatedTime;
                    string StopTime = StopDate + " " + formatedTime1;

                    int modId = 0;
                    modId = item.ModuleNumber;


                    dt7.Rows.Add(modId, Convert.ToDateTime(starttime), Convert.ToDateTime(StopTime), item.OperatorNumber, item.BusNumber, item.ETMNumber, item.ETMTotal, item.DutyRevenue);

                }
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn))
                    {
                        sqlBulkCopy.DestinationTableName = "dbo.Waybill";

                        sqlBulkCopy.ColumnMappings.Add("ModuleID", "ModuleID");
                        sqlBulkCopy.ColumnMappings.Add("dat_Start", "dat_Start");
                        sqlBulkCopy.ColumnMappings.Add("dat_End", "dat_End");
                        sqlBulkCopy.ColumnMappings.Add("int4_Operator", "int4_Operator");
                        sqlBulkCopy.ColumnMappings.Add("str8_BusID", "str8_BusID");
                        sqlBulkCopy.ColumnMappings.Add("str6_EtmID", "str6_EtmID");
                        sqlBulkCopy.ColumnMappings.Add("int4_EtmGrandTotal", "int4_EtmGrandTotal");
                        sqlBulkCopy.ColumnMappings.Add("int4_Revenue", "int4_Revenue");
                        conn.Close();
                        conn.Open();
                        sqlBulkCopy.WriteToServer(dt7);
                        conn.Close();
                    }
                }

                LogwriterSuccess("ImportWayBill", path);

            }
            catch (Exception ex)
            {

                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Waybill into DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                MoveErrorFile(path, dbname);
            }
        }
        /// <summary>
        /// Modules import for all modules
        /// </summary>
        /// <param name="path"></param>
        public void InsertModules(string ModuleESN, DateTime DTUsed, string dbname)
        {
            try
            {
                DataTable dtModules = new DataTable();

                dtModules.Columns.AddRange(new DataColumn[6]
                {
                    new DataColumn("str6_Modules",typeof(string)),
                    new DataColumn("int_ModuleStatus",typeof(int)),
                    new DataColumn("int_ModuleType",typeof(int)),
                    new DataColumn("int_ModuleSerial",typeof(int)),
                    new DataColumn("dat_FirstUsed",typeof(DateTime)),
                    new DataColumn("dat_LastUsed",typeof(DateTime))
                });
                object obj = null;

                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from Modules where str6_Modules=" + ModuleESN + "", conn))
                    {
                        obj = cmd.ExecuteScalar();
                    }
                    conn.Close();
                }

                if (obj != null)
                {

                }
                else if (obj == null)
                {
                    dtModules.Rows.Add(ModuleESN, 1, 0, 0, DTUsed, DTUsed);

                    using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                        {
                            conn.Close();
                            conn.Open();
                            sbc.DestinationTableName = "modules";

                            sbc.ColumnMappings.Add("str6_Modules", "str6_Modules");
                            sbc.ColumnMappings.Add("int_ModuleStatus", "int_ModuleStatus");
                            sbc.ColumnMappings.Add("int_ModuleType", "int_ModuleType");
                            sbc.ColumnMappings.Add("int_ModuleSerial", "int_ModuleSerial");
                            sbc.ColumnMappings.Add("dat_FirstUsed", "dat_FirstUsed");
                            sbc.ColumnMappings.Add("dat_LastUsed", "dat_LastUsed");
                            sbc.WriteToServer(dtModules);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Import Modules into DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }
        /// <summary>
        /// insert new drivers
        /// </summary>
        /// <param name="path"></param>
        public void InsertDrivers(string driverNumber, string dbname)
        {
            try
            {
                DataTable dtModules = new DataTable();

                dtModules.Columns.AddRange(new DataColumn[6]
                {
                    new DataColumn("int4_StaffID",typeof(int)),
                    new DataColumn("str50_StaffName",typeof(string)),
                    new DataColumn("bit_InUse",typeof(bool)),
                    new DataColumn("int4_StaffTypeID",typeof(int)),
                    new DataColumn("int4_StaffSubTypeID",typeof(int)),
                    new DataColumn("str4_LocationCode",typeof(string))
                });
                object obj = null;

                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select * from staff where int4_StaffID=" + driverNumber + "", conn))
                    {
                        obj = cmd.ExecuteScalar();
                    }
                    conn.Close();
                }

                if (obj != null)
                {

                }
                else if (obj == null)
                {
                    dtModules.Rows.Add(driverNumber, "New Driver - " + driverNumber, true, 1, 0, 0020);

                    using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                    {
                        using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                        {
                            conn.Close();
                            conn.Open();
                            sbc.DestinationTableName = "staff";

                            sbc.ColumnMappings.Add("int4_StaffID", "int4_StaffID");
                            sbc.ColumnMappings.Add("str50_StaffName", "str50_StaffName");
                            sbc.ColumnMappings.Add("bit_InUse", "bit_InUse");
                            sbc.ColumnMappings.Add("int4_StaffTypeID", "int4_StaffTypeID");
                            sbc.ColumnMappings.Add("int4_StaffSubTypeID", "int4_StaffSubTypeID");
                            sbc.ColumnMappings.Add("str4_LocationCode", "str4_LocationCode");
                            sbc.WriteToServer(dtModules);
                            conn.Close();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Insert drivers into DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }
        /// <summary>
        ///update modules in table
        /// </summary>
        /// <param name="path"></param>
        public void ModuleUpdate(int ModID, int DriverNumber, int Tickets, string dbname)
        {
            try
            {
                int drNo = 0;
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Update Module set int4_HdrModuleTickets =" + Tickets + " , int4_ModuleTickets = " + Tickets + "   Where id_Module=" + ModID + "", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select int4_StaffTypeID from Staff where int4_StaffID =" + DriverNumber + "", conn))
                    {
                        object obj = cmd.ExecuteScalar();
                        drNo = Convert.ToInt32(obj);
                    }
                    conn.Close();
                }
                byte b = Convert.ToByte(drNo);

                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Update Module set byt_ModuleType =" + b + " Where id_Module=" + ModID + "", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "update modules in DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

        }
        /// <summary>
        /// Update duty
        /// </summary>
        /// <param name="path"></param>
        public void DutyUpdate(int DutyID, int Tickets, int ModID, string dbname)
        {
            int driver = 0;
            try
            {
                // Select DriverNumber
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Select int4_OperatorID from duty   Where id_Duty =" + DutyID + " and id_Module=" + ModID + "", conn))
                    {
                        object obj = cmd.ExecuteScalar();
                        driver = Convert.ToInt32(obj);
                    }
                    conn.Close();
                }

                //ticket count comes from Mj Deposit and recharge
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("Update Duty set int4_DutyTickets =" + Tickets + "   Where id_Duty =" + DutyID + "and id_Module=" + ModID + "", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    conn.Close();
                }

                ModuleUpdate(ModID, driver, Tickets, dbname);
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Revenue update in DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }

        }
        /// <summary>
        /// Update Journey
        /// </summary>
        /// <param name="path"></param>
        public void UpdateJourney(int journeyID, int DutyID, int ModID, int Tickets1, string dbname)
        {
            //Journey number needs to be changed to datetime numbers
            //ticket count comes from Mj Deposit and recharge
            try
            {
                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand("select distinct(id_Journey) from Journey  where id_Module = " + ModID + "", conn))
                    {
                        object obj = cmd.ExecuteScalar();
                        int id = 0;
                        object obj1 = null;
                        int test = 0;
                        int Journeytickets = 0;
                        int Total = 0;
                        DataSet ds = new DataSet();
                        DataTable dt = new DataTable();
                        if (obj != null)
                        {
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);
                            if (ds.Tables[0].Rows.Count > 0)
                            {

                                dt = ds.Tables[0];

                                foreach (DataRow dr in dt.Rows)
                                {
                                    id = Convert.ToInt32(dr["id_Journey"]);

                                    using (SqlCommand cmd1 = new SqlCommand("Select Count(int2_TicketCount) from trans where id_Module =" + ModID + " and id_Journey =" + id + "and int2_TicketCount = 1", conn))
                                    {

                                        obj1 = cmd1.ExecuteScalar();
                                        if (obj1 == DBNull.Value)
                                        {
                                            Journeytickets = 0;
                                        }
                                        else if (obj1 != null)
                                        {
                                            Journeytickets = Convert.ToInt16(obj1);
                                        }
                                    }

                                    Total = Total + Journeytickets;

                                    if (tickets != 0)
                                    {

                                        using (SqlCommand cmdJourney = new SqlCommand("Update journey set int4_JourneyTickets = " + Journeytickets + "  where id_Module =" + ModID + "and id_Journey =" + id + "", conn))
                                        {
                                            cmdJourney.ExecuteNonQuery();
                                        }
                                        using (SqlCommand cmdDuty = new SqlCommand("Update Duty set int4_DutyTickets = " + Total + "  where id_Module =" + ModID + "", conn))
                                        {
                                            cmdDuty.ExecuteNonQuery();
                                        }
                                        using (SqlCommand cmd1Module = new SqlCommand("Update Module set int4_ModuleTickets = " + Total + ", int4_HdrModuleTickets = " + Total + "  where id_Module =" + ModID + "", conn))
                                        {
                                            cmd1Module.ExecuteNonQuery();
                                        }
                                    }
                                    else
                                    {
                                        using (SqlCommand cmdDuty = new SqlCommand("Update Duty set int4_DutyTickets = " + Total + "  where id_Module =" + ModID + "", conn))
                                        {
                                            cmdDuty.ExecuteNonQuery();
                                        }
                                        using (SqlCommand cmd1Module = new SqlCommand("Update Module set int4_ModuleTickets = " + Total + ", int4_HdrModuleTickets = " + Total + "  where id_Module =" + ModID + "", conn))
                                        {
                                            cmd1Module.ExecuteNonQuery();
                                        }
                                    }
                                }

                            }

                        }

                    }
                    conn.Close();
                }

                //DutyUpdate(DutyID, Tickets, ModID, dbname);
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Update Journey tickets in DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }
        /// <summary>
        /// waybill import for all modules
        /// </summary>
        /// <param name="path"></param>
        public void InsertGPSRecord(int iDTrans, int IDStage, int IDJourney, int IDDuty, int IDModule, string dbname)
        {
            //Card Event needs to be added after stages has been added
            try
            {

                DataTable dtGPS = new DataTable();

                dtGPS.Columns.AddRange(new DataColumn[14] {

                new DataColumn("id_Trans",typeof(int)),
                new DataColumn("id_Stage",typeof(int)),
                new DataColumn("id_Journey",typeof(int)),
                new DataColumn("id_Duty",typeof(int)),
                new DataColumn("id_Module",typeof(int)),
                new DataColumn("int4_Type",typeof(int)),
                new DataColumn("int1_SatCount",typeof(int)),
                new DataColumn("r_Lat",typeof(int)),
                new DataColumn("r_Long",typeof(int)),
                new DataColumn("int4_Distance",typeof(int)),
                new DataColumn("int2_Speed",typeof(int)),
                new DataColumn("int4_UpCount",typeof(int)),
                new DataColumn("int4_DownCount",typeof(int)),
                new DataColumn("dat_GPSDateTime",typeof(DateTime))
            });

                dtGPS.Rows.Add(iDTrans, IDStage, IDJourney, IDDuty, IDModule, 55, 116, 0, 0, 0, 111, 0, 0, Convert.ToDateTime(newModuleOutTime));

                using (SqlConnection conn = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    conn.Close();
                    conn.Open();
                    using (SqlBulkCopy sbc = new SqlBulkCopy(conn))
                    {

                        sbc.DestinationTableName = "dbo.GPSRecord";

                        sbc.ColumnMappings.Add("id_Trans", "id_Trans");
                        sbc.ColumnMappings.Add("id_Stage", "id_Stage");
                        sbc.ColumnMappings.Add("id_Journey", "id_Journey");
                        sbc.ColumnMappings.Add("id_Duty", "id_Duty");
                        sbc.ColumnMappings.Add("id_Module", "id_Module");
                        sbc.ColumnMappings.Add("int4_Type", "int4_Type");
                        sbc.ColumnMappings.Add("int1_SatCount", "int1_SatCount");
                        sbc.ColumnMappings.Add("r_Lat", "r_Lat");
                        sbc.ColumnMappings.Add("r_Long", "r_Long");
                        sbc.ColumnMappings.Add("int4_Distance", "int4_Distance");
                        sbc.ColumnMappings.Add("int2_Speed", "int2_Speed");
                        sbc.ColumnMappings.Add("int4_UpCount", "int4_UpCount");
                        sbc.ColumnMappings.Add("int4_DownCount", "int4_DownCount");
                        sbc.ColumnMappings.Add("dat_GPSDateTime", "dat_GPSDateTime");
                        sbc.WriteToServer(dtGPS);
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Insert GPS record into DB: " + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ModID"></param>
        /// <returns></returns>
        /// 
        public int CalculateNonRevenue(int ModID, string dbname, int NonRevenue, string path)
        {
            try
            {
                int NonRevs = 0;
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    con.Close();
                    con.Open();
                    DutyNonRevenues = NonRevenue;
                    //
                    int GrandNonRevenue = 0;

                    using (SqlCommand cmd = new SqlCommand("select distinct(id_Journey) from Journey  where id_Module =" + ModID + "", con))
                    {
                        object obj = cmd.ExecuteScalar();
                        int id = 0;
                        object obj1 = null;
                        object obj2 = null;
                        int rev = 0;
                        Int16 trans = 0;
                        if (obj != null)
                        {
                            ////////
                            DataSet ds = new DataSet();
                            DataTable dt = new DataTable();
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);
                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                dt = ds.Tables[0];
                                foreach (DataRow dr in dt.Rows)
                                {
                                    id = Convert.ToInt32(dr["id_Journey"]);

                                    using (SqlCommand cmd1 = new SqlCommand("Select sum(int4_NonRevenue) from trans where id_Module =" + ModID + " and id_Journey =" + id + "", con))
                                    {
                                        obj1 = cmd1.ExecuteScalar();
                                        if (obj1 == DBNull.Value)
                                        {
                                            rev = 0;
                                        }
                                        else if (obj1 != null)
                                        {
                                            rev = Convert.ToInt32(obj1);
                                        }
                                    }
                                    using (SqlCommand cmd1 = new SqlCommand("Select Count(int2_Transfers) from trans where id_Module =" + ModID + " and id_Journey =" + id + "and int2_Transfers = 1", con))
                                    {
                                        obj2 = cmd1.ExecuteScalar();
                                        if (obj2 == DBNull.Value)
                                        {
                                            trans = 0;
                                        }
                                        else if (obj2 != null)
                                        {
                                            trans = Convert.ToInt16(obj2);
                                        }
                                    }
                                    using (SqlCommand cmd1 = new SqlCommand("Update Journey set int4_JourneyNonRevenue =" + rev + ",int4_JourneyTransfer = " + trans + " where id_Module =" + ModID + " and id_Journey=" + id + "", con))
                                    {
                                        cmd1.ExecuteNonQuery();
                                    }
                                    GrandNonRevenue = GrandNonRevenue + rev;
                                }
                            }
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("Update Duty set int4_DutyNonRevenue =" + GrandNonRevenue + " where id_Module =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand("Update Module set int4_ModuleNonRevenue =" + GrandNonRevenue + " where id_Module   =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Calculate non-Revenue into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                MoveErrorFile(path, dbname);
            }
            return ModID;
        }
        /// <summary>
        /// waybill import for all modules
        /// </summary>
        /// <param name="path"></param>
        public int CalculateRevenue(int ModID, string dbname, int NonRevenue, string path)
        {
            try
            {
                int NonRevs = 0;
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    con.Close();
                    con.Open();
                    int DutyRevenues = 0;

                    // building the connection
                    using (SqlCommand cmd = new SqlCommand("select distinct(id_Journey) from Journey  where id_Module =" + ModID + "", con))
                    {
                        object obj = cmd.ExecuteScalar();
                        int id = 0;
                        object obj1 = null;
                        object obj2 = null;
                        int rev = 0;
                        Int16 trans = 0;
                        if (obj != null)
                        {
                            ////////
                            DataSet ds = new DataSet();
                            DataTable dt = new DataTable();
                            //declare sqp data adapter
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                dt = ds.Tables[0];
                                foreach (DataRow dr in dt.Rows)
                                {
                                    id = Convert.ToInt32(dr["id_Journey"]);

                                    using (SqlCommand cmd1 = new SqlCommand("Select sum(int4_Revenue) from trans where id_Module =" + ModID + " and id_Journey =" + id + "", con))
                                    {
                                        obj1 = cmd1.ExecuteScalar();
                                        if (obj1 == DBNull.Value)
                                        {
                                            rev = 0;
                                        }
                                        else if (obj1 != null)
                                        {
                                            rev = Convert.ToInt32(obj1);
                                        }
                                    }
                                    using (SqlCommand cmd1 = new SqlCommand("Update Journey set int4_JourneyRevenue =" + rev + " where id_Module =" + ModID + " and id_Journey=" + id + "", con))
                                    {
                                        cmd1.ExecuteNonQuery();
                                    }
                                    DutyRevenues = DutyRevenues + rev;
                                }
                            }
                            ///////
                        }
                    }
                    using (SqlCommand cmd = new SqlCommand("Update Duty set int4_DutyRevenue =" + DutyRevenues + " where id_Module =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    using (SqlCommand cmd = new SqlCommand("Update Module set int4_ModuleRevenue =" + DutyRevenues + " where id_Module   =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Calculate Revenue into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                MoveErrorFile(path, dbname);
            }
            return ModID;
        }
        /// <summary>
        /// waybill import for all modules
        /// </summary>
        /// <param name="path"></param>
        public int UPdateTransfers(int JourneyID, int ModID, string dbname, string path)
        {
            try
            {
                int MainTrans = 0;
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    con.Close();
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("select distinct(id_Journey) from trans  where id_Module = " + ModID + "", con))
                    {

                        object obj = cmd.ExecuteScalar();
                        int id = 0;
                        object obj1 = null;
                        object obj2 = null;
                        Int16 Trans = 0;


                        if (obj != DBNull.Value)
                        {
                            DataSet ds = new DataSet();
                            DataTable dt = new DataTable();
                            //declare sqp data adapter
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                dt = ds.Tables[0];
                                // loops through all journey ids to update correctly 
                                foreach (DataRow dr in dt.Rows)
                                {
                                    //Journey id per rotation
                                    id = Convert.ToInt32(dr["id_journey"]);
                                    //selects sum of all transfers for specific ID
                                    using (SqlCommand cmd1 = new SqlCommand("Select sum(int2_Transfers) from trans where id_Module =" + ModID + " and id_journey =" + id + "", con))
                                    {
                                        obj1 = cmd1.ExecuteScalar();
                                        if (obj1 == DBNull.Value)
                                        {
                                            //if no transfers then set it to 0
                                            Trans = 0;
                                        }
                                        else if (obj1 != DBNull.Value)
                                        {
                                            // assigns the value
                                            Trans = Convert.ToInt16(obj1);
                                        }
                                    }
                                    using (SqlCommand cmd1 = new SqlCommand("Update Journey set int4_JourneyTransfer =" + Trans + " where id_Module =" + ModID + " and id_journey=" + id + "", con))
                                    {
                                        //updates the table
                                        cmd1.ExecuteNonQuery();
                                    }
                                    MainTrans = MainTrans + Trans;
                                }
                            }
                        }

                    }
                    //updates Duty
                    using (SqlCommand cmd = new SqlCommand("Update Duty set int4_DutyTransfer =" + MainTrans + " where id_Module =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    //update Module
                    using (SqlCommand cmd = new SqlCommand("Update Module set int4_ModuleTransfer =" + MainTrans + " where id_Module   =" + ModID + "", con))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "update Transfers into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                MoveErrorFile(path, dbname);
            }
            return JourneyID;
        }
        /// <summary>
        /// waybill import for all modules
        /// </summary>
        /// <param name="path"></param>
        public void CalculatePasses(int modID, string dbname, string path)
        {
            //declares values
            int JourneyPassCount = 0;
            int JourneyNewPassCount = 0;
            try
            {
                using (SqlConnection con = ConnectToSql(Settings.Default.Server, Settings.Default.User, Settings.Default.Password, dbname))
                {
                    con.Close();
                    con.Open();

                    //select distinct journey ID's
                    using (SqlCommand cmd = new SqlCommand("select distinct(id_Journey) from Journey  where id_Module =" + modID + "", con))
                    {
                        //gets the values from the queies
                        object obj = cmd.ExecuteScalar();
                        int id = 0;
                        object obj1 = null;
                        object obj2 = null;

                        Int16 Journeytrans = 0;
                        //checks if the object is null
                        if (obj != DBNull.Value)
                        {
                            ////////
                            DataSet ds = new DataSet();
                            DataTable dt = new DataTable();

                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);

                            if (ds.Tables[0].Rows.Count > 0)
                            {
                                dt = ds.Tables[0];

                                foreach (DataRow dr in dt.Rows)
                                {
                                    //gets the id value per rotation
                                    id = Convert.ToInt32(dr["id_Journey"]);
                                    //counts transfers
                                    using (SqlCommand cmd1 = new SqlCommand("Select Count(int2_Transfers) from trans where id_Module =" + modID + " and id_Journey =" + id + "and int2_Transfers = 1", con))
                                    {

                                        obj2 = cmd1.ExecuteScalar();
                                        if (obj2 == DBNull.Value)
                                        {
                                            Journeytrans = 0;
                                        }
                                        else if (obj2 != DBNull.Value)
                                        {
                                            Journeytrans = Convert.ToInt16(obj2);
                                        }
                                    }
                                    //select sum passes
                                    using (SqlCommand cmd1 = new SqlCommand("select sum(int2_PassCount) from trans where id_Module =" + modID + "and id_Journey =" + id, con))
                                    {
                                        obj1 = cmd1.ExecuteScalar();
                                        if (obj1 == DBNull.Value)
                                        {
                                            JourneyPassCount = 0;
                                        }
                                        else if (obj1 != DBNull.Value)
                                        {
                                            JourneyPassCount = Convert.ToInt32(obj1);

                                        }
                                    }
                                    //calculate passes if there is any transfers
                                    //  JourneyPassCount = JourneyPassCount - Journeytrans;

                                    JourneyNewPassCount = JourneyNewPassCount + JourneyPassCount;
                                    using (SqlCommand cmdJourney = new SqlCommand("Update journey set int4_JourneyPasses = " + JourneyPassCount + "  where id_Module =" + modID + "and id_Journey =" + id + "", con))
                                    {
                                        //updates table
                                        cmdJourney.ExecuteNonQuery();
                                    }
                                }
                                //updates tables
                                if (Journeytrans != 0)
                                {
                                    using (SqlCommand cmdDuty = new SqlCommand("Update Duty set int4_DutyPasses = " + JourneyNewPassCount + "  where id_Module =" + modID + "", con))
                                    {
                                        cmdDuty.ExecuteNonQuery();
                                    }
                                    using (SqlCommand cmd1Module = new SqlCommand("Update Module set int4_ModulePasses = " + JourneyNewPassCount + ", int4_HdrModulePasses = " + JourneyNewPassCount + "  where id_Module =" + modID + "", con))
                                    {
                                        cmd1Module.ExecuteNonQuery();
                                    }
                                }
                                //update tables
                                else
                                {
                                    using (SqlCommand cmdDuty = new SqlCommand("Update Duty set int4_DutyPasses = " + JourneyNewPassCount + "  where id_Module =" + modID + "", con))
                                    {
                                        cmdDuty.ExecuteNonQuery();
                                    }
                                    using (SqlCommand cmd1Module = new SqlCommand("Update Module set int4_ModulePasses = " + JourneyNewPassCount + ", int4_HdrModulePasses = " + JourneyNewPassCount + "  where id_Module =" + modID + "", con))
                                    {
                                        cmd1Module.ExecuteNonQuery();
                                    }
                                }
                            }
                        }
                    }
                    con.Close();
                }
            }
            catch (Exception ex)
            {
                DateTime dtime = DateTime.Now;
                string file = "Error " + dtime.ToString("yyyyMMdd");
                string errorPath = Settings.Default.LogPath + @"\" + file + @".txt";

                using (StreamWriter writer = new StreamWriter(errorPath, true))
                {
                    writer.WriteLine("Message :" + ex.Message + "<br/>" + "Calculate Passes into DB:" + dbname + Environment.NewLine + "StackTrace :" + ex.StackTrace +
                       "" + Environment.NewLine + "Date :" + DateTime.Now.ToString());
                    writer.WriteLine(Environment.NewLine + "-----------------------------------------------------------------------------" + Environment.NewLine);
                }
                MoveErrorFile(path, dbname);
            }
        }
        #endregion
    }
}