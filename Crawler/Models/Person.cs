using System;
using System.Collections.Generic;

namespace Gurus.Models
{
    public class Person
    {
        public string Id {get; set;}
        public string Source {get; set;}
        public DateTime Timestamp {get; set;}
        public string Name {get; set;}
        public string Position {get; set;}
        public string Location {get; set;}
        public string Photo {get; set;}
        public string Connections {get; set;}
        public string Distance {get; set;}
        public string Email {get; set;}
        public string Phone {get; set;}
        public DateTime Birthday {get; set;}
        public string Search {get; set;}
        public string URL {get; set;}
        public List<string> Tags {get; set;}
        public List<Alias> Alias {get; set;}
        public List<Contact> Contacts {get; set;}
        public List<Attatchment> Attatchments {get; set;}

        public Person()
        {
            Id =  string.Empty;
            Name = string.Empty;
            Position = string.Empty;
            Location = string.Empty;
            Photo = string.Empty;
            Connections = string.Empty;
            Distance = string.Empty;
            Search = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            URL = string.Empty;
            Alias = new List<Alias>();
            Tags = new List<string>(); 
            Contacts = new List<Contact>();
            Attatchments = new List<Attatchment>(); 
        }

    }

    public class Alias
    {
        public string Source {get; set;}
        public string Value {get; set;}
        public Alias(string source, string value)
        {
            Source = source;
            Value = value;
        }
    }

    public class Contact
    {
        public string PointOfContact {get; set;}
        public DateTime Timestamp {get; set;}
        public string Text {get; set;}

        public Contact(string pointOfContact, DateTime timestamp, string text)
        {
            PointOfContact = pointOfContact;
            Timestamp = timestamp;
            Text = text;
        }

        public Contact(string text)
        {
            PointOfContact = "Unknown";
            Timestamp = DateTime.Now;
            Text = text;
        }
    }

    public class Attatchment
    {
        public string Id {get; set;}
        public string Name {get; set;}
        public string URL {get; set;}
        public string FileType {get; set;}
        public string Body {get; set;}
        public DateTime CreationDate {get; set;}

        public Attatchment(string id, string name, string url, string filetype, string body, DateTime creationDate)
        {
            Id = id;
            Name = name;
            URL = url;
            FileType = filetype;
            Body = body;            
            CreationDate = creationDate;
        }
    }
}