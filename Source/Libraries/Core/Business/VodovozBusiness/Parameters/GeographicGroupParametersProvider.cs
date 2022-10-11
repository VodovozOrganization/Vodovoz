using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class GeographicGroupParametersProvider : IGeographicGroupParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public GeographicGroupParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int SouthGeographicGroupId => _parametersProvider.GetIntValue("south_geographic_group_id");
		public int NorthGeographicGroupId => _parametersProvider.GetIntValue("north_geographic_group_id");
		public int EastGeographicGroupId => _parametersProvider.GetIntValue("east_geographic_group_id");
	}
}
