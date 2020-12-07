using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace letscard_cafe.DAO
{
    class Auctions
    {
        DataTable table;
        private int last;
        private int current;
        static public string ToString() => "Auctions";
        public Auctions(DataTable _table)
        {
            table = _table;
            if (table.Rows.Count == 0)
            {
                this.last = 0;
            }
            else
            {
                this.last = table.Rows.Count;
            }

            current = last + 1;
        }
        public void Clear()
        {
            table.Clear();
            table = null;
        }
        public DataTable Table
        {
            get { return table; }
        }
        public void InsertLine(int articleid, int num, string week, int category, 
            string seller, string item_type, int upload_fee, string closing_at)
        {
            Console.WriteLine("From Auctions.InsertLine [result] : [number = {0}], [week = {1}], [category = {2}], [closing_at = {3}]", num.ToString(), week, category.ToString(), closing_at);
            DataRow row = this.table.NewRow();

            row["articleid"] = articleid;
            row["num"] = num;
            row["week"] = week;
            row["category"] = category;
            row["seller"] = seller;
            row["item_type"] = item_type;
            row["upload_fee"] = upload_fee;
            row["CLOSING_AT"] = DateTime.Parse(closing_at).ToString("yyyy-MM-dd HH:mm:00");
            row["CREATED_AT"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            this.table.Rows.Add(row);
        }
        public int Last { get { return last; } }
        public int Next { get { return last + 1; } }
        public int Number()
        {
            return current++;
        }
        public void NumberMinusOne()
        {
            current -= 1;
        }
        public void RollBackNumber()
        {
            current = last + 1;
        }
    }
}
