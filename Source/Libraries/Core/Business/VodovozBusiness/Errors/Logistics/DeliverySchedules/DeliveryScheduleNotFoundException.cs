using System;

namespace VodovozBusiness.Errors.Logistics.DeliverySchedules
{
	public class DeliveryScheduleNotFoundException : Exception
	{
		public DeliveryScheduleNotFoundException(int deliveryScheduleId)
		{
			DeliveryScheduleId = deliveryScheduleId;
		}

		public int DeliveryScheduleId { get; }

		public override string Message => $"Интервал доставки с идентификатором {DeliveryScheduleId} не найден.";
	}
}
