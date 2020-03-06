using System;
using QS.Project.VersionControl;

namespace Vodovoz.Infrastructure
{
	public class DBInfo : IDataBaseInfo
	{
		public string Name { get; set; }

		public DBInfo(string name)
		{
			Name = name;
		}
	}
}
