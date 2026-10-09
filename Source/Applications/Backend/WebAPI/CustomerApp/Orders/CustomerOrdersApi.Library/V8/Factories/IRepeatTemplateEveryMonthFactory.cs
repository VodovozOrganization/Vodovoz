using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Factories
{
	/// <summary>
	/// Фабрика создания параметров доставок по числам месяца для автозаказа
	/// </summary>
	public interface IRepeatTemplateEveryMonthFactory
	{
		/// <summary>
		/// Получение параметров доставок по числам месяца
		/// </summary>
		/// <param name="scheduleLookup">Список доступных интервалов по дням</param>
		/// <returns></returns>
		TemplateMonthlySchedule Create(ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup);
	}
}
