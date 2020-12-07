using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace letscard_cafe.DAO
{
    class Pictures
    {
        DataTable table;
        static public string ToString() => "Pictures";
        public Pictures(DataTable _table)
        {
            this.table = _table;
        }
        public void InsertLine(int articleid, string week, int upload_order,
            string path, string name)
        {
            DataRow row = this.table.NewRow();

            row["articleid"] = articleid;
            row["week"] = week;
            row["upload_order"] = upload_order;
            row["path"] = path;
            row["name"] = name;
            row["created_at"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            this.table.Rows.Add(row);
        }
        public void Clear()
        {
            table.Clear();
            table = null;
        }
        public string[] getPictureInfo(int articleid, string week)
        {
            string[] picture_info = new string[4];
            try
            {
                DataRow[] rows = table.Select("articleid='" + articleid.ToString() + "' AND week='" + week + "'", "upload_order ASC");
                if(rows.Length == 2)
                {
                    picture_info[0] = rows[0]["path"].ToString();
                    picture_info[1] = rows[0]["name"].ToString();
                    picture_info[2] = rows[1]["path"].ToString();
                    picture_info[3] = rows[1]["name"].ToString();
                }
                else
                {
                    Console.WriteLine("Error in Pictures.getPictureInfo [error - index out of range] : [week = {0}], [articleid = {1}]", week, articleid.ToString());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }
            return picture_info;
        }
    }
}
