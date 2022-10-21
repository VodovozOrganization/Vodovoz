using System;

namespace Vodovoz.Parameters
{
	public class RegistrationTypeSettings : IRegistrationTypeSettings
	{
		private readonly IParametersProvider _parametersProvider;

		public RegistrationTypeSettings(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		//TODO добавить в параметры
		public int GetContractRegistrationTypeId => _parametersProvider.GetIntValue("contract_registration_type_id");
		public int GetLaborCodeRegistrationTypeId => _parametersProvider.GetIntValue("labor_code_registration_type_id");
	}
}
