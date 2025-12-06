using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using KZConsole.KZUtility;
using MemoryPack;
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
				if(string.IsNullOrEmpty(code))
				{
					continue;
				}

				syntaxTreeList.Add(CSharpSyntaxTree.ParseText(code));
			}

			var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

			var referenceList = new List<MetadataReference>
			{
				MetadataReference.CreateFromFile(typeof(object).Assembly.Location),

				MetadataReference.CreateFromFile(typeof(MemoryPackableAttribute).Assembly.Location),

				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,Global.DATA_FILE_NAME)),
				MetadataReference.CreateFromFile(Path.Combine(baseDirectory,"UnityEngine.dll")),
			};
			
			var assemblies = AppDomain.CurrentDomain.GetAssemblies();
			var filteredReferences = new List<MetadataReference>();
			
			foreach (var assembly in assemblies)
			{
				if (!assembly.IsDynamic && !string.IsNullOrEmpty(assembly.Location))
				{
					var reference = MetadataReference.CreateFromFile(assembly.Location);

					filteredReferences.Add(reference);
				}
			}

			referenceList.AddRange(filteredReferences);

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
				foreach(var diagnostic in result.Diagnostics)
				{
					if(diagnostic.IsWarningAsError || diagnostic.Severity == DiagnosticSeverity.Error)
					{
						Console.Error.WriteLine(diagnostic.ToString());
					}
				}

				throw new InvalidOperationException("-Compilation failed.");
			}
		}
	}
}