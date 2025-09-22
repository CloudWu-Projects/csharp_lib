using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Serialization;
namespace MainAPP.csharp_lib.baseLib
{
    public static class Serialize
    {
        #region Json_serialization
        #region Json_serialization_options
        private static readonly JsonSerializerOptions DefaultDeserializeOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static readonly JsonSerializerOptions PropertyOnlyOptions = new(DefaultDeserializeOptions)
        {
            IncludeFields = false
        };

        private static readonly JsonSerializerOptions PrettyPrintOptions = new()
        {
            WriteIndented = true,
            IncludeFields = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        #endregion
        public static TObject ToObject<TObject>(this string json) =>
            JsonSerializer.Deserialize<TObject>(json, DefaultDeserializeOptions);

        public static TObject ToObject<TObject>(this string json, bool includeFields) =>
            JsonSerializer.Deserialize<TObject>(json,
                includeFields ? DefaultDeserializeOptions : PropertyOnlyOptions);

        public static TObject ToObject<TObject>(this string json,IList<JsonConverter> jsonConverters)
        {
            var options = new JsonSerializerOptions(DefaultDeserializeOptions)
            {
                Converters = { }
            };
            if (jsonConverters != null)
            {
                foreach (var converter in jsonConverters)
                {
                    options.Converters.Add(converter);
                }
            }
            return JsonSerializer.Deserialize<TObject>(json, options);

        }

        public static string ToJsonWithFormat<TObject>(this TObject obj) => JsonSerializer.Serialize(obj, PropertyOnlyOptions);

        #endregion


        #region Xml_serialization
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
        #endregion

    }
}