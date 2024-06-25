
namespace csharp_lib.baseLib
{
    internal class RepaceObjAttrib
    {
        internal static string replaceObjAtrribe(object datum, ref string sql)
        {
            var properties = datum.GetType().GetProperties();
            var fields = datum.GetType().GetFields();
            var newSql = sql.ToLower();
            foreach (var property in properties)
            {
                var name = property.Name;
                var value = property.GetValue(datum);
                if (value != null)
                {
                    newSql = newSql.Replace($"{{json.{name}}}".ToLower(), value.ToString())
                        .Replace($"json.{name}".ToLower(), value.ToString());
                }
            }
            foreach (var property in fields)
            {
                var name = property.Name;
                var value = property.GetValue(datum);
                if (value != null)
                {
                    newSql = newSql.Replace($"{{json.{name}}}".ToLower(), value.ToString())
                        .Replace($"json.{name}".ToLower(), value.ToString());
                }
            }
            return newSql;
        }
    }
}
