using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Domain.Client;
using Vodovoz.Domain.Orders;
using Vodovoz.Tools;

namespace Vodovoz.Presentation.ViewModels.Common.IncludeExcludeFilters
{
	public class CustomReportParametersFunc
	{
		public static readonly Func<IncludeExcludeFilter, StringBuilder, bool, IDictionary<string, object>> CounterpartyTypeReportParametersFunc =
			(filter, sb, withCounts) =>
			{
				var result = new Dictionary<string, object>();

				// Тип контрагента
				var includeCounterpartyTypeValues = filter.IncludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<CounterpartyType, CounterpartyType>))
					.ToArray();

				if(includeCounterpartyTypeValues.Length > 0)
				{
					result.Add(typeof(CounterpartyType).Name + IncludeExcludeFilter.defaultIncludePrefix,
						includeCounterpartyTypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Вкл. {typeof(CounterpartyType).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, includeCounterpartyTypeValues)}");
				}
				else
				{
					result.Add(typeof(CounterpartyType).Name + IncludeExcludeFilter.defaultIncludePrefix, new object[] { "0" });
				}

				var excludeCounterpartyTypeValues = filter.ExcludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<CounterpartyType, CounterpartyType>))
					.ToArray();

				if(excludeCounterpartyTypeValues.Length > 0)
				{
					result.Add(typeof(CounterpartyType).Name + IncludeExcludeFilter.defaultExcludePrefix,
						excludeCounterpartyTypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Искл. {typeof(CounterpartyType).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, excludeCounterpartyTypeValues)}");
				}
				else
				{
					result.Add(typeof(CounterpartyType).Name + IncludeExcludeFilter.defaultExcludePrefix, new object[] { "0" });
				}

				// Клиент Рекламного Отдела
				var includeCounterpartySubtypeValues = filter.IncludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, CounterpartySubtype>))
					.ToArray();

				if(includeCounterpartySubtypeValues.Length > 0)
				{
					result.Add(typeof(CounterpartySubtype).Name + IncludeExcludeFilter.defaultIncludePrefix,
						includeCounterpartySubtypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Вкл. {typeof(CounterpartySubtype).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, includeCounterpartySubtypeValues)}");
				}
				else
				{
					result.Add(typeof(CounterpartySubtype).Name + IncludeExcludeFilter.defaultIncludePrefix, new object[] { "0" });
				}

				var excludeCounterpartySubtypeValues = filter.ExcludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, CounterpartySubtype>))
					.ToArray();

				if(excludeCounterpartySubtypeValues.Length > 0)
				{
					result.Add(typeof(CounterpartySubtype).Name + IncludeExcludeFilter.defaultExcludePrefix,
						excludeCounterpartySubtypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Искл. {typeof(CounterpartySubtype).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, excludeCounterpartySubtypeValues)}");
				}
				else
				{
					result.Add(typeof(CounterpartySubtype).Name + IncludeExcludeFilter.defaultExcludePrefix, new object[] { "0" });
				}

				return result;
			};

		public static readonly Func<IncludeExcludeFilter, StringBuilder, bool, IDictionary<string, object>> PaymentTypeReportParametersFunc =
			(filter, sb, withCounts) =>
			{
				var result = new Dictionary<string, object>();

				// Тип оплаты
				var includePaymentTypeValues = filter.IncludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
					.ToArray();

				if(includePaymentTypeValues.Length > 0)
				{
					result.Add(typeof(PaymentType).Name + IncludeExcludeFilter.defaultIncludePrefix,
						includePaymentTypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Вкл. {typeof(PaymentType).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, includePaymentTypeValues)}");
				}
				else
				{
					result.Add(typeof(PaymentType).Name + IncludeExcludeFilter.defaultIncludePrefix, new object[] { "0" });
				}

				var excludePaymentTypeValues = filter.ExcludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentType, PaymentType>))
					.ToArray();

				if(excludePaymentTypeValues.Length > 0)
				{
					result.Add(typeof(PaymentType).Name + IncludeExcludeFilter.defaultExcludePrefix,
						excludePaymentTypeValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Искл. {typeof(PaymentType).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, excludePaymentTypeValues)}");
				}
				else
				{
					result.Add(typeof(PaymentType).Name + IncludeExcludeFilter.defaultExcludePrefix, new object[] { "0" });
				}

				// Оплата по терминалу
				var includePaymentByTerminalSourceValues = filter.IncludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
					.ToArray();

				if(includePaymentByTerminalSourceValues.Length > 0)
				{
					result.Add(typeof(PaymentByTerminalSource).Name + IncludeExcludeFilter.defaultIncludePrefix,
						includePaymentByTerminalSourceValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Вкл. {typeof(PaymentByTerminalSource).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, includePaymentByTerminalSourceValues)}");
				}
				else
				{
					result.Add(typeof(PaymentByTerminalSource).Name + IncludeExcludeFilter.defaultIncludePrefix, new object[] { "0" });
				}

				var excludePaymentByTerminalSourceValues = filter.ExcludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<PaymentByTerminalSource, PaymentByTerminalSource>))
					.ToArray();

				if(excludePaymentByTerminalSourceValues.Length > 0)
				{
					result.Add(typeof(PaymentByTerminalSource).Name + IncludeExcludeFilter.defaultExcludePrefix,
						excludePaymentByTerminalSourceValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Искл. {typeof(PaymentByTerminalSource).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, excludePaymentByTerminalSourceValues)}");
				}
				else
				{
					result.Add(typeof(PaymentByTerminalSource).Name + IncludeExcludeFilter.defaultExcludePrefix, new object[] { "0" });
				}

				// Оплачено онлайн
				var includePaymentFromValues = filter.IncludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
					.ToArray();

				if(includePaymentFromValues.Length > 0)
				{
					result.Add(typeof(PaymentFrom).Name + IncludeExcludeFilter.defaultIncludePrefix,
						includePaymentFromValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Вкл. {typeof(PaymentFrom).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, includePaymentFromValues)}");
				}
				else
				{
					result.Add(typeof(PaymentFrom).Name + IncludeExcludeFilter.defaultIncludePrefix, new object[] { "0" });
				}

				var excludePaymentFromValues = filter.ExcludedElements
					.Where(x => x.GetType() == typeof(IncludeExcludeElement<int, PaymentFrom>))
					.ToArray();

				if(excludePaymentFromValues.Length > 0)
				{
					result.Add(typeof(PaymentFrom).Name + IncludeExcludeFilter.defaultExcludePrefix,
						excludePaymentFromValues.Select(x => x.Number).ToArray());
					sb.AppendLine($"Искл. {typeof(PaymentFrom).GetClassUserFriendlyName().GenitivePlural.ToLower()}: {GetFilterValue(withCounts, excludePaymentFromValues)}");
				}
				else
				{
					result.Add(typeof(PaymentFrom).Name + IncludeExcludeFilter.defaultExcludePrefix, new object[] { "0" });
				}

				return result;
			};
		
		private static string GetFilterValue(bool withCounts, IncludeExcludeElement[] filterElements)
		{
			var res = withCounts
				? filterElements.Length.ToString()
				: string.Join(",", filterElements.Select(x => x.Title));
			
			return res;
		}
	}
}
