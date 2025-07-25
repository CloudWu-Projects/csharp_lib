﻿using System;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
namespace csharp_lib.baseLib
{
    /// <summary>
    /// INI文件的操作类
    /// </summary>
    public class IniFile
    {
        public byte[] Path;
        private bool bWriteIni = false;

        bool isUtf8Bom(byte[] bytes)
        {
            return (bytes.Length >= 3) && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        }
        byte[] RemoveUtf8Bom(byte[] bytes)
        {
            return bytes[3..];
        }
        void ConvertIniFileToUTF8(string filePath)
        {
            byte[] fileBytes = File.ReadAllBytes(filePath);

            if (isUtf8Bom(fileBytes))
            {
                byte[] utf8Bytes = RemoveUtf8Bom(fileBytes);
                string utf8Str = Encoding.UTF8.GetString(utf8Bytes);
                File.WriteAllText(filePath, utf8Str);
            }
        }

        public IniFile()
        {
            string _path = ".\\config.ini";
            this.Path = getBytes(_path);
            bWriteIni = (!System.IO.File.Exists(_path));
            if (System.IO.File.Exists(_path))
            {
                ConvertIniFileToUTF8(_path);
            }
        }
        #region 声明读写INI文件的API函数 
        //[DllImport("kernel32",CharSet=CharSet.Unicode)]
        //private static extern long WritePrivateProfileString(byte[] section, byte[] key, byte[] val, string filePath);
        //[DllImport("kernel32", CharSet=CharSet.Unicode)]
        //private static extern int GetPrivateProfileString(string section, string key, string defVal, StringBuilder retVal, int size, string filePath);
        //[DllImport("kernel32")]
        //private static extern int GetPrivateProfileString(byte[] section, byte[] key, byte[] defVal, byte[] retVal, int size, string filePath);
        //[DllImport("kernel32", CharSet=CharSet.Unicode)]
        //private static extern int GetPrivateProfileString(string section, string key, string defVal, Byte[] retVal, int size, string filePath);
        [DllImport("kernel32")] public static extern bool WritePrivateProfileString(byte[] section, byte[] key, byte[] val, byte[] filePath);
        [DllImport("kernel32")] public static extern int GetPrivateProfileString(byte[] section, byte[] key, byte[] def, byte[] retVal, int size, byte[] filePath);
        #endregion
        /// <summary>
        /// 写INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <param name="iValue">值</param>
        public void IniWriteValue(string section, string key, string iValue)
        {
            var sec = getBytes(section);
            var keya = getBytes(key);
            var value = getBytes(iValue);
            WritePrivateProfileString(sec, keya, value, this.Path);
        }
        private static byte[] getBytes(string s, string encodingName = "utf-8")
        {
            return null == s ? null : Encoding.GetEncoding(encodingName).GetBytes(s);
        }
        /// <summary>
        /// 读取INI文件
        /// </summary>
        /// <param name="section">段落</param>
        /// <param name="key">键</param>
        /// <returns>返回的键值</returns>
        public string IniReadValue(string section, string key, string defaultValue = "")
        {
            if (bWriteIni)
                IniWriteValue(section, key, defaultValue);
            byte[] buffer = new byte[1024];
            int count = GetPrivateProfileString(getBytes(section), getBytes(key), getBytes(defaultValue), buffer, 1024, this.Path);
            return Encoding.GetEncoding("utf-8").GetString(buffer, 0, count).Trim();
        }
        //public string IniReadValue<T>(string section, string key, T defaultValue )
        //{
        //   return IniReadValue(section, key, defaultValue.ToString());
        //}
        public T IniReadValueT<T>(string section, string key, T defaultValue)
        {
            if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(IniReadValue(section, key, defaultValue.ToString()));
            }
            else if(typeof(T) == typeof(TimeSpan))
            {
                TimeSpan tempValue = defaultValue as TimeSpan? ?? TimeSpan.Zero;
                return (T)(object)TimeSpan.Parse(IniReadValue(section, key, tempValue.TotalSeconds.ToString()));
            }
            return (T)(object)IniReadValue(section, key, defaultValue.ToString());
        }
        public T IniReadValueT2<T>(string section, string key, T defaultValue,Type valueType)
        {
            if (valueType == typeof(int))
            {
                return (T)(object)int.Parse(IniReadValue(section, key, defaultValue.ToString()));
            }
            else if (valueType == typeof(bool))
            {
                var readValue = IniReadValue(section, key, defaultValue.ToString());

                return (T)(object)(readValue.ToLower()== "true");
            }
            else if (valueType == typeof(TimeSpan))
            {
                TimeSpan tempValue = defaultValue as TimeSpan? ?? TimeSpan.Zero;
                return (T)(object)TimeSpan.FromSeconds(Int32.Parse(IniReadValue(section, key, tempValue.TotalSeconds.ToString())));
            }
            return (T)(object)IniReadValue(section, key, defaultValue?.ToString());
        }
        public TimeSpan IniReadValue(string section,string key,TimeSpan defaultValue)
        {
             return  TimeSpan.FromSeconds(Int32.Parse(IniReadValue(section,key, defaultValue.TotalSeconds.ToString())));
        }
    }
}
