using System.Security.Cryptography;
using System.Text;

namespace BankMore.API.Services
{
    public interface ICryptoService
    {
        string Criptografar(string texto);
        string Descriptografar(string textoCriptografado);
        string Hash(string texto);
    }

    public class CryptoService : ICryptoService
    {
        private readonly string _encryptionKey = "SuaChaveSecreta32CaracteresAqui123456";
        private readonly byte[] _salt = Encoding.UTF8.GetBytes("BankMoreSalt2024");

        // Construtor SEM parâmetro temporariamente
        public CryptoService()
        {
        }

        public string Criptografar(string texto)
        {
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(_encryptionKey, _salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

            var textoBytes = Encoding.UTF8.GetBytes(texto);
            cs.Write(textoBytes, 0, textoBytes.Length);
            cs.FlushFinalBlock();

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Descriptografar(string textoCriptografado)
        {
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(_encryptionKey, _salt, 10000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(Convert.FromBase64String(textoCriptografado));
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }

        public string Hash(string texto)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(texto);
            var hashBytes = sha256.ComputeHash(bytes);

            var saltedBytes = new byte[hashBytes.Length + _salt.Length];
            Buffer.BlockCopy(hashBytes, 0, saltedBytes, 0, hashBytes.Length);
            Buffer.BlockCopy(_salt, 0, saltedBytes, hashBytes.Length, _salt.Length);

            return Convert.ToBase64String(sha256.ComputeHash(saltedBytes));
        }
    }
}