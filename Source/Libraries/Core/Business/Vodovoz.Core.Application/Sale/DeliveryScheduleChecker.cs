using System;
using System.Linq;
using QS.DomainModel.UoW;
using Vodovoz.Core.Domain.Sale;
using Vodovoz.EntityRepositories.BasicHandbooks;
using VodovozBusiness.Services.Sale;

namespace Vodovoz.Core.Application.Sale
{
	public class DeliveryScheduleChecker : IDeliveryScheduleChecker
	{
		private readonly IDeliveryScheduleRepository _deliveryScheduleRepository;

		public DeliveryScheduleChecker(
			IDeliveryScheduleRepository deliveryScheduleRepository
			)
		{
			_deliveryScheduleRepository = deliveryScheduleRepository ?? throw new ArgumentNullException(nameof(deliveryScheduleRepository));
		}
		
		public bool IsDeliveryIntervalAvailable(
			IUnitOfWork uow,
			DayOfWeek weekDay,
			int deliveryPointId,
			int deliveryScheduleId
			)
		{
			return _deliveryScheduleRepository.Available(
				uow,
				weekDay,
				deliveryPointId,
				deliveryScheduleId
				);
		}
		
		public DeliveryScheduleAvailability IsDeliveryIntervalAvailable(
			IUnitOfWork uow,
			DayOfWeek deliveryWeekDay,
			DayOfWeek templateWeekDay,
			int deliveryPointId,
			int deliveryScheduleId
		)
		{
			if(deliveryWeekDay != templateWeekDay)
			{
				var schedulesLookup = _deliveryScheduleRepository.GetWeekdaysWithDeliveryInterval(
					uow,
					deliveryWeekDay,
					templateWeekDay,
					deliveryPointId,
					deliveryScheduleId
				);

				if(!schedulesLookup.Contains(templateWeekDay)
					|| !schedulesLookup[templateWeekDay].Any())
				{
					return DeliveryScheduleAvailability.NotAvailable;
				}

				if(schedulesLookup.Contains(templateWeekDay)
					&& schedulesLookup[templateWeekDay].Any()
					&& (!schedulesLookup.Contains(deliveryWeekDay)
						|| !schedulesLookup[deliveryWeekDay].Any()))
				{
					return DeliveryScheduleAvailability.AvailableNotDeliveryDate;
				}

				return DeliveryScheduleAvailability.Available;
			}

			var available = _deliveryScheduleRepository.Available(
				uow,
				deliveryWeekDay,
				deliveryPointId,
				deliveryScheduleId
			);
				
			return available ? DeliveryScheduleAvailability.Available : DeliveryScheduleAvailability.NotAvailable;
		}
	}
}
