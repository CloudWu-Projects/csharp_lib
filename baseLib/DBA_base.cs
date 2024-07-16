using HeiFei_20220103;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using wu_jiaxing20220115;

namespace csharp_lib.baseLib
{
    public class DBA_base
    {
        public MyLogger Logger = null;
        public SqlConnection conn;
        public bool isConnected = false;
        public DBA_base(string logName)
        {
            // Targets where to log to: File and Console

            Init(MyLogger.GetLogger(logName));
        }

        public DBA_base(MyLogger _logger)
        {
            Init(_logger);
        }
        void Init(MyLogger _logger)
        {
            Logger = _logger;
            Logger.Info("=================================================");
            Logger.Info("===============DBA_base=START============================");
        }
        public static string GetVarName(System.Linq.Expressions.Expression<Func<string, string>> exp)
        {
            var a = ((System.Linq.Expressions.MemberExpression)exp.Body);

            var b = a.Expression.Type.FullName+"." + a.Member.Name;
            return b;
        }
        public void loadSQL(string prefix,string dataSetItem, ref string output)
        {
            loadSQL(prefix+"_"+dataSetItem,ref output);
        }
        public void loadSQL(string dataSetItem,ref string output)
        {
            var sqlFolder = $"{System.Environment.CurrentDirectory}/sql";
            var path = $"{sqlFolder}/{dataSetItem}.sql";
            Logger.Info($"SQL path: {path}");
            if (!System.IO.File.Exists(path))
            {
                try { 
                    System.IO.Directory.CreateDirectory(sqlFolder);
                }
                catch (Exception es) { }
                System.IO.File.WriteAllText(path, output, System.Text.Encoding.UTF8);
            }
            output = System.IO.File.ReadAllText(path, System.Text.Encoding.UTF8);
            Logger.Info($"{dataSetItem}: {output}");
        }
        public string getFirstValue_bySQL(string sql)
        {
            string result = "";
            try
            {
                using (var reader = ExecuteReader(sql))
                {
                    if (reader.Read() && reader.HasRows)
                    {
                        result = reader.GetString(0);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"{sql} Exception " + ex.Message + "\n" + ex.StackTrace);
            }
            return result;
        }
        public int getcount_bySQL(string sql)
        {
            int result = -1;
            string resultStr = getFirstValue_bySQL(sql);
            if (!string.IsNullOrEmpty(resultStr))
                result = int.Parse(resultStr);
            return result;
        }

        public bool Open(string dbServer,string dbName,string dbUserName,string dbPassword)
        {
            if (isConnected)
                return true;
            try
            {
                var cs = $@"Data Source={dbServer};Initial Catalog={dbName};User ID={dbUserName};Password={dbPassword}";
                Logger.Info($"Db Connectstr :{cs}");
                var stm = "SELECT @@VERSION";
                conn = new SqlConnection(cs);
                conn.Open();
                using var cmd = new SqlCommand(stm, conn);
                string version = cmd.ExecuteScalar().ToString();
                Logger.Info($"SQL version:{version}");
                isConnected = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"exception {ex.Message}\n{ex.StackTrace}");
                isConnected= false;
            }
            return isConnected;
        }

        public SqlDataReader ExecuteReader(string sql)
        {
            try
            {
                Logger.Debug($"ExecuteReader sql  {sql}");
                using var cmd = new SqlCommand(sql, conn);
                return cmd.ExecuteReader();
            }
            catch (Exception ex)
            {
                Logger.Error($"ExecuteReader exception \n {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }
        public int ExcuteProcedure_nonQuery(string proceDureName)
        {
            try
            {
                Logger.Debug($"ExcuteProcedure {proceDureName}");
                SqlCommand aCommand = new SqlCommand(proceDureName, conn);
                aCommand.CommandType = CommandType.StoredProcedure;
                int i= aCommand.ExecuteNonQuery();

                Logger.Debug($"ExcuteProcedure={i} [{proceDureName}]");
                return i;
            }
            catch (Exception ex)
            {
                Logger.Error($"ExcuteNonQuery exception \n {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }
        public string ExcuteProcedure_withResult(string proceDureName)
        {
            try
            {
                Logger.Debug($"ExcuteProcedure_withResult {proceDureName}");
                SqlCommand aCommand = new SqlCommand(proceDureName, conn);
                aCommand.CommandType = CommandType.StoredProcedure;

                aCommand.Parameters.AddWithValue("@pin1", "aaa");

                SqlParameter parOutput = aCommand.Parameters.Add("@pout1",SqlDbType.NVarChar,50);
                parOutput.Direction = ParameterDirection.Output;



                SqlParameter returnValueParam = new SqlParameter();
                returnValueParam.ParameterName = "@returnValue";
                returnValueParam.Direction = ParameterDirection.ReturnValue;
                aCommand.Parameters.Add( returnValueParam );

                int i = aCommand.ExecuteNonQuery();
                int rv = (int)aCommand.Parameters["@returnValue"].Value;

                Logger.Debug($"ExcuteProcedure_withResult={i} [{proceDureName}]  output:{parOutput.Value}");
                return parOutput.Value.ToString();
            }
            catch (Exception ex)
            {
                Logger.Error($"ExcuteProcedure_withResult exception \n {ex.Message}\n {ex.StackTrace}");
                throw;
            }
            return "";
        }

        public int ExcuteNonQuery(string sql)
        {
            try
            {
                Logger.Debug($"ExcuteNonQuery sql  {sql}");
                using var cmd = new SqlCommand(sql, conn);
                int i = cmd.ExecuteNonQuery();
                Logger.Debug($"ExcuteNonQuery={i} [{sql}]");
                return i;
            }
            catch (Exception ex)
            {
                Logger.Error($"ExcuteNonQuery exception \n {ex.Message}\n {ex.StackTrace}");
                throw;
            }
        }
        public string convertPlateTypeFromColor(string colorText)
        {
            if (colorText == "蓝色") return "2";
            if (colorText == "黄色") return "3";
            return colorText;
        }
        public string convertMachineStatus(string status)
        {
            return status;
        }
        public string convertMachineType(string type)
        {
            if (type == "装载机") return "1";
            if (type == "叉车") return "2";
            if (type == "泵车") return "3";
            if (type == "清扫车") return "4";
            if (type == "其他") return "5";
            return type;
        }
        public string convertPlateType(string typeText)
        {
            if (typeText == "微型车") return "1";
            if (typeText == "小型车") return "2";
            if (typeText == "紧凑型车") return "3";
            if (typeText == "中型车") return "4";
            if (typeText == "中大型车") return "5";
            return typeText;
        }
        public string convertFuelType(string typeText)
        {
            if (typeText == "汽油") return "A";
            if (typeText == "柴油") return "B";
            if (typeText == "混合油") return "C";
            if (typeText == "天然气") return "D";
            if (typeText == "液化石油气") return "E";
            if (typeText == "甲醇") return "F";
            if (typeText == "乙醇") return "L";
            if (typeText == "太阳能") return "M";
            if (typeText == "电") return "N";
            if (typeText == "混合动力") return "O";
            if (typeText == "氢") return "P";
            if (typeText == "生物燃料") return "Q";
            if (typeText == "无") return "Y";
            if (typeText == "其他") return "Z";
            return typeText;
        }
        public string covertEmissionStandard(string typeText)
        {
            if (typeText == "国1") return "1";
            if (typeText == "国2") return "2";
            if (typeText == "国3") return "3";
            if (typeText == "国4") return "4";
            if (typeText == "国5") return "5";
            if (typeText == "国6") return "6";
            if (typeText == "电动") return "D";

            if (typeText == "无排放阶段") return "X";
            return typeText;

        }
        public string getplateColor(string colorText)
        {
            if (colorText == "蓝色") return "0";
            if (colorText == "黄色") return "1";
            if (colorText == "白色") return "2";
            if (colorText == "黑色") return "3";
            if (colorText == "新能源") return "4";
            if (colorText == "其他")
                return "5";
            return "0";
        }

        public string getDBString(object ob)
        {
            if (ob == null || ob == DBNull.Value) return "";
            var b =string.Format("{0}", ob);
            return b;            
        }
        public int getDBInt(object ob)
        {
            if (ob == null || ob == DBNull.Value) return 0;
            return (int)ob;
        }
        public DateTime getDBDatetime(object ob)
        {
            if (ob == null || ob == DBNull.Value) return DateTime.Now;
            return Convert.ToDateTime(ob);
        }
        public long getDBDatetimestamp(object ob)
        {
            var aa = typeof(object);
            //2016-12-12 12:11:20,
            if (ob == null || ob == DBNull.Value) return 0;
            var timeStamp = new DateTimeOffset(Convert.ToDateTime(ob)).ToUnixTimeSeconds();

            return timeStamp;
        }
        public bool try_getDBValue<T>(SqlDataReader rdr, string fieldName, ref T t1, string outKeyName)
        {
            try
            {
                if (getDBValue(rdr, fieldName, ref t1, outKeyName))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"try_getDBValue exception \n {ex.Message}\n {ex.StackTrace}");
            }
            Logger.Debug($"try_getDBValue {fieldName}  {outKeyName}={t1.ToString()}");
            throw new Exception($"try_getDBValue {fieldName}  {outKeyName}={t1.ToString()}");
            return false;
        }
        public bool getDBValue<T>(SqlDataReader reader, ref T t)
        {
            try
            {
                var properties = typeof(T).GetProperties();
                foreach (var property in properties)
                {
                    var attribute = property.GetCustomAttribute<ColumnMappingAttribute>();
                    var columnName = attribute?.ColumnName ?? property.Name;
                    if (reader[columnName] != DBNull.Value)
                    {
                        object value = getDBValue(reader, columnName, property.PropertyType);

                        property.SetValue(t, value);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error($"getDBValue exception \n {ex.Message}\n {ex.StackTrace}");
            }
            return false; ;
        }
        object getDBValue(SqlDataReader rdr,string fieldName, Type  t1)
        {
            //2016-12-12 12:11:20,
            var ob = rdr[fieldName];
            if (ob == null || ob == DBNull.Value)
            {
                return null;
            }
            if (ob.GetType() == t1) 
                return ob;

            var value = string.Format("{0}", ob);
            try
            {
                if (t1 == typeof(string))
                {
                    if (ob.GetType() == typeof(System.DateTime))
                    {
                        DateTime otime = Convert.ToDateTime(ob);
                        return (object)otime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                        return (object)value;
                }
                else if (t1 == typeof(int) || t1 == typeof(long))
                {
                    if (ob.GetType() == typeof(System.DateTime))
                    {
                        var timeStamp = new DateTimeOffset(Convert.ToDateTime(ob)).ToUnixTimeSeconds();
                        return (object)timeStamp;
                    }
                    else
                    {
                        if (t1 == typeof(int))
                            return (object)int.Parse(value);
                        else
                            return (object)long.Parse(value);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error($" getDBValue({ob.GetType()} fieldName:{fieldName} value={value}  {t1}");
                throw ex;
            }
            return null;
        }
        public bool getDBValue<T>(SqlDataReader rdr,string fieldName, ref T t1,string outKeyName)
        {
            //2016-12-12 12:11:20,
            var ob = rdr[fieldName];
            if (ob == null || ob == DBNull.Value)
            {
                return false ;
            }
            var value = string.Format("{0}", ob);
            try
            {
                if (typeof(T) == typeof(string))
                {
                    if(ob.GetType() == typeof(System.DateTime))
                    {
                        DateTime otime = Convert.ToDateTime(ob);
                        t1 = (T)(object)otime.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    else
                        t1 = (T)(object)value;
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(long))
                {
                    if (ob.GetType() == typeof(System.DateTime))
                    {
                        var timeStamp = new DateTimeOffset(Convert.ToDateTime(ob)).ToUnixTimeSeconds();
                        t1 = (T)(object)timeStamp;
                    }
                    else
                    {
                        if (typeof(T) == typeof(int))
                            t1 = (T)(object)int.Parse(value);
                        else
                            t1 = (T)(object)long.Parse(value);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Error($" getDBValue({ob.GetType()} fieldName:{fieldName} value={value} {outKeyName} {typeof(T)}");
                throw ex;
                return false;
            }
            return true ;
        }
        public void getDBValue<T>(object ob, ref T t1)
        {
            //2016-12-12 12:11:20,
            if (ob == null || ob == DBNull.Value)
            {
                return;
            }
            var value = string.Format("{0}", ob);
            try
            {
                if (typeof(T) == typeof(string))
                {
                    t1 = (T)(object)value;
                }
                else if (typeof(T) == typeof(int))
                {
                    t1 = (T)(object)int.Parse(value);
                }
                else if (typeof(T) == typeof(long))
                {
                    t1 = (T)(object)long.Parse(value);
                }
            }
            catch (Exception ex)
            {
                Logger.Error($" getDBValue value={value}  {typeof(T)}");
                throw ex;
            }
        }

        public string getString(object value)
        {
            if (value == DBNull.Value) return "";
            return (string)value;
        }
        public virtual void ThreadChild() { }
        Thread th = null;
        public void doWork()
        {
            th = new Thread(ThreadChild);
            th.Start();
        }
        public void wait()
        {
            th.Join();
        }
    }
}
