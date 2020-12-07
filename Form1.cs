using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using letscard_cafe.Lib;
using letscard_cafe.DAO;
using System.Threading;
using System.Diagnostics;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace letscard_cafe
{
    public partial class MainForm : Form
    {
        ORMcover orm;

        Config config;
        Categorys categorys;

        Auctions auctions;
        Pictures pictures;

        Biddings biddings;

        UploadConfig upload_config;
        Closing closing;

        private string auction_week;
        public MainForm()
        {
            InitializeComponent();
            orm = new ORMcover();
            
            orm.Select(Config.ToString(), "SELECT * FROM " + Config.ToString());
            config = new Config(orm.getTable(Config.ToString()));

            orm.Select(Categorys.ToString(), "SELECT * FROM " + Categorys.ToString());
            categorys = new Categorys(orm.getTable(Categorys.ToString()));

            this.auctions = null;

            Config_Print();
            Category_Print();
        }
      
        // Config Logic
        public void Config_Print()
        {
            AuctionTitle.Text = config.Title;
            AuctionContent.Text = config.Content;
            AuctionCommonBid.Text = config.CommonBid.ToString();
            AuctionCustomBid.Text = config.CustomBid.ToString();
        }
        private void ConfigApplyButton_Click(object sender, EventArgs e)
        {
            config.Title = AuctionTitle.Text;
            config.Content = AuctionContent.Text;
            config.CommonBid = Convert.ToInt32(AuctionCommonBid.Text);
            config.CustomBid = Convert.ToInt32(AuctionCustomBid.Text);
            orm.Update(Config.ToString());
        }
        public void Category_Print()
        {
            AuctionCategoryList.Items.Add(new ListViewItem(new string[] {"100원 경매", categorys.Custom.ToString()}));
            AuctionCategoryList.Items.Add(new ListViewItem(new string[] { "농구", categorys.Basketball.ToString() }));
            AuctionCategoryList.Items.Add(new ListViewItem(new string[] { "야구", categorys.Baseball.ToString() }));
            AuctionCategoryList.Items.Add(new ListViewItem(new string[] { "축구", categorys.Football.ToString() }));
            AuctionCategoryList.Items.Add(new ListViewItem(new string[] { "기타", categorys.Etc.ToString() }));
        }

        private void ConfigButton_Click(object sender, EventArgs e)
        {
            LoginBrowser.Visible = false;
            ConfigPanel.Visible = true;

        }
        private void LoginButton_Click(object sender, EventArgs e)
        {
            ConfigPanel.Visible = false;
            LoginBrowser.Visible = true;
            LoginBrowser.Url = new Uri("http://letscard.ddns.net/naverlogin");
        }
        
        // Upload Logic
        private const int IMAGE_COUNT = 2;
        private void FolderSelect_Click(object sender, EventArgs e)
        {
            if(auctions == null)
            {
                AuctionWeek.Focus();
                MessageBox.Show("경매주차가 선택되지 않았습니다.");
                return;
            }

            FolderBrowserDialog open = new FolderBrowserDialog();
            if(open.ShowDialog() == DialogResult.OK)
            {
                var FileNames = Directory.GetFiles(open.SelectedPath)
                    .Select(file => new { FileName = file, FileNumber = long.Parse(Path.GetFileNameWithoutExtension(file)) })
                    .OrderBy(data => data.FileNumber);

                int flag = 1;
                ListViewItem item = new ListViewItem(auctions.Number().ToString());

                foreach (var file in FileNames)
                {
                    if (flag == 1)
                    {
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add(Path.GetDirectoryName(file.FileName));
                        item.SubItems.Add(Path.GetFileName(file.FileName));
                        flag++;
                    }
                    else if(flag == 2)
                    {
                        item.SubItems.Add(Path.GetDirectoryName(file.FileName));
                        item.SubItems.Add(Path.GetFileName(file.FileName));
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        UploadList.Items.Add(item);
                        flag = 1;
                        item = new ListViewItem(auctions.Number().ToString());
                    }
                }
                auctions.NumberMinusOne();
                if (flag == 2)
                {
                    MessageBox.Show("리스트에 추가되지 않은 사진이 존재합니다. : ");
                }
            }
        }
        private void FilesSelect_Click(object sender, EventArgs e)
        {
            if (auctions == null)
            {
                AuctionWeek.Focus();
                MessageBox.Show("경매주차가 선택되지 않았습니다.");
                return;
            }

            OpenFileDialog open = new OpenFileDialog();
            open.Multiselect = true;

            if(open.ShowDialog() == DialogResult.OK)
            {

                int flag = 1;
                ListViewItem item = new ListViewItem(auctions.Number().ToString());

                foreach (string file in open.FileNames)
                {
                    if (flag == 1)
                    {
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add(Path.GetDirectoryName(file));
                        item.SubItems.Add(Path.GetFileName(file));
                        flag++;
                    }
                    else if (flag == 2)
                    {
                        item.SubItems.Add(Path.GetDirectoryName(file));
                        item.SubItems.Add(Path.GetFileName(file));
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        item.SubItems.Add("");
                        UploadList.Items.Add(item);
                        flag = 1;
                        item = new ListViewItem(auctions.Number().ToString());
                    }
                }
                auctions.NumberMinusOne();
                if (flag == 2)
                {
                    MessageBox.Show("리스트에 추가되지 않은 사진이 존재합니다. : ");
                }
            }
        }
        // Server Sync Button to Upload Sequence
        private void UploadServerButton_Click(object sender, EventArgs e)
        {   
            if (UploadList.Items.Count != 0)
            {
                DialogResult caution;
                caution = MessageBox.Show("리스트가 초기화 됩니다.", "주의", MessageBoxButtons.YesNo);
                if (caution == DialogResult.No)
                    return;

                UploadList.Items.Clear();

                this.auctions.Clear();
                this.auctions = null;

                this.pictures.Clear();
                this.pictures = null;
            }


            auction_week = AuctionWeek.Text;

            try
            {
                orm.Select(Auctions.ToString(), "SELECT * FROM " + Auctions.ToString() + " WHERE week = \'" + this.auction_week + "\' ORDER BY num ASC");
                this.auctions = new Auctions(orm.getTable(Auctions.ToString()));

                orm.Select(Pictures.ToString(), "SELECT * FROM " + Pictures.ToString() + " WHERE week = \'" + this.auction_week + "\'");
                this.pictures = new Pictures(orm.getTable(Pictures.ToString()));
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }

            ///////////////////////////////////////////////////////////////////

            foreach(DataRow row in this.auctions.Table.Rows)
            {
                ListViewItem item = new ListViewItem(row["num"].ToString());
                string[] pic_info = pictures.getPictureInfo(Convert.ToInt32(row["articleid"].ToString()), row["week"].ToString());

                item.SubItems.Add(row["category"].ToString());
                item.SubItems.Add(categorys.ConvertCategory(Convert.ToInt32(row["category"].ToString())));
                if (pic_info != null)
                {
                    item.SubItems.Add(pic_info[0]);
                    item.SubItems.Add(pic_info[1]);
                    item.SubItems.Add(pic_info[2]);
                    item.SubItems.Add(pic_info[3]);
                }
                else
                {
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                    item.SubItems.Add("");
                }
                item.SubItems.Add(DateTime.Parse(row["closing_at"].ToString()).ToString("yyyy-MM-dd HH:mm:00"));
                item.SubItems.Add(row["seller"].ToString());
                item.SubItems.Add(row["item_type"].ToString());
                item.SubItems.Add(row["upload_fee"].ToString());
                item.ForeColor = Color.Gray;
                UploadList.Items.Add(item);
            }

            /*
            // get table column name
            string[] col = orm.getTable(Auctions.ToString()).Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToArray();
            foreach (string name in col)
                Console.WriteLine(name);
            */
            UploadSyncPanel.Visible = true;
            UploadSyncText1.Text = "동기화완료";
            UploadSyncText2.Text = "선택 경매주차 : " + auction_week;
            UploadSyncText3.Text = "다음 경매번호 : " + this.auctions.Next;
        }
        // Control Upload List
        private void ClearSelect_Click(object sender, EventArgs e)
        {
            if (auctions == null)
            {
                AuctionWeek.Focus();
                MessageBox.Show("경매주차가 선택되지 않았습니다.");
                return;
            }

            UploadList.Items.Clear();
            auctions.RollBackNumber();
        }
        private void UploadList_Click(object sender, EventArgs e)
        {
            PreviewClear();
            
            if(UploadList.SelectedItems.Count == 1)
            {
                UploadList_CreatePreviewSingleLine();
            }
            else if(UploadList.SelectedItems.Count > 1)
            {
                upload_config = new UploadConfig();
                upload_config.Count = UploadList.SelectedItems.Count;

                upload_config.Start = UploadList.SelectedItems[0].Index;
                UploadConfigStartIndex.Text = UploadList.SelectedItems[0].Index.ToString();
                UploadConfigStartNumber.Text = UploadList.SelectedItems[0].SubItems[0].Text;

                upload_config.End = UploadList.SelectedItems[upload_config.Count - 1].Index;
                UploadConfigEndIndex.Text = UploadList.SelectedItems[upload_config.Count - 1].Index.ToString();
                UploadConfigEndNumber.Text = UploadList.SelectedItems[upload_config.Count - 1].SubItems[0].Text;
            }
        }
        private void UploadList_CreatePreviewSingleLine()
        {
            ListViewItem select = UploadList.FocusedItem;
            PreviewNumber.Text = select.SubItems[0].Text + "번 경매 [경매주차 : " + auction_week + "]" + " - " + select.SubItems[2].Text;
            try
            {
                Image image1 = Image.FromFile(Path.Combine(select.SubItems[3].Text, select.SubItems[4].Text));
                image1.RotateFlip(RotateFlipType.Rotate90FlipNone);
                Image image2 = Image.FromFile(Path.Combine(select.SubItems[5].Text, select.SubItems[6].Text));
                image2.RotateFlip(RotateFlipType.Rotate90FlipNone);
                PreviewImage1.Image = image1;
                PreviewImage2.Image = image2;
            }
            catch (Exception de)
            {
                //MessageBox.Show(de.Message);
                return;
            }

            // if category and closing_at is defined then
            if(select.SubItems[1].Text != "" && select.SubItems[7].Text != "")
            {
                PreviewTitle.Text = config.ConvertTitle(config.Title, Convert.ToInt32(select.SubItems[0].Text), Convert.ToInt32(select.SubItems[1].Text), select.SubItems[7].Text);
                PreviewContent.Text = config.ConvertContent(config.Content, Convert.ToInt32(select.SubItems[1].Text), select.SubItems[7].Text);
            }

            upload_config = new UploadConfig();
            upload_config.Count = UploadList.SelectedItems.Count;
            upload_config.Start = select.Index;
            UploadConfigStartIndex.Text = select.Index.ToString();
            UploadConfigStartNumber.Text = select.SubItems[0].Text;

            upload_config.End = select.Index;
            UploadConfigEndIndex.Text = select.Index.ToString();
            UploadConfigEndNumber.Text = select.SubItems[0].Text;
            
        }
        private void PreviewClear()
        {
            PreviewNumber.Text = "";
            PreviewImage1.Image = null;
            PreviewImage2.Image = null;
            PreviewTitle.Text = "";
            PreviewContent.Text = "";
        }
        private void UploadConfigApply_Click(object sender, EventArgs e)
        {
            if(UploadConfigCategory1.Checked == true)
            {
                upload_config.Category = categorys.Basketball;
                upload_config.Bid = config.CommonBid;
            }
            else if(UploadConfigCategory2.Checked == true)
            {
                upload_config.Category = categorys.Baseball;
                upload_config.Bid = config.CommonBid;
            }
            else if (UploadConfigCategory3.Checked == true)
            {
                upload_config.Category = categorys.Football;
                upload_config.Bid = config.CommonBid;
            }
            else if (UploadConfigCategory4.Checked == true)
            {
                upload_config.Category = categorys.Etc;
                upload_config.Bid = config.CommonBid;
            }
            else if (UploadConfigCategory5.Checked == true)
            {
                upload_config.Category = categorys.Custom;
                upload_config.Bid = config.CustomBid;
            }

            for (int i = upload_config.Start; i <= upload_config.End; i++)
            {
                if (upload_config.Category != 0 && UploadConfigCategoryCheckBox.Checked == true)
                {
                    UploadList.Items[i].SubItems[1].Text = upload_config.Category.ToString();
                    UploadList.Items[i].SubItems[2].Text = categorys.ConvertCategory(upload_config.Category);
                }

                if(UploadConfigDateTimeCheckBox.Checked == true)
                    UploadList.Items[i].SubItems[7].Text = UploadConfigDate.Value.ToString("yyyy-MM-dd") + " " + UploadConfigTime.Value.AddMinutes(i - upload_config.Start).ToString("HH:mm:00");

                if (UploadConfigSeller.Text != "")
                    UploadList.Items[i].SubItems[8].Text = UploadConfigSeller.Text;

                if(UploadConfigItemType.SelectedItem != null)
                {
                    string type = UploadConfigItemType.SelectedItem.ToString();
                    UploadList.Items[i].SubItems[9].Text = type;
                    if (type == "단체싱글" || type == "내카드")
                        UploadList.Items[i].SubItems[10].Text = "500";
                    else
                        UploadList.Items[i].SubItems[10].Text = "1000";
                }

                upload_config.Date = UploadConfigDate.Value;
                upload_config.Time = UploadConfigTime.Value.AddMinutes(i - upload_config.Start);
            }
            
        }
        private void UploadConfigDate_CloseUp(object sender, EventArgs e)
        {
            if (UploadConfigDate.Value < DateTime.Now)
            {
                MessageBox.Show("이전날짜를 선택하셨습니다.");
                UploadConfigDate.Value = DateTime.Now;
                UploadConfigDate.Focus();
            }
        }
        // Upload Button Click
        private void UploadButton_Click(object sender, EventArgs e)
        {
            if (UploadButton.Text == "업로드")
                backgroundWorker_upload.RunWorkerAsync();
            else
                backgroundWorker_upload.CancelAsync();
            
        }
        private void backgroundWorker_upload_DoWork(object sender, DoWorkEventArgs e)
        {
            Console.WriteLine("From Form1.backgroundWorker_upload_DoWork [start] : backgroundwork start");
            NaverUpload uploader = new NaverUpload(config.Cafeid, AccessToken.Text);

            int articleid;
            int num;
            int category;
            string seller;
            string item_type;
            int upload_fee;
            string title;
            string content;
            string closing_at;

            string picpath1;
            string picpath2;
            string picname1;
            string picname2;

            int count = UploadList.Items.Count;
            auctions.RollBackNumber();

            int current = auctions.Number();
            int index = current - 1;
            int progress_init_value = index;

            UploadButton.Text = "업로드 취소";

            while (index < count)
            {
                if (backgroundWorker_upload.CancellationPending)
                {
                    UploadButton.Text = "업로드";
                    break;
                }
                else
                {
                    Console.WriteLine("From Form1.backgroundWorker_upload_DoWork [result] : [count = {0}]. [index = {1}], [current = {2}]", count.ToString(), index.ToString(), current.ToString());

                    // naver upload logic
                    num = Convert.ToInt32(UploadList.Items[index].SubItems[0].Text);
                    category = Convert.ToInt32(UploadList.Items[index].SubItems[1].Text);
                    closing_at = UploadList.Items[index].SubItems[7].Text;
                    title = config.ConvertTitle(config.Title, Convert.ToInt32(UploadList.Items[index].SubItems[0].Text), Convert.ToInt32(UploadList.Items[index].SubItems[1].Text), UploadList.Items[index].SubItems[7].Text);
                    content = config.ConvertContent(config.Content, Convert.ToInt32(UploadList.Items[index].SubItems[1].Text), UploadList.Items[index].SubItems[7].Text);

                    picpath1 = UploadList.Items[index].SubItems[3].Text;
                    picname1 = UploadList.Items[index].SubItems[4].Text;
                    picpath2 = UploadList.Items[index].SubItems[5].Text;
                    picname2 = UploadList.Items[index].SubItems[6].Text;

                    articleid = Convert.ToInt32(uploader.CreateArticle(category, title, content, Path.Combine(picpath1, picname1), Path.Combine(picpath2, picname2)));
                    
                    while(articleid == -1)
                    {
                        if (backgroundWorker_upload.CancellationPending)
                            break;

                        articleid = Convert.ToInt32(uploader.CreateArticle(category, title, content, Path.Combine(picpath1, picname1), Path.Combine(picpath2, picname2)));
                        Console.WriteLine("업로드 대기 중 articleid : "+ articleid.ToString());
                        Delay(11000);
                    }

                    if (articleid == 0)
                    {
                        MessageBox.Show("업로드 도중 에러 발생 : Where " + index.ToString());
                        return;
                    }

                    seller = UploadList.Items[index].SubItems[8].Text;
                    item_type = UploadList.Items[index].SubItems[9].Text;
                    if (item_type == "단체싱글" || item_type == "내카드")
                        upload_fee = 500;
                    else
                        upload_fee = 1000;

                    auctions.InsertLine(articleid,
                        num,
                        auction_week,
                        category,
                        seller,
                        item_type,
                        upload_fee,
                        closing_at);

                    current = auctions.Number();
                    index = current - 1;

                    
                    orm.Insert(Auctions.ToString());

                    pictures.InsertLine(articleid, auction_week, 1, picpath1, picname1);
                    orm.Insert(Pictures.ToString());
                    Thread.Sleep(1000);
                    pictures.InsertLine(articleid, auction_week, 2, picpath2, picname2);
                    orm.Insert(Pictures.ToString());

                    backgroundWorker_upload.ReportProgress(Convert.ToInt32((double)(index - progress_init_value) / (double)(count - progress_init_value) * 100));
                    Delay(11000);
                }
            }
        }
        private void backgroundWorker_upload_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UploadProgressBar.Value = (int)e.ProgressPercentage;

            for (int i = 0; i <= UploadProgressBar.Value / 10; i++)
                Console.Write("==");
            Console.Write("=> " + UploadProgressBar.Value.ToString() + "%");
            Console.WriteLine("");
        }
        private void backgroundWorker_upload_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("From Form1.backgroundWorker_upload_DoWork [end] : backgroundwork end");
            UploadButton.Text = "업로드";
            if (backgroundWorker_upload.CancellationPending)
            {
                backgroundWorker_upload.Dispose();
                MessageBox.Show("업로드가 취소되었습니다.");
            }
            else
            {
                backgroundWorker_upload.Dispose();
                MessageBox.Show("업로드가 완료되었습니다.");
            }
        }
        private static bool Delay(int MS)
        {
            DateTime ThisMoment = DateTime.Now;
            TimeSpan duration = new TimeSpan(0, 0, 0, 0, MS);
            DateTime AfterWards = ThisMoment.Add(duration);
            while (AfterWards >= ThisMoment)
            {
                ThisMoment = DateTime.Now;
            }
            return true;
        }
        // Category Change Logic
        private bool ItemSelectFlag = false;
        private void UploadList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (UploadList.SelectedItems.Count != 1)
            {
                this.ItemSelectFlag = false;
                return;
            }
            this.ItemSelectFlag = true;
            UploadList_CreatePreviewSingleLine();
        }
        private void UploadList_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (this.ItemSelectFlag != true)
                return;

            switch (e.KeyChar)
            {
                case 'q':
                    UploadList.FocusedItem.SubItems[1].Text = categorys.Basketball.ToString();
                    UploadList.FocusedItem.SubItems[2].Text = categorys.ConvertCategory(categorys.Basketball);
                    break;
                case 'w':
                    UploadList.FocusedItem.SubItems[1].Text = categorys.Baseball.ToString();
                    UploadList.FocusedItem.SubItems[2].Text = categorys.ConvertCategory(categorys.Baseball);
                    break;
                case 'e':
                    UploadList.FocusedItem.SubItems[1].Text = categorys.Football.ToString();
                    UploadList.FocusedItem.SubItems[2].Text = categorys.ConvertCategory(categorys.Football);
                    break;
                case 'r':
                    UploadList.FocusedItem.SubItems[1].Text = categorys.Etc.ToString();
                    UploadList.FocusedItem.SubItems[2].Text = categorys.ConvertCategory(categorys.Etc);
                    break;
                case 't':
                    UploadList.FocusedItem.SubItems[1].Text = categorys.Custom.ToString();
                    UploadList.FocusedItem.SubItems[2].Text = categorys.ConvertCategory(categorys.Custom);
                    break;
            }
        }

        // Closing Logic

        // Server Sync Button to Closing Sequence
        private void ClosingServerButton_Click(object sender, EventArgs e)
        {
            CheckForIllegalCrossThreadCalls = false;
            if (ClosingList.Items.Count != 0)
            {
                DialogResult caution;
                caution = MessageBox.Show("리스트가 초기화 됩니다.", "주의", MessageBoxButtons.YesNo);
                if (caution == DialogResult.No)
                    return;

                ClosingList.Items.Clear();

                this.auctions.Clear();
                this.auctions = null;

                this.biddings.Clear();
                this.biddings = null;
            }

            auction_week = ClosingWeek.Text;

            try
            {
                orm.Select(Auctions.ToString(), "SELECT * FROM " + Auctions.ToString() + " WHERE week = \'" + this.auction_week + "\' ORDER BY num ASC");
                this.auctions = new Auctions(orm.getTable(Auctions.ToString()));

                orm.Select(Biddings.ToString(), "SELECT * FROM " + Biddings.ToString() + " WHERE  week = \'" + this.auction_week + "\' ORDER BY articleid ASC");
                this.biddings = new Biddings(orm.getTable(Biddings.ToString()));
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
            }

            foreach (DataRow row in this.auctions.Table.Rows)
            {
                DataRow[] bidRows = this.biddings.Table.Select("articleid = '" + row["articleid"].ToString() + "'");
                string bidder = "";
                string bid = "";
                if(bidRows.Length > 0)
                {
                    bidder = bidRows[0]["bidder"].ToString();
                    bid = bidRows[0]["bid"].ToString();
                }

                ListViewItem item = new ListViewItem(row["num"].ToString());
                item.SubItems.Add(row["category"].ToString());
                item.SubItems.Add(categorys.ConvertCategory(Convert.ToInt32(row["category"].ToString())));
                item.SubItems.Add(DateTime.Parse(row["closing_at"].ToString()).AddMinutes(-1.0).ToString("yyyy-MM-dd HH:mm:00") );
                item.SubItems.Add(DateTime.Parse(row["closing_at"].ToString()).ToString("yyyy-MM-dd HH:mm:00"));
                item.SubItems.Add(row["seller"].ToString());
                item.SubItems.Add(row["item_type"].ToString());
                item.SubItems.Add(row["upload_fee"].ToString());
                item.SubItems.Add(row["basic_fee"].ToString());
                item.SubItems.Add(row["fee"].ToString());
                item.SubItems.Add(row["seller_get"].ToString());
                item.SubItems.Add(bidder);
                item.SubItems.Add(bid);
                item.SubItems.Add(row["articleid"].ToString());
                ClosingList.Items.Add(item);
            }

            ClosingSyncPanel.Visible = true;
            ClosingSyncText1.Text = "동기화완료";
            ClosingSyncText2.Text = "선택 경매주차 : " + auction_week;
            ClosingSyncText3.Text = "미낙찰 수 : ";
            ClosingSyncText4.Text = "다음 마감번호 : ";
        }

        public void CreateClosing()
        {
            if (this.closing != null)
            {
                this.closing.Driver.Close();
                this.closing = null;
            }
            Thread create_closing_thread = new Thread(new ThreadStart( () =>
            {
                closing = new Closing();
            }));
            create_closing_thread.SetApartmentState(ApartmentState.STA);
            create_closing_thread.Start();
            create_closing_thread.Join();
        }
        private void ClosingTestButton_Click(object sender, EventArgs e)
        {
            CreateClosing();
            Delay(3000);
            DateTime closing_at = DateTime.Parse(ClosingTestDate.Value.ToString("yyyy-MM-dd") + " " + ClosingTestTime.Value.ToString("HH:mm:00"));
            closing.SetUrl(ClosingTestUrl.Text);
            ReplyItem successful_bid = closing.AssignBidder(closing_at);

            //CreateClosingReplyList(closing_at, items);
        }

        private delegate void ClosingReplyPanelAdd(Control item);
        private void ClosingReplyPanellAdder(Control item)
        {
            ClosingReplyPanel.Controls.Add(item);
        }
        private void CreateClosingReplyList(DateTime closing_at, ReplyItem[] items)
        {
            try
            {
                ClosingReplyPanel.Controls.Clear();

                Padding closing_text_padding = new Padding(0, 10, 0, 10);
                Padding reply_padding = new Padding(30, 0, 0, 0);

                Label closing_text = new Label();
                closing_text.Size = new Size(200, 20);
                closing_text.Margin = closing_text_padding;
                closing_text.Text = closing_at.ToString("yyyy년 MM dd일 HH시 mm분 마감");

                var adder = new ClosingReplyPanelAdd(ClosingReplyPanellAdder);

                //ClosingReplyPanel.Invoke(adder, new object[] { closing_text });
                Invoke(adder, new object[] { closing_text });
                Invoke(new Action(() => { }));
                
               
                //ClosingReplyPanel.Controls.Add(closing_text);

                foreach (ReplyItem item in items)
                {
                    RadioButton radio_button = new RadioButton();

                    radio_button.Size = new Size(400, 20);
                    if (item.IsReply)
                        radio_button.Margin = reply_padding;

                    radio_button.Text = item.Nickname + " : " + (item.Content.Length > 6 ? item.Content.Substring(0, 6) + "..." : item.Content) + " [" + item.Created_at.ToString("yyyy년 MM dd일 HH시 mm분") + "]";

                    //ClosingReplyPanel.Controls.Add(radio_button);


                    //ClosingReplyPanel.Invoke(adder, new object[] { radio_button });
                    Invoke(adder, new object[] { radio_button });
                }
                Thread.Sleep(2000);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        private void ClosingButton_Click(object sender, EventArgs e)
        {
            if (ClosingButton.Text == "마감")
            {
                ClosingButton.Text = "중지";
                backgroundWorker_Closing.RunWorkerAsync();
            }
            else
                backgroundWorker_Closing.CancelAsync();
        }

        private void backgroundWorker_Closing_DoWork(object sender, DoWorkEventArgs e)
        {
            
            if (ClosingList.Items.Count == 0)
            {
                MessageBox.Show("리스트가 존재하지 않습니다.");
                backgroundWorker_Closing.CancelAsync();
            }
                
            Console.WriteLine("From Form1.backgroundWorker_Closing_DoWork [start] : backgroundwork start");
            
            DateTime closing_at;
            DateTime now;

            foreach (ListViewItem item in ClosingList.Items)
           {
                // if cancel button click
                if (backgroundWorker_Closing.CancellationPending)
                {
                    e.Cancel = true;
                    return;
                }
                    
                closing_at = DateTime.Parse(item.SubItems[3].Text);
                if(ClosingTestUrl.Text == "test")
                {
                    now = DateTime.Parse(ClosingTestDate.Value.ToString("yyyy-MM-dd") + " " + ClosingTestTime.Value.ToString("HH:mm:00"));
                }
                else
                {
                    now = DateTime.Now;
                }
                
                // if bidder is not "" than continue
                if (item.SubItems[11].Text != "")
                    continue;

                // yet closing time than roop
                while (closing_at >= now)
                {
                    if (backgroundWorker_Closing.CancellationPending)
                    {
                        e.Cancel = true;
                        return;
                    }
                    Console.WriteLine( "--다음 경매 종료 까지 " + (closing_at - DateTime.Now).Hours.ToString() + "시간 " + (closing_at - DateTime.Now).Minutes.ToString() + "분 ");
                    Delay(10000);
                }

                // pass closing time
                if(closing_at < now)
                {
                    if (this.closing == null)
                    {
                        Console.WriteLine("closing is null");
                        CreateClosing();
                    }
                        
                    if (!this.closing.IsLogin)
                    {
                        Console.WriteLine("not login");
                        this.closing.Login();
                    }
                        
                    while(!this.closing.IsLogin)
                    {
                        Console.WriteLine("not login");
                    }

                    Delay(3000);
                    this.closing.SetArticleId(Convert.ToInt32(item.SubItems[13].Text));
                    Delay(3000);
                    ReplyItem successful_bid = closing.AssignBidder(closing_at, Convert.ToInt32(item.SubItems[1].Text));

                    // ul.comment_list false == no comment
                    if (successful_bid == null)
                    {
                        item.SubItems[11].Text = "미낙찰";
                        item.SubItems[12].Text = "0";
                        item.SubItems[8].Text = "0";
                        item.SubItems[9].Text = "0";
                        item.SubItems[10].Text = "0";
                    }
                    else
                    {
                        item.SubItems[11].Text = successful_bid.Nickname;
                        item.SubItems[12].Text = successful_bid.Bid.ToString();

                        if( Convert.ToInt32(item.SubItems[1].Text) == 40 )
                        {
                            item.SubItems[8].Text = (successful_bid.Bid * 0.5).ToString();
                        }
                        else
                        {
                            item.SubItems[8].Text = (successful_bid.Bid * 0.1).ToString();
                        }
                        item.SubItems[9].Text = (Convert.ToInt32(item.SubItems[7].Text) + Convert.ToInt32(item.SubItems[8].Text)).ToString();
                        item.SubItems[10].Text = (Convert.ToInt32(item.SubItems[12].Text) - Convert.ToInt32(item.SubItems[9].Text)).ToString();

                    }
                }
                SaveClosingList(item);
            }
        }

        private void backgroundWorker_Closing_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }
        public void thread_info()
        {
            Process[] allProc = Process.GetProcesses();
            Console.WriteLine("#### process : {0} ####", allProc.Length);
            int i = 1;
            foreach(Process p in allProc)
            {
                Console.WriteLine("{0} Process", i++);
                ProcessThreadCollection ptc = p.Threads;
                int j = 1;
                foreach(ProcessThread pt in ptc)
                {
                    Console.WriteLine("{0} Thread", j++);
                    Console.WriteLine("id : {0}", pt.Id);
                    Console.WriteLine("state : {0}", pt.ThreadState);
                    Console.WriteLine("prior : {0}", pt.BasePriority);
                    Console.WriteLine("start at : {0}", pt.StartTime);
                    Console.WriteLine("");
                }
            }
        }
        private void backgroundWorker_Closing_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Console.WriteLine("From Form1.backgroundWorker_Closing_DoWork [end] : backgroundwork end");
            ClosingButton.Text = "마감";
            if (e.Cancelled)
            {
                backgroundWorker_Closing.Dispose();
                MessageBox.Show("댓글마감이 취소되었습니다.");
            }
            else
            {
                backgroundWorker_Closing.Dispose();
                MessageBox.Show("댓글마감이 완료되었습니다.");
            }
        }
        private void ClosingSaveButton_Click(object sender, EventArgs e)
        {
            SaveExcelClosingList();
        }
        private void SaveClosingList(ListViewItem item)
        {
            if (item.SubItems[11].Text == "")
                return;

            int articleid = Convert.ToInt32(item.SubItems[13].Text);
            int basic_fee = Convert.ToInt32(item.SubItems[8].Text);
            int fee = Convert.ToInt32(item.SubItems[9].Text);
            int seller_get = Convert.ToInt32(item.SubItems[10].Text);

            string bidder = item.SubItems[11].Text;
            int bid = Convert.ToInt32(item.SubItems[12].Text);

            if (bidder == "")
                return;

            DataRow[] auction_rows = this.auctions.Table.Select("articleid = '" + articleid.ToString() + "'");

            if (auction_rows.Length != 1)
                return;

            DataRow row = auction_rows[0];
            row.BeginEdit();
            row["basic_fee"] = basic_fee;
            row["fee"] = fee;
            row["seller_get"] = seller_get;
            row.EndEdit();
            orm.Update(Auctions.ToString());

            this.biddings.InsertLine(articleid, auction_week, bidder, bid);
            orm.Insert(Biddings.ToString());
        }
        private void SaveExcelClosingList()
        {
            Excel.Application app = null;
            Excel.Workbook book = null;
            Excel.Worksheet sheet = null;

            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string filename = auction_week + DateTime.Now.ToString("_HHmmss") + ".xlsx";
            string path = Path.Combine(desktopPath, filename);
            
            SaveFileDialog.CreatePrompt = true;
            SaveFileDialog.OverwritePrompt = true;

            SaveFileDialog.FileName = filename;
            SaveFileDialog.DefaultExt = "xlsx";
            SaveFileDialog.Filter = "엑셀파일|*.xlsx" +
                                                "|All Files|*.*";

            DialogResult result = SaveFileDialog.ShowDialog();

            if (result != DialogResult.OK)
                return;

            filename = SaveFileDialog.FileName;

            try
            {
                app = new Excel.Application();
                book = app.Workbooks.Add();
                sheet = book.Worksheets.Add();
                app.Visible = false;

                sheet.Cells[1, 1] = "경매주차";
                sheet.Cells[1, 2] = "경매번호";
                sheet.Cells[1, 3] = "낙찰자";
                sheet.Cells[1, 4] = "낙찰금액";
                sheet.Cells[1, 5] = "판매자";
                sheet.Cells[1, 6] = "싱글, 랏 구분";
                sheet.Cells[1, 7] = "등록수수료";
                sheet.Cells[1, 8] = "기본수수료";
                sheet.Cells[1, 9] = "수수료";
                sheet.Cells[1, 10] = "입금해줄금액";

                int index = 2;
                foreach(ListViewItem item in ClosingList.Items)
                {
                    sheet.Cells[index, 1] = auction_week;
                    sheet.Cells[index, 2] = item.SubItems[0].Text;
                    sheet.Cells[index, 3] = item.SubItems[11].Text;
                    sheet.Cells[index, 4] = item.SubItems[12].Text;
                    sheet.Cells[index, 5] = item.SubItems[5].Text;
                    sheet.Cells[index, 6] = item.SubItems[6].Text;
                    sheet.Cells[index, 7] = item.SubItems[7].Text;
                    sheet.Cells[index, 8] = item.SubItems[8].Text;
                    sheet.Cells[index, 9] = item.SubItems[9].Text;
                    sheet.Cells[index, 10] = item.SubItems[10].Text;
                    index++;
                }
                sheet.Columns.AutoFit();
                book.SaveAs(filename);
                book.Close();
                app.Quit();
            }
            catch(Exception error)
            {

            }
            finally
            {
                ReleaseExcelObject(sheet);
                ReleaseExcelObject(book);
                ReleaseExcelObject(app);
            }
        }
        private void ReleaseExcelObject(object obj)
        {
            try
            {
                if(obj != null)
                {
                    Marshal.ReleaseComObject(obj);
                    obj = null;
                }
            }
            catch(Exception error)
            {
                obj = null;
                throw error;
            }
            finally
            {
                GC.Collect();
            }
        }
    }
    // Upload Class
    public class UploadConfig
    {
        private int count;
        private int start;
        private int end;
        private int category;
        private int bid;
        private DateTime date;
        private DateTime time;
        public int Count { get { return count; } set { count = value; } }
        public int Start { get { return start; } set { start = value; } }
        public int End { get { return end; } set { end = value; } }
        public int Category { get { return category; } set { category = value; } }
        public int Bid { get { return bid; } set { bid = value; } }
        public DateTime Date { get { return date; } set { date = value; } }
        public DateTime Time { get { return time; } set { time = value; } }
    }

    // Closing Class
}
