namespace VodovozBusiness.Services.Orders
{
	public interface IClientDeliveryPointsChecker
	{
		bool ClientDeliveryPointExists(int counterpartyId, int deliveryPointId);
	}
}
