using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using mshtml;
using System.Web;
using System.Collections.Specialized;
using System.Collections;

namespace XSSFuzzer
{
    public partial class Form1 : Form
    {
        bool loading = false;

        public Form1()
        {
            InitializeComponent();
            xsslib = new ArrayList();

            xsslib.Add(">\\\';</script>>\\\"><script>alert(9527)</script>\'");
            xsslib.Add("'>\\\\\';</script>>\\\\\"><script>alert(9527)</script>'");
            xsslib.Add("\"><img src=1 onerror=alert(9527)>");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String url = textBox1.Text;

       //     this.webBrowser1.Navigate(url);
            doXssFuzz(url);
        }
        
        private void doXssFuzz(String url)
        {
            Uri uri = new Uri(url);

       //     MessageBox.Show("schema " +uri.Scheme+"\r\n"+"host "+uri.Host+"absolute uri "+uri.AbsoluteUri+ "path query " +uri.PathAndQuery);

            String path = url.Substring(0, url.IndexOf('?') );

            MessageBox.Show(path);

            NameValueCollection collect = HttpUtility.ParseQueryString(uri.ToString());

            foreach (String s in collect.AllKeys)
            {
         //       String v = collect[s]+
                foreach (string payload in xsslib)
                {
                    String orig = collect[s];

                    String v = collect[s] + payload;

                    collect[s] = v;

                    String query = ToQueryString(collect);

                    collect[s] = orig;

                    String fuzzedurl  = path + query;

              //      MessageBox.Show(fuzzedurl);

                    this.webBrowser1.Navigate(fuzzedurl);

                    loading = true;

                    while (loading)
                    {
                        Application.DoEvents();
                    }
                }
            }

            MessageBox.Show("扫描结束");
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}",key, value))
                .ToArray();
            return "?" + string.Join("&", array);
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            loading = false;
        }

        private void webBrowser1_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
           

            IHTMLWindow2 win = (IHTMLWindow2)webBrowser1.Document.Window.DomWindow;
            string s = @"function confirm() {return true;}";
            s += @"function alert(str) { window.external.alertMessage(str, window.location.href); }";
            win.execScript(s, "javascript");
            
        }

        public void alertMessage(string s, string url)
        {
            MessageBox.Show("接收到alert消息： url is "+url+ " alert " + s); //换成你自己想要执行的动作
        }
    }
}
