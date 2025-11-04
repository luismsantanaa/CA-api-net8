using System.Data;
using System.Xml;

namespace Shared.Services.Contracts
{
    public interface IJsonService
    {
        string ObjectToJson(object jsonObject);
        T JsonToObject<T>(object jsonObject);
        string Serialize<T>(T obj, Type type);
        DataTable JsonToTable(string jsonString);
        string TableToJson(DataTable dt);
        XmlDocument JsonToXML(string jsonString);
        string XMLToJson(string XML);
    }
}
