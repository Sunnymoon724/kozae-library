using System.IO;
using KZConsole.Utilities;

namespace KZConsole
{
	public class Program
	{
		private const string c_aesKeyFileName = "Encryption.key";
		private const string c_publicKeyFileName = "PublicKey.pem";
		private const string c_encryptedPrivateKeyFileName = "EncryptedPrivateKey.pem";
		private const string c_rsaPublicKeyPemHeader = "RSA PUBLIC KEY";
		private const string c_encryptedPrivateKeyPemHeader = "ENCRYPTED PRIVATE KEY";

		/// <summary>
		/// 0 -> resultFolderRelativePath (relative to exe working directory)
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,1,"KZCipherGenerator <resultFolderRelativePath>",onPlayProgram);
		}

		private static void onPlayProgram(string[] argumentArray)
		{
			var currentPath = KZFileKit.GetProjectPath();
			var resultFolderPath = Path.GetFullPath(Path.Combine(currentPath,argumentArray[0]));

			KZCommonKit.WriteLog($"Result folder path : {resultFolderPath}",LogType.Info);

			var keyInfo = Encryptor.GenerateKey();

			KZCommonKit.WriteLog("Save keys",LogType.Info);

			KZFileKit.CreateFolder(resultFolderPath);

			var aesKeyFilePath = Path.Combine(resultFolderPath,c_aesKeyFileName);
			var publicKeyFilePath = Path.Combine(resultFolderPath,c_publicKeyFileName);
			var encryptedPrivateKeyFilePath = Path.Combine(resultFolderPath,c_encryptedPrivateKeyFileName);

			KZFileKit.WriteTextToFile(aesKeyFilePath,keyInfo.AesKeyBase64);
			KZFileKit.WriteTextToFile(publicKeyFilePath,KZCryptoKit.WrapPemFormat(keyInfo.PublicKeyBase64,c_rsaPublicKeyPemHeader));
			KZFileKit.WriteTextToFile(encryptedPrivateKeyFilePath,KZCryptoKit.WrapPemFormat(keyInfo.EncryptedPrivateKeyBase64,c_encryptedPrivateKeyPemHeader));

			KZCommonKit.WriteLog($"-Save {c_aesKeyFileName}, {c_publicKeyFileName}, {c_encryptedPrivateKeyFileName}",LogType.Info);
		}
	}
}
