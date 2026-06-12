using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// Static helpers for AES, RSA, and SHA-256 encryption and hashing.
/// </summary>
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
	/// <summary>
	/// Encrypts <paramref name="source"/> with AES and returns a Base64 string.
	/// </summary>
	public static string AESEncryptToString(string source,byte[] key)
	{
		return Convert.ToBase64String(AESEncryptToBytes(source,key));
	}

	/// <summary>
	/// Encrypts <paramref name="source"/> with AES. The returned bytes are IV (16 bytes) followed by ciphertext.
	/// </summary>
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

	/// <summary>
	/// Decrypts a Base64 AES payload produced by <see cref="AESEncryptToString"/>.
	/// </summary>
	public static string AESDecryptFromString(string source,byte[] key)
	{
		_Validate(source,c_source);

		return AESDecryptFromBytes(Convert.FromBase64String(source),key);
	}

	/// <summary>
	/// Decrypts AES bytes whose first 16 bytes are the IV.
	/// </summary>
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

	/// <summary>
	/// Generates a random 128-bit (16-byte) AES key.
	/// </summary>
	public static byte[] AESGenerate16Key()
	{
		return _AESGenerateKey(c_aesKeySize128);
	}

	/// <summary>
	/// Generates a random 192-bit (24-byte) AES key.
	/// </summary>
	public static byte[] AESGenerate24Key()
	{
		return _AESGenerateKey(c_aesKeySize192);
	}

	/// <summary>
	/// Generates a random 256-bit (32-byte) AES key.
	/// </summary>
	public static byte[] AESGenerate32Key()
	{
		return _AESGenerateKey(c_aesKeySize256);
	}

	/// <summary>
	/// Generates a random AES key with a randomly chosen valid size (128, 192, or 256 bits).
	/// </summary>
	public static byte[] AESGenerateRandomKey()
	{
		var sizeArray = new int[] { c_aesKeySize128, c_aesKeySize192, c_aesKeySize256 };
		var idx = RandomNumberGenerator.GetInt32(0,sizeArray.Length);

		return _AESGenerateKey(sizeArray[idx]);
	}

	/// <summary>
	/// Derives a 256-bit AES key from <paramref name="password"/> using PBKDF2 (SHA-256, 100,000 iterations).
	/// A random salt is generated internally and is not returned.
	/// </summary>
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
	/// <summary>
	/// Encrypts <paramref name="source"/> with RSA-OAEP (SHA-256) and returns a Base64 string.
	/// </summary>
	public static string RSAEncryptToString(string source,string publicKey)
	{
		return Convert.ToBase64String(RSAEncryptToBytes(source,publicKey));
	}

	/// <summary>
	/// Encrypts <paramref name="source"/> with RSA-OAEP (SHA-256).
	/// <paramref name="publicKey"/> is a Base64-encoded RSA public key blob.
	/// </summary>
	public static byte[] RSAEncryptToBytes(string source,string publicKey)
	{
		_Validate(source,c_source);
		_Validate(publicKey,c_publicKey);
		_RSAValidateBuffer(publicKey);

		using var rsa = RSA.Create();
		rsa.ImportRSAPublicKey(Convert.FromBase64String(publicKey),out _);

		return rsa.Encrypt(Encoding.UTF8.GetBytes(source),RSAEncryptionPadding.OaepSHA256);
	}

	/// <summary>
	/// Decrypts a Base64 RSA payload produced by <see cref="RSAEncryptToString"/>.
	/// </summary>
	public static string RSADecryptFromString(string source,string privateKey)
	{
		_Validate(source,c_source);

		return RSADecryptFromBytes(Convert.FromBase64String(source),privateKey);
	}

	/// <summary>
	/// Decrypts RSA-OAEP (SHA-256) bytes.
	/// <paramref name="privateKey"/> is a Base64-encoded RSA private key blob.
	/// </summary>
	public static string RSADecryptFromBytes(byte[] source,string privateKey)
	{
		_Validate(source,c_source);
		_Validate(privateKey,c_privateKey);
		_RSAValidateBuffer(privateKey);

		using var rsa = RSA.Create();
		rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey),out _);

		return Encoding.UTF8.GetString(rsa.Decrypt(source,RSAEncryptionPadding.OaepSHA256));
	}

	/// <summary>
	/// Generates a new RSA key pair as Base64-encoded public and private key blobs.
	/// </summary>
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
	/// <summary>
	/// Computes a SHA-256 hash of UTF-8 <paramref name="source"/> and returns a lowercase hex string.
	/// </summary>
	public static string SHAComputeHashToString(string source)
	{
		return _ByteToString(SHAComputeHashToBytes(source));
	}

	/// <summary>
	/// Computes a SHA-256 hash of UTF-8 <paramref name="source"/>.
	/// </summary>
	public static byte[] SHAComputeHashToBytes(string source)
	{
		_Validate(source,c_source);

		return SHAComputeHashToBytes(Encoding.UTF8.GetBytes(source));
	}

	/// <summary>
	/// Computes a SHA-256 hash of <paramref name="source"/> and returns a lowercase hex string.
	/// </summary>
	public static string SHAComputeHashToString(byte[] source)
	{
		return _ByteToString(SHAComputeHashToBytes(source));
	}

	/// <summary>
	/// Computes a SHA-256 hash of <paramref name="source"/>.
	/// </summary>
	public static byte[] SHAComputeHashToBytes(byte[] source)
	{
		_Validate(source,c_source);

		using var provider = SHA256.Create();

		return provider.ComputeHash(source);
	}
	#endregion SHA

	#region PEM
	/// <summary>
	/// Wraps <paramref name="text"/> in PEM headers for <paramref name="header"/>.
	/// </summary>
	public static string WrapPemFormat(string text,string header)
	{
		return $"-----BEGIN {header}-----\n{text}\n-----END {header}-----";
	}

	/// <summary>
	/// Extracts the body from a PEM block for <paramref name="header"/>, or returns an empty string when not found.
	/// </summary>
	public static string UnwrapPemFormat(string text,string header)
	{
		var head = $"-----BEGIN {header}-----\n";
		var tail = $"\n-----END {header}-----";

		var start = text.IndexOf(head);
		var end = text.IndexOf(tail);

		if(start == -1 || end == -1)
		{
			return string.Empty;
		}

		return text[(start+head.Length)..end];
	}
	#endregion PEM

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