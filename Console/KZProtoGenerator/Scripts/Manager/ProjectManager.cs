
using KZLib.KZUtility;

namespace KZConsole
{
	public class ProjectManager
	{
		private readonly string m_projectFolderPath = "";
		private readonly string m_outputFolderPath = "";
		private readonly string m_projectFilePath = "";

		public string ProjectFolderPath => m_projectFolderPath;

		public ProjectManager(string currentPath,string outputFolderPath)
		{
			m_projectFolderPath = Path.GetFullPath(Path.Combine(currentPath,"../ProtoProject"));
			m_projectFilePath = Path.Combine(m_projectFolderPath,"ProtoProject.csproj");

			FileUtility.CreateFolder(m_projectFolderPath);

			m_outputFolderPath = outputFolderPath;

			var sourceDllFilePath = Path.Combine(currentPath,"KZData.dll");

			FileUtility.CopyFile(sourceDllFilePath,m_projectFolderPath,true);
		}

		public void CreateProject()
		{
			var projectText = @"
			<Project Sdk=""Microsoft.NET.Sdk"">

			<PropertyGroup>
			<TargetFramework>netstandard2.1</TargetFramework>
			<AssemblyName>KZProto</AssemblyName>
			<LangVersion>9.0</LangVersion>
			<DebugType>portable</DebugType>
			</PropertyGroup>
			
			<ItemGroup>
			<PackageReference Include=""MessagePack"" Version=""3.1.4"" />
			<PackageReference Include=""Unity3D.SDK"" Version=""2021.1.14.1"" />
			</ItemGroup>
			
			<ItemGroup>
			<Reference Include=""KZData""><HintPath>.KZData.dll</HintPath></Reference>
			</ItemGroup>

			<PropertyGroup>
			<NoWarn>NU1701</NoWarn><!-- Unity warning -->
			</PropertyGroup>

			</Project>";

			FileUtility.WriteTextToFile(m_projectFilePath,projectText);

			var globalFilePath = Path.Combine(m_projectFolderPath,"Global.cs");

			var globalText = @"
			using System.ComponentModel;

			namespace System.Runtime.CompilerServices
			{
				[EditorBrowsable(EditorBrowsableState.Never)]
				internal class IsExternalInit { }
			}";

			FileUtility.WriteTextToFile(globalFilePath,globalText);
		}

		public void BuildProject()
		{
			var process = new System.Diagnostics.Process();
			process.StartInfo.FileName = "dotnet";
			process.StartInfo.Arguments = $"build \"{m_projectFilePath}\" --configuration Release --output \"{m_outputFolderPath}\"";
			process.StartInfo.RedirectStandardOutput = true;
			process.StartInfo.RedirectStandardError = true;
			process.StartInfo.UseShellExecute = false;

			Console.WriteLine("-Build start");

			try
			{
				process.Start();

				var output = process.StandardOutput.ReadToEnd();
				var error = process.StandardError.ReadToEnd();

				process.WaitForExit();

				Console.WriteLine("-Build Output:");
				Console.WriteLine(output);

				if(!string.IsNullOrEmpty(error))
				{
					Console.WriteLine("-Build Error:");
					Console.WriteLine(error);
				}
			}
			catch(Exception exception)
			{
				Console.WriteLine($"Error executing build: {exception.Message}");
			}
		}

		public void DeleteProject()
		{
			Directory.Delete(m_projectFolderPath,true);
		}
	}
}