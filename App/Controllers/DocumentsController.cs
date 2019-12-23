using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Models;

namespace App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DocumentsController : ControllerBase
    {

        private readonly ILogger<DocumentsController> _logger;

        public DocumentsController(ILogger<DocumentsController> logger)
        {
            _logger = logger;
        }

        [HttpGet("{query}")]
        public string Get(string query)
        {
            string result = string.Empty;

            string url = "http://localhost:9200/attatchments/_search";
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
	        httpWebRequest.ContentType = "application/json";
	        httpWebRequest.Method = "POST";
	        httpWebRequest.Accept = "application/json";
	        	        
	        
            StringBuilder inputJson = new StringBuilder();
            inputJson.AppendLine("{");
            inputJson.AppendLine("\"from\": 0,");
            inputJson.AppendLine("\"size\": 1000,");
            inputJson.AppendLine("\"query\":{");
            inputJson.AppendLine("\"match\":{\"Body\": \""+query+"\"}");
  	        inputJson.AppendLine("},");
            inputJson.AppendLine("\"_source\": [\"Name\", \"URL\"],");
            inputJson.AppendLine("\"highlight\" :{");
            inputJson.AppendLine("\"fields\":{");
            inputJson.AppendLine("\"Body\":{}");
            inputJson.AppendLine("}");
            inputJson.AppendLine("}");
            inputJson.AppendLine("}");

            Console.WriteLine(inputJson.ToString());
	        
	        using (StreamWriter streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
	        {
	            streamWriter.Write(inputJson.ToString());
	            streamWriter.Flush();
	            streamWriter.Close();
	        }
	
	        HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
	        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
	        {
	            result = streamReader.ReadToEnd();
	            //pass.Text = result.ToString();
	            
	            //Console.WriteLine(result);
	            
	            //JObject jsonResult = JObject.Parse(result);
            }

            return result;
        }
    }
}
