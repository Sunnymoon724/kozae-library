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

				var randomKey = CryptoUtility.AES.GenerateRandomKey();

				CryptoUtility.RSA.GenerateKey(out var publicKey,out var privateKey);

				var encryptKey = CryptoUtility.AES.EncryptToString(privateKey,randomKey);
				var convertKey = Convert.ToBase64String(randomKey);

				Console.WriteLine($"Public Key : {publicKey}\n\nPrivate Key : {privateKey}\n\nEncrypt Key : {encryptKey}\n\nRandom Key : {convertKey}");

				Console.WriteLine("Save keys");

				var resultFolderPath = argumentArray[0];

				FileUtility.CreateFolder(resultFolderPath);

				var convertFilePath = Path.Combine(resultFolderPath,"Encryption.key");
				var publicFilePath = Path.Combine(resultFolderPath,"PublicKey.pem");
				var privateFilePath = Path.Combine(resultFolderPath,"PrivateKey.pem");

				FileUtility.WriteTextToFile(convertFilePath,convertKey);
				FileUtility.WriteTextToFile(publicFilePath,FileUtility.WrapPemFormat(publicKey,"PUBLIC KEY"));
				FileUtility.WriteTextToFile(privateFilePath,FileUtility.WrapPemFormat(encryptKey,"PRIVATE KEY"));


				Console.WriteLine("Press enter to exit...");

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