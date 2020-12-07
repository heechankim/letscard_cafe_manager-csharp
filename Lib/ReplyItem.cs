using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace letscard_cafe.Lib
{
    class ReplyItem
    {
        private int index;
        private string nickname;
        private string content;
        private int bid;
        private DateTime created_at;
        private bool is_reply;
        private bool is_canceled;
        private bool is_bidded;
        public int Index { get => index; set => index = value; }
        public string Nickname { get => nickname; set => nickname = value; }
        public string Content { get => content; set => content = value; }
        public int Bid { get => bid; set => bid = value; }
        public DateTime Created_at { get => created_at; set => created_at = value; }
        public bool IsReply { get => is_reply; set => is_reply = value; }
        public bool IsCanceled { get => is_canceled; set => is_canceled = value; }
        public bool IsBidded { get => is_bidded; set => is_bidded = value; }

        public ReplyItem(int index, string _nickname, string _content, DateTime _created_at, bool _is_reply = false, bool _is_canceled = false, bool _is_bidded = false)
        {
            Index = index;
            Nickname = _nickname;
            Content = _content;
            Created_at = _created_at;
            IsReply = _is_reply;
            IsCanceled = _is_canceled;
            IsBidded = _is_bidded;
        }
    }
}
