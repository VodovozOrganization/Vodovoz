namespace CustomerAppsApi.Library.Models
{
	public interface IOrderModel
	{
		bool CanCounterpartyOrderPromoSetForNewClients(int counterpartyId);
	}
}
