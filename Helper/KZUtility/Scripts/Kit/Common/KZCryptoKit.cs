using System;
using System.IO;
using System.Text;

public static class KZCryptoKit
{
	public static class AES
	{
		public static string EncryptToString(string source,byte[] key)
		{
			return Convert.ToBase64String(EncryptToBytes(source,key));
		}

		public static byte[] EncryptToBytes(string source,byte[] key)
		{
			_Validate(source,"Source");
			_Validate(key,"Key");
			_ValidateKey(key);

			using var aes = System.Security.Cryptography.Aes.Create();
			aes.Key = key;
			aes.GenerateIV();

			using var encryptor = aes.CreateEncryptor(aes.Key,aes.IV);
			using var memoryStream = new MemoryStream();

			memoryStream.Write(aes.IV,0,aes.IV.Length);

			using(var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream,encryptor,System.Security.Cryptography.CryptoStreamMode.Write))
			{
				using var writer = new StreamWriter(cryptoStream);

				writer.Write(source);
			}

			return memoryStream.ToArray();
		}

		public static string DecryptFromString(string source,byte[] key)
		{
			_Validate(source,"Source");

			return DecryptFromBytes(Convert.FromBase64String(source),key);
		}

		public static string DecryptFromBytes(byte[] source,byte[] key)
		{
			_Validate(source,"Source");
			_Validate(key,"Key");
			_ValidateKey(key);

			using var aes = System.Security.Cryptography.Aes.Create();
			using var memoryStream = new MemoryStream(source);
			var iv = new byte[16];
			memoryStream.Read(iv,0,iv.Length);

			aes.Key = key;
			aes.IV = iv;

			using var decryptor = aes.CreateDecryptor(aes.Key,aes.IV);
			using var cryptoStream = new System.Security.Cryptography.CryptoStream(memoryStream,decryptor,System.Security.Cryptography.CryptoStreamMode.Read);
			using var reader = new StreamReader(cryptoStream);

			return reader.ReadToEnd();
		}

		public static byte[] Generate16Key()
		{
			return _GenerateKey(16);
		}

		public static byte[] Generate24Key()
		{
			return _GenerateKey(24);
		}

		public static byte[] Generate32Key()
		{
			return _GenerateKey(32);
		}

		public static byte[] GenerateRandomKey()
		{
			var sizeArray = new int[] { 16, 24, 32 };
			var random = new Random();

			return _GenerateKey(sizeArray[random.Next(0,2)]);
		}

		private static void _ValidateKey(byte[] key)
		{
			if(key.Length != 16 && key.Length != 24 && key.Length != 32)
			{
				throw new ArgumentException("Key must be 16 or 24 or 32 bytes.");
			}
		}
		
		private static byte[] _GenerateKey(int size)
		{
			using var provider = new System.Security.Cryptography.RNGCryptoServiceProvider();
			byte[] key = new byte[size];

			provider.GetBytes(key);

			return key;
		}

		public static byte[] GenerateKeyByPassword(string password)
		{
			var salt = new byte[16];

			using var derivation = new System.Security.Cryptography.Rfc2898DeriveBytes(password,salt,10000);
			var key = derivation.GetBytes(16);

			return key;
		}
	}

	public static class RSA
	{
		public static string EncryptToString(string source,string publicKey)
		{
			return Convert.ToBase64String(EncryptToBytes(source,publicKey));
		}

		public static byte[] EncryptToBytes(string source,string publicKey)
		{
			_Validate(source,"Source");
			_Validate(publicKey,"Key");
			_ValidateBuffer(publicKey);

			using var rsa = System.Security.Cryptography.RSA.Create();
			rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey),out _);

			var encrypt = Encoding.UTF8.GetBytes(source);

			return rsa.Encrypt(encrypt,System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);
		}

		public static string DecryptFromString(string source,string privateKey)
		{
			_Validate(source,"Source");

			return DecryptFromBytes(Convert.FromBase64String(source),privateKey);
		}

		public static string DecryptFromBytes(byte[] source,string privateKey)
		{
			_Validate(source,"Source");
			_Validate(privateKey,"Key");
			_ValidateBuffer(privateKey);

			using var rsa = System.Security.Cryptography.RSA.Create();
			rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey),out _);

			var decrypt = rsa.Decrypt(source,System.Security.Cryptography.RSAEncryptionPadding.OaepSHA256);

			return Encoding.UTF8.GetString(decrypt);
		}

		public static void GenerateKey(out string publicKey,out string privateKey)
		{
			using var rsa = System.Security.Cryptography.RSA.Create();

			publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
			privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
		}

		private static void _ValidateBuffer(string key)
		{
			var buffer = new Span<byte>(new byte[key.Length*3/4]);

			if(!Convert.TryFromBase64String(key,buffer,out int _))
			{
				throw new ArgumentException("The key is not a valid Base64 encoded string.");
			}
		}
	}

	public static class SHA
	{
		public static string ComputeHashToString(string source)
		{
			return _ByteToString(ComputeHashToBytes(source));
		}

		public static byte[] ComputeHashToBytes(string source)
		{
			_Validate(source,"Source");

			return ComputeHashToBytes(Encoding.UTF8.GetBytes(source));
		}

		public static string ComputeHashToString(byte[] source)
		{
			return _ByteToString(ComputeHashToBytes(source));
		}

		public static byte[] ComputeHashToBytes(byte[] source)
		{
			_Validate(source, "Source"); 

			using var provider = System.Security.Cryptography.SHA256.Create();

			return provider.ComputeHash(source);
		}

		// public static string ComputeHMACSHA256ToString(string source,string secretKey)
		// {
		// 	return _ByteToString(ComputeHMACSHA256ToBytes(source,secretKey));
		// }

		// public static byte[] ComputeHMACSHA256ToBytes(string source,string secretKey)
		// {
		// 	_Validate(source,"Source");

		// 	return ComputeHMACSHA256ToBytes(Encoding.UTF8.GetBytes(source),secretKey);
		// }

		// public static string ComputeHMACSHA256ToString(byte[] source,string secretKey)
		// {
		// 	return _ByteToString(ComputeHMACSHA256ToBytes(source,secretKey));
		// }

		// public static byte[] ComputeHMACSHA256ToBytes(byte[] source,string secretKey)
		// {
		// 	_Validate(source,"Source");
		// 	_Validate(secretKey,"SecretKey");

		// 	using var data = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
		// 	var result = data.ComputeHash(source);

		// 	return result;
		// }
	}

	private static void _Validate(byte[] source,string text)
	{
		if(source == null || source.Length == 0)
		{
			throw new NullReferenceException($"{text} is empty");
		}
	}

	private static void _Validate(string source,string text)
	{
		if(string.IsNullOrEmpty(source))
		{
			throw new NullReferenceException($"{text} is empty");
		}
	}

	private static string _ByteToString(byte[] source)
	{
		return BitConverter.ToString(source).Replace("-","").ToLower();
	}
}