using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kanbanize.Api.Tests
{
    [XmlRoot(ElementName ="xml")]
    public class RootXmlCreateCards
    {
        [XmlElement(ElementName ="id")]
        public long id { get; set; }

        [XmlElement(ElementName = "details")]
        public Details details { get; set; }
    }

    [XmlRoot(ElementName = "details")]
    public class Details
    {
        [XmlElement(ElementName = "taskid")]
        public long taskId { get; set; }

        [XmlElement(ElementName = "boardid")]
        public long boardId { get; set; }
       
        [XmlElement(ElementName = "title")]
        public string title { get; set; }

        [XmlElement(ElementName = "description")]
        public string description { get; set; }

        [XmlElement(ElementName = "priority")]
        public string priority { get; set; }
        
        [XmlElement(ElementName = "deadlineoriginalformat")]
        public string deadLine { get; set; }
        
        [XmlElement(ElementName = "lanename")]
        public string laneName { get; set; }
    }

    [XmlRoot(ElementName = "xml")]
    public class RootXmlGetTaskDetails
    {
        [XmlElement(ElementName = "taskid")]
        public long taskId { get; set; }

        [XmlElement(ElementName = "boardid")]
        public long boardId { get; set; }

        [XmlElement(ElementName = "title")]
        public string title { get; set; }

        [XmlElement(ElementName = "description")]
        public string description { get; set; }

        [XmlElement(ElementName = "priority")]
        public string priority { get; set; }

        [XmlElement(ElementName = "deadlineoriginalformat")]
        public string deadLine { get; set; }

        [XmlElement(ElementName = "lanename")]
        public string laneName { get; set; }

        [XmlElement(ElementName = "color")]
        public string color { get; set; }
    }

    [XmlRoot(ElementName = "xml")]
    public class RootXmlInvalidCardInput
    {
        [XmlElement(ElementName ="item")]
        public string errMsg { get; set; }
    }

    [XmlRoot(ElementName = "xml")]
    public class RootXmlAllCardsFromBoard
    {
        [XmlElement(ElementName = "item")]
        public List<Details> cards { get; set; }
    }
}
