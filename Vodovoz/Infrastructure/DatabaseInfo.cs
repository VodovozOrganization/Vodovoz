using QS.Project.VersionControl;

namespace Vodovoz.Infrastructure
{
	public class DatabaseInfo : IDataBaseInfo
	{
		public string Name { get; set; }

		public DatabaseInfo(string name)
		{
			Name = name;
		}
	}
}
