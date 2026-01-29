using System.Security.Cryptography;
using System.Text;

namespace BankMore.API.Services;

public class CryptoService
{
    private readonly byte[] _key;

    public CryptoService()
    {
        var keyString = "Chave32BytesParaCriptografiaAES256!!";
        _key = Encoding.UTF8.GetBytes(keyString);

        if (_key.Length != 32)
        {
            Array.Resize(ref _key, 32);
        }
    }

    public string Encrypt(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = new byte[16];

        var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

        using var ms = new MemoryStream();
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            cs.Write(bytes, 0, bytes.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText)) return encryptedText;

        try
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = new byte[16];

            var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

            var bytes = Convert.FromBase64String(encryptedText);

            using var ms = new MemoryStream(bytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
        catch
        {
            return encryptedText;
        }
    }
}