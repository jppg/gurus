using System;
using System.Collections.Generic;
using Elasticsearch.Net;
using System.Linq;
using Gurus.Models;

namespace Gurus.Controllers
{
    public class Elastic
    {
        private string _address = "http://localhost:9200";
        private string _index = "people";
        private int _timeoutMinutes = 2;

        private ElasticLowLevelClient elastic = null;
        //https://www.elastic.co/guide/en/elasticsearch/client/net-api/master/elasticsearch-net-getting-started.html

        public Elastic()
        {
            var settings = new ConnectionConfiguration(new Uri(_address))
                .RequestTimeout(TimeSpan.FromMinutes(_timeoutMinutes));

            elastic = new ElasticLowLevelClient(settings);
        }

        public bool SaveList(List<Person> lstToSave)
        {
            foreach(Person p in lstToSave)
            {
                var ndexResponse = elastic.Index<BytesResponse>(_index, p.Id, PostData.Serializable(p)); 
                byte[] responseBytes = ndexResponse.Body;
            }
            return true;
        }

        public BytesResponse DeleteAllIndex()
        {
            return elastic.DeleteByQuery<BytesResponse>(_index, PostData.Serializable(new
                            {
                                query = new
                                {
                                    match_all = new
                                    {
                                    }
                                }
                            }
                        ));
        }

        public Person SearchPersonById(string id)
        {
            Person person = new Person();
            //http://localhost:9200/people/_doc/6f91353b-974e-402e-b2e6-2018f8dae3e7

            
            return person;
        }

        public List<Person> SearchByTerm(PostData body)
        {
            List<Person> results = new List<Person>();
            /*

            https://dzone.com/articles/23-useful-elasticsearch-example-queries

            {
                "query": {
                    "multi_match" : {
                        "query" : "PL/SQL",
                        "fields" : ["Name", "Position"]
                    }
                }
            } 

            var request = new GetRequest("myindex", "mytype", "1")
            {
                Fields = new PropertyPathMarker[] { "content", "name", "id" }
            };

            var response = client.Get<ElasticsearchProject>(request);
            */

            return results;
        }

        public void SearchById(string id)
        {
            var response = elastic.Get<BytesResponse>(_index, id); 
            //byte[] responseBytes = nResponse.Body;
            
            /*
            var name = response.Fields.FieldValue<string>(p => p.Name);
            var id = response.Fields.FieldValue<int>(p => p.Id);
            var doubleValue = response.Fields.FieldValue<double>(p => p.DoubleValue);
            */
        }
    }
}