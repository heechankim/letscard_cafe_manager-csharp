using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace letscard_cafe.DAO
{
    class Biddings
    {
        DataTable table;
        static public string ToString() => "Biddings";
        public Biddings(DataTable _table)
        {
            table = _table;
        }
        public void Clear()
        {
            table.Clear();
            table = null;
        }
        public DataTable Table => table;

        public void InsertLine(int articleid, string week, string bidder, int bid)
        {
            Console.WriteLine("From Biddings.InsertLine [result] : [articleid = {0}], [week = {1}], [bidder = {2}], [bid = {3}]", articleid.ToString(), week, bidder, bid);
            DataRow row = this.table.NewRow();

            row["articleid"] = articleid;
            row["week"] = week;
            row["bidder"] = bidder;
            row["bid"] = bid;
            row["CREATED_AT"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            this.table.Rows.Add(row);
        }
    }
}
