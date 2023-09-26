namespace CustomerAppsApi.Models
{
	public interface IOrderModel
	{
		bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId);
	}
}
