using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wu_jiaxing20220115.csharp_lib.baseLib
{
    internal class TimeUtils
    {
       static public DateTime ConvertLongToDateTime(long unixTimeStamp)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            DateTime dt = startTime.AddSeconds(unixTimeStamp);
            return dt;
        }
    }
}
