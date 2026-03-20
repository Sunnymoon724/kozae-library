using System.IO;
using KZConsole.Utilities;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			AppRunner.Execute(argumentArray,onPlayProgram);
		}

		private static void onPlayProgram(string[] argumentArray)
		{
			var keyInfo = Encryptor.GenerateKey();

			KZCommonKit.WriteLog("Save keys",LogType.Info);

			var resultFolderPath = argumentArray[0];

			KZFileKit.CreateFolder(resultFolderPath);

			var convertFilePath = Path.Combine(resultFolderPath,"Encryption.key");
			var publicFilePath = Path.Combine(resultFolderPath,"PublicKey.pem");
			var privateFilePath = Path.Combine(resultFolderPath,"PrivateKey.pem");

			KZFileKit.WriteTextToFile(convertFilePath,keyInfo.ConvertKey);
			KZFileKit.WriteTextToFile(publicFilePath,KZFileKit.WrapPemFormat(keyInfo.PublicKey,"PUBLIC KEY"));
			KZFileKit.WriteTextToFile(privateFilePath,KZFileKit.WrapPemFormat(keyInfo.PrivateKey,"PRIVATE KEY"));
		}
	}
}