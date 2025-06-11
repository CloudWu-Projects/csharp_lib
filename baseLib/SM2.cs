using Org.BouncyCastle.Crypto.EC;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
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
    public class SM2Encryption
    {
        ECPublicKeyParameters ecPublicKey = null;

        SM2Engine cipher = null;

        static SM2Encryption instance = new SM2Encryption();
        public static SM2Encryption Instance()
        {
            return instance;
        }
        public SM2Encryption()
        {

        }
        string lastPublicKey = "";
        private static byte[] HexStringToByteArray(string hex)
        {
            int length = hex.Length;
            byte[] bytes = new byte[length / 2];
            for (int i = 0; i < length; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            }
            return bytes;
        }
        public SM2Encryption Init(string publicKey)
        {
            var ecParams = CustomNamedCurves.GetByName("sm2p256v1"); // SM2 使用的椭圆曲线

            // 将十六进制公钥转换为字节数组
            byte[] publicKeyBytes = HexStringToByteArray(publicKey);
            var point = ecParams.Curve.DecodePoint(publicKeyBytes);
            var ecPublicKey = new ECPublicKeyParameters(point, new ECDomainParameters(ecParams.Curve, ecParams.G, ecParams.N));
            cipher = new SM2Engine();
            cipher.Init(true, new ParametersWithRandom(ecPublicKey, new SecureRandom()));
            lastPublicKey = publicKey;
            return this;
        }
        public string Encrypt(string data, string publicKey)
        {
            if (publicKey == null)
            {
                throw new Exception("SM2.Encrypt public key is null");
            }


            if (cipher == null || lastPublicKey != publicKey)
            {
                Init(publicKey);
            }
            return Convert.ToBase64String(cipher.ProcessBlock(Encoding.UTF8.GetBytes(data), 0, Encoding.UTF8.GetBytes(data).Length));
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
        public static string Encrypt(string data,string base64PublicKey)
        {
            string pemPublicKey = $"-----BEGIN PUBLIC KEY-----\n{base64PublicKey}\n-----END PUBLIC KEY-----";

            // 创建公钥参数对象
            ECPublicKeyParameters publicKey = SM2KeyHelper.GetPublicKeyFromPem(pemPublicKey);
            // 执行加密
            byte[] encryptedData = Encrypt(Encoding.UTF8.GetBytes(data), publicKey);
            // 返回加密后的数据（Base64编码）
            return Convert.ToBase64String(encryptedData);
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
        public static string Decrypt(string base64EncryptedData, string base64PrivateKey)
        {
            // 将Base64编码的私钥转换为PEM格式
            string pemPrivate = $"-----BEGIN PRIVATE KEY-----\n{base64PrivateKey}\n-----END PRIVATE KEY-----";
            var privateKey = SM2KeyHelper.PrivateKeyFromPem(pemPrivate);
            // 执行解密
            // 将Base64编码的加密数据转换为字节数组
            // 并使用私钥进行解密
            // 返回解密后的数据（UTF-8编码）

            byte[] decryptedData = SM2Decryptor.Decrypt(Convert.FromBase64String(base64EncryptedData), privateKey);
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
        public static void Test1()
        {
            // 在使用前添加这行代码
            //   Security.AddProvider(new Org.BouncyCastle.Security.BouncyCastleProvider());
            string input = "Hello, SM3!";

            // 您的PEM格式公钥
            string base64PrivateKey = "MIGTAgEAMBMGByqGSM49AgEGCCqBHM9VAYItBHkwdwIBAQQgQO/kQcZ01uELNlRD6vRvcSCA+vRmkr7cWLBlkJhIzTugCgYIKoEcz1UBgi2hRANCAATkJS6QR8BZ1QfFbBaASheSeVMJI2itEdJ5AQkz6P4Ri2oZkddVZ4AS7kSZG1tMnqBZKCKVJt+GeFDiXWTUWk0k";

            string pemPrivate = $"-----BEGIN PRIVATE KEY-----\n{base64PrivateKey}\n-----END PRIVATE KEY-----";


            string base64PublicKey = "MFkwEwYHKoZIzj0CAQYIKoEcz1UBgi0DQgAE5CUukEfAWdUHxWwWgEoXknlTCSNorRHSeQEJM+j+EYtqGZHXVWeAEu5EmRtbTJ6gWSgilSbfhnhQ4l1k1FpNJA==";
            // 转换为PEM格式
            string pemPublicKey = $"-----BEGIN PUBLIC KEY-----\n{base64PublicKey}\n-----END PUBLIC KEY-----";
            //   string pemPublicKey = @"MFkwEwYHKoZIzj0CAQYIKoEcz1UBgi0DQgAE5CUukEfAWdUHxWwWgEoXknlTCSNorRHSeQEJM +j+EYtqGZHXVWeAEu5EmRtbTJ6gWSgilSbfhnhQ4l1k1FpNJA==";
            // 转换为公钥对象
            ECPublicKeyParameters publicKey = SM2KeyHelper.GetPublicKeyFromPem(pemPublicKey);

            // 要加密的数据
            string originalText = "这是要加密的测试数据AAffd";
            byte[] dataToEncrypt = Encoding.UTF8.GetBytes(originalText);

            // 执行加密
            byte[] encryptedData = SM2Encryptor.Encrypt(dataToEncrypt, publicKey);

            var strencryptedData = Convert.ToBase64String(encryptedData);


            //jiemi 
            var privateKey = SM2KeyHelper.PrivateKeyFromPem(pemPrivate);
            byte[] decryptedData = SM2Decryptor.Decrypt(encryptedData, privateKey);
            string decryptedText = Encoding.UTF8.GetString(decryptedData);

            Console.WriteLine("加密结果(Base64): " + Convert.ToBase64String(encryptedData));


        }

    }

}
