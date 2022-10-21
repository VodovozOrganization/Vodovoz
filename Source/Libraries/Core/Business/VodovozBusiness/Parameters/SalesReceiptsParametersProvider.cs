using System;
using Vodovoz.Services;

namespace Vodovoz.Parameters
{
	public class SalesReceiptsParametersProvider : ISalesReceiptsParametersProvider
	{
		private readonly IParametersProvider _parametersProvider;

		public SalesReceiptsParametersProvider(IParametersProvider parametersProvider)
		{
			_parametersProvider = parametersProvider ?? throw new ArgumentNullException(nameof(parametersProvider));
		}

		public bool SendUniqueOrderSumOrders => _parametersProvider.GetBoolValue("send_unique_order_sum_orders");
	}
}
