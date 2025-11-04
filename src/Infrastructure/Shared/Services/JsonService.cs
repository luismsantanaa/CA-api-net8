using System.Data;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Shared.Services.Contracts;

namespace Shared.Services
{
    public class JsonService : IJsonService
    {
        private readonly JsonSerializerSettings _serializerOptions = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            NullValueHandling = NullValueHandling.Ignore,
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter() { NamingStrategy = new CamelCaseNamingStrategy() }
            },
            Formatting = Newtonsoft.Json.Formatting.Indented,
        };

        /// <summary>
        /// Serialize an object to Json format
        /// </summary>
        /// <param name="jsonObject">Object to Serialize</param>
        /// <returns>String Json Fomat</returns>
        public string ObjectToJson(object jsonObject)
        {
            return JsonConvert.SerializeObject(jsonObject, _serializerOptions);
        }

        /// <summary>
        /// DesSerialize a Json Object to a Class 
        /// </summary>
        /// <typeparam name="T">Destintion Class</typeparam>
        /// <param name="jsonObject">String Json format</param>
        /// <returns>Class intenced</returns>
        public T JsonToObject<T>(object jsonObject)
        {
            return JsonConvert.DeserializeObject<T>(jsonObject.ToString()!, _serializerOptions)!;
        }

        public string Serialize<T>(T obj, Type type)
        {
            return JsonConvert.SerializeObject(obj, type, new());
        }

        public DataTable JsonToTable(string jsonString)
        {
            return (DataTable)JsonConvert.DeserializeObject(jsonString, typeof(DataTable))!;
        }

        public string TableToJson(DataTable dt)
        {
            return JsonConvert.SerializeObject(dt, Newtonsoft.Json.Formatting.Indented);
        }

        public XmlDocument JsonToXML(string jsonString)
        {
            return JsonConvert.DeserializeXmlNode(jsonString)!;
        }

        public string XMLToJson(string XML)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(XML);

            return JsonConvert.SerializeXmlNode(doc);
        }
    }
}
