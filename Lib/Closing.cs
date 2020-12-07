using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Interactions.Internal;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace letscard_cafe.Lib
{
    class Closing
    {
        protected ChromeDriverService DriverService = null;
        protected ChromeOptions options = null;
        protected ChromeDriver driver = null;
        private bool is_login = false;
        public ChromeDriver Driver => driver;
        public bool IsLogin => is_login;

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
        public void SetArticleId(int articleid)
        {
            if (driver == null)
                return;

            driver.Navigate().GoToUrl("https://cafe.naver.com/cbx900/" + articleid.ToString());
        }
        public void SetUrl(string url)
        {
            if (driver == null)
                return;

            driver.Navigate().GoToUrl(url);
        }
        public Closing()
        {
            Initialize();
        }
        private void Initialize()
        {
            CreateDriver();
            Login();
        }
        private void CreateDriver()
        {
            DriverService = ChromeDriverService.CreateDefaultService();
            DriverService.HideCommandPromptWindow = false;

            options = new ChromeOptions();
            options.AddArgument("disable-gpu");

            driver = new ChromeDriver(DriverService, options);
        }
        public void Login()
        {
            string id = "hroal";
            string pw = "@*cr6812hc#*";

            driver.Navigate().GoToUrl("https://www.naver.com");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);

            var element = driver.FindElementByXPath("//*[@id='account']");
            element.Click();

            Delay(3000);

            Clipboard.SetData(DataFormats.Text, (object)id);
            element = driver.FindElementByXPath("//*[@id='id']");
            element.Click();

            Actions id_action = new Actions(driver);
            id_action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").KeyUp(OpenQA.Selenium.Keys.Control).Perform();

            Clipboard.SetData(DataFormats.Text, (object)pw);
            element = driver.FindElementByXPath("//*[@id='pw']");
            element.Click();

            Actions pw_action = new Actions(driver);
            pw_action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").KeyUp(OpenQA.Selenium.Keys.Control).Perform();

            element = driver.FindElementByXPath("//*[@id='log.login']");
            element.Click();

            this.is_login = true;
        }

        public ReplyItem AssignBidder(DateTime closing_at, int _category = 1)
        {
            while(driver == null)
            {
                Initialize();
            }

            // for iframe html 
            driver.SwitchTo().Frame("cafe_main");
            var title_text = WaitForVisible(Driver, By.CssSelector("h3.title_text"));
            Console.WriteLine(title_text);
            var ul_comment_list = WaitForVisible(driver, By.CssSelector("ul.comment_list"));
            if (ul_comment_list == null)
                return null;

            var comment_list = ul_comment_list.FindElements(By.CssSelector("li"));

            /*
            Console.WriteLine(comment.GetAttribute("class") );
            CommentItem
            CommentItem CommentItem--reply
             */

            //ReplyItem[] reply_items = new ReplyItem[comment_list.Count];



            List<ReplyItem> reply_items;
            try
            {
                reply_items = new List<ReplyItem>();

                string nickname;
                string content;
                string created_at;
                int index = 0;
                foreach (var comment in comment_list)
                {
                    ReplyItem item;
                    nickname = comment.FindElement(By.CssSelector("a.comment_nickname")).Text;
                    content = comment.FindElement(By.CssSelector("span.text_comment")).Text;
                    created_at = comment.FindElement(By.CssSelector("span.comment_info_date")).Text;

                    if (comment.GetAttribute("class") == "CommentItem")
                    {
                        item = new ReplyItem(index, nickname, content, DateTime.Parse(created_at));
                    }
                    else
                    {
                        item = new ReplyItem(index, nickname, content, DateTime.Parse(created_at), true);
                    }

                    try
                    {
                        item.Bid = Convert.ToInt32(Regex.Replace(item.Content, @"\D", ""));
                    }
                    catch(Exception error)
                    {
                        Console.WriteLine("content to int" + error.Message);
                    }
                    finally { 
                    }

                    try
                    {
                        if (item.Content.Contains("취소"))
                            item.IsCanceled = true;
                    }
                    catch (Exception error)
                    {
                        Console.WriteLine("is contain cancel" + error.Message);
                    }

                    
                    reply_items.Add(item);
                    index++;
                }
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
                return null;
            }

            // 비드풀 insert 로직
            List<ReplyItem> bid_pool = new List<ReplyItem>();
            foreach(ReplyItem item in reply_items)
            {
                // closing_at보다 작거나 같다면
                if (item.Created_at <= closing_at)
                {
                    // "취소" 라는 단어가 content에 들어가 있었다면
                    if (item.IsCanceled)
                    {
                        // 대댓글이라면 그 위에꺼 삭제
                        if(item.IsReply)
                        {
                            // 근데 created_at 날이랑 closing_at 날이랑 같지 않아야지만 삭제가능
                            // 같지 않다면 비드풀의 아이템중 대댓글 취소 위의 녀석을 삭제
                            if (item.Created_at.Day != closing_at.Day)
                                bid_pool.Remove(reply_items[item.Index - 1]);

                            // 그리고 대댓글은 비드풀에 넣지 않고 그대로 진행
                            // -> 이렇게 되면 글 수정 한 녀석들도 잡을수 있음
                            continue;
                        }


                        //if (item.Created_at.Day != closing_at.Day)
                        // bid_pool.RemoveAt(item.Index - 1);
                    }

                    if(item.IsReply)
                    {
                        continue;
                    }

                    if(_category == 40)
                    {
                        // 100원 경매일 경우 100으로 나누었을때 나머지가 0이 아니라면 
                        if (item.Bid % 100 != 0)
                            continue; //추가하지 않음
                    }
                    else
                    {
                        // 100원 경매가 아닐경우 1000으로 나누었을때 나머지가 0이 아니라면
                        if (item.Bid % 1000 != 0)
                            continue; //추가하지 않음
                    }
                    bid_pool.Add(item);
                }
            }
            var sorted_bid_pool = from bid in bid_pool orderby bid.Bid select bid;

            ReplyItem successful_bid;
            ReplyItem final = null;

            try
            {
                successful_bid = sorted_bid_pool.Last();
            }
            catch (InvalidOperationException error)
            {
                return null;
            }
            
            
            foreach(ReplyItem item in sorted_bid_pool)
            {
                if (item.Bid == successful_bid.Bid)
                {
                    if (item.Index < successful_bid.Index)
                        final = item;
                }
            }
            if (final == null)
                final = successful_bid;


            //print
            foreach(ReplyItem item in reply_items)
            {
                if (item.Index == final.Index)
                    Console.Write("=>");

                if (item.IsCanceled)
                    Console.Write("X ");

                Console.WriteLine((item.IsReply ? "\t" : "") + item.Nickname + " : " + (item.Content.Length > 6 ? item.Content.Substring(0, 6) + "..." : item.Content) + " [" + item.Created_at.ToString("yyyy년 MM dd일 HH시 mm분") + "]");
            }
            /*
            try
            {
                var reply_button = comment_list[final.Index].FindElement(By.CssSelector("a.comment_info_button"));
                reply_button.Click();

                string reply = final.Nickname + "님 " + final.Bid.ToString() + "원 낙찰";
                //Clipboard.SetData(DataFormats.Text, (object)reply);

                var reply_textarea = WaitForVisible(driver, By.CssSelector("textarea.comment_inbox_text"));
                reply_textarea.Click();
                reply_textarea.SendKeys(reply);
                //Actions action = new Actions(driver);
                //action.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").KeyUp(OpenQA.Selenium.Keys.Control).Perform();
                Delay(500);
                var register_button = WaitForVisible(driver, By.CssSelector("a.button.btn_register.is_active"));
                register_button.Click();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            */
            Delay(3000);

            return final;
        }
        private static IWebElement WaitForVisible(IWebDriver driver, By by)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(20));
                return wait.Until(ExpectedConditions.ElementIsVisible (by));
            }
            catch (Exception error)
            {
                return null;
            }
        }
    }
    
}
/*
 * 
 * <div>Article
-> <div>CommentBox
-> <ul>comment_list
-> <li> CommentItem


<div>Article
-> <div>CommentBox
-> <ul>comment_list
-> <li> CommentItem
-> <div> comment_box
-> 	<div> comment_nick_box
		<div> comment_nick_info
			<a> comment_nickname
	<div> comment_text_box
		<span> text_comment
	<div> comment_info_box
		<span> comment_info_date

 * 
 */