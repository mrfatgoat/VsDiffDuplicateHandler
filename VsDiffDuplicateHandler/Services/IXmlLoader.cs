using System.Xml.Linq;

namespace VsDiffDuplicateHandler.Services
{
    public interface IXmlLoader
    {
        XDocument Load(string xmlFile);
    }
}
