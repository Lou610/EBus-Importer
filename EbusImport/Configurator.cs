using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
//using System.Threading.Tasks;
using System.Windows.Forms;

namespace EbusImport
{
    public partial class Configurator : Form
    {
        public Configurator()
        {
            InitializeComponent();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string aDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Ebus";
            if (Directory.Exists(aDir) == false)
            {

                Directory.CreateDirectory(aDir);
            }

            EbusImport.IniFiles ini = new EbusImport.IniFiles(aDir + "\\EbusConfig.ini");
            ini.IniWriteValue("EbusCostingConnection", "ServerName", tbxServer.Text);
            ini.IniWriteValue("EbusCostingConnection", "Username", tbxUserName.Text);
            ini.IniWriteValue("EbusCostingConnection", "Password", tbxPassword.Text);
            ini.IniWriteValue("EbusCostingConnection", "Database", cmbDatabase.SelectedItem.ToString());

            MessageBox.Show("Connected successfully.", "Succesful Connection Established", MessageBoxButtons.OK, MessageBoxIcon.Information);

            this.Close();
        }

        private void Configurator_Load(object sender, EventArgs e)
        {
            string aDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\Coop";
            if (File.Exists(aDir + "\\NRFConfig.ini") == true)
            {
                EbusImport.IniFiles ini = new EbusImport.IniFiles(aDir + "\\NRFConfig.ini");
                //ini.IniReadValue("WorksiteConnection", "Username");

                tbxServer.Text = ini.IniReadValue("EbusCostingConnection", "ServerName");
                tbxUserName.Text = ini.IniReadValue("EbusCostingConnection", "Username");
                tbxPassword.Text = ini.IniReadValue("EbusCostingConnection", "Password");

                SqlConnections sqlConnections = new SqlConnections();
                SqlConnection sqlConnection = sqlConnections.ConnectToSql(tbxServer.Text.Trim(), tbxUserName.Text.Trim(), tbxPassword.Text.Trim());

                // Need to populate the combobox first
                PopulateDatabases(sqlConnection);

                string sDatabase = ini.IniReadValue("EbusCostingConnection", "Database");
                cmbDatabase.SelectedItem = sDatabase;
            }

        }

        private void cmbDatabase_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDatabase.Items.Count == 0)
            {
                SqlConnections sqlConnections = new SqlConnections();
                SqlConnection sqlConnection = sqlConnections.ConnectToSql(tbxServer.Text.Trim(), tbxUserName.Text.Trim(), tbxPassword.Text.Trim());

                PopulateDatabases(sqlConnection);
            }
        }

        private void PopulateDatabases(SqlConnection sqlConnection)
        {
            try
            {
                if (sqlConnection != null)
                {
                    DataTable databases = sqlConnection.GetSchema("Databases");
                    foreach (DataRow database in databases.Rows)
                    {
                        String databaseName = database.Field<String>("database_name");

                        cmbDatabase.Items.Add(databaseName);
                    }

                    btnSave.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Unable to connect to the database server. Please check you connection details and try again", "An Error Occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An Error Occured: " + ex.ToString());
            }
            finally
            {
                if (sqlConnection != null)
                {
                    sqlConnection.Close();
                }
            }
        }
    }
}
