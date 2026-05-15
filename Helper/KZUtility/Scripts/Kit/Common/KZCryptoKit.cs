using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public static class KZCryptoKit
{
	#region Constant
	private const int c_aesKeySize128 = 16;
	private const int c_aesKeySize192 = 24;
	private const int c_aesKeySize256 = 32;

	private const int c_aesIvSize = 16;
	private const int c_aesSaltSize = 16;
	private const int c_aesPbkdf2Iterations = 100000;
	private const int c_aesPbkdf2KeySize = 32;

	private const string c_source = "Source";
	private const string c_key = "Key";
	private const string c_publicKey = "PublicKey";
	private const string c_privateKey = "PrivateKey";
	#endregion Constant

	#region AES
	public static string AESEncryptToString(string source,byte[] key)
	{
		return Convert.ToBase64String(AESEncryptToBytes(source,key));
	}

	public static byte[] AESEncryptToBytes(string source,byte[] key)
	{
		_Validate(source,c_source);
		_Validate(key,c_key);
		_AESValidateKey(key);

		using var aes = Aes.Create();
		aes.Key = key;
		aes.GenerateIV();

		using var encryptor = aes.CreateEncryptor(aes.Key,aes.IV);
		using var memoryStream = new MemoryStream();

		memoryStream.Write(aes.IV,0,aes.IV.Length);

		using(var cryptoStream = new CryptoStream(memoryStream,encryptor,CryptoStreamMode.Write))
		{
			using var writer = new StreamWriter(cryptoStream);

			writer.Write(source);
		}

		return memoryStream.ToArray();
	}

	public static string AESDecryptFromString(string source,byte[] key)
	{
		_Validate(source,c_source);

		return AESDecryptFromBytes(Convert.FromBase64String(source),key);
	}

	public static string AESDecryptFromBytes(byte[] source,byte[] key)
	{
		_Validate(source,c_source);
		_Validate(key,c_key);
		_AESValidateKey(key);

		using var aes = Aes.Create();
		using var memoryStream = new MemoryStream(source);
		var iv = new byte[c_aesIvSize];
		memoryStream.Read(iv,0,iv.Length);

		aes.Key = key;
		aes.IV = iv;

		using var decryptor = aes.CreateDecryptor(aes.Key,aes.IV);
		using var cryptoStream = new CryptoStream(memoryStream,decryptor,CryptoStreamMode.Read);
		using var reader = new StreamReader(cryptoStream);

		return reader.ReadToEnd();
	}

	public static byte[] AESGenerate16Key()
	{
		return _AESGenerateKey(c_aesKeySize128);
	}

	public static byte[] AESGenerate24Key()
	{
		return _AESGenerateKey(c_aesKeySize192);
	}

	public static byte[] AESGenerate32Key()
	{
		return _AESGenerateKey(c_aesKeySize256);
	}

	public static byte[] AESGenerateRandomKey()
	{
		var sizeArray = new int[] { c_aesKeySize128, c_aesKeySize192, c_aesKeySize256 };
		var index = RandomNumberGenerator.GetInt32(0,sizeArray.Length);

		return _AESGenerateKey(sizeArray[index]);
	}

	public static byte[] AESGenerateKeyByPassword(string password)
	{
		var salt = new byte[c_aesSaltSize];

		RandomNumberGenerator.Fill(salt);

		using var derivation = new Rfc2898DeriveBytes(password,salt,c_aesPbkdf2Iterations,HashAlgorithmName.SHA256);

		return derivation.GetBytes(c_aesPbkdf2KeySize);
	}

	private static void _AESValidateKey(byte[] key)
	{
		if(key.Length != c_aesKeySize128 && key.Length != c_aesKeySize192 && key.Length != c_aesKeySize256)
		{
			throw new ArgumentException($"Key must be {c_aesKeySize128} or {c_aesKeySize192} or {c_aesKeySize256} bytes.");
		}
	}

	private static byte[] _AESGenerateKey(int size)
	{
		var key = new byte[size];

		RandomNumberGenerator.Fill(key);

		return key;
	}
	#endregion AES

	#region RSA
	public static string RSAEncryptToString(string source,string publicKey)
	{
		return Convert.ToBase64String(RSAEncryptToBytes(source,publicKey));
	}

	public static byte[] RSAEncryptToBytes(string source,string publicKey)
	{
		_Validate(source,c_source);
		_Validate(publicKey,c_publicKey);
		_RSAValidateBuffer(publicKey);

		using var rsa = RSA.Create();
		rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey),out _);

		return rsa.Encrypt(Encoding.UTF8.GetBytes(source),RSAEncryptionPadding.OaepSHA256);
	}

	public static string RSADecryptFromString(string source,string privateKey)
	{
		_Validate(source,c_source);

		return RSADecryptFromBytes(Convert.FromBase64String(source),privateKey);
	}

	public static string RSADecryptFromBytes(byte[] source,string privateKey)
	{
		_Validate(source,c_source);
		_Validate(privateKey,c_privateKey);
		_RSAValidateBuffer(privateKey);

		using var rsa = RSA.Create();
		rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey),out _);

		return Encoding.UTF8.GetString(rsa.Decrypt(source,RSAEncryptionPadding.OaepSHA256));
	}

	public static void RSAGenerateKey(out string publicKey,out string privateKey)
	{
		using var rsa = RSA.Create();

		publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
		privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
	}

	private static void _RSAValidateBuffer(string key)
	{
		var buffer = new Span<byte>(new byte[key.Length*3/4]);

		if(!Convert.TryFromBase64String(key,buffer,out int _))
		{
			throw new ArgumentException("The key is not a valid Base64 encoded string.");
		}
	}
	#endregion RSA

	#region SHA
	public static string SHAComputeHashToString(string source)
	{
		return _ByteToString(SHAComputeHashToBytes(source));
	}

	public static byte[] SHAComputeHashToBytes(string source)
	{
		_Validate(source,c_source);

		return SHAComputeHashToBytes(Encoding.UTF8.GetBytes(source));
	}

	public static string SHAComputeHashToString(byte[] source)
	{
		return _ByteToString(SHAComputeHashToBytes(source));
	}

	public static byte[] SHAComputeHashToBytes(byte[] source)
	{
		_Validate(source,c_source);

		using var provider = SHA256.Create();

		return provider.ComputeHash(source);
	}
	#endregion SHA

	#region Common
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
	#endregion Common
}