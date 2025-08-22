using System.Reflection;

namespace csharp_lib.baseLib
{
    public static class AutoLoadValue
    {
        public static void autoload<T>(T pthis, IniFile iniFile)
        {
            var properties = typeof(T).GetProperties();

            foreach (var prop in properties)
            {
                var dname = prop.DeclaringType.Name;
                var columnName = prop.Name;
                object value = prop.GetValue(pthis);
                Type propType = prop.PropertyType;
                var aa = iniFile.IniReadValueT2(dname, columnName, value, propType);
                prop.SetValue(pthis, aa);
            }
            FieldInfo[] fields = typeof(T).GetFields(
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);//| BindingFlags.DeclaredOnly);
            foreach (var field in fields)
            {
                // Console.WriteLine($"{field.Name}: {field.GetValue(new Person())}");
                var dname = field.DeclaringType.Name;
                Console.WriteLine(field.Name);
                var columnName = field.Name;
                object value = field.GetValue(pthis);
                Type propType = field.FieldType;
                if (propType != typeof(string)
                    && propType != typeof(int)
                    && propType != typeof(TimeSpan)
                    && propType != typeof(Boolean)
                    )
                {
                    Console.WriteLine($"field {field.Name} type {propType} not supported");
                    continue;
                }
                var aa = iniFile.IniReadValueT2(dname.ToString(), columnName, value, propType);
                field.SetValue(pthis, aa);
            }
        }
        public static bool AutoLoadX<T>(this IniFile iniFile, T pValue)
        {
            try
            {
                autoload<T>(pValue, iniFile);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoLoad Exception:{ex.Message}");
                return false;
            }
            return false;
        }
    }

    public static class  MyIniExtern
    {
        public static T IniReadValueT<T>(this IniFile iniFile,string section, string key, T defaultValue)
        {
            return iniFile.IniReadValueT2(section, key, defaultValue, typeof(T));
        }
        public static T IniReadValueT2<T>(this IniFile iniFile, string section, string key, T defaultValue, Type valueType)
        {
            if (valueType == typeof(int))
            {
                return (T)(object)int.Parse(iniFile.IniReadValue(section, key, defaultValue.ToString()));
            }
            else if (valueType == typeof(Boolean) || valueType == typeof(bool))
            {
                var readValue = iniFile.IniReadValue(section, key, defaultValue.ToString());

                return (T)(object)(readValue.ToLower() == "true");
            }
            else if (valueType == typeof(TimeSpan))
            {
                TimeSpan tempValue = defaultValue as TimeSpan? ?? TimeSpan.Zero;
                return (T)(object)TimeSpan.FromSeconds(Int32.Parse(iniFile.IniReadValue(section, key, tempValue.TotalSeconds.ToString())));
            }
            return (T)(object)iniFile.IniReadValue(section, key, defaultValue?.ToString());
        }
    }
}