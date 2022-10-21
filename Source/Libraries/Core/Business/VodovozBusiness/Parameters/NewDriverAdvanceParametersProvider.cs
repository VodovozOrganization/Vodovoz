using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class NewDriverAdvanceParametersProvider : INewDriverAdvanceParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public NewDriverAdvanceParametersProvider(IParametersProvider parametersProvider)
		{
			this._parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public int NewDriverAdvanceFirstDay => _parametersProvider.GetIntValue("new_driver_advance_first_day");
		public int NewDriverAdvanceLastDay => _parametersProvider.GetIntValue("new_driver_advance_last_day");
		public decimal NewDriverAdvanceSum => _parametersProvider.GetDecimalValue ("new_driver_advance_sum");
		public bool IsNewDriverAdvanceEnabled => _parametersProvider.GetBoolValue("is_new_driver_advance_enabled");
	}
}
