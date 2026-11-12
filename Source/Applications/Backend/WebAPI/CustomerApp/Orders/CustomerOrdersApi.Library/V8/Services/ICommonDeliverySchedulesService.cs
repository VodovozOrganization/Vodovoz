using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Services
{
	public interface ICommonDeliverySchedulesService
	{
		/// <summary>
		/// Обработка списка интервалов доставки по дням, для получения общих значений
		/// </summary>
		/// <param name="scheduleLookup">Lookup интервалов по дням недели</param>
		/// <param name="commonSchedules">Подобранные общие интервалы из всех дней. Если таких нет - пустой список</param>
		/// <returns>
		/// Если доставка производится не во все дни или во всех интервалах нет общих значений - false и пустой список интервалов
		/// Иначе true и список общих интервалов
		/// </returns>
		bool ProcessSchedules(
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup,
			out IList<DeliveryScheduleDto> commonSchedules
		);
	}
}
