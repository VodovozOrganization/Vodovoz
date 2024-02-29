using Autofac;
using QS.Osrm;
using System;
using Vodovoz.Settings.Common;

namespace Vodovoz.Factories
{
	public class OsrmClientFactory
	{
		private static OsrmClient _instance;
		public static OsrmClient Instance
		{
			get
			{
				if(_instance == null)
				{
					IGlobalSettings gs = ScopeProvider.Scope.Resolve<IGlobalSettings>();
					_instance = new OsrmClient(gs.OsrmServiceUrl);
				}
				return _instance;
			}
		}

		private readonly IGlobalSettings _globalSettings;

		public OsrmClientFactory(IGlobalSettings globalSettings)
		{
			_globalSettings = globalSettings ?? throw new ArgumentNullException(nameof(globalSettings));
		}

		public OsrmClient CreateClient()
		{
			return new OsrmClient(_globalSettings.OsrmServiceUrl);
		}
	}
}
