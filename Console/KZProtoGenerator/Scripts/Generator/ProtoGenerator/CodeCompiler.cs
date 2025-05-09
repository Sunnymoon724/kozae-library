using System.Reflection;
using System.Runtime.InteropServices;
using MessagePack;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace KZConsole.KZProto
{
	public class CodeCompiler
	{
		public static Assembly CompileToAssembly(IEnumerable<string> codeGroup)
		{
			var syntaxTreeList = new List<SyntaxTree>();

			foreach(var code in codeGroup)
			{
				if(code == null)
				{
					continue;
				}

				syntaxTreeList.Add(CSharpSyntaxTree.ParseText(code));
			}

			var runtimeDirectory = RuntimeEnvironment.GetRuntimeDirectory();
			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			var referenceList = new List<MetadataReference>
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
				MetadataReference.CreateFromFile(typeof(MessagePackObjectAttribute).Assembly.Location),
				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,"KZData.dll")),
				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,"UnityEngine.dll")),
			};

			referenceList.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location)).Select(a => MetadataReference.CreateFromFile(a.Location)));

			var compilation = CSharpCompilation.Create("KZProto",syntaxTreeList,referenceList,new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

			using var memoryStream = new MemoryStream();

			var result = compilation.Emit(memoryStream);

			if(result.Success)
			{
				Console.WriteLine("-Compilation succeeded.");
				Console.WriteLine("-Save dll & pdb.");

				memoryStream.Seek(0,SeekOrigin.Begin);

				return Assembly.Load(memoryStream.ToArray());
			}
			else
			{
				var failures = result.Diagnostics.Where(diagnostic => diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error);

				foreach(var diagnostic in failures)
				{
					Console.Error.WriteLine(diagnostic.ToString());
				}

				throw new InvalidOperationException("-Compilation failed.");
			}
		}
	}
}