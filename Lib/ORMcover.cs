using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Oracle.ManagedDataAccess.Client;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace letscard_cafe.Lib
{
    class ORMcover
    {
        private string dao;
        OracleDataAdapter adapter;
        OracleCommandBuilder builder;
        DataSet data;

        Database db;

        public override string ToString()
        {
            return this.dao;
        }
        private bool DataEmpty(DataSet _data)
        {
            foreach (DataTable table in _data.Tables)
                if (table.Rows.Count != 0)
                    return false;

            return true;
        }
        public ORMcover()
        {
            db = new Database();

            this.adapter = new OracleDataAdapter();
            this.builder = new OracleCommandBuilder(this.adapter);
            this.data = new DataSet();
        }
        public bool Select(string _dao, string query)
        {
            this.dao = _dao;


            Console.WriteLine(query);
            try
            {
                adapter.SelectCommand = new OracleCommand(query, db.Conn);
                adapter.Fill(data, _dao);
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        public bool Update(string _dao)
        {
            this.dao = _dao;

            try
            {
                DataSet updatedSet = data.GetChanges(DataRowState.Modified);
                if(updatedSet.HasErrors)
                {
                    MessageBox.Show("update set error");
                }
                else
                {
                    this.AdapterUpdate(updatedSet, _dao);
                }
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        public bool Insert(string _dao)
        {
            this.dao = _dao;

            try
            {
                this.AdapterUpdate(this.data, _dao);
                return true;
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }
        private void AdapterUpdate(DataSet _data, string _dao)
        {
            this.adapter.Update(_data, _dao);
            this.data.AcceptChanges();
        }
        public DataTable getTable(string _dao)
        {
            try
            {
                if (!data.Tables.Contains(_dao))
                    return null;

                return data.Tables[_dao];
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return null;
            }
        }
    }
}
