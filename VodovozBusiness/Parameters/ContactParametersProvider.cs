using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class ContactParametersProvider : IContactsParameters
	{
		private readonly IParametersProvider _parametersProvider;

		public ContactParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int MinSavePhoneLength => _parametersProvider.GetValue<int>("MinSavePhoneLength");
		public string DefaultCityCode => _parametersProvider.GetValue<string>("default_city_code");
	}
}
