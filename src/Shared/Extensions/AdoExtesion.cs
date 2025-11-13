using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;
using Newtonsoft.Json;

namespace Shared.Extensions
{
    public static class AdoExtesion
    {
        public static DataSet GetDataSet(this string xmlData)
        {
            var set = new DataSet();
            var buffer = Encoding.UTF8.GetBytes(xmlData);
            using (var stream = new MemoryStream(buffer))
            {
                var reader = XmlReader.Create(stream);
                set.ReadXml(reader);
            }
            return set;
        }
        public static List<T> TableToList<T>(DataTable dt)
        {
            List<T> data = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                T item = GetItem<T>(row);
                data.Add(item);
            }
            return data;
        }
        private static T GetItem<T>(DataRow dr)
        {
            Type temp = typeof(T);
            T obj = Activator.CreateInstance<T>();

            foreach (DataColumn column in dr.Table.Columns)
            {
                foreach (PropertyInfo pro in temp.GetProperties())
                {
                    try
                    {
                        if (pro.Name == column.ColumnName)
                        {
                            pro.SetValue(obj, Convert.ChangeType(dr[column.ColumnName], Nullable.GetUnderlyingType(pro.PropertyType) ?? pro.PropertyType), null);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
            return obj;
        }
        public static string DatasetToJson(DataSet Ds)
        {
            var Json = JsonConvert.SerializeObject(Ds, Newtonsoft.Json.Formatting.Indented);
            return Json;
        }
    }
}
