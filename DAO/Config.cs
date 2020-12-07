using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Windows.Forms;

namespace letscard_cafe.DAO
{
    class Config
    {
        DataTable table;
        DataRow row;

        static public string ToString() => "Config";
        public Config(DataTable _table)
        {
            table = _table;
            row = table.Rows[0];
        }
        public string Title
        {
            get
            {
                return row["title"].ToString();
            }
            set
            {
                row.BeginEdit();
                row["title"] = value;
                row.EndEdit();
            }
        }
        public string Content
        {
            get
            {
                return row["content"].ToString();
            }
            set
            {
                row.BeginEdit();
                row["content"] = value;
                row.EndEdit();
            }
        }
        public int CommonBid
        {
            get
            {
                return Convert.ToInt32(row["common_bid"]);
            }
            set
            {
                row.BeginEdit();
                row["common_bid"] = value;
                row.EndEdit();
            }
        }
        public int CustomBid
        {
            get
            {
                return Convert.ToInt32(row["custom_bid"]);
            }
            set
            {
                row.BeginEdit();
                row["custom_bid"] = value;
                row.EndEdit();
            }
        }
        public int Cafeid
        {
            get
            {
                return Convert.ToInt32(row["cafe_id"]);
            }
        }
        public string ConvertTitle(string _title, int num, int category, string closing_at)
        {
            //string title = "{type}경매 - {x}번 {month}월{day}일({days}) 경매 종료 시각 {hour}:{minute}";
            DateTime date = DateTime.Parse(closing_at);
            string title = _title;

            //{type}
            if (category == 40)
                title = title.Replace("{type}", "100원 ");
            else
                title = title.Replace("{type}", "");

            //{x}
            title = title.Replace("{x}", num.ToString());

            //{month}
            title = title.Replace("{month}", date.ToString("MM"));

            //{day}
            title = title.Replace("{day}", date.ToString("dd"));

            //{days}
            title = title.Replace("{days}", this.GetDays(date));

            //{hour}
            title = title.Replace("{hour}", date.ToString("HH"));

            //{minute}
            title = title.Replace("{minute}", date.ToString("mm"));
            return title;
        }
        public string ConvertContent(string _content, int category, string closing_at)
        {
            //string content = "이전과 경매내용과 동일하게 댓글은 네이버 시 계 기준이며 경매종료시간까지 최고가를댓글로 쓰신분께 낙찰되는 방식입니다" +
            //   "입찰단위({bid})낙찰후 배송비는 낙찰자 부담입니다. 이점 확인후 입찰부탁드립니다. (배송비 {fee} 별도 입니다)경매 종료시각{month}월{day}일({days}) 경매 종료 시각 {hour}:{minute} ({month}월{day}일({days}) 댓글마감 {-hour}:{-minute})";

            DateTime date = DateTime.Parse(closing_at);
            DateTime reply_close_date = date.AddMinutes(-1);
            string content = _content;

            //{bid}
            if (category == 40)
                content = content.Replace("{bid}", "100원");
            else
                content = content.Replace("{bid}", "1천원");

            //{fee}
            content = content.Replace("{fee}", "3천원");

            //{month}
            content = content.Replace("{month}", date.ToString("MM"));

            //{day}
            content = content.Replace("{day}", date.ToString("dd"));

            //{days}
            content = content.Replace("{days}", this.GetDays(date));

            //{hour}
            content = content.Replace("{hour}", date.ToString("HH"));

            //{minute}
            content = content.Replace("{minute}", date.ToString("mm"));

            //{-hour}
            content = content.Replace("{-hour}", reply_close_date.ToString("HH"));

            //{-minute}
            content = content.Replace("{-minute}", reply_close_date.ToString("mm"));

            return content;
        }
        private string GetDays(DateTime dt)
        {
            string days = "";

            switch (dt.DayOfWeek)
            {
                case DayOfWeek.Monday:
                    days = "월";
                    break;
                case DayOfWeek.Tuesday:
                    days = "화";
                    break;
                case DayOfWeek.Wednesday:
                    days = "수";
                    break;
                case DayOfWeek.Thursday:
                    days = "목";
                    break;
                case DayOfWeek.Friday:
                    days = "금";
                    break;
                case DayOfWeek.Saturday:
                    days = "토";
                    break;
                case DayOfWeek.Sunday:
                    days = "일";
                    break;
            }
            return days;
        }
    }
}
