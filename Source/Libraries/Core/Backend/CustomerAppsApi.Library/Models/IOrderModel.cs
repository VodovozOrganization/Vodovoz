using CustomerAppsApi.Library.Dto.Counterparties;

namespace CustomerAppsApi.Library.Models
{
	public interface IOrderModel
	{
		bool CanCounterpartyOrderPromoSetForNewClients(FreeLoaderCheckingDto freeLoaderCheckingDto);
	}
}
