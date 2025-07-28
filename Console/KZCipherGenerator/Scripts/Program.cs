using System.Globalization;
using KZLib.KZUtility;

namespace KZConsole
{
	public class Program
	{
		/// <summary>
		/// 0 -> resultFolderPath
		/// </summary>
		internal static void Main(string[] argumentArray)
		{
			try
			{
				Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");

				var keyInfo = Encryptor.GenerateKey();

				Console.WriteLine("Save keys");

				var resultFolderPath = argumentArray[0];

				FileUtility.CreateFolder(resultFolderPath);

				var convertFilePath = Path.Combine(resultFolderPath,"Encryption.key");
				var publicFilePath = Path.Combine(resultFolderPath,"PublicKey.pem");
				var privateFilePath = Path.Combine(resultFolderPath,"PrivateKey.pem");

				FileUtility.WriteTextToFile(convertFilePath,keyInfo.ConvertKey);
				FileUtility.WriteTextToFile(publicFilePath,FileUtility.WrapPemFormat(keyInfo.PublicKey,"PUBLIC KEY"));
				FileUtility.WriteTextToFile(privateFilePath,FileUtility.WrapPemFormat(keyInfo.PrivateKey,"PRIVATE KEY"));

				Console.WriteLine("Program is done");
				Console.ReadLine();
			}
			catch(Exception exception)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine($"{exception}");
				Console.ResetColor();

				Environment.Exit(-1);
			}
		}
	}
}