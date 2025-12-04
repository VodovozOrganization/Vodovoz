using System;
using System.Collections.Generic;
using System.Linq;
using Vodovoz.Domain.Client;

namespace VodovozBusiness.Extensions
{
	/// <summary>
	/// Расширения для откуда клиент <see cref="CounterpartyFrom"/>
	/// </summary>
	public static class CounterpartyFromExtensions
	{
		public static IEnumerable<CounterpartyFrom> ExceptCurrentValues(this CounterpartyFrom source)
		{
			var array = Enum.GetValues(typeof(CounterpartyFrom));
			return array.Cast<CounterpartyFrom>()
				.Where(item => source != item)
				.ToList();
		}
	}
}
