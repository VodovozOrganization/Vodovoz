using QS.DomainModel.UoW;
using System.Collections.Generic;
using Vodovoz.Core.Domain.Clients;
using Vodovoz.Core.Domain.Clients.DeliveryPoints;

namespace Vodovoz.Core.Domain.Goods.Recomendations
{
	/// <summary>
	/// Сервис получения рекомендаций номенклатур
	/// </summary>
	public interface IRecomendationService
	{
		/// <summary>
		/// Получить рекомендации для оператора из программы ДВ
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="personType">Тип КА</param>
		/// <param name="roomType">Тип помещения</param>
		/// <param name="excludeNomenclatures">Исключить номенклатуры</param>
		/// <returns>Список рекомендаций</returns>
		IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
		IEnumerable<RecomendationItem> GetRecomendationItemsForRobot(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
		IEnumerable<RecomendationItem> GetRecomendationItemsForIpz(IUnitOfWork unitOfWork, Source source, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
	}
}
