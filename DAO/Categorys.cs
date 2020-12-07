using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace letscard_cafe.DAO
{
    class Categorys
    {
        public enum Ca : int
        {
            CUSTOM = 0,
            BASKETBALL = 1,
            BASEBALL = 2,
            FOOTBALL = 3,
            ETC = 4
        }
        private int CUSTOM;
        private int BASKETBALL;
        private int BASEBALL;
        private int FOOTBALL;
        private int ETC;

        DataTable table;
        static public string ToString() => "Categorys";

        public Categorys(DataTable _table)
        {
            table = _table;
            
            CUSTOM = Convert.ToInt32(table.Rows[(int)Ca.CUSTOM]["category"]);
            BASKETBALL = Convert.ToInt32(table.Rows[(int)Ca.BASKETBALL]["category"]);
            BASEBALL = Convert.ToInt32(table.Rows[(int)Ca.BASEBALL]["category"]);
            FOOTBALL = Convert.ToInt32(table.Rows[(int)Ca.FOOTBALL]["category"]);
            ETC = Convert.ToInt32(table.Rows[(int)Ca.ETC]["category"]);
        }
        public string ConvertCategory(int ca)
        {
            string category = "";
            switch(ca)
            {
                case 40:
                    category = "100원경매";
                    break;
                case 42:
                    category = "농구";
                    break;
                case 43:
                    category = "야구";
                    break;
                case 44:
                    category = "축구";
                    break;
                case 45:
                    category = "기타";
                    break;
            }
            return category;
        }
        public int Custom { get { return CUSTOM; } }
        public int Basketball { get { return BASKETBALL; } }
        public int Baseball { get { return BASEBALL; } }
        public int Football { get { return FOOTBALL; } }
        public int Etc { get { return ETC; } }
    }
}
