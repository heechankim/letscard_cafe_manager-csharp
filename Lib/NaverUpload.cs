using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json.Linq;


namespace letscard_cafe.Lib
{
    class NaverUpload
    {
        private static string CRLF = "\r\n";
        private static string boundary;
        private static Stream DataStream;
        private static byte[] formData;

        private int cafeid;
        private string token;

        public NaverUpload(int cafeid, string access_token)
        {
            this.cafeid = cafeid;
            this.token = access_token;
        }

        public int CreateArticle(int menuid, string title, string content, string pic1, string pic2)
        {
            try
            {
                string header = "Bearer " + token;
                string url = "https://openapi.naver.com/v1/cafe/" + this.cafeid + "/menu/" + menuid + "/articles"; // cafe api url ( 상품 게시판은 글쓰기 불가)
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                boundary = "----" + DateTime.Now.Ticks.ToString("x") + "----";
                DataStream = new MemoryStream();
                request.Method = "POST";
                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Headers.Add("Authorization", header);
                buildParam("subject", title); // 제목
                buildParam("content", content); // 본문
                buildFileParam("image[0]", pic1); // 파일 [0]
                buildFileParam("image[1]", pic2); // 파일 [1]
                buildByteParam(); // Byte Array 생성
                Stream stream = request.GetRequestStream();
                stream.Write(formData, 0, formData.Length); // request 전송
                stream.Close();
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string text = reader.ReadToEnd();
                stream.Close();
                response.Close();
                reader.Close();
                //Console.WriteLine(text);

                JObject upload_response = JObject.Parse(text);
                if (upload_response["message"]["status"].ToString() != "200")
                {
                    Console.WriteLine("Error in NaverUpload.CreateArticle [error - status is not 200] : upload failed");
                    return 0;
                }
                else
                {
                    Console.WriteLine("From NaverUpload.CreateArticle [result] : [success = {0}], [articleid = {1}]", upload_response["message"]["status"].ToString(), upload_response["message"]["result"]["articleId"].ToString());
                    return Convert.ToInt32(upload_response["message"]["result"]["articleId"]);
                }
            }
            catch(Exception error)
            {
                Console.WriteLine(error.Message);
                return -1;
            }

            
        }

        private static void buildParam(String name, String value)
        {
            string paramName1 = name; // cafe
            string paramValue1 = HttpUtility.UrlEncode(value); // cafe는 인코딩 필요
            string res = "--" + boundary + CRLF + "Content-Disposition: form-data; name=\"" + paramName1 + "\"" + CRLF;
            res += "Content-Type: text/plain; charset=UTF-8" + CRLF + CRLF;
            res += paramValue1 + CRLF;
            DataStream.Write(Encoding.UTF8.GetBytes(res), 0, Encoding.UTF8.GetByteCount(res));
        }

        private static void buildFileParam(String fileParamName, String filePathName)
        {
            FileStream fs = new FileStream(filePathName, FileMode.Open, FileAccess.Read);
            byte[] fileData = new byte[fs.Length];
            fs.Read(fileData, 0, fileData.Length);
            fs.Close();
            string postData = "--" + boundary + CRLF + "Content-Disposition: form-data; name=\"" + fileParamName + "\"; filename=\"";
            postData += Path.GetFileName(filePathName) + "\"" + CRLF + "Content-Type: image/jpeg" + CRLF;
            postData += "Content-Transfer-Encoding: binary" + CRLF + CRLF;
            DataStream.Write(Encoding.UTF8.GetBytes(postData), 0, Encoding.UTF8.GetByteCount(postData));
            DataStream.Write(fileData, 0, fileData.Length);
            DataStream.Write(Encoding.UTF8.GetBytes("\r\n"), 0, 2);
        }

        private static void buildByteParam()
        {
            string footer = "--" + boundary;
            DataStream.Write(Encoding.UTF8.GetBytes(footer), 0, Encoding.UTF8.GetByteCount(footer));
            DataStream.Position = 0;
            formData = new byte[DataStream.Length];
            DataStream.Read(formData, 0, formData.Length); DataStream.Close();
            DataStream.Close();
        }
    }
}
