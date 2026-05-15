using System;
using KZConsole.Utilities;

namespace KZConsole
{
	public record KeyInfo
	{
		public string PublicKey { get; }
		public string PrivateKey { get; }
		public string ConvertKey { get; }

		public KeyInfo(string publicKey,string privateKey,string convertKey)
		{
			PublicKey = publicKey;
			PrivateKey = privateKey;
			ConvertKey = convertKey;
		}
	}

	public class Encryptor
	{
		public static KeyInfo GenerateKey()
		{
			var randomKey = KZCryptoKit.AESGenerateRandomKey();

			KZCryptoKit.RSAGenerateKey(out var publicKey,out var privateKey);

			var encryptKey = KZCryptoKit.AESEncryptToString(privateKey,randomKey);
			var convertKey = Convert.ToBase64String(randomKey);

			KZCommonKit.WriteLog($"Public Key : {publicKey}\n\nPrivate Key : {privateKey}\n\nEncrypt Key : {encryptKey}\n\nRandom Key : {convertKey}",LogType.Info);

			return new KeyInfo(publicKey,encryptKey,convertKey);
		}
	}
}