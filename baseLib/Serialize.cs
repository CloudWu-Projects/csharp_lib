using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

using System.Xml.Serialization;
namespace MainAPP.csharp_lib.baseLib
{
    public static class Serialize
    {
        private static readonly JsonSerializerOptions _obj1 = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        private static readonly JsonSerializerOptions _obj2 = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,    // 属性名不区分大小写
            IncludeFields = false,                 // 不序列化字段（仅处理属性）
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping  // 宽松的JSON字符转义
        };
        private static readonly JsonSerializerOptions _json1 = new JsonSerializerOptions
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        public static TObject ToObject<TObject>(this string str) => JsonSerializer.Deserialize<TObject>(str, _obj1);
       
        public static string ToJsonWithFormat<TObject>(this TObject obj) => JsonSerializer.Serialize(obj, _json1);

        public static TObject ToObjectFromXml<TObject>(this string str) where TObject : class
        {
            using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(str)))
                return new XmlSerializer(typeof(TObject)).Deserialize(memoryStream) as TObject;
        }

        public static TObject ToObjectFromXmlFile<TObject>(this string filePath) where TObject : class
        {
            using (StreamReader streamReader = new StreamReader(filePath, Encoding.UTF8))
                return new XmlSerializer(typeof(TObject)).Deserialize(streamReader) as TObject;
        }


    }
}