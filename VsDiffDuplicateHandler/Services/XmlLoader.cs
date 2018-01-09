using System.Xml.Linq;

namespace VsDiffDuplicateHandler.Services
{
    class XmlLoader : IXmlLoader
    {
        public XDocument Load(string xmlFile) => XDocument.Load(xmlFile);
    }
}
