using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;

namespace EbusImport
{
    class SqlConnections
    {
        public SqlConnection ConnectToSql()
        {
            try
            {
                SqlConnection conn = null;
                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

                string aDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ebus";
                if (File.Exists(aDir + "\\EbusConfig.ini") == true)
                {
                    EbusImport.IniFiles ini = new EbusImport.IniFiles(aDir + "\\EbusConfig.ini");

                    csb.DataSource = ini.IniReadValue("EbusCostingConnection", "ServerName");
                    csb.UserID = ini.IniReadValue("EbusCostingConnection", "Username");
                    csb.Password = ini.IniReadValue("EbusCostingConnection", "Password");
                    csb.InitialCatalog = ini.IniReadValue("EbusCostingConnection", "Database");
                }

                conn = new SqlConnection(csb.ConnectionString);
                conn.Open();

                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Error Occured: " + ex.ToString());
                return null;
            }
        }

        public SqlConnection ConnectToSql(string sDataSource, string sUserID, string sPassword)
        {
            try
            {
                SqlConnection conn = null;
                SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();

                csb.DataSource = sDataSource;
                csb.UserID = sUserID;
                csb.Password = sPassword;
                //csb.InitialCatalog = "DR01";

                conn = new SqlConnection(csb.ConnectionString);
                conn.Open();

                return conn;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Error Occured: " + ex.ToString());
                return null;
            }
        }
    }
}
