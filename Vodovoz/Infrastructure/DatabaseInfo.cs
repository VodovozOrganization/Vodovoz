using QS.Project.Versioning;

namespace Vodovoz.Infrastructure
{
	public class DatabaseInfo : IDataBaseInfo
	{
		public string Name { get; set; }
		public bool IsDemo { get; set; }

		public DatabaseInfo(string name, bool isDemo)
		{
			Name = name;
			IsDemo = isDemo;
		}
	}
}
