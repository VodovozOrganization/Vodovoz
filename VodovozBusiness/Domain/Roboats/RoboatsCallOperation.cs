namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallOperation
	{
		OnClientHandle,
		ClientCheck,
		GetClientName,
		GetClientPatronymic,
		OnAddressHandle,
		GetDeliveryPoints,
		GetStreetId,
		GetHouseNumber,
		GetCorpusNumber,
		GetApartmentNumber,
		OnDeliveryIntervalsHandle,
		GetDeliveryIntervals,
		OnLastOrderHandle,
		GetLastOrderId,
		GetWaterInfo,
		GetBottlesReturn,
		OnWaterTypeHandle,
		OnOrderHandle,
		CalculateOrderPrice,
		CreateOrder
	}

	public class RoboatsCallOperationStringType : NHibernate.Type.EnumStringType
	{
		public RoboatsCallOperationStringType() : base(typeof(RoboatsCallOperation))
		{
		}
	}
}
