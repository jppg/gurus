using System;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;
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

            dynamic _sources = JsonConvert.DeserializeObject(File.ReadAllText("Sources.txt"));

            string DEFAULT_INDEX = "people";

            foreach(var source in _sources)
            {
                bool active = source.active ?? false;
                Console.WriteLine(string.Format("==> Source: {0} active:({1})", source.source, source.active));

                if(active)
                {
                    string index = source.index ?? DEFAULT_INDEX;
                    bool truncateIndex = source.truncate ?? false;
                    bool getUrlDetail = source.getUrlDetail ?? false;

                    Console.WriteLine("Index: " + index);
                    Console.WriteLine("Truncate: " + truncateIndex);

                    ExternalConnection conn = new ExternalConnection(source);

                    Elastic elastic = new Elastic(index);

                    //Clean items on elasticsearch
                    if(truncateIndex)
                    {
                        Console.WriteLine("Truncate index " + index);
                        elastic.DeleteAllIndex();
                    }

                    if(index == DEFAULT_INDEX)
                    {
                        //Get initial data
                        List<Person> lstPersons = conn.FetchPersonData();

                        //Get details
                        if(getUrlDetail)
                        {
                            conn.GetAttatchments(ref lstPersons);
                        }

                        //Save list of people on elasticsearch
                        elastic.SaveList(lstPersons); 
                    }
                    else if(index == "attatchments")
                    {
                        List<Attatchment> lstAttachemts = conn.FetchAttachementsData();
                        elastic.SaveList(lstAttachemts);
                    }
                    
                    conn.CloseDriver();
                }
            }
        }

        

        
    }
}
