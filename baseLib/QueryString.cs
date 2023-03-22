using System;
using System.Collections.Immutable;
using System.Linq;
using System.Web;

namespace csharp_lib.baseLib
{
    internal class QueryString
    {
        public static string getQueryString<T>(T t,string filterKey="")
        {
            var properties = from p in t.GetType().GetProperties()
                             where p.GetValue(t, null) != null && p.Name != filterKey
                             select p.Name + "=" + HttpUtility.UrlEncode(p.GetValue(t, null).ToString());

            var c = properties.ToImmutableSortedSet();
            string queryString = String.Join("&", c);
            return queryString;
        }
    }
}
