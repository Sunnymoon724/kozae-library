using System.Collections.Generic;
using System.IO;
using KZLib.KZData;
using YamlDotNet.Serialization;

namespace KZLib.KZUtility
{
	public class RouteManager : Singleton<RouteManager>
	{
		private readonly Dictionary<string,Route> m_routeDict = new Dictionary<string,Route>();

		private readonly Dictionary<string,string> m_pathDict = new Dictionary<string,string>();

		private bool m_disposed = false;

		protected override void Release(bool disposing)
		{
			if(m_disposed)
			{
				return;
			}

			if(disposing)
			{
				m_routeDict.Clear();
			}

			m_disposed = true;

			base.Release(disposing);
		}

		protected override void Initialize()
		{
			base.Initialize();

			var routeFilePath = Path.Combine(Directory.GetCurrentDirectory(),"Route.yaml");

			if(!File.Exists(routeFilePath))
			{
				// add default route
				var content = "# resources folder\ndefaultRes : Assets/Resources\n\n# addressable folder\ngameRes : Assets/GameResources\n\n# for indirect references\nworkRes : Assets/WorkResources\n\n# config folder\nconfig : Texts/Configs";

				File.WriteAllText(routeFilePath,content.Trim());
			}

			var routeText = File.ReadAllText(routeFilePath);

			var deserializer = new DeserializerBuilder().Build();

			foreach(var pair in deserializer.Deserialize<Dictionary<string,string>>(routeText))
			{
				m_pathDict.Add(pair.Key,pair.Value);
			}
		}

		public Route GetOrCreateRoute(string path)
		{
			if(!m_routeDict.TryGetValue(path,out var route))
			{
				var pathArray = path.Split(":");

				var header = string.Empty;
				var body = string.Empty;
				var extension = string.Empty;

				if(pathArray.Length == 0)
				{
					header = ConvertPath(path);
				}
				else
				{
					var count = path.Contains(".") ? pathArray.Length-1 : pathArray.Length;
					var headerArray = new string[count];

					for(var i=0;i<count;i++)
					{
						headerArray[i] = ConvertPath(pathArray[i]);
					}

					header = Path.Combine(headerArray);

					if(path.Contains("."))
					{
						body = Path.GetFileNameWithoutExtension(pathArray[^1]);
						extension = Path.GetExtension(path);
					}
				}

				route = new Route(header,body,extension);

				m_routeDict.Add(path,route);
			}

			return route;
		}

		private string ConvertPath(string text)
		{
			return m_pathDict.TryGetValue(text,out var path) ? path : text;
		}
	}
}