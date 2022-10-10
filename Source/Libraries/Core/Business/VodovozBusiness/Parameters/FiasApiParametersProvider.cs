using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class FiasApiParametersProvider : IFiasApiParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public FiasApiParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public string FiasApiBaseUrl => _parametersProvider.GetStringValue("FiasApiBaseUrl");
		public string FiasApiToken => _parametersProvider.GetStringValue("FiasApiToken");
	}
}
