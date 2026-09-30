using System;
using Vodovoz.Core.Application.Sale;
using Vodovoz.Core.Domain.Common;

namespace Vodovoz.Core.Application.Extensions
{
	public static class DataContextExtensions
	{
		public static CommonRecalculateDiscount ContextDataToCommonRecalculateDiscount(this IDataContext context)
		{
			if(context.Data is not CommonRecalculateDiscount data)
			{
				throw new InvalidOperationException($"Передаваемый контекст должен быть {nameof(CommonRecalculateDiscount)}");
			}

			return data;
		}
	}
}
