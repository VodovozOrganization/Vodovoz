using System.Collections.Generic;
using System.Linq;
using CustomerApp.Contracts.Sale.Templates;
using Vodovoz.Core.Domain.Sale;

namespace CustomerOrdersApi.Library.V8.Services
{
	/// <summary>
	/// Класс для получения общих графиков доставки из всех дней
	/// </summary>
	public class CommonDeliverySchedulesService
	{
		/// <summary>
		/// Обработка списка интервалов доставки по дням, для получения общих значений
		/// </summary>
		/// <param name="scheduleLookup"></param>
		/// <param name="commonSchedules"></param>
		/// <returns>
		/// Если доставка производится не во все дни или во всех интервалах нет общих значений - false и пустой список интервалов
		/// Иначе true и список общих интервалов
		/// </returns>
		public bool ProcessSchedules(
			ILookup<WeekDayName, DeliveryScheduleDto> scheduleLookup,
			out IList<DeliveryScheduleDto> commonSchedules
		)
		{
			var i = 0;
			commonSchedules = new List<DeliveryScheduleDto>();

			foreach(var groupedWeeklySchedules in scheduleLookup)
			{
				if(i == 0)
				{
					commonSchedules = groupedWeeklySchedules.ToList();

					if(!commonSchedules.Any())
					{
						return false;
					}
					
					i++;
					continue;
				}

				commonSchedules = commonSchedules
					.Intersect(groupedWeeklySchedules.ToList())
					.ToList();

				//если нет пересечений хотя бы по одному дню, то бракуем
				if(!commonSchedules.Any())
				{
					return false;
				}
				
				i++;
			}

			return commonSchedules.Any();
		}
	}
}
