using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;
using System.Numerics;
using System.Text;

namespace MainAPP.csharp_lib.baseLib
{
    public class SM2KeyHelper
    {
        public static ECPublicKeyParameters GetPublicKeyFromPem(string pemPublicKey)
        {
            using (var reader = new StringReader(pemPublicKey))
            {
                var pemReader = new PemReader(reader);
                var publicKey = (ECPublicKeyParameters)pemReader.ReadObject();
                return publicKey;
            }
        }
        // 从PEM格式恢复私钥
        public static ECPrivateKeyParameters PrivateKeyFromPem(string pem)
        {
            using (StringReader sr = new StringReader(pem))
            {
                PemReader pemReader = new PemReader(sr);
                return (ECPrivateKeyParameters)pemReader.ReadObject();
            }
        }
    }


    public class SM2Encryptor
    {
        public static byte[] Encrypt(byte[] data, ECPublicKeyParameters publicKey)
        {
            // 创建SM2加密引擎
            SM2Engine engine = new SM2Engine();
            // 初始化加密器（使用公钥）
            engine.Init(true, new ParametersWithRandom(publicKey, new SecureRandom()));

            // 执行加密
            return engine.ProcessBlock(data, 0, data.Length);
        }
        public static string Encrypt(string data, string base64PublicKey)
        {
            string pemPublicKey = $"-----BEGIN PUBLIC KEY-----\n{base64PublicKey}\n-----END PUBLIC KEY-----";

            // 创建公钥参数对象
            ECPublicKeyParameters publicKey = SM2KeyHelper.GetPublicKeyFromPem(pemPublicKey);
            // 执行加密
            byte[] cipherBytes = Encrypt(Encoding.UTF8.GetBytes(data), publicKey);

            // 转换为C1 + C3 + C2格式
            // cipherBytes默认是C1 + C2 + C3格式，需要转换为C1 + C3 + C2
            // C1: 65字节 (04 + x + y)
            // C3: 32字节 (SM3摘要)
            // C2: 明文长度字节
            var c1 = new byte[65];
            Array.Copy(cipherBytes, 0, c1, 0, 65);

            var c3 = new byte[32];
            Array.Copy(cipherBytes, cipherBytes.Length - 32, c3, 0, 32);

            var c2 = new byte[cipherBytes.Length - 65 - 32];
            Array.Copy(cipherBytes, 65, c2, 0, c2.Length);

            // 合并为C1 + C3 + C2格式
            var result = new byte[c1.Length + c3.Length + c2.Length];
            Array.Copy(c1, 0, result, 0, c1.Length);
            Array.Copy(c3, 0, result, c1.Length, c3.Length);
            Array.Copy(c2, 0, result, c1.Length + c3.Length, c2.Length);

          //  return Hex.ToHexString(result);

            // 返回加密后的数据（Base64编码）
             return Convert.ToBase64String(result);
        }
    }
    public class SM2Decryptor
    {
        public static byte[] Decrypt(byte[] encryptedData, ECPrivateKeyParameters privateKey)
        {
            // 创建SM2解密引擎
            SM2Engine engine = new SM2Engine();

            // 初始化解密器（使用私钥）
            engine.Init(false, privateKey);

            // 执行解密
            return engine.ProcessBlock(encryptedData, 0, encryptedData.Length);
        }
       
        public static string Decrypt(string cipherTextBase64, string base64PrivateKey)
        {
            // 将Base64编码的私钥转换为PEM格式
            string pemPrivate = $"-----BEGIN PRIVATE KEY-----\n{base64PrivateKey}\n-----END PRIVATE KEY-----";
            var privateKey = SM2KeyHelper.PrivateKeyFromPem(pemPrivate);
            // 执行解密
            // 将Base64编码的加密数据转换为字节数组
            // 并使用私钥进行解密
            // 返回解密后的数据（UTF-8编码）
            // 将密文从16进制转换为字节数组

            var cipherBytes = Convert.FromBase64String(cipherTextBase64);
           

            // 解析C1 + C3 + C2格式
            // C1: 65字节 (04 + x + y)
            // C3: 32字节 (SM3摘要)
            // C2: 剩余部分 (实际加密数据)
            var c1 = new byte[65];
            Array.Copy(cipherBytes, 0, c1, 0, 65);

            var c3 = new byte[32];
            Array.Copy(cipherBytes, 65, c3, 0, 32);

            var c2 = new byte[cipherBytes.Length - 65 - 32];
            Array.Copy(cipherBytes, 65 + 32, c2, 0, c2.Length);

            // 重组为SM2Engine默认的C1 + C2 + C3格式
            var standardCipherBytes = new byte[c1.Length + c2.Length + c3.Length];
            Array.Copy(c1, 0, standardCipherBytes, 0, c1.Length);
            Array.Copy(c2, 0, standardCipherBytes, c1.Length, c2.Length);
            Array.Copy(c3, 0, standardCipherBytes, c1.Length + c2.Length, c3.Length);

            // 解密数据
            // var plainBytes = sm2Engine.ProcessBlock(standardCipherBytes, 0, standardCipherBytes.Length);


           // byte[] decryptedData = SM2Decryptor.Decrypt(standardCipherBytes, privateKey);

            // 创建SM2解密引擎
            SM2Engine engine = new SM2Engine();

            // 初始化解密器（使用私钥）
            engine.Init(false, privateKey);

            // 执行解密
            byte[] decryptedData= engine.ProcessBlock(standardCipherBytes, 0, standardCipherBytes.Length);

            string decryptedText = Encoding.UTF8.GetString(decryptedData);
            return decryptedText;
        }
    }
    public class TestSM2
    {
        public static void Test()
        {
            // 您的PEM格式公钥
            string base64PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqBHM9VAYItBHkwdwIBAQQgQO/kQcZ01uELNlRD6vRvcSCA+vRmkr7cWLBlkJhIzTugCgYIKoEcz1UBgi2hRANCAATkJS6QR8BZ1QfFbBaASheSeVMJI2itEdJ5AQkz6P4Ri2oZkddVZ4AS7kSZG1tMnqBZKCKVJt+GeFDiXWTUWk0k";
            string base64PublicKey = "MFkwEwYHKoZIzj0CAQYIKoEcz1UBgi0DQgAE5CUukEfAWdUHxWwWgEoXknlTCSNorRHSeQEJM+j+EYtqGZHXVWeAEu5EmRtbTJ6gWSgilSbfhnhQ4l1k1FpNJA==";
            string originalText = "这是要加密的测试数据AAffd";

            var encrypetedData = SM2Encryptor.Encrypt(originalText, base64PublicKey);
            Console.WriteLine("加密结果(Base64): " + encrypetedData);
            var decryptedText = SM2Decryptor.Decrypt(encrypetedData, base64PrivateKey);
            Console.WriteLine("解密结果: " + decryptedText);
        }
    }

}
