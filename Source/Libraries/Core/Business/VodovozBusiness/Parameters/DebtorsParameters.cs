using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class DebtorsParameters : IDebtorsParameters
	{
		private readonly IParametersProvider _parametersProvider;

		public DebtorsParameters(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int GetSuspendedCounterpartyId => _parametersProvider.GetIntValue("HideSuspendedCounterparty");
		public int GetCancellationCounterpartyId => _parametersProvider.GetIntValue("HideCancellationCounterparty");
	}
}
