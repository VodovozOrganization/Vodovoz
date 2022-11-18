using QS.Report;
using System;
using VodovozInfrastructure.Connections;

namespace Vodovoz.Reports
{
	public class ReportFactory
    {
		private readonly IConnectionStringProvider _connectionStringProvider;

		public ReportFactory(IConnectionStringProvider connectionStringProvider)
		{
			_connectionStringProvider = connectionStringProvider ?? throw new ArgumentNullException(nameof(connectionStringProvider));
		}

		public ReportInfo CreateReport()
		{
			var slaveConnectionString = _connectionStringProvider.SlaveConnectionString;

			return new ReportInfo(slaveConnectionString);
		}
    }
}
