using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace wu_jiaxing20220115.csharp_lib.baseLib
{
    using Org.BouncyCastle.Crypto.Digests;
    using System;
    using System.Text;

    public class SM3
    {
        public static string ComputeSM3Hash(string input)
        {
            // 将输入字符串转换为字节数组
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            // 创建 SM3 摘要实例
            SM3Digest sm3 = new SM3Digest();

            // 更新摘要内容
            sm3.BlockUpdate(inputBytes, 0, inputBytes.Length);

            // 获取最终的哈希值
            byte[] result = new byte[sm3.GetDigestSize()];
            sm3.DoFinal(result, 0);

            // 将字节数组转换为十六进制字符串
            return BitConverter.ToString(result).Replace("-", "").ToLower();
        }
    }

    public class TestSM3
    {
      public  static void Test()
        {
            string input = "Hello, SM3!";
         
            Console.WriteLine("SM3 Hash:");
            var goHash = "21b937fed61e685b8ac08c67fe9a3300437f2ca44547dea06e0cfe30219fdc4c";
            Console.WriteLine(goHash);

            var xhash =SM3.ComputeSM3Hash(input);
            Console.WriteLine(xhash);
        }
       
    }

}
