using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class CarEventSettings : ICarEventSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public CarEventSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider;
		}

		public int DontShowCarEventByReportId => _parametersProvider.GetIntValue("dont_show_car_event_by_report_id");
	}
}
