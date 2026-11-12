using System;
using QS.DomainModel.UoW;
using Vodovoz.EntityRepositories.BasicHandbooks;

namespace Vodovoz.Core.Application.Sale
{
	public class DeliveryScheduleChecker
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
	}
}
