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

		public int CompensationFromInsuranceByCourtId => _parametersProvider.GetIntValue("compensation_from_insurance_by_court_id");
		public int CarEventStartNewPeriodDay => _parametersProvider.GetIntValue("CarEventStartNewPeriodDay");
	}
}
