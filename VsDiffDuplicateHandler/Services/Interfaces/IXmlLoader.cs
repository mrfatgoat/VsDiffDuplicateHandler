using System.Xml.Linq;

namespace VsDiffDuplicateHandler.Services.Interfaces
{
    public interface IXmlLoader
    {
        XDocument Load(string xmlFile);
    }
}
