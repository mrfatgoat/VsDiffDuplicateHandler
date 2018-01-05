using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace VsDiffDuplicateHandler.Services
{
    class XmlLoader : IXmlLoader
    {
        public XDocument Load(string xmlFile) => XDocument.Load(xmlFile);
    }
}
