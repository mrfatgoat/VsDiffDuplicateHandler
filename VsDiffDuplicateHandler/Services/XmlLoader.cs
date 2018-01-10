using System.Xml.Linq;
using VsDiffDuplicateHandler.Services.Interfaces;

namespace VsDiffDuplicateHandler.Services
{
    class XmlLoader : IXmlLoader
    {
        public XDocument Load(string xmlFile) => XDocument.Load(xmlFile);
    }
}
