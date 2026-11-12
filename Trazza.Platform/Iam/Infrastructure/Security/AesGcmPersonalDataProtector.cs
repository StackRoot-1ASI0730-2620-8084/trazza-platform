using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using TrazzaPlatform.API.Iam.Application.Internal.OutboundServices;

namespace TrazzaPlatform.API.Iam.Infrastructure.Security;

/// <summary>
///     AES-256-GCM encryption stored as <c>nonce (12) + ciphertext + tag (16)</c>, and HMAC-SHA256 of the DNI.
/// </summary>
public class AesGcmPersonalDataProtector : IPersonalDataProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _encryptionKey;
    private readonly byte[] _hashKey;

    public AesGcmPersonalDataProtector(IOptions<PersonalDataSettings> options)
    {
        _encryptionKey = Convert.FromBase64String(options.Value.EncryptionKey);
        _hashKey = Convert.FromBase64String(options.Value.HashKey);
        if (_encryptionKey.Length != 32 || _hashKey.Length != 32)
            throw new InvalidOperationException("PersonalData keys must be 32 bytes encoded in Base64.");
    }

    public byte[] Encrypt(string plainText)
    {
        var plain = Encoding.UTF8.GetBytes(plainText);
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[plain.Length];
        var tag = new byte[TagSize];
        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Encrypt(nonce, plain, cipher, tag);

        var result = new byte[NonceSize + cipher.Length + TagSize];
        nonce.CopyTo(result, 0);
        cipher.CopyTo(result, NonceSize);
        tag.CopyTo(result, NonceSize + cipher.Length);
        return result;
    }

    public string Decrypt(byte[] cipher)
    {
        var nonce = cipher.AsSpan(0, NonceSize);
        var tag = cipher.AsSpan(cipher.Length - TagSize, TagSize);
        var data = cipher.AsSpan(NonceSize, cipher.Length - NonceSize - TagSize);
        var plain = new byte[data.Length];
        using var aes = new AesGcm(_encryptionKey, TagSize);
        aes.Decrypt(nonce, data, tag, plain);
        return Encoding.UTF8.GetString(plain);
    }

    public byte[] HashDni(string dni) => HMACSHA256.HashData(_hashKey, Encoding.UTF8.GetBytes(dni));
}
