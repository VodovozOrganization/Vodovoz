using QS.Project.DB;
using System;

namespace Vodovoz.Infrastructure.Database
{
	public class DatabaseInfo : IDataBaseInfo
	{
		public string Name { get; set; }
		public bool IsDemo { get; set; }

		public Guid? BaseGuid { get; }

		public Version Version { get; }

		public DatabaseInfo(string name, bool isDemo = false)
		{
			Name = name;
			IsDemo = isDemo;
		}
	}
}
