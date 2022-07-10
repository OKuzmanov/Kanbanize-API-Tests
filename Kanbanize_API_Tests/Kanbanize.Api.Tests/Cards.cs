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

        [XmlElement(ElementName = "subtaskdetails")]
        public SubtaskDetailItems subtaskdetails { get; set; }

        [XmlElement(ElementName = "comments")]
        public CommentItems commentItems { get; set; }
    }

    [XmlRoot(ElementName = "comments")]
    public class CommentItems
    {
        [XmlElement(ElementName ="item")]
        public List<CommentDetails> comments { get; set;}
    }

    [XmlRoot(ElementName ="item")]
    public class CommentDetails
    {
        [XmlElement(ElementName = "commentid")]
        public long commentId { get; set; }

        [XmlElement(ElementName = "author")]
        public string author { get; set; }

        [XmlElement(ElementName = "event")]
        public string eventName { get; set; }

        [XmlElement(ElementName = "text")]
        public string text { get; set; }
    }

    [XmlRoot(ElementName ="subtaskdetails")]
    public class SubtaskDetailItems
    {
        [XmlElement(ElementName = "item")]
        public List<SubtaskDetail> subtasks { get; set; }
    }

    public class SubtaskDetail
    {
        [XmlElement(ElementName = "taskid")]
        public long id { get; set; }

        [XmlElement(ElementName = "title")]
        public string title { get; set; }
    }

    [XmlRoot(ElementName = "xml")]
    public class RootXmlAddComment
    {
        [XmlElement(ElementName = "commentid")]
        public long commentId { get; set; }

        [XmlElement(ElementName = "commentText")]
        public string commentText { get; set; }
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

    [XmlRoot(ElementName = "xml")]
    public class RootXmlAddSubtask
    {
        [XmlElement(ElementName = "item")]
        public long id { get; set; }
    }
}
