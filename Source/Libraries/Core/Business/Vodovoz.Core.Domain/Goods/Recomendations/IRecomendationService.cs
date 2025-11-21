using QS.DomainModel.UoW;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
		/// Получить рекомендации для ИПЗ
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="source">Внешний источник</param>
		/// <param name="personType">Тип КА</param>
		/// <param name="roomType">Тип помещения</param>
		/// <param name="excludeNomenclatures">Исключить номенклатуры</param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список строк рекомендаций</returns>
		Task<IEnumerable<RecomendationItem>> GetRecomendationItemsForIpz(IUnitOfWork unitOfWork, Source source, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures, CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить рекомендации для робота
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="personType">Тип КА</param>
		/// <param name="roomType">Тип помещения</param>
		/// <param name="excludeNomenclatures"></param>
		/// <param name="cancellationToken">Токен отмены</param>
		/// <returns>Список строк рекомендаций</returns>
		Task<IEnumerable<RecomendationItem>> GetRecomendationItemsForRobot(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures, CancellationToken cancellationToken = default);

		/// <summary>
		/// Получить рекомендации для оператора из программы ДВ
		/// </summary>
		/// <param name="unitOfWork">UnitOfWork</param>
		/// <param name="personType">Тип КА</param>
		/// <param name="roomType">Тип помещения</param>
		/// <param name="excludeNomenclatures">Исключить номенклатуры</param>
		/// <returns>Список строк рекомендаций</returns>
		IEnumerable<RecomendationItem> GetRecomendationItemsForOperator(IUnitOfWork unitOfWork, PersonType personType, RoomType roomType, IEnumerable<int> excludeNomenclatures);
	}
}
