namespace Vodovoz.Domain.Roboats
{
	public enum RoboatsCallFailType
	{
		None,
		UnknownRequestType,
		PhoneMissing,
		ClientDuplicate,
		ClientNotFound,
		ClientNameNotFound,
		ClientPatronymicNotFound,
		DeliveryPointsNotFound,
		IncorrectAddressId,
		StreetNotFound,
		HouseNotFound,
		CorpusNotFound,
		ApartmentNotFound,
		DeliveryIntervalsNotFound,
		AddressIdNotSpecified,
		OrderNotFound,
		IncorrectOrderId,
		AvailableWatersNotFound,
		WaterInOrderNotFound,
		WaterNotSupported,
		BottlesReturnNotFound,
		Exception,
		NegativeOrderSum,
		IncorrectOrderDate,
		IncorrectOrderInterval,
		OrderIntervalNotFound,
		UnknownIsTerminalValue
	}

	public class RoboatsCallFailTypeStringType : NHibernate.Type.EnumStringType
	{
		public RoboatsCallFailTypeStringType() : base(typeof(RoboatsCallFailType))
		{
		}
	}
}
