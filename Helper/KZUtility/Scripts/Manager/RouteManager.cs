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
				var content = @"
				# resources folder\n
				defaultRes : Assets/Resources\n\n

				# addressable folder\n
				gameRes : Assets/GameResources\n\n

				# for indirect references\n
				workRes : Assets/WorkResources\n\n

				# config folder\n
				config : Text/Config\n\n

				# proto folder\n
				proto : Text/Proto";

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

				if(pathArray.Length == 0)
				{
					route = new Route(ConvertPath(path),string.Empty,string.Empty);
				}
				else
				{
					var count = path.Contains(".") ? pathArray.Length-1 : pathArray.Length;
					var headerArray = new string[count];

					for(var i=0;i<count;i++)
					{
						headerArray[i] = ConvertPath(pathArray[i]);
					}

					var header = Path.Combine(headerArray);
					var body = string.Empty;
					var extension = string.Empty;

					if(path.Contains("."))
					{
						body = Path.Combine(Path.GetDirectoryName(pathArray[^1]),Path.GetFileNameWithoutExtension(pathArray[^1]));
						extension = Path.GetExtension(path);
					}

					route = new Route(header,body,extension);
				}

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