using System.Collections.Generic;
using Vodovoz.Core.Domain.Sale;

namespace VodovozBusiness.Sorting
{
	/// <summary>
	/// Класс для получения приоритета сортировки карточек шаблонов автозаказов при запросе GetOrderTemplates CustomerOrdersApi
	/// </summary>
	public class OrderTemplateCardSortingPriority
	{
		/// <summary>
		/// Приоритет сортировки
		/// </summary>
		public static readonly IReadOnlyDictionary<OnlineOrderTemplateStatus, int> SortingPriority =
			new Dictionary<OnlineOrderTemplateStatus, int>
			{
				[OnlineOrderTemplateStatus.PaymentProblem] = 0,
				[OnlineOrderTemplateStatus.ManualProcessing] = 1,
				[OnlineOrderTemplateStatus.AwaitingPayment] = 2,
				[OnlineOrderTemplateStatus.Active] = 3,
				[OnlineOrderTemplateStatus.Paused] = 4
			};
		
		/// <summary>
		/// Значение по умолчанию
		/// </summary>
		public const int DefaultPriority = int.MaxValue;
		
		/// <summary>
		/// Получение приоритета исходя из статуса шаблона <see cref="OnlineOrderTemplateStatus"/> 
		/// </summary>
		/// <param name="templateStatus">Статус шаблона автозаказа</param>
		/// <returns>Приоритет сортировки</returns>
		public static int GetByStatus(OnlineOrderTemplateStatus templateStatus)
			=> SortingPriority.TryGetValue(templateStatus, out var priority)
				? priority
				: DefaultPriority;
	}
}
