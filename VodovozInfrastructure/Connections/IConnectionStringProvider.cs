using System;
using System.Collections.Generic;
using System.Text;

namespace VodovozInfrastructure.Connections
{
	public interface IConnectionStringProvider
	{
		string MasterConnectionString { get; }
		string SlaveConnectionString { get; }
	}
}
