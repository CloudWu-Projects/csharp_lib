
namespace csharp_lib.baseLib
{
    internal class RepaceObjAttrib
    {
        internal static string replaceObjAtrribe(object datum, ref string sql)
        {
            var properties = datum.GetType().GetProperties();
            foreach (var property in properties)
            {
                var name = property.Name;
                var value = property.GetValue(datum);
                if (value != null)
                {
                    sql = sql.Replace($"{{json.{name}}}".ToLower(), value.ToString());
                }
            }
            return sql;
        }
    }
}
