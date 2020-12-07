using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Windows.Forms;

namespace letscard_cafe
{
    class Database
    {
        private string connection_string;
        private OracleConnection conn;

        public Database()
        {
            try
            {
                connection_string = "User Id=chan; Password=cr6812hc; " +
                "Data Source=(DESCRIPTION = (ADDRESS = (PROTOCOL = TCP)" +
                "(HOST = letscard.ciyidr0ylaug.ap-northeast-2.rds.amazonaws.com)" +
                "(PORT = 1521)) (CONNECT_DATA = (SERVER = DEDICATED)(SERVICE_NAME = orcl)));";

                conn = new OracleConnection(this.connection_string);
            }
            catch(DataException de)
            {
                this.conn = null;
                MessageBox.Show(de.Message);
            }
        }
        ~Database()
        {
            this.conn.Close();
        }
        public OracleConnection Conn
        {
            get
            {
                if (this.conn == null)
                    return null;

                return this.conn;
            }
        }
        
    }
}
