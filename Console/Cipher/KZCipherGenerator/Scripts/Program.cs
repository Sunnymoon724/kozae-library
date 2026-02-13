using System.IO;
using KZConsole.Utilities;
using KZLib.Utilities;

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

			CommonUtility.WriteLog("Save keys",LogType.Info);

			var resultFolderPath = argumentArray[0];

			FileUtility.CreateFolder(resultFolderPath);

			var convertFilePath = Path.Combine(resultFolderPath,"Encryption.key");
			var publicFilePath = Path.Combine(resultFolderPath,"PublicKey.pem");
			var privateFilePath = Path.Combine(resultFolderPath,"PrivateKey.pem");

			FileUtility.WriteTextToFile(convertFilePath,keyInfo.ConvertKey);
			FileUtility.WriteTextToFile(publicFilePath,FileUtility.WrapPemFormat(keyInfo.PublicKey,"PUBLIC KEY"));
			FileUtility.WriteTextToFile(privateFilePath,FileUtility.WrapPemFormat(keyInfo.PrivateKey,"PRIVATE KEY"));
		}
	}
}