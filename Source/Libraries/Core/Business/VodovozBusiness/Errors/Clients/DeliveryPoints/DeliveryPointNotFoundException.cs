using System;

namespace VodovozBusiness.Errors.Clients.DeliveryPoints
{
	public class DeliveryPointNotFoundException : Exception
	{
		public DeliveryPointNotFoundException(int deliveryPointId, int counterpartyId)
		{
			DeliveryPointId = deliveryPointId;
			CounterpartyId = counterpartyId;
		}

		public int DeliveryPointId { get; }
		public int CounterpartyId { get; }

		public override string Message => $"Точка доставки с идентификатором {DeliveryPointId} контрагента {CounterpartyId} не найдена.";
	}
}
