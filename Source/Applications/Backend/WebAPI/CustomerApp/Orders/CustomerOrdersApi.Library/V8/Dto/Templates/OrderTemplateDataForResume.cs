using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Dto.Templates
{
	/// <summary>
	/// Данные, необходимые для возобновления шаблона автозаказа
	/// </summary>
	public class OrderTemplateDataForResume
	{
		/// <summary>
		/// Шаблон автозаказа
		/// </summary>
		public OnlineOrderTemplate Template { get; set; }

		/// <summary>
		/// Интервал доставки
		/// </summary>
		public DeliveryScheduleData DeliveryInterval { get; set; }
	}
}
