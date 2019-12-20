using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Runtime;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Toxy;
using Toxy.Parsers;
using Gurus.Utils;
using Gurus.Models;
using Gurus.Controllers;

namespace Gurus.Controllers
{
    public class ExternalConnection
    {
        ChromeDriver _driver = null;

        bool _useSelenium = false;

        dynamic _source = null;
        
        const int NMAX_PAGES = 0;
        const int NMAX_PERSONS = 5;

        public ExternalConnection(dynamic source)
        {
            _source = source;
            OpenDriver();
            
        }

        public void OpenDriver(bool incongnito = false)
        {
            bool useSelenium = _source.useSelenium ?? false;

            if(useSelenium)
            {
                ChromeOptions options = new ChromeOptions();
                if(incongnito)
                {
                    options.AddArguments("--incognito");
                }
                _driver = new ChromeDriver("C:\\Aplics\\ChromeDriver", options);
            }
        }

        public void CloseDriver()
        {
            if(_driver != null)
                _driver.Close();
        }

        public void RestartDriver(bool incongnito = false)
        {
            CloseDriver();
            OpenDriver(incongnito);
        }

        public void ClearCacheAndCookies()
        {
            _driver.Manage().Cookies.DeleteAllCookies();
            System.Threading.Thread.Sleep(1000);
            _driver.Navigate().GoToUrl("chrome://settings/clearBrowserData");
            System.Threading.Thread.Sleep(5000);
            _driver.FindElementByXPath("//settings-ui").SendKeys(Keys.Enter);
        }


        private bool Login(dynamic login)
        {
            bool success = false;
            int NMAX_RETRIES = 3;
            IWebElement emailTextBox = null;
            IWebElement passTextBox = null;
            IWebElement signUpButton = null;

            for(int i=0; i < NMAX_RETRIES && !success; i++)
            {
                _driver.Url = login.url;

                try
                {
                    emailTextBox = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", login.user.tag)));
                    passTextBox = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", login.pass.tag)));
                    //signUpButton = _driver.FindElement(By.ClassName(string.Format("{0}", login.submit)));
                    signUpButton = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}'] | .//*[@class='{0}']", login.submit)));

                    success = true;
                }
                catch(Exception)
                {
                    //_driver.Navigate().GoToUrl(login.url);
                }                
            }

            string user = Utils.Cryptography.DecryptString((string)login.user.value);
            string pass = Utils.Cryptography.DecryptString((string)login.pass.value);

            emailTextBox.SendKeys(user);
            passTextBox.SendKeys(pass);
            signUpButton.Click();

            return success;
        }


        public List<Person> FetchPersonData()
        {
            List<Person> lstPersons = new List<Person>();

            if((bool)_source.active)
            {
                if(!Login(_source.login))
                    throw new Exception("Error on login");

                if(_source.searchby == "keywords")
                {
                    foreach(var keyword in _source.keywords)
                    {
                        for(int n = 0; n <= NMAX_PAGES; n++)
                        {
                            string url = (string)_source.url;
                            
                            Console.WriteLine(string.Format("===>> GET: {0}", url));

                            string key = (string)keyword;
                            url = url.Replace("{{keyword}}", key).Replace("{{pageNumber}}", n.ToString());

                            Console.WriteLine(string.Format("===>> GET: {0}", url));
                            
                            _driver.Navigate().GoToUrl(url);
                            
                            lstPersons.AddRange(GetPersons());                               
                        }
                    }
                }
                else if(_source.searchby == "dates")
                {
                    int N_MONTHS = 1;

                    string url = (string)_source.url;
                            
                    Console.WriteLine(string.Format("===>> GET: {0}", url));

                    DateTime dtInit = new DateTime(2010,1,1);
                    DateTime dtEnd = dtInit.AddMonths(N_MONTHS);

                    while(dtEnd < DateTime.Now)
                    {
                        _driver.Navigate().GoToUrl(url);

                        dtInit = dtEnd.AddDays(1);
                        dtEnd = dtInit.AddMonths(N_MONTHS);

                        IWebElement dtInitTextBox = null;
                        IWebElement dtEndTextBox = null;
                        IWebElement searchButton = null;
                        try
                        {
                            dtInitTextBox = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", _source.dates[0])));
                            dtEndTextBox = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", _source.dates[1])));
                            searchButton = _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", _source.searchButton)));
                        }
                        catch(Exception){}

                        dtInitTextBox.SendKeys(dtInit.ToString("dd/MM/yyyy"));
                        dtEndTextBox.SendKeys(dtEnd.ToString("dd/MM/yyyy"));
                        searchButton.Click();        

                        lstPersons.AddRange(GetPersons());
                    }
                }
            }
            

            return lstPersons;
        }

        public List<Attatchment> FetchAttachementsData()
        {
            string index = _source.index;
            Elastic elastic = new Elastic(index);

            List<Attatchment> lstAttachments = new List<Attatchment>();

            int id = _source.initialId ?? 0;
            int maxRetries = _source.retries ?? 5;

            WebClient wc = new WebClient();

            bool end = false;
            int retry = 0;
            do
            {
                var tempFileName = Path.GetTempFileName();

                try
                {
                    string url = _source.url ?? "{0}";
                    url = string.Format(url, id++);
                    Console.WriteLine("=> Get " + url);

                    wc.DownloadFile(url, tempFileName);
                    var mimeType = wc.ResponseHeaders["content-type"];
                    Console.WriteLine("=> Mimetype " + mimeType);
                    var fileName = wc.ResponseHeaders["Content-Disposition"].Substring(wc.ResponseHeaders["Content-Disposition"].IndexOf("filename=") + 9).Replace("\"", "");
                    Console.WriteLine("=> Filename " + fileName);

                    if(string.IsNullOrEmpty(mimeType))
                        end = true;
                    
                    var body = string.Empty;

                    if(fileName.ToLower().EndsWith(".pdf"))
                    {
                        var pdf = new PDFTextParser(new Toxy.ParserContext(tempFileName));
                        body = pdf.Parse();
                    }
                    else if(fileName.ToLower().EndsWith(".docx"))
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        var docx = new Word2007TextParser(new Toxy.ParserContext(tempFileName));
                        body = docx.Parse();
                    }
                    else if(fileName.ToLower().EndsWith(".rtf"))
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        var rtf = new RTFTextParser(new Toxy.ParserContext(tempFileName));                        
                        body = rtf.Parse();
                    }
                    else if(fileName.ToLower().EndsWith(".doc"))
                    {
                        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                        var doc = new Word2003TextParser(new Toxy.ParserContext(tempFileName));
                        
                        body = doc.Parse();
                    }

                    if(!string.IsNullOrEmpty(body))
                    {
                        Attatchment attatch = new Attatchment(id.ToString(), fileName, url, mimeType, body, DateTime.Now);
                        elastic.SaveItem(attatch);
                    }
                    retry = 0;
                    //lstAttachments.Add(attatch);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex);
                    retry++;
                }

                try { File.Delete(tempFileName); }
                catch { } // best effort
                tempFileName = null;
                
            } while (!end && retry < maxRetries);

            return lstAttachments;
        }

        public List<Person> GetPersons()
        {
            List<Person> lstPersons = new List<Person>();

            int idx = 0;
            ReadOnlyCollection<IWebElement> itemList = null;

            do
            {
                itemList = _driver.FindElements(By.ClassName((string)_source.listResults));
                if(itemList.Count == 0)
                    itemList = _driver.FindElements(By.XPath((string)_source.listResults));
                    
                dynamic item = itemList[idx++];

                try
                {
                    //Check if it's a button link, and in that case click on this option
                    item.Click();

                    //Follow the link to the personal data section
                    _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", _source.personalDataSection))).Click();
                    item = _driver;
                }
                catch(Exception){}

                Person person = new Person();

                person.Source = (string)_source.source;

                person.Search = _source.keyword;
                person.Timestamp = DateTime.Now;

                person.Id = GetAttribute(item, _source.id);
                person.Name = GetAttribute(item, _source.name);
                person.Position = GetAttribute(item, _source.position);
                person.Location = GetAttribute(item, _source.location);
                person.Photo = GetAttribute(item, _source.photo);
                person.URL = GetAttribute(item, _source.link);
                person.Email = GetAttribute(item, _source.email);
                person.Phone = GetAttribute(item, _source.phone);
                
                try
                {
                    string birthday = GetAttribute(item, _source.birthday);
                    if(!string.IsNullOrEmpty(birthday))
                        person.Birthday = DateTime.ParseExact(birthday, "dd/MM/yyyy", null);
                }
                catch(Exception){}

                person.Distance = GetAttribute(item, _source.distance);

                if(string.IsNullOrEmpty(person.Id) && !string.IsNullOrEmpty(person.URL))
                    person.Id = person.URL;
                else
                    person.Id = Guid.NewGuid().ToString();

                if(!string.IsNullOrEmpty(person.Distance))
                    person.Name = person.Name.Replace(person.Distance, "");
                
                //Go to back in history
                _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", "ctl00_ContentPlaceHolder1_HumanRe_sourceFile1_ImageButton2"))).Click();

                //Get contacts
                _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", "ctl00_ContentPlaceHolder1_HumanRe_sourceFile1_BtInterview"))).Click();
                person.Contacts.Add(new Contact(GetAttribute(_driver, _source.contacts)));

                //Go to back in history
                _driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']", "ctl00_ContentPlaceHolder1_HumanRe_sourceFile1_ImageButton2"))).Click();

                //Get Attatchements

                //Output results
                Console.WriteLine(string.Format("» Name: {0} / Position: {1} / URL: {2} / Distance: {3} / Photo: {4} / Location: {5}", person.Name, person.Position, person.URL, person.Distance, person.Photo, person.Location));

                if(!string.IsNullOrEmpty(person.Id))
                    lstPersons.Add(person);
            }
            while(idx < 5);//itemList.Count);

            return lstPersons;
        }

        private string GetAttribute(dynamic item, dynamic metadata)
        {
            string result = string.Empty;

            //try
            //{
                if(metadata != null)
                {
                    string elem = string.Join((string)metadata.cssClass, (string)metadata.id);
                    string attribute = (string)metadata.attribute;
                    if(!string.IsNullOrEmpty(elem))
                        result = item.FindElement(By.XPath(string.Format(".//*[@id='{0}'] | .//*[@class='{0}']", elem))).GetAttribute(attribute);

                    int positionOfNewLine = result.IndexOf("\r\n");
                    if (positionOfNewLine >= 0)
                    {
                        result = result.Substring(0, positionOfNewLine);
                    }
                }
            //}
            //catch(Exception){}

            return result;
        }

        public void GetAttatchments(ref List<Person> lstPersons)
        {

            int counter = 0;

            foreach(Person p in lstPersons)
            {
                bool clearCache = _source.clearcache ?? false;

                if((counter == 0 || counter % 10 == 0) && clearCache)
                {
                    ClearCacheAndCookies();
                    RestartDriver(true);
                }

                if(!string.IsNullOrEmpty(p.URL))
                {
                    string className = string.Empty;
                    string attribute = string.Empty;

                    if(_source.detail != null)
                    {
                        className = (string)_source.detail.cssClass;
                        attribute = (string)_source.detail.attribute;
                    }

                    _driver.Navigate().GoToUrl(p.URL); 
                    System.Threading.Thread.Sleep(1000);

                    Console.WriteLine(string.Format("_source: {0} / cssClass: {1} / attribute: {2}", p.Source, className, attribute));          

                    try
                    {
                        string detail = _driver.FindElement(By.ClassName(className)).GetAttribute(attribute);

                        string id = Guid.NewGuid().ToString();
                        string filetype = "html";

                        Attatchment attach = new Attatchment(id, p.Source, p.URL, filetype, detail, DateTime.Now);

                        p.Attatchments.Add(attach);
                    }
                    catch(Exception){}
                }
            }
        }
    }

    
}