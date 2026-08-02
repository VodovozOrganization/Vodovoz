using CustomerAppsApi.Library.V1.Dto.Counterparties;

namespace CustomerAppsApi.Library.V1.Models
{
	public interface IOrderModel
	{
		/// <summary>
		/// Может ли клиент заказывать промонаборы для новых клиентов
		/// </summary>
		/// <param name="freeLoaderCheckingDto">Входящие данные для проверки <see cref="FreeLoaderCheckingDto"/></param>
		/// <returns><c>true</c> - да, <c>false</c> - нет</returns>
		bool CanCounterpartyOrderPromoSetForNewClients(FreeLoaderCheckingDto freeLoaderCheckingDto);
	}
}
