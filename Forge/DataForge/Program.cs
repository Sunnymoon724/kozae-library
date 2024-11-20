using System;

class Program
{
	/// <summary>
	/// Convert excel to config
	/// </summary>
	/// <param name="_argumentArray"></param>

	static void Main(string[] _argumentArray)
	{
		Console.WriteLine("Create.");

		Console.WriteLine("정보: 프로그램이 시작되었습니다.");

		// var _logger = LogManager.Instance.AddLogger(typeof(Program).Name);
		// LogManager.Instance.DefaultHandlerList.Add(new ConsoleLogHandler());
		// var currentDir = Directory.GetCurrentDirectory();
		// var gameDirInfo = Directory.GetParent(Directory.GetParent(currentDir).FullName);
		// var gameDir = gameDirInfo.FullName;
		// var helper = new Helper(_logger, currentDir);

		// //var excelDirPath = ProgramHelper.JoinWorkPath("Excel");
		// var csvDirPath = Path.Join(gameDir, "CSV");
		// var csvDir = Path.Join(csvDirPath, "ProtoGSA");
		// var protocolDir = Path.Join(gameDir, "Code/GSAGameProtocol");
		// var protoScriptDir = Path.Join(protocolDir, "Proto");
		// var templatePath = Path.Join(protoScriptDir, "ProtoCSTemplate.txt");

		// var protocolProjPath = Path.Join(protocolDir, "GSAGameProtocol.csproj");

		// var generator = new ProtoClassGenerator();
		// if (!generator.LoadTemplate(templatePath))
		// {
		// 	_logger.Error("Load Template Failed. TemplatePath({TemplatePath})", templatePath);
		// 	return;
		// }

		// helper.CheckOutFiles( protoScriptDir, "cs");
		// helper.CheckOutFile( protocolProjPath);
		// EnumRegistry.Register();

		// var preProtoClassList = Directory.GetFiles(protoScriptDir, "*.cs").Select(Path.GetFileNameWithoutExtension).ToList();
		// //var isSuccess = generator.TryGenerateWithExcelDir(csvDir, protoScriptDir);
		// var isSuccess = generator.TryGenerateWithCsvDir(csvDir, protoScriptDir);
		// if (isSuccess)
		// {
		// 	_logger.Info("Generate Class Success. CSVDir({CSVDir})", csvDir);

		// }
		// else
		// {
		// 	_logger.Error("Generate Class Failed. CSVDir({CSVDir})", csvDir);
		// }

		// var newProtoClassList = Directory.GetFiles(protoScriptDir, "*.cs").Select(Path.GetFileNameWithoutExtension).ToList();
		// foreach (var protoClass in newProtoClassList)
		// {
		// 	if (!preProtoClassList.Contains(protoClass))
		// 	{
		// 		var protoClassPath = Path.Join(protoScriptDir, $"{protoClass}.cs");
		// 		helper.MarkForAdd(protoClassPath);
		// 	}
		// }

		// EnumRegistry.Unregister();
		// helper.RevertUnchangedFiles(protoScriptDir, "cs");
	}
}