using System;
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

namespace Gurus.Crawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(@"
   ____                              
  / ___|  _   _   _ __   _   _   ___ 
 | |  _  | | | | | '__| | | | | / __|
 | |_| | | |_| | | |    | |_| | \__ \
  \____|  \__,_| |_|     \__,_| |___/                                  
            ");

            Program prg = new Program();
            prg.Login();
        }

        public void Login()
        {
            string cfg = File.ReadAllText("Login.txt");
            dynamic login = JsonConvert.DeserializeObject(cfg);

            var m_driver = new ChromeDriver("C:\\Aplics\\ChromeDriver");

            m_driver.Url = login.url;

            
            IWebElement emailTextBox = m_driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']",login.user.tag)));
            IWebElement passTextBox = m_driver.FindElement(By.XPath(string.Format(".//*[@id='{0}']",login.pass.tag)));
            IWebElement signUpButton = m_driver.FindElement(By.ClassName(string.Format("{0}",login.submit)));

            string user = Utils.Cryptography.DecryptString((string)login.user.value);
            string pass = Utils.Cryptography.DecryptString((string)login.pass.value);

            emailTextBox.SendKeys(user);
            passTextBox.SendKeys(pass);
            signUpButton.Click();
            
        }   

    }
}
