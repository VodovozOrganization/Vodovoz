using CustomerAppsApi.Library.V2.Dto.Counterparties;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace CustomerAppsApi.Library.V2.Models.Identification
{
	/// <summary>
	/// Сопоставление адреса из запроса с точкой доставки
	/// </summary>
	public interface IAddressMatcher
	{
		/// <summary>
		/// Сопоставление улицы
		/// </summary>
		bool MatchesStreet(string requestStreet, string street, string streetType);
		/// <summary>
		/// Возвращает номер дома из запроса
		/// </summary>
		string GetBuildingNumber(CounterpartyAddressSearchDto request);
		/// <summary>
		/// Сопоставление дома, корпуса, строения и литературы
		/// </summary>
		bool MatchesBuilding(CounterpartyAddressSearchDto request, string building, string letter);
		/// <summary>
		/// Сопоставление помещения
		/// </summary>
		bool MatchesRoom(CounterpartyAddressSearchDto request, string room, RoomType roomType, string placement);
	}
}
