using Microsoft.Extensions.Logging;
using QS.Osrm;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vodovoz.Settings.Common;
using QsOsrmClient = QS.Osrm.OsrmClient;

namespace Osrm
{
	public class OsrmClient : IOsrmClient
	{
		private QsOsrmClient _qsOsrmClient;

		public OsrmClient(ILoggerFactory loggerFactory, IOsrmSettings osrmSettings)
		{
			var logger = loggerFactory.CreateLogger<QsOsrmClient>();
			var url = osrmSettings.OsrmServiceUrl;
			_qsOsrmClient = new QsOsrmClient(logger, url);
		}

		public RouteResponse GetRoute(
			List<PointOnEarth> routePOIs,
			bool alt = false,
			GeometryOverview geometry = GeometryOverview.False,
			bool excludeToll = false
		)
		{
			return ((IOsrmClient)_qsOsrmClient).GetRoute(routePOIs, alt, geometry, excludeToll);
		}

		public Task<RouteResponse> GetRouteAsync(
			List<PointOnEarth> routePOIs,
			CancellationToken cancellationToken,
			bool alt = false,
			GeometryOverview geometry = GeometryOverview.False,
			bool excludeToll = false
		)
		{
			return ((IOsrmClient)_qsOsrmClient).GetRouteAsync(routePOIs, cancellationToken, alt, geometry, excludeToll);
		}
	}
}
