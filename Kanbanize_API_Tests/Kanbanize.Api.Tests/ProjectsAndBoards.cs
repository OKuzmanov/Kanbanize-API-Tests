using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Kanbanize.Api.Tests
{
    [XmlRoot(ElementName = "xml")]
    public class RootXmlProjectsAndBoards
    {

        [XmlElement(ElementName = "projects")]
        public Projects projects { get; set; }

    }

    [XmlRoot(ElementName = "projects")]
    public class Projects
    {
        [XmlElement(ElementName = "item")]
        public List<ProjectItem> projectItems { get; set; }
    }

    [XmlRoot(ElementName = "item")]
    public class ProjectItem
    {
        [XmlElement(ElementName = "id")]
        public long id { get; set; }

        [XmlElement(ElementName = "name")]
        public string name { get; set; }

        [XmlElement(ElementName = "boards")]
        public Boards boards { get; set; }
    }

    [XmlRoot(ElementName = "boards")]
    public class Boards
    {
        [XmlElement(ElementName = "item")]
        public List<BoardItem> boardItems { get; set; }


    }

    [XmlRoot(ElementName ="item")]
    public class BoardItem
    {
        [XmlElement(ElementName = "id")]
        public long id { get; set; }

        [XmlElement(ElementName = "name")]
        public string name { get; set; }
    }
}
