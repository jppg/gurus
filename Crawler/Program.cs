using System;
using Gurus.Models;
using Gurus.Controllers;

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


            ExternalConnection conn = new ExternalConnection();
            Elastic elastic = new Elastic();

            elastic.DeleteAllIndex();
            elastic.SaveList(conn.FetchData()); 
            conn.Close();
        }

        

        
    }
}
