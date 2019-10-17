using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;
using System.IO;
using System.Runtime;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Gurus.Utils;
using Gurus.Models;
using Gurus.Controllers;

namespace Gurus.Controllers
{
    public class ExternalConnection
    {
        ChromeDriver _driver = new ChromeDriver("C:\\Aplics\\ChromeDriver");
        const int NMAX_PAGES = 1;

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
                    signUpButton = _driver.FindElement(By.ClassName(string.Format("{0}", login.submit)));
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


        public List<Person> FetchData()
        {
            string cfgFile = File.ReadAllText("Sources.txt");
            dynamic sources = JsonConvert.DeserializeObject(cfgFile);

            DateTime timestamp = DateTime.Now;

            List<Person> lstPersons = new List<Person>();

            foreach(var src in sources)
            {
                 if(!Login(src.login))
                    throw new Exception("Error on login");

                foreach(var keyword in src.keywords)
                {
                    for(int n = 0; n <= NMAX_PAGES; n++)
                    {
                        string url = (string)src.url;
                        
                        Console.WriteLine(string.Format("===>> GET: {0}", url));

                        string key = (string)keyword;
                        url = url.Replace("{{keyword}}", key).Replace("{{pageNumber}}", n.ToString());

                        Console.WriteLine(string.Format("===>> GET: {0}", url));
                        
                        _driver.Navigate().GoToUrl(url);
                        string html = _driver.PageSource;

                        Console.WriteLine((string)src.listResults);

                        ReadOnlyCollection<IWebElement> itemList = _driver.FindElements(By.ClassName((string)src.listResults));
                        //if(itemList.Count == 0)
                        //    itemList = _driver.FindElementsByCssSelector("li");

                        foreach(var item in itemList)
                        {
                            Person person = new Person();

                            person.Search = keyword;
                            person.Timestamp = timestamp;

                            string className = (string)src.name.cssClass;
                            string attribute = (string)src.name.attribute;
                            person.Name = item.FindElement(By.ClassName(className)).GetAttribute(attribute);

                            className = (string)src.position.cssClass;
                            attribute = (string)src.position.attribute;
                            person.Position = item.FindElement(By.ClassName(className)).GetAttribute(attribute);
                            

                            className = (string)src.location.cssClass;
                            attribute = (string)src.location.attribute;
                            person.Location = item.FindElement(By.ClassName(className)).GetAttribute(attribute);
                            

                            className = (string)src.photo.cssClass;
                            attribute = (string)src.photo.attribute;
                            person.Photo = item.FindElement(By.ClassName(className)).GetAttribute(attribute);
                            
                            className = (string)src.link.cssClass;
                            attribute = (string)src.link.attribute;
                            person.URL = item.FindElement(By.ClassName(className)).GetAttribute(attribute);

                            try
                            {
                                className = (string)src.id.cssClass;
                                attribute = (string)src.id.attribute;
                                person.Id = item.FindElement(By.ClassName(className)).GetAttribute(attribute);
                            }
                            catch(Exception)
                            {
                                person.Id = person.URL;
                            }

                            className = (string)src.distance.cssClass;
                            attribute = (string)src.distance.attribute;
                            person.Distance = item.FindElement(By.ClassName(className)).GetAttribute(attribute);

                            //Format data
                            person.Name = person.Name.Replace(person.Distance, "");


                            //Output results
                            Console.WriteLine(string.Format("» Name: {0} / Position: {1} / URL: {2} / Distance: {3} / Photo: {4} / Location: {5}", person.Name, person.Position, person.URL, person.Distance, person.Photo, person.Location));

                            lstPersons.Add(person);
                        }
                        
                    }
                }
                
            }   

            return lstPersons;
        }

        public void Close()
        {
            _driver.Close();
        }
    }

    
}