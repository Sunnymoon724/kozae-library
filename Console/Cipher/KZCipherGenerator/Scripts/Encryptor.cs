using System;
using KZConsole.Utilities;

namespace KZConsole
{
	/// <summary>
	/// PublicKeyBase64: RSA public key (Base64) / EncryptedPrivateKeyBase64: AES-encrypted RSA private key (Base64) / AesKeyBase64: AES key for decryption (Base64)
	/// </summary>
	public record KeyInfo(string PublicKeyBase64,string EncryptedPrivateKeyBase64,string AesKeyBase64);

	public class Encryptor
	{
		public static KeyInfo GenerateKey()
		{
			var aesKey = KZCryptoKit.AESGenerateRandomKey();

			KZCryptoKit.RSAGenerateKey(out var publicKeyBase64,out var privateKeyBase64);

			var encryptedPrivateKeyBase64 = KZCryptoKit.AESEncryptToString(privateKeyBase64,aesKey);
			var aesKeyBase64 = Convert.ToBase64String(aesKey);

			KZCommonKit.WriteLog("-Keys generated",LogType.Info);

			return new KeyInfo(publicKeyBase64,encryptedPrivateKeyBase64,aesKeyBase64);
		}
	}
}
